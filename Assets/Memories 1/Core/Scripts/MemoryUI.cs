using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Memories
{
    /// <summary>
    /// The memory HUD: storyline at the bottom, objective top left, timer top centre,
    /// choices in the middle (time freezes), hold-E bar, result banner, objective marker.
    /// Built by Tools > Beyond Time > Memories and stored in Assets/Memories/Resources/MemoryUI.prefab,
    /// so any memory dropped into any scene spawns it automatically.
    /// Restyle freely: change fonts, colours and sprites on the prefab; keep the references filled.
    /// </summary>
    public class MemoryUI : MonoBehaviour
    {
        public static MemoryUI Instance { get; private set; }

        [Header("Story (bottom)")]
        public GameObject storyPanel;
        public TMP_Text storyText;
        public GameObject continueHint;

        [Header("Objective (top left)")]
        public GameObject objectivePanel;
        public TMP_Text titleText;
        public TMP_Text objectiveText;

        [Header("Timer (top centre)")]
        public TMP_Text timerText;
        public TMP_Text penaltyText;
        public Color timerNormal = new Color(0.92f, 0.89f, 0.82f);
        public Color timerPressure = new Color(1f, 0.55f, 0.15f);
        public Color timerFinal = new Color(1f, 0.15f, 0.1f);

        [Header("Ledger (top right)")]
        public TMP_Text ledgerText;

        [Header("Hold E")]
        public GameObject holdPanel;
        public TMP_Text holdText;
        public Image holdFill;

        [Header("Choice (centre)")]
        public GameObject choicePanel;
        public TMP_Text choicePrompt;
        public Button[] choiceButtons;
        public TMP_Text[] choiceLabels;
        public Image choiceTimerFill;

        [Header("Banner")]
        public CanvasGroup banner;
        public TMP_Text bannerTitle;
        public TMP_Text bannerBody;

        [Header("Objective marker")]
        public RectTransform marker;
        public TMP_Text markerDistance;
        public float markerHeight = 2.2f;

        [Header("Hit flash")]
        public CanvasGroup hitFlash;

        public bool IsTyping { get; private set; }

        Coroutine typing, bannerRoutine, penaltyRoutine, flashRoutine;
        Transform markerTarget, player;
        Camera cam;
        int picked = -1;

        // ------------------------------------------------------------ setup

        public static MemoryUI Ensure()
        {
            if (Instance) return Instance;
            var found = FindFirstObjectByType<MemoryUI>();
            if (found) { Instance = found; return found; }
            var prefab = Resources.Load<GameObject>("MemoryUI");
            if (prefab) return Instantiate(prefab).GetComponent<MemoryUI>();
            Debug.LogError("Beyond Time: MemoryUI prefab missing. Run Tools > Beyond Time > Memories > 1. Build Memory Kit.");
            return null;
        }

        void Awake()
        {
            Instance = this;
            if (EventSystem.current == null && FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            HideAll();
        }

        void OnEnable()
        {
            MemoryEvents.PlayerHit += OnPlayerHit;
            MemoryProgress.Changed += RefreshLedger;
        }

        void OnDisable()
        {
            MemoryEvents.PlayerHit -= OnPlayerHit;
            MemoryProgress.Changed -= RefreshLedger;
        }

        void Start() { RefreshLedger(); }

        public void HideAll()
        {
            Set(storyPanel, false); Set(objectivePanel, false); Set(holdPanel, false); Set(choicePanel, false);
            if (timerText) timerText.gameObject.SetActive(false);
            if (penaltyText) penaltyText.gameObject.SetActive(false);
            if (banner) banner.alpha = 0f;
            if (hitFlash) hitFlash.alpha = 0f;
            if (marker) marker.gameObject.SetActive(false);
            Set(continueHint, false);
        }

        static void Set(GameObject g, bool on) { if (g && g.activeSelf != on) g.SetActive(on); }

        // ------------------------------------------------------------ memory lifecycle

        public void BeginMemory(string title)
        {
            Set(objectivePanel, true);
            if (titleText) titleText.text = title.ToUpperInvariant();
            if (objectiveText) objectiveText.text = "";
            if (timerText) timerText.gameObject.SetActive(true);
        }

        public void EndMemory()
        {
            Set(objectivePanel, false); Set(holdPanel, false); Set(choicePanel, false); Set(continueHint, false);
            if (timerText) timerText.gameObject.SetActive(false);
            SetMarker(null);
            StartCoroutine(HideStoryLater(4f));
        }

        IEnumerator HideStoryLater(float s)
        {
            yield return new WaitForSecondsRealtime(s);
            if (MemoryZone.Active == null) Set(storyPanel, false);
        }

        // ------------------------------------------------------------ story

        public void ShowStory(string text)
        {
            if (string.IsNullOrEmpty(text) || !storyText) return;
            Set(storyPanel, true);
            if (typing != null) StopCoroutine(typing);
            typing = StartCoroutine(Type(text));
        }

        IEnumerator Type(string text)
        {
            IsTyping = true;
            storyText.text = text;
            storyText.maxVisibleCharacters = 0;
            var s = MemorySettings.Load();
            float cps = s ? s.typewriterCharsPerSecond : 45f;
            float shown = 0f;
            while (shown < text.Length)
            {
                shown += cps * Time.unscaledDeltaTime;
                storyText.maxVisibleCharacters = Mathf.FloorToInt(shown);
                yield return null;
            }
            storyText.maxVisibleCharacters = 99999;
            IsTyping = false;
        }

        public void FinishTyping()
        {
            if (typing != null) StopCoroutine(typing);
            if (storyText) storyText.maxVisibleCharacters = 99999;
            IsTyping = false;
        }

        public void SetContinueHint(bool on) { Set(continueHint, on); }

        // ------------------------------------------------------------ objective, timer, ledger

        public void SetObjective(string text)
        {
            if (objectiveText && objectiveText.text != text) objectiveText.text = text;
        }

        public void SetTimer(float seconds)
        {
            if (!timerText) return;
            int s = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            timerText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
            timerText.color = seconds <= 10f ? timerFinal : seconds <= 30f ? timerPressure : timerNormal;
        }

        public void ShowPenalty(float seconds)
        {
            if (!penaltyText || seconds <= 0f) return;
            if (penaltyRoutine != null) StopCoroutine(penaltyRoutine);
            penaltyRoutine = StartCoroutine(Penalty(seconds));
        }

        IEnumerator Penalty(float seconds)
        {
            penaltyText.text = "-" + Mathf.RoundToInt(seconds) + "s";
            penaltyText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            penaltyText.gameObject.SetActive(false);
        }

        void OnPlayerHit(float _)
        {
            if (!hitFlash) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash());
        }

        IEnumerator Flash()
        {
            float t = 0f;
            while (t < 0.35f) { t += Time.unscaledDeltaTime; hitFlash.alpha = Mathf.Lerp(0.55f, 0f, t / 0.35f); yield return null; }
            hitFlash.alpha = 0f;
        }

        public void RefreshLedger()
        {
            if (!ledgerText) return;
            int total = MemoryZone.All.Count, done = 0, redeemed = 0;
            foreach (var z in MemoryZone.All)
            {
                if (z.IsCompleted) done++;
                if (MemoryProgress.IsRedeemed(z.memoryId)) redeemed++;
            }
            ledgerText.text = total == 0 ? "" : "MEMORIES  " + done + " / " + total + "\nREDEEMED  " + redeemed;
        }

        // ------------------------------------------------------------ hold E

        public void ShowHold(bool on, float progress01, string text)
        {
            Set(holdPanel, on);
            if (!on) return;
            if (holdText && holdText.text != text) holdText.text = text;
            if (holdFill) holdFill.fillAmount = Mathf.Clamp01(progress01);
        }

        // ------------------------------------------------------------ choice (time is frozen by the zone)

        public IEnumerator RunChoice(string prompt, string[] labels, float timeout, int defaultIndex, System.Action<int> result)
        {
            Set(choicePanel, true);
            if (choicePrompt) choicePrompt.text = prompt;
            picked = -1;

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                bool on = i < labels.Length;
                choiceButtons[i].gameObject.SetActive(on);
                if (!on) continue;
                if (i < choiceLabels.Length && choiceLabels[i]) choiceLabels[i].text = "[" + (i + 1) + "]   " + labels[i];
                int idx = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => picked = idx);
            }

            if (choiceTimerFill) { choiceTimerFill.gameObject.SetActive(timeout > 0f); choiceTimerFill.fillAmount = 1f; }

            var prevLock = Cursor.lockState; bool prevVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;

            float t = 0f;
            while (picked < 0)
            {
                var k = Keyboard.current;
                if (k != null)
                {
                    if ((k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) && labels.Length > 0) picked = 0;
                    else if ((k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) && labels.Length > 1) picked = 1;
                    else if ((k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) && labels.Length > 2) picked = 2;
                }
                if (timeout > 0f && picked < 0)
                {
                    t += Time.unscaledDeltaTime;
                    if (choiceTimerFill) choiceTimerFill.fillAmount = 1f - t / timeout;
                    if (t >= timeout) picked = Mathf.Clamp(defaultIndex, 0, labels.Length - 1);
                }
                yield return null;
            }

            Cursor.lockState = prevLock; Cursor.visible = prevVisible;
            Set(choicePanel, false);
            result?.Invoke(picked);
        }

        // ------------------------------------------------------------ banner

        public void ShowBanner(string title, string body, float seconds)
        {
            if (!banner) return;
            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(Banner(title, body, seconds));
        }

        public IEnumerator Banner(string title, string body, float seconds)
        {
            if (!banner) yield break;
            if (bannerTitle) bannerTitle.text = title;
            if (bannerBody) bannerBody.text = body;
            float t = 0f;
            while (t < 0.4f) { t += Time.unscaledDeltaTime; banner.alpha = t / 0.4f; yield return null; }
            banner.alpha = 1f;
            yield return new WaitForSecondsRealtime(seconds);
            t = 0f;
            while (t < 0.6f) { t += Time.unscaledDeltaTime; banner.alpha = 1f - t / 0.6f; yield return null; }
            banner.alpha = 0f;
        }

        // ------------------------------------------------------------ marker

        public void SetMarker(Transform target) { markerTarget = target; }

        void LateUpdate()
        {
            if (!marker) return;
            if (!markerTarget) { if (marker.gameObject.activeSelf) marker.gameObject.SetActive(false); return; }
            if (!cam) cam = Camera.main;
            if (!player) { var p = GameObject.FindGameObjectWithTag("Player"); if (p) player = p.transform; }
            if (!cam) return;

            marker.gameObject.SetActive(true);
            Vector3 sp = cam.WorldToScreenPoint(markerTarget.position + Vector3.up * markerHeight);
            if (sp.z < 0f) { sp.x = Screen.width - sp.x; sp.y = 40f; }
            sp.x = Mathf.Clamp(sp.x, 40f, Screen.width - 40f);
            sp.y = Mathf.Clamp(sp.y, 40f, Screen.height - 40f);
            marker.position = new Vector3(sp.x, sp.y, 0f);
            if (markerDistance && player)
                markerDistance.text = Mathf.RoundToInt(Vector3.Distance(player.position, markerTarget.position)) + " m";
        }
    }
}
