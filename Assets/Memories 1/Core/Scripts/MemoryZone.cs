using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BeyondTime.Memories
{
    public enum StepType
    {
        Narrate,        // story line at the bottom, E or wait to continue (timer paused by default)
        ReachPoint,     // walk to a target
        HoldInteract,   // stand near a target and hold E; any hit resets progress
        DefeatShadows,  // kill every shadow that is alive
        SurviveTime,    // survive for X seconds (optionally stay inside the target's radius)
        Choice,         // time freezes, pick 1 / 2 / 3; can branch, punish, or end the memory
        Escort,         // lead a MemoryNPC to the target while shadows hurt it
        Collect,        // pick up every MemoryCollectible under Collect Root
        DefeatFighters  // knock out every Fighter in the list (NPC brawlers); optional person to protect
    }

    [System.Serializable]
    public class MemoryChoice
    {
        public string label = "Choice";
        [TextArea(2, 4)] public string resultText;
        [Tooltip("Right choice = memory is REDEEMED. Wrong = the wrong repeats and the memory is CONDEMNED.")]
        public bool righteous;
        public float timePenalty;
        public int extraShadows;
        [Tooltip("-1 = continue with the step after the choice.")]
        public int goToStep = -1;
        public bool endMemory;
        [Tooltip("Optional: this person is hurt because of the choice (for example the victim while you look away).")]
        public Fighter hurtFighter;
        public float hurtAmount;
    }

    [System.Serializable]
    public class MemoryStep
    {
        [Tooltip("Only a label for you. It becomes the element name in the list.")]
        public string label = "Step";
        public StepType type = StepType.Narrate;
        [TextArea(2, 5)] public string story;
        public string objective;

        [Header("Target (Reach, Hold, Survive, Escort)")]
        public Transform target;
        public float radius = 3f;

        [Header("Shadows spawned when the step starts")]
        public int shadowsOnStart;
        public Transform[] spawnPoints;

        [Header("Hold E")]
        public float holdTime = 3f;
        public string holdText = "Hold E";

        [Header("Survive")]
        public float surviveSeconds = 20f;
        public bool mustStayNearTarget;
        [Tooltip("Keeps topping shadows up to this number while you survive.")]
        public int keepShadowsAlive;

        [Header("Collect")]
        public Transform collectRoot;

        [Header("Escort")]
        public MemoryNPC escortNpc;

        [Header("Fighters (DefeatFighters)")]
        public Fighter[] fighters;
        [Tooltip("Shown as a percentage in the objective. Add MemoryProtectee to them to fail the memory if they fall.")]
        public Fighter protect;
        [Tooltip("These NPCs start fighting when the step begins (attackers begin their attack).")]
        public BrawlerAI[] engageOnStart;

        [Header("Choice")]
        public string choicePrompt;
        [Tooltip("Seconds to decide. 0 = no limit. If time runs out, Default Choice is picked (hesitation repeats the wrong).")]
        public float choiceTimeout = 10f;
        public int defaultChoice;
        public MemoryChoice[] choices;

        [Header("When this step starts")]
        public GameObject[] activate;
        public GameObject[] deactivate;
        public MemoryActor actor;
        public string actorState;
        public float bonusTime;

        [Header("Flow")]
        [Tooltip("-1 = next step in the list.")]
        public int nextStep = -1;
        public bool endMemoryAfter;
    }

    /// <summary>
    /// One memory. Put it on the root of a memory island. A Box Collider (trigger) covers the island:
    /// walking in starts the memory. Steps run top to bottom; choices can branch.
    /// Win: redeemed or condemned, unlocks the next bridge. Lose (timer or escort dies): collapse and retry.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MemoryZone : MonoBehaviour
    {
        public static readonly List<MemoryZone> All = new List<MemoryZone>();
        public static MemoryZone Active { get; private set; }

        [Header("Identity")]
        public string memoryId = "memory_01";
        public string memoryTitle = "Memory";

        [Header("Rules")]
        public float timeLimit = 120f;
        public bool timerDuringNarration = false;
        public float narrateAutoAdvance = 3.5f;
        public bool autoRetry = true;

        [Header("Shadows")]
        [Tooltip("Empty = use MemorySettings > Shadow Prefab.")]
        public GameObject shadowPrefabOverride;
        public Transform[] defaultSpawnPoints;
        public float npcDamagePerSecond = 14f;
        public float npcDamageRange = 1.9f;

        [Header("Places")]
        public Transform respawnPoint;

        [Header("Steps")]
        public List<MemoryStep> steps = new List<MemoryStep>();

        [Header("Endings")]
        [TextArea(2, 4)] public string redeemedText = "You made it right. The memory is at peace.";
        [TextArea(2, 4)] public string condemnedText = "The memory is stable, but the wrong was repeated.";
        [TextArea(2, 4)] public string collapsedText = "The flames consumed the memory before you could make it right.";

        [Header("On completion")]
        public GameObject[] unlockOnComplete;       // bridges (with BridgeReveal), doors
        public GameObject[] disableOnComplete;      // this island's fires
        public UnityEvent onStarted, onCompleted, onFailed;

        public bool IsRunning => state == State.Running;
        public bool IsCompleted => state == State.Completed;
        public float TimeLeft => timeLeft;

        enum State { Idle, Running, Collapsing, Completed }
        State state = State.Idle;

        float timeLeft;
        bool timerOn, failed, wasHit, endNow, madeChoice, lastChoiceRight, choosing;
        int nextIndex, spawnCursor;
        string failReason;

        readonly List<GameObject> spawned = new List<GameObject>();
        readonly Dictionary<GameObject, bool> initialActive = new Dictionary<GameObject, bool>();
        Transform player;
        MemoryUI ui;
        MemorySettings settings;

        // ------------------------------------------------------------ Unity

        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            settings = MemorySettings.Load();

            foreach (var s in steps)
            {
                Remember(s.activate); Remember(s.deactivate);
                if (s.collectRoot) { initialActive[s.collectRoot.gameObject] = false; s.collectRoot.gameObject.SetActive(false); }
            }
        }

        void Remember(GameObject[] list)
        {
            if (list == null) return;
            foreach (var g in list) if (g && !initialActive.ContainsKey(g)) initialActive[g] = g.activeSelf;
        }

        void OnEnable()
        {
            All.Add(this);
            MemoryEvents.PlayerHit += OnPlayerHit;
        }

        void OnDisable()
        {
            All.Remove(this);
            MemoryEvents.PlayerHit -= OnPlayerHit;
            if (choosing) Time.timeScale = 1f;
            if (Active == this) Active = null;
        }

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;

            if (MemoryProgress.IsDone(memoryId))
            {
                state = State.Completed;
                ApplyCompletionWorld();
            }
            var u = MemoryUI.Instance; if (u) u.RefreshLedger();
        }

        void OnTriggerEnter(Collider other)
        {
            if (state == State.Idle && other.CompareTag("Player")) Begin();
        }

        void Update()
        {
            if (state != State.Running) return;
            if (timerOn && !failed)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0f) { timeLeft = 0f; Fail(null); }
            }
            if (ui) ui.SetTimer(timeLeft);
        }

        void OnPlayerHit(float seconds)
        {
            if (state != State.Running) return;
            wasHit = true;
            timeLeft = Mathf.Max(0f, timeLeft - seconds);
            if (ui) ui.ShowPenalty(seconds);
            if (timeLeft <= 0f) Fail(null);
        }

        void Fail(string reason) { if (!failed) { failed = true; failReason = reason; } }

        /// <summary>Collapse the running memory from outside (for example the person you protect falls).</summary>
        public void FailMemory(string reason) { if (state == State.Running) Fail(reason); }

        // ------------------------------------------------------------ run

        public void Begin()
        {
            if (state == State.Running || state == State.Completed) return;
            if (Active && Active != this && Active.IsRunning) return;
            if (!player) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) player = p.transform; }
            ui = MemoryUI.Ensure();
            if (!ui || !player) { Debug.LogError("MemoryZone '" + memoryTitle + "': needs a MemoryUI and a Player-tagged object.", this); return; }

            Active = this;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            state = State.Running;
            failed = false; failReason = null; madeChoice = false; lastChoiceRight = true;
            timeLeft = timeLimit; timerOn = false;
            if (respawnPoint) MemoryCheckpoint.SetRespawn(respawnPoint);

            ui.BeginMemory(memoryTitle);
            ui.SetTimer(timeLeft);
            onStarted?.Invoke();

            int i = 0;
            while (i >= 0 && i < steps.Count)
            {
                var s = steps[i];
                nextIndex = s.nextStep >= 0 ? s.nextStep : i + 1;
                endNow = s.endMemoryAfter;

                StartStep(s);
                yield return RunStep(s);

                ui.ShowHold(false, 0f, "");
                ui.SetMarker(null);
                if (failed || endNow) break;
                i = nextIndex;
            }

            timerOn = false;
            if (failed) yield return Collapse();
            else Complete();
        }

        void StartStep(MemoryStep s)
        {
            if (s.activate != null) foreach (var g in s.activate) if (g) g.SetActive(true);
            if (s.deactivate != null) foreach (var g in s.deactivate) if (g) g.SetActive(false);
            if (s.actor) s.actor.Play(s.actorState);
            if (s.bonusTime > 0f) timeLeft += s.bonusTime;

            if (s.type != StepType.Narrate && s.type != StepType.Choice)
            {
                timerOn = true;
                ui.ShowStory(s.story);
            }
            ui.SetObjective(s.objective);

            if (s.type == StepType.ReachPoint || s.type == StepType.HoldInteract ||
                s.type == StepType.Escort || (s.type == StepType.SurviveTime && s.mustStayNearTarget))
                ui.SetMarker(s.target);
            if (s.type == StepType.DefeatFighters && s.fighters != null && s.fighters.Length > 0 && s.fighters[0])
                ui.SetMarker(s.fighters[0].transform);

            if (s.shadowsOnStart > 0) Spawn(s.shadowsOnStart, s.spawnPoints);
            if (s.engageOnStart != null) foreach (var ai in s.engageOnStart) if (ai) ai.Engage();
        }

        IEnumerator RunStep(MemoryStep s)
        {
            switch (s.type)
            {
                case StepType.Narrate:       yield return Narrate(s); break;
                case StepType.ReachPoint:    yield return Reach(s); break;
                case StepType.HoldInteract:  yield return Hold(s); break;
                case StepType.DefeatShadows: yield return Defeat(s); break;
                case StepType.SurviveTime:   yield return Survive(s); break;
                case StepType.Choice:        yield return Choose(s); break;
                case StepType.Escort:        yield return Escort(s); break;
                case StepType.Collect:       yield return Collect(s); break;
                case StepType.DefeatFighters: yield return DefeatFighters(s); break;
            }
        }

        // ------------------------------------------------------------ steps

        IEnumerator Narrate(MemoryStep s)
        {
            timerOn = timerDuringNarration;
            ui.ShowStory(s.story);
            ui.SetContinueHint(true);
            float wait = 0f;
            yield return null;
            while (!failed)
            {
                bool e = EPressed();
                if (ui.IsTyping) { if (e) ui.FinishTyping(); }
                else
                {
                    wait += Time.deltaTime;
                    if (e || wait >= narrateAutoAdvance) break;
                }
                yield return null;
            }
            ui.SetContinueHint(false);
        }

        IEnumerator Reach(MemoryStep s)
        {
            if (!s.target) { Warn(s, "has no Target"); yield break; }
            while (!failed && Dist(s.target) > s.radius) yield return null;
        }

        IEnumerator Hold(MemoryStep s)
        {
            if (!s.target) { Warn(s, "has no Target"); yield break; }
            float p = 0f, lockedUntil = 0f;
            wasHit = false;
            while (!failed)
            {
                if (wasHit) { wasHit = false; if (p > 0f) lockedUntil = Time.time + 1f; p = 0f; }

                if (Dist(s.target) > s.radius)
                {
                    ui.ShowHold(false, 0f, "");
                    p = Mathf.Max(0f, p - Time.deltaTime * 2f);
                    yield return null; continue;
                }
                if (Time.time < lockedUntil)
                {
                    ui.ShowHold(true, 0f, "Interrupted. Clear some space.");
                    yield return null; continue;
                }

                if (EHeld()) p += Time.deltaTime; else p = Mathf.Max(0f, p - Time.deltaTime);
                ui.ShowHold(true, p / Mathf.Max(0.01f, s.holdTime), s.holdText);
                if (p >= s.holdTime) break;
                yield return null;
            }
        }

        IEnumerator Defeat(MemoryStep s)
        {
            int last = -1;
            while (!failed)
            {
                int alive = AliveCount();
                if (alive == 0) break;
                if (alive != last) { last = alive; ui.SetObjective(s.objective + "   (" + alive + " left)"); }
                yield return null;
            }
        }

        IEnumerator Survive(MemoryStep s)
        {
            float t = 0f, nextTopUp = 0f;
            while (!failed && t < s.surviveSeconds)
            {
                bool inside = !s.mustStayNearTarget || (s.target && Dist(s.target) <= s.radius);
                if (inside) t += Time.deltaTime;

                if (s.keepShadowsAlive > 0 && Time.time >= nextTopUp && AliveCount() < s.keepShadowsAlive)
                {
                    Spawn(1, s.spawnPoints);
                    nextTopUp = Time.time + 2.5f;
                }

                ui.SetObjective(inside
                    ? s.objective + "   " + Mathf.CeilToInt(s.surviveSeconds - t) + "s"
                    : "Return to the light!");
                yield return null;
            }
        }

        IEnumerator Choose(MemoryStep s)
        {
            if (s.choices == null || s.choices.Length == 0) { Warn(s, "has no choices"); yield break; }

            timerOn = false;
            if (!string.IsNullOrEmpty(s.story)) ui.ShowStory(s.story);

            var labels = new string[Mathf.Min(3, s.choices.Length)];
            for (int i = 0; i < labels.Length; i++) labels[i] = s.choices[i].label;

            int pickedIndex = 0;
            choosing = true;
            Time.timeScale = 0f;                       // time stands still while you decide
            yield return ui.RunChoice(s.choicePrompt, labels, s.choiceTimeout, s.defaultChoice, r => pickedIndex = r);
            Time.timeScale = 1f;
            choosing = false;
            timerOn = true;

            var c = s.choices[pickedIndex];
            madeChoice = true;
            lastChoiceRight = c.righteous;

            if (c.timePenalty > 0f) { timeLeft = Mathf.Max(1f, timeLeft - c.timePenalty); ui.ShowPenalty(c.timePenalty); }
            if (c.extraShadows > 0) Spawn(c.extraShadows, s.spawnPoints);
            if (c.hurtFighter && c.hurtAmount > 0f) c.hurtFighter.TakeHit(Mathf.RoundToInt(c.hurtAmount), 0.8f);
            if (!string.IsNullOrEmpty(c.resultText)) ui.ShowStory(c.resultText);

            if (c.goToStep >= 0) nextIndex = c.goToStep;
            if (c.endMemory)
            {
                endNow = true;
                timerOn = false;
                yield return null;
                while (ui.IsTyping) yield return null;
                yield return new WaitForSeconds(1.5f);
            }
        }

        IEnumerator Escort(MemoryStep s)
        {
            var npc = s.escortNpc;
            if (!npc || !s.target) { Warn(s, "needs an Escort Npc and a Target"); yield break; }

            npc.BeginFollow(player);
            int n = 0;
            foreach (var g in spawned)
            {
                if (!g) continue;
                var ms = g.GetComponent<MemoryShadow>();
                if (ms && (n++ % 2 == 0)) ms.targetOverride = npc.transform;
            }

            while (!failed)
            {
                foreach (var g in spawned)
                    if (g && g.activeInHierarchy && Vector3.Distance(g.transform.position, npc.transform.position) <= npcDamageRange)
                        npc.Damage(npcDamagePerSecond * Time.deltaTime);

                if (npc.IsDead) { Fail(npc.DisplayName + " was taken by the shadows."); break; }
                if (Vector3.Distance(npc.transform.position, s.target.position) <= s.radius) break;

                ui.SetObjective(s.objective + "   (" + npc.DisplayName + " " + Mathf.CeilToInt(npc.Health01 * 100f) + "%)");
                yield return null;
            }
            npc.StopFollow();
        }

        IEnumerator Collect(MemoryStep s)
        {
            if (!s.collectRoot) { Warn(s, "has no Collect Root"); yield break; }
            s.collectRoot.gameObject.SetActive(true);
            var items = s.collectRoot.GetComponentsInChildren<MemoryCollectible>(true);
            foreach (var it in items) it.ResetItem();

            int last = -1;
            while (!failed)
            {
                int got = 0;
                foreach (var it in items) if (it.Collected) got++;
                if (got != last) { last = got; ui.SetObjective(s.objective + "   " + got + " / " + items.Length); }
                if (got >= items.Length) break;
                yield return null;
            }
        }

        IEnumerator DefeatFighters(MemoryStep s)
        {
            if (s.fighters == null || s.fighters.Length == 0) { Warn(s, "has no Fighters"); yield break; }
            string last = null;
            while (!failed)
            {
                int left = 0;
                foreach (var f in s.fighters) if (f && !f.IsDefeated) left++;
                if (left == 0) break;

                string text = s.objective + (s.fighters.Length > 1 ? "   (" + left + " left)" : "");
                if (s.protect) text += "\n" + s.protect.displayName + "   " + Mathf.CeilToInt(s.protect.Health01 * 100f) + "%";
                if (text != last) { last = text; ui.SetObjective(text); }
                yield return null;
            }
        }

        // ------------------------------------------------------------ end states

        IEnumerator Collapse()
        {
            state = State.Collapsing;
            timerOn = false;
            Time.timeScale = 1f;
            ClearShadows();
            ui.ShowHold(false, 0f, "");
            ui.SetMarker(null);
            onFailed?.Invoke();

            yield return ui.Banner("THE MEMORY COLLAPSED", string.IsNullOrEmpty(failReason) ? collapsedText : failReason, 2f);

            ResetMemory();
            if (respawnPoint) MemoryCheckpoint.Teleport(player, respawnPoint);
            ui.EndMemory();
            state = State.Idle;
            if (Active == this) Active = null;

            if (autoRetry)
            {
                yield return new WaitForSeconds(0.8f);
                Begin();
            }
        }

        void Complete()
        {
            state = State.Completed;
            ClearShadows();
            bool redeemed = !madeChoice || lastChoiceRight;
            MemoryProgress.Record(memoryId, redeemed);

            ui.EndMemory();
            ui.ShowBanner(redeemed ? "MEMORY REDEEMED" : "MEMORY STABILIZED", redeemed ? redeemedText : condemnedText, 3.5f);

            ApplyCompletionWorld();
            onCompleted?.Invoke();
            if (Active == this) Active = null;
        }

        void ApplyCompletionWorld()
        {
            if (unlockOnComplete != null) foreach (var g in unlockOnComplete) if (g) g.SetActive(true);
            if (disableOnComplete != null) foreach (var g in disableOnComplete) if (g) g.SetActive(false);
        }

        void ResetMemory()
        {
            foreach (var kv in initialActive) if (kv.Key) kv.Key.SetActive(kv.Value);
            foreach (var npc in GetComponentsInChildren<MemoryNPC>(true)) npc.ResetNPC();
            foreach (var r in GetComponentsInChildren<IMemoryResettable>(true)) r.ResetForMemory();
        }

        /// <summary>Editor and debug helper: reset a finished memory so it can be played again.</summary>
        [ContextMenu("Reset This Memory")]
        public void ResetForReplay()
        {
            StopAllCoroutines();
            ClearShadows();
            ResetMemory();
            MemoryProgress.Forget(memoryId);
            state = State.Idle;
            if (Active == this) Active = null;
        }

        // ------------------------------------------------------------ shadows

        void Spawn(int count, Transform[] points)
        {
            var prefab = shadowPrefabOverride ? shadowPrefabOverride : (settings ? settings.shadowPrefab : null);
            if (!prefab) { Debug.LogError("Beyond Time: no shadow prefab. Set MemorySettings > Shadow Prefab.", this); return; }

            var pts = (points != null && points.Length > 0) ? points : defaultSpawnPoints;
            int cap = settings ? settings.maxAliveShadows : 8;
            bool nav = prefab.GetComponent<NavMeshAgent>() != null;

            for (int k = 0; k < count; k++)
            {
                if (AliveCount() >= cap) break;
                Transform p = (pts != null && pts.Length > 0) ? pts[spawnCursor++ % pts.Length] : transform;
                Vector3 pos = p.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                if (nav && NavMesh.SamplePosition(pos, out NavMeshHit hit, 4f, NavMesh.AllAreas)) pos = hit.position;

                Vector3 look = player ? player.position - pos : Vector3.forward; look.y = 0f;
                var go = Instantiate(prefab, pos, look.sqrMagnitude > 0.01f ? Quaternion.LookRotation(look) : Quaternion.identity);
                spawned.Add(go);
            }
        }

        int AliveCount()
        {
            spawned.RemoveAll(g => g == null);
            int n = 0;
            foreach (var g in spawned)
            {
                if (!g.activeInHierarchy) continue;
                var ms = g.GetComponent<MemoryShadow>();
                if (ms && ms.IsDead) continue;
                n++;
            }
            return n;
        }

        void ClearShadows()
        {
            foreach (var g in spawned)
            {
                if (!g) continue;
                var ms = g.GetComponent<MemoryShadow>();
                if (ms) ms.Die(); else Destroy(g);
            }
            spawned.Clear();
        }

        // ------------------------------------------------------------ helpers

        float Dist(Transform t)
        {
            if (!t || !player) return float.MaxValue;
            Vector3 d = t.position - player.position; d.y = 0f;
            return d.magnitude;
        }

        static bool EPressed() { var k = Keyboard.current; return k != null && k.eKey.wasPressedThisFrame; }
        static bool EHeld()    { var k = Keyboard.current; return k != null && k.eKey.isPressed; }

        void Warn(MemoryStep s, string what) { Debug.LogWarning("MemoryZone '" + memoryTitle + "' step '" + s.label + "' " + what + ". Skipping it.", this); }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.8f);
            foreach (var s in steps)
                if (s.target) Gizmos.DrawWireSphere(s.target.position, s.radius);
        }
    }
}
