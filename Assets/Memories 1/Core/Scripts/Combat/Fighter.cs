using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BeyondTime.Memories
{
    public enum FighterTeam { Player, Hostile, Civilian }
    public enum HitResult { Ignored, Dodged, Blocked, Parried, Hit, Staggered, KnockedDown, Defeated }
    public enum PoseMove { None, JabLeft, CrossRight, HookRight, Uppercut, FrontKick, Roundhouse, KnifeSlash, KnifeStab, Shove }

    [Serializable]
    public class AttackMove
    {
        public string name = "Jab";
        public PoseMove pose = PoseMove.JabLeft;
        [Tooltip("Optional: Animator state with a real clip. Leave empty to use the procedural pose.")]
        public string animState = "";
        public float duration = 0.4f;
        [Range(0f, 1f)] public float hitStart = 0.35f;
        [Range(0f, 1f)] public float hitEnd = 0.55f;
        public float damage = 6f;
        public float reach = 1.1f;          // distance in front of the body
        public float radius = 0.55f;
        public float height = 1.3f;
        public float lunge = 0.5f;          // metres stepped forward during the windup
        public float knockback = 1.5f;
        public float stagger = 0.3f;        // seconds the target is stunned on a clean hit
        public float guardDamage = 15f;
        public bool knockdown;
        [Tooltip("Cannot be blocked, only dodged. The attacker glows red during the windup.")]
        public bool unblockable;

        public AttackMove Clone() => (AttackMove)MemberwiseClone();
    }

    /// <summary>
    /// Melee combat for the player and every NPC: health, guard, poise, combos, kicks, blocking,
    /// parrying (block just before a hit), dodging with invulnerability, stagger, knockdown, knockout.
    /// Needs a CharacterController. Put FighterPose on the same object to see punches and kicks.
    /// Right-click the component header for presets (Player, Thug, Killer, Civilian).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Fighter : MonoBehaviour, IMemoryResettable, IMemoryDamageable
    {
        public static readonly List<Fighter> All = new List<Fighter>();

        public string displayName = "Fighter";
        public FighterTeam team = FighterTeam.Hostile;

        [Header("Health")]
        public float maxHealth = 80f;
        [Tooltip("Off = knocked out at zero (stays down). On = dies at zero.")]
        public bool canDie;
        [Tooltip("Player: hits cost memory time instead of health.")]
        public bool damageCostsTime;
        public float secondsPerDamage = 0.25f;

        [Header("Poise (stagger resistance)")]
        public float maxPoise = 25f;
        public float poiseRegen = 12f;

        [Header("Guard")]
        public float maxGuard = 60f;
        public float guardRegen = 18f;
        public float parryWindow = 0.2f;
        [Range(0f, 1f)] public float blockDamageMultiplier = 0.15f;

        [Header("Dodge")]
        public float dodgeDistance = 3.2f;
        public float dodgeDuration = 0.3f;
        public float dodgeCooldown = 0.7f;

        [Header("Moves")]
        public AttackMove[] combo;
        public AttackMove kick;
        [Tooltip("Pressing kick during an attack chains into this (for example a knockdown roundhouse).")]
        public AttackMove kickFinisher;
        public float softLockRange = 3.5f;

        [Header("Optional real animations (state names)")]
        public string hitState = "";
        public string knockdownState = "";

        [Header("Feedback (optional)")]
        public AudioClip[] hitSounds;
        public AudioClip blockSound;
        public AudioClip whooshSound;
        public Light telegraphLight;        // glows during unblockable windups
        public bool hitStopOnLand;          // tiny freeze when this fighter lands a hit (player feel)

        // ------------------------------------------------------------ state

        public float Health { get; private set; }
        public float Health01 => maxHealth <= 0f ? 0f : Health / maxHealth;
        public float Guard01 => maxGuard <= 0f ? 0f : guard / maxGuard;
        public bool IsBlocking { get; private set; }
        public bool IsDodging => Time.time < dodgeUntil;
        public bool IsAttacking => current != null;
        public bool IsStaggered => Time.time < staggerUntil;
        public bool IsDown { get; private set; }
        public bool IsDefeated { get; private set; }
        public bool IsDead { get; private set; }
        public bool CanAct => !IsAttacking && !IsDodging && !IsStaggered && !IsDown && !IsDefeated;
        public AttackMove CurrentMove => current;
        public float AttackProgress => attackT;
        public Fighter LastAttacker { get; private set; }
        public float LastHitTime { get; private set; } = -99f;
        public float StaggerStarted { get; private set; } = -99f;

        /// <summary>Set by controllers. FighterPose shows a guard stance or cowering pose.</summary>
        [NonSerialized] public bool inStance;
        [NonSerialized] public bool cowering;
        /// <summary>Set by AI: attacks lock onto this fighter instead of the nearest one.</summary>
        [NonSerialized] public Fighter focus;

        public event Action<Fighter, float> Damaged;    // attacker, amount
        public event Action<Fighter> Defeated;
        public event Action<HitResult> LandedHit;

        CharacterController cc;
        NavMeshAgent agent;
        Animator anim;
        Transform visual;
        AudioSource audioSrc;

        AttackMove current, queuedMove;
        float attackT, dodgeUntil, nextDodge, staggerUntil, blockStart, guard, poise;
        int comboIndex;
        float comboResetAt;
        Vector3 dodgeDir, knockVel;
        readonly HashSet<Fighter> hitThisMove = new HashSet<Fighter>();
        readonly HashSet<IMemoryDamageable> hitOthers = new HashSet<IMemoryDamageable>();

        Vector3 startPos;
        Quaternion startRot;
        Quaternion visualRestRot;
        Vector3 visualRestPos;
        Coroutine downRoutine;

        // ------------------------------------------------------------ Unity

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponentInChildren<Animator>();
            audioSrc = GetComponent<AudioSource>();
            visual = anim ? anim.transform : null;
            if (visual == transform) visual = null;   // the player: never tip over the camera rig
            if (visual) { visualRestRot = visual.localRotation; visualRestPos = visual.localPosition; }
            startPos = transform.position;
            startRot = transform.rotation;
            Health = maxHealth; guard = maxGuard; poise = maxPoise;
            if (telegraphLight) telegraphLight.enabled = false;
        }

        void OnEnable()  { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (!IsBlocking) guard = Mathf.Min(maxGuard, guard + guardRegen * dt);
            poise = Mathf.Min(maxPoise, poise + poiseRegen * dt);

            if (knockVel.sqrMagnitude > 0.01f)
            {
                Displace(knockVel * dt);
                knockVel = Vector3.Lerp(knockVel, Vector3.zero, 8f * dt);
            }

            if (IsDodging) Displace(dodgeDir * (dodgeDistance / dodgeDuration) * dt);

            if (current != null) TickAttack(dt);
            if (Time.time > comboResetAt) comboIndex = 0;
        }

        // ------------------------------------------------------------ actions

        /// <summary>Next punch of the combo. Returns true if it started or was queued.</summary>
        public bool Attack()
        {
            if (combo == null || combo.Length == 0) return false;
            if (current != null)
            {
                if (queuedMove == null && attackT > 0.35f && comboIndex < combo.Length)
                {
                    queuedMove = combo[comboIndex % combo.Length];
                    return true;
                }
                return false;
            }
            if (!CanAct || IsBlocking) return false;
            Begin(combo[comboIndex % combo.Length]);
            return true;
        }

        public bool Kick()
        {
            if (current != null)
            {
                if (queuedMove == null && attackT > 0.35f && kickFinisher != null && kickFinisher.duration > 0f && kickFinisher.pose != PoseMove.None)
                {
                    queuedMove = kickFinisher;
                    return true;
                }
                return false;
            }
            if (kick == null || kick.duration <= 0f || !CanAct || IsBlocking) return false;
            Begin(kick);
            return true;
        }

        public bool Perform(AttackMove move)
        {
            if (move == null || !CanAct || IsBlocking) return false;
            Begin(move);
            return true;
        }

        public void SetBlock(bool on)
        {
            bool allowed = on && !IsAttacking && !IsDodging && !IsStaggered && !IsDown && !IsDefeated;
            if (allowed && !IsBlocking) blockStart = Time.time;
            IsBlocking = allowed;
        }

        public bool Dodge(Vector3 worldDir)
        {
            if (Time.time < nextDodge || IsDown || IsDefeated || IsStaggered) return false;
            worldDir.y = 0f;
            dodgeDir = worldDir.sqrMagnitude > 0.01f ? worldDir.normalized : -transform.forward;
            CancelAttack();
            IsBlocking = false;
            dodgeUntil = Time.time + dodgeDuration;
            nextDodge = Time.time + dodgeCooldown;
            Play(whooshSound);
            return true;
        }

        public void Stagger(float seconds)
        {
            if (IsDefeated || seconds <= 0f) return;
            CancelAttack();
            IsBlocking = false;
            staggerUntil = Mathf.Max(staggerUntil, Time.time + seconds);
            StaggerStarted = Time.time;
            PlayState(hitState);
        }

        public void KnockDown(float seconds)
        {
            if (IsDefeated) return;
            CancelAttack();
            IsBlocking = false;
            if (!visual) { Stagger(Mathf.Min(seconds, 1f)); return; }   // player: long stagger instead of falling
            if (downRoutine != null) StopCoroutine(downRoutine);
            downRoutine = StartCoroutine(DownAndUp(seconds));
        }

        // ------------------------------------------------------------ attacks

        void Begin(AttackMove m)
        {
            current = m;
            attackT = 0f;
            queuedMove = null;
            hitThisMove.Clear();
            hitOthers.Clear();
            if (m == (combo != null && combo.Length > 0 ? combo[comboIndex % combo.Length] : null)) comboIndex++;
            comboResetAt = Time.time + m.duration + 0.6f;
            FaceNearestTarget();
            PlayState(m.animState);
            if (telegraphLight) telegraphLight.enabled = m.unblockable;
            Play(whooshSound);
        }

        void TickAttack(float dt)
        {
            var m = current;
            float prev = attackT;
            attackT += dt / Mathf.Max(0.05f, m.duration);

            if (attackT < m.hitStart && m.lunge > 0f)
                Displace(transform.forward * (m.lunge / Mathf.Max(0.01f, m.hitStart * m.duration)) * dt);

            if (attackT >= m.hitStart && prev < m.hitEnd)
            {
                if (telegraphLight) telegraphLight.enabled = false;
                DetectHits(m);
            }

            if (attackT >= 1f)
            {
                current = null;
                if (telegraphLight) telegraphLight.enabled = false;
                if (queuedMove != null && !IsStaggered && !IsDown)
                {
                    var next = queuedMove;
                    queuedMove = null;
                    Begin(next);
                }
            }
        }

        void CancelAttack()
        {
            current = null;
            queuedMove = null;
            if (telegraphLight) telegraphLight.enabled = false;
        }

        void DetectHits(AttackMove m)
        {
            Vector3 centre = transform.position + Vector3.up * m.height + transform.forward * m.reach;
            foreach (var col in Physics.OverlapSphere(centre, m.radius, ~0, QueryTriggerInteraction.Ignore))
            {
                var f = col.GetComponentInParent<Fighter>();
                if (f != null)
                {
                    if (f == this || !CanHurt(f) || !hitThisMove.Add(f)) continue;
                    Vector3 dir = f.transform.position - transform.position; dir.y = 0f;
                    var result = f.TakeHit(this, m, dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward);
                    LandedHit?.Invoke(result);
                    if (hitStopOnLand && (result == HitResult.Hit || result == HitResult.Staggered || result == HitResult.KnockedDown || result == HitResult.Defeated))
                        HitStop.Do(result == HitResult.KnockedDown || result == HitResult.Defeated ? 0.09f : 0.05f);
                    continue;
                }

                if (team == FighterTeam.Player)   // punches also work on placeholder shadows
                {
                    var d = col.GetComponentInParent<IMemoryDamageable>();
                    if (d != null && hitOthers.Add(d))
                    {
                        d.TakeHit(m.damage >= 12f ? 2 : 1, m.stagger + 0.2f);
                        if (hitStopOnLand) HitStop.Do(0.04f);
                    }
                }
            }
        }

        bool CanHurt(Fighter other)
        {
            if (other.IsDefeated) return false;
            switch (team)
            {
                case FighterTeam.Player: return other.team == FighterTeam.Hostile;
                case FighterTeam.Hostile: return other.team != FighterTeam.Hostile;
                default: return false;
            }
        }

        void FaceNearestTarget()
        {
            if (focus && !focus.IsDefeated)
            {
                Vector3 fd = focus.transform.position - transform.position; fd.y = 0f;
                if (fd.magnitude <= softLockRange && fd.sqrMagnitude > 0.0001f) { transform.rotation = Quaternion.LookRotation(fd); return; }
            }
            Fighter best = null;
            float bestScore = float.MaxValue;
            foreach (var f in All)
            {
                if (f == this || !CanHurt(f)) continue;
                Vector3 d = f.transform.position - transform.position; d.y = 0f;
                float dist = d.magnitude;
                if (dist > softLockRange || dist < 0.01f) continue;
                float angle = Vector3.Angle(transform.forward, d);
                if (angle > 110f) continue;
                float score = dist + angle * 0.02f;
                if (score < bestScore) { bestScore = score; best = f; }
            }
            if (best)
            {
                Vector3 d = best.transform.position - transform.position; d.y = 0f;
                transform.rotation = Quaternion.LookRotation(d);
            }
        }

        // ------------------------------------------------------------ receiving hits

        public HitResult TakeHit(Fighter attacker, AttackMove m, Vector3 hitDir)
        {
            if (IsDefeated) return HitResult.Ignored;
            if (IsDodging) return HitResult.Dodged;

            bool facing = Vector3.Dot(transform.forward, -hitDir) > 0.25f;
            if (IsBlocking && facing && !m.unblockable)
            {
                if (attacker && Time.time - blockStart <= parryWindow)
                {
                    attacker.Stagger(1.1f);
                    Play(blockSound);
                    HitStop.Do(0.08f);
                    return HitResult.Parried;
                }
                guard -= m.guardDamage;
                ApplyDamage(m.damage * blockDamageMultiplier, attacker);
                knockVel = hitDir * m.knockback * 0.4f;
                Play(blockSound);
                if (guard <= 0f) { guard = 0f; IsBlocking = false; Stagger(1f); return HitResult.Staggered; }
                return HitResult.Blocked;
            }

            ApplyDamage(m.damage, attacker);
            if (IsDefeated) { knockVel = hitDir * m.knockback; return HitResult.Defeated; }

            knockVel = hitDir * m.knockback;
            PlayRandom(hitSounds);
            poise -= m.damage;
            if (m.knockdown || poise <= 0f)
            {
                poise = maxPoise;
                KnockDown(1.6f);
                return HitResult.KnockedDown;
            }
            Stagger(m.stagger);
            return HitResult.Hit;
        }

        /// <summary>Simple hit (repel, hazards). Damage counts as poise and health.</summary>
        public void TakeHit(int damage, float stunSeconds)
        {
            if (IsDefeated) return;
            ApplyDamage(damage, null);
            if (!IsDefeated) Stagger(stunSeconds);
        }

        void ApplyDamage(float amount, Fighter attacker)
        {
            if (amount <= 0f) return;
            LastAttacker = attacker;
            LastHitTime = Time.time;
            Damaged?.Invoke(attacker, amount);

            if (damageCostsTime) { MemoryEvents.RaisePlayerHit(amount * secondsPerDamage); return; }

            Health = Mathf.Max(0f, Health - amount);
            if (Health <= 0f) Defeat();
        }

        void Defeat()
        {
            if (IsDefeated) return;
            IsDefeated = true;
            IsDead = canDie;
            CancelAttack();
            IsBlocking = false;
            cowering = false;
            inStance = false;
            if (downRoutine != null) StopCoroutine(downRoutine);
            if (visual) downRoutine = StartCoroutine(FallOver());
            if (agent && agent.enabled) agent.enabled = false;
            Defeated?.Invoke(this);
        }

        // ------------------------------------------------------------ knockdown visuals

        IEnumerator DownAndUp(float seconds)
        {
            IsDown = true;
            PlayState(knockdownState);
            yield return Tip(0f, 1f, 0.35f);
            yield return new WaitForSeconds(seconds);
            if (IsDefeated) yield break;
            yield return Tip(1f, 0f, 0.45f);
            IsDown = false;
            poise = maxPoise;
        }

        IEnumerator FallOver()
        {
            IsDown = true;
            PlayState(knockdownState);
            yield return Tip(0f, 1f, 0.4f);
            if (cc) cc.enabled = false;
        }

        IEnumerator Tip(float from, float to, float time)
        {
            float t = 0f;
            Quaternion lying = visualRestRot * Quaternion.Euler(-85f, 0f, 0f);
            Vector3 lyingPos = visualRestPos + new Vector3(0f, 0.12f, 0f);
            while (t < time)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(from, to, t / time);
                visual.localRotation = Quaternion.Slerp(visualRestRot, lying, k);
                visual.localPosition = Vector3.Lerp(visualRestPos, lyingPos, k);
                yield return null;
            }
            visual.localRotation = Quaternion.Slerp(visualRestRot, lying, to);
            visual.localPosition = Vector3.Lerp(visualRestPos, lyingPos, to);
        }

        // ------------------------------------------------------------ movement helper

        public void Displace(Vector3 delta)
        {
            delta.y = 0f;
            if (cc && cc.enabled) cc.Move(delta);
            else transform.position += delta;
        }

        /// <summary>Puts an NPC back where it started (used when knocked into lava).</summary>
        public void ReturnToStart()
        {
            if (cc) cc.enabled = false;
            transform.SetPositionAndRotation(startPos, startRot);
            if (cc && !IsDefeated) cc.enabled = true;
            knockVel = Vector3.zero;
        }

        // ------------------------------------------------------------ reset (memory retry)

        public void ResetForMemory()
        {
            StopAllCoroutines();
            downRoutine = null;
            CancelAttack();
            IsBlocking = false; IsDown = false; IsDefeated = false; IsDead = false;
            cowering = false; inStance = false;
            staggerUntil = 0f; dodgeUntil = 0f; knockVel = Vector3.zero;
            Health = maxHealth; guard = maxGuard; poise = maxPoise;
            LastAttacker = null; LastHitTime = -99f;
            if (visual) { visual.localRotation = visualRestRot; visual.localPosition = visualRestPos; }
            if (cc) cc.enabled = false;
            transform.SetPositionAndRotation(startPos, startRot);
            if (cc) cc.enabled = true;
            if (agent) { agent.enabled = true; if (agent.isOnNavMesh) agent.Warp(startPos); }
        }

        // ------------------------------------------------------------ audio and animation

        void PlayState(string state)
        {
            if (string.IsNullOrEmpty(state) || !anim) return;
            int h = Animator.StringToHash(state);
            if (anim.HasState(0, h)) anim.CrossFade(h, 0.08f);
        }

        void Play(AudioClip c) { if (audioSrc && c) audioSrc.PlayOneShot(c); }
        void PlayRandom(AudioClip[] list) { if (list != null && list.Length > 0) Play(list[UnityEngine.Random.Range(0, list.Length)]); }

        void OnDrawGizmosSelected()
        {
            var m = current ?? (combo != null && combo.Length > 0 ? combo[0] : null);
            if (m == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * m.height + transform.forward * m.reach, m.radius);
        }

        // ------------------------------------------------------------ presets

        [ContextMenu("Preset: Player")]   void PresetPlayer()   { FighterPresets.Player(this); }
        [ContextMenu("Preset: Thug")]     void PresetThug()     { FighterPresets.Thug(this); }
        [ContextMenu("Preset: Killer")]   void PresetKiller()   { FighterPresets.Killer(this); }
        [ContextMenu("Preset: Civilian")] void PresetCivilian() { FighterPresets.Civilian(this); }
    }

    /// <summary>Tiny global freeze on impact. Never touches a frozen (choice) timescale.</summary>
    public static class HitStop
    {
        static HitStopRunner runner;

        public static void Do(float seconds)
        {
            if (Time.timeScale < 0.99f) return;
            if (!runner) runner = new GameObject("HitStop").AddComponent<HitStopRunner>();
            runner.Run(seconds);
        }

        public class HitStopRunner : MonoBehaviour
        {
            public void Run(float s) { StopAllCoroutines(); StartCoroutine(Freeze(s)); }
            IEnumerator Freeze(float s)
            {
                Time.timeScale = 0.05f;
                yield return new WaitForSecondsRealtime(s);
                if (Time.timeScale > 0.01f) Time.timeScale = 1f;
            }
        }
    }

    /// <summary>Ready-made fighting styles. Tweak numbers here to rebalance everyone of that type.</summary>
    public static class FighterPresets
    {
        static AttackMove M(string n, PoseMove p, float dur, float h0, float h1, float dmg, float reach, float lunge,
                            float knock, float stagger, float guardDmg, bool down = false, bool unblockable = false)
            => new AttackMove { name = n, pose = p, duration = dur, hitStart = h0, hitEnd = h1, damage = dmg, reach = reach,
                                lunge = lunge, knockback = knock, stagger = stagger, guardDamage = guardDmg, knockdown = down, unblockable = unblockable };

        /// <summary>An empty slot. Unity turns null moves into default moves when saving, so use this instead.</summary>
        public static AttackMove None() => new AttackMove { name = "None", pose = PoseMove.None, duration = 0f };

        public static void Player(Fighter f)
        {
            f.displayName = "You"; f.team = FighterTeam.Player;
            f.maxHealth = 100f; f.canDie = false; f.damageCostsTime = true; f.secondsPerDamage = 0.25f;
            f.maxPoise = 30f; f.maxGuard = 70f; f.parryWindow = 0.22f;
            f.dodgeDistance = 3.4f; f.dodgeDuration = 0.28f; f.dodgeCooldown = 0.6f;
            f.combo = new[] {
                M("Jab",   PoseMove.JabLeft,    0.32f, 0.35f, 0.55f,  6f, 1.0f, 0.35f, 1.0f, 0.30f, 12f),
                M("Cross", PoseMove.CrossRight, 0.38f, 0.38f, 0.58f,  8f, 1.1f, 0.45f, 1.5f, 0.35f, 15f),
                M("Hook",  PoseMove.HookRight,  0.50f, 0.42f, 0.60f, 13f, 1.0f, 0.40f, 3.0f, 0.55f, 22f) };
            f.kick = M("Front kick", PoseMove.FrontKick, 0.55f, 0.45f, 0.62f, 10f, 1.3f, 0.3f, 4.5f, 0.5f, 45f);
            f.kickFinisher = M("Roundhouse", PoseMove.Roundhouse, 0.62f, 0.45f, 0.62f, 16f, 1.3f, 0.4f, 5f, 0.6f, 35f, down: true);
            f.softLockRange = 3.8f; f.hitStopOnLand = true;
        }

        public static void Thug(Fighter f)
        {
            f.displayName = "Lookout"; f.team = FighterTeam.Hostile;
            f.maxHealth = 70f; f.canDie = false; f.damageCostsTime = false;
            f.maxPoise = 22f; f.maxGuard = 45f; f.parryWindow = 0f;
            f.dodgeDistance = 2.6f; f.dodgeCooldown = 1.5f;
            f.combo = new[] {
                M("Jab",   PoseMove.JabLeft,    0.45f, 0.45f, 0.62f,  6f, 1.0f, 0.3f, 1.2f, 0.3f, 12f),
                M("Cross", PoseMove.CrossRight, 0.50f, 0.45f, 0.62f,  9f, 1.1f, 0.4f, 1.8f, 0.4f, 16f),
                M("Hook",  PoseMove.HookRight,  0.65f, 0.50f, 0.66f, 12f, 1.0f, 0.4f, 2.5f, 0.5f, 22f) };
            f.kick = M("Shove", PoseMove.Shove, 0.6f, 0.5f, 0.66f, 5f, 1.0f, 0.2f, 4f, 0.4f, 40f);
            f.kickFinisher = None(); f.softLockRange = 2.5f; f.hitStopOnLand = false;
        }

        public static void Killer(Fighter f)
        {
            f.displayName = "The Attacker"; f.team = FighterTeam.Hostile;
            f.maxHealth = 130f; f.canDie = false; f.damageCostsTime = false;
            f.maxPoise = 40f; f.maxGuard = 70f; f.parryWindow = 0f;
            f.dodgeDistance = 3f; f.dodgeCooldown = 1.2f;
            f.combo = new[] {
                M("Slash",      PoseMove.KnifeSlash, 0.55f, 0.45f, 0.62f, 12f, 1.2f, 0.5f, 1.5f, 0.4f, 20f),
                M("Backslash",  PoseMove.KnifeSlash, 0.50f, 0.45f, 0.62f, 12f, 1.2f, 0.4f, 1.5f, 0.4f, 20f),
                M("Stab",       PoseMove.KnifeStab,  0.85f, 0.62f, 0.75f, 22f, 1.4f, 0.9f, 3.0f, 0.8f, 30f, unblockable: true) };
            f.kick = M("Kick", PoseMove.FrontKick, 0.6f, 0.48f, 0.64f, 9f, 1.3f, 0.3f, 4.5f, 0.5f, 40f);
            f.kickFinisher = None(); f.softLockRange = 3f; f.hitStopOnLand = false;
        }

        public static void Civilian(Fighter f)
        {
            f.displayName = "The Man"; f.team = FighterTeam.Civilian;
            f.maxHealth = 100f; f.canDie = true; f.damageCostsTime = false;
            f.maxPoise = 10f; f.maxGuard = 0f;
            f.combo = new AttackMove[0]; f.kick = None(); f.kickFinisher = None(); f.hitStopOnLand = false;
        }
    }
}
