using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>Pause (Esc), settings, controls and the game over screen inside the world.</summary>
    public class GameMenus : MonoBehaviour
    {
        public GameObject pausePanel;
        public GameObject settingsPanel;
        public GameObject controlsPanel;
        public GameObject gameOverPanel;
        public TMP_Text pauseScoreText;
        public TMP_Text gameOverScoreText;

        public bool IsPaused { get; private set; }

        float prevTimeScale = 1f;
        CursorLockMode prevLock;
        bool prevVisible;
        Component inputs;
        FieldInfo lookField, cursorLookField;

        void Start()
        {
            HideAll();
            foreach (var f in Fighter.All)
                if (f.team == FighterTeam.Player)
                {
                    inputs = f.GetComponent("StarterAssetsInputs");
                    if (inputs) { lookField = inputs.GetType().GetField("look"); cursorLookField = inputs.GetType().GetField("cursorInputForLook"); }
                    break;
                }
        }

        void HideAll()
        {
            foreach (var p in new[] { pausePanel, settingsPanel, controlsPanel, gameOverPanel }) if (p) p.SetActive(false);
        }

        void Update()
        {
            var k = Keyboard.current;
            if (k == null || !k.escapeKey.wasPressedThisFrame) return;
            if (GameSession.Instance && GameSession.Instance.IsGameOver) return;
            if (settingsPanel && settingsPanel.activeSelf) { Back(); return; }
            if (controlsPanel && controlsPanel.activeSelf) { Back(); return; }
            if (IsPaused) Resume(); else Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            prevTimeScale = Time.timeScale;
            prevLock = Cursor.lockState; prevVisible = Cursor.visible;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (pausePanel) pausePanel.SetActive(true);
            if (pauseScoreText) pauseScoreText.text = "SCORE  " + GameScore.Score.ToString("N0") + "      SOULS  " + GameScore.Souls;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            SetLook(false);
            Click();
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            HideAll();
            Time.timeScale = prevTimeScale;
            AudioListener.pause = false;
            Cursor.lockState = prevLock; Cursor.visible = prevVisible;
            SetLook(true);
            Click();
        }

        public void RestartMemory()
        {
            Resume();
            var z = MemoryZone.Active;
            if (z && z.IsRunning) z.RestartNow();
            else foreach (var f in Fighter.All)
                    if (f.team == FighterTeam.Player && MemoryCheckpoint.Current) { MemoryCheckpoint.Teleport(f.transform, MemoryCheckpoint.Current); break; }
        }

        public void OpenSettings() { if (pausePanel) pausePanel.SetActive(false); if (settingsPanel) settingsPanel.SetActive(true); Click(); }
        public void OpenControls() { if (pausePanel) pausePanel.SetActive(false); if (controlsPanel) controlsPanel.SetActive(true); Click(); }

        public void Back()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            if (controlsPanel) controlsPanel.SetActive(false);
            if (IsPaused && pausePanel) pausePanel.SetActive(true);
            Click();
        }

        public void ShowGameOver()
        {
            HideAll();
            AudioListener.pause = false;
            if (gameOverPanel) gameOverPanel.SetActive(true);
            if (gameOverScoreText)
                gameOverScoreText.text = "SCORE   " + GameScore.Score.ToString("N0") + "\nBEST   " + Mathf.Max(GameScore.HighScore, GameScore.Score).ToString("N0") +
                                         "\n\nTry again from your last redeemed memory with three new souls.";
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            SetLook(false);
        }

        public void TryAgain() { Click(); GameScore.RetryFromCheckpoint(); SceneFlow.Load(SceneFlow.World); }
        public void MainMenu() { Click(); AudioListener.pause = false; Time.timeScale = 1f; SceneFlow.Load(SceneFlow.MainMenu); }
        public void Quit()
        {
            Click();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void SetLook(bool on)
        {
            if (!inputs) return;
            if (cursorLookField != null) cursorLookField.SetValue(inputs, on);
            if (!on && lookField != null) lookField.SetValue(inputs, Vector2.zero);
        }

        static void Click()
        {
            var lib = BeyondTimeAssets.Load();
            if (lib && lib.buttonClick) MusicPlayer.PlaySting(lib.buttonClick);
        }
    }
}
