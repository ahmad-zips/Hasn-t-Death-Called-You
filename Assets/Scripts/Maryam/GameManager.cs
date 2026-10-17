using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Beyond Time - core mission manager.
/// Handles the countdown timer (3 phases), time penalties from shadow hits,
/// win / fail / pause, and retry. One of these per trial scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Phase { Normal, Pressure, Final }
    public enum GameState { Playing, Paused, Won, Failed }

    [Header("Timer (seconds)")]
    public float totalTime = 90f;
    public float pressureAt = 30f;   // Phase 2 starts when this much time is left
    public float finalAt = 10f;      // Phase 3 starts when this much time is left

    [Header("HUD")]
    public TMP_Text timerText;
    public TMP_Text penaltyText;     // optional: flashes "-3s" when a shadow hits you
    public Color normalColor = Color.white;
    public Color pressureColor = new Color(1f, 0.6f, 0.1f);
    public Color finalColor = Color.red;

    [Header("Panels (start disabled or enabled, script hides them)")]
    public GameObject winPanel;
    public GameObject failPanel;
    public GameObject pausePanel;

    public float TimeLeft { get; private set; }
    public Phase CurrentPhase { get; private set; } = Phase.Normal;
    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;

    public event Action<Phase> OnPhaseChanged;
    public event Action OnWin;
    public event Action OnFail;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeLeft = totalTime;

        if (winPanel) winPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (penaltyText) penaltyText.gameObject.SetActive(false);

        SetCursor(false);
        UpdateTimerUI();
    }

    void OnEnable()  { ShadowScript.OnPlayerTouched += ApplyPenalty; }
    void OnDisable() { ShadowScript.OnPlayerTouched -= ApplyPenalty; }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();

        if (State != GameState.Playing) return;

        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            UpdateTimerUI();
            Fail();
            return;
        }

        UpdatePhase();
        UpdateTimerUI();
    }

    // ---------- PHASES ----------
    void UpdatePhase()
    {
        Phase p = TimeLeft <= finalAt ? Phase.Final
                : TimeLeft <= pressureAt ? Phase.Pressure
                : Phase.Normal;

        if (p == CurrentPhase) return;
        CurrentPhase = p;
        OnPhaseChanged?.Invoke(p);
    }

    // ---------- PENALTY ----------
    void ApplyPenalty(float seconds)
    {
        if (!IsPlaying) return;
        TimeLeft = Mathf.Max(0f, TimeLeft - seconds);

        if (penaltyText)
        {
            penaltyText.text = "-" + seconds.ToString("0") + "s";
            penaltyText.gameObject.SetActive(true);
            CancelInvoke(nameof(HidePenalty));
            Invoke(nameof(HidePenalty), 1f);
        }
    }

    void HidePenalty() { if (penaltyText) penaltyText.gameObject.SetActive(false); }

    // ---------- WIN / FAIL ----------
    /// <summary>Call this from the objective (hold-E) when the corrective action is done.</summary>
    public void CompleteMission()
    {
        if (!IsPlaying) return;
        State = GameState.Won;

        // shadows stop attacking and disappear
        foreach (var s in FindObjectsByType<ShadowScript>(FindObjectsSortMode.None))
            Destroy(s.gameObject);

        if (winPanel) winPanel.SetActive(true);
        SetCursor(true);
        OnWin?.Invoke();
    }

    void Fail()
    {
        State = GameState.Failed;
        if (failPanel) failPanel.SetActive(true);
        SetCursor(true);
        OnFail?.Invoke();
        Time.timeScale = 0f;
    }

    // ---------- PAUSE ----------
    void TogglePause()
    {
        if (State == GameState.Playing) Pause();
        else if (State == GameState.Paused) Resume();
    }

    void Pause()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
        SetCursor(true);
    }

    // Hook these to UI buttons
    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        SetCursor(false);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------- UI ----------
    void UpdateTimerUI()
    {
        if (!timerText) return;
        int s = Mathf.CeilToInt(TimeLeft);
        timerText.text = (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        timerText.color = CurrentPhase == Phase.Final ? finalColor
                        : CurrentPhase == Phase.Pressure ? pressureColor
                        : normalColor;
    }

    void SetCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }
}
