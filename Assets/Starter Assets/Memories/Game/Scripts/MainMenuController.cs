using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Game
{
    /// <summary>Main menu buttons: New Game, Continue, Settings, Controls, Credits, Quit.</summary>
    public class MainMenuController : MonoBehaviour
    {
        public Button continueButton;
        public TMP_Text bestScoreText;
        public GameObject settingsPanel;
        public GameObject controlsPanel;
        public GameObject creditsPanel;

        void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (continueButton) continueButton.interactable = GameScore.HasSave;
            if (bestScoreText) bestScoreText.text = GameScore.HighScore > 0 ? "BEST SCORE  " + GameScore.HighScore.ToString("N0") : "";
            Back();
        }

        public void NewGame()   { Click(); GameScore.NewGame(); SceneFlow.Load(SceneFlow.Intro); }
        public void Continue()  { Click(); GameScore.LoadSaved(); SceneFlow.Load(SceneFlow.World); }
        public void Settings()  { Click(); Back(); if (settingsPanel) settingsPanel.SetActive(true); }
        public void Controls()  { Click(); Back(); if (controlsPanel) controlsPanel.SetActive(true); }
        public void Credits()   { Click(); Back(); if (creditsPanel) creditsPanel.SetActive(true); }

        public void Back()
        {
            foreach (var p in new[] { settingsPanel, controlsPanel, creditsPanel }) if (p) p.SetActive(false);
        }

        public void Quit()
        {
            Click();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        static void Click()
        {
            var lib = BeyondTimeAssets.Load();
            if (lib && lib.buttonClick) MusicPlayer.PlaySting(lib.buttonClick);
        }
    }
}
