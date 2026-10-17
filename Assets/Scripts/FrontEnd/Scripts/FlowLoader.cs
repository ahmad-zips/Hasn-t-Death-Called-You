using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeyondTime.FrontEnd
{
    /// <summary>Scene names used by the whole flow. Change a name here and rebuild if a teammate renames a scene.</summary>
    public static class FrontEndScenes
    {
        public const string MainMenu  = "MainMenu";
        public const string Grave     = "Grave";
        public const string Briefing1 = "Briefing1";
        public const string Trial1    = "Trial1";      // Maryam's scene on GamePlay2
        public const string EndScreen = "EndScreen";
    }

    public enum CursorOnStart { Show, Lock, LeaveAlone }

    /// <summary>
    /// Beyond Time front end - one object per scene that every button talks to.
    /// NewGame, Continue, LoadScene("Name"), SaveProgress("Name"), Quit.
    /// </summary>
    public class FlowLoader : MonoBehaviour
    {
        public const string SaveKey = "BeyondTime_NextScene";

        public string firstScene = FrontEndScenes.Grave;
        public CursorOnStart cursorOnStart = CursorOnStart.Show;
        public Button continueButton;       // greyed out when there is no save
        public ScreenFade fade;             // optional: fade to black before loading

        bool loading;

        void Start()
        {
            switch (cursorOnStart)
            {
                case CursorOnStart.Show: Cursor.lockState = CursorLockMode.None; Cursor.visible = true; break;
                case CursorOnStart.Lock: Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; break;
            }

            if (continueButton) continueButton.interactable = PlayerPrefs.HasKey(SaveKey);
        }

        public void NewGame()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            LoadScene(firstScene);
        }

        public void Continue() { LoadScene(PlayerPrefs.GetString(SaveKey, firstScene)); }

        public void SaveProgress(string sceneName)
        {
            PlayerPrefs.SetString(SaveKey, sceneName);
            PlayerPrefs.Save();
        }

        public void LoadScene(string sceneName)
        {
            if (loading) return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("Beyond Time: scene '" + sceneName + "' is not in Build Profiles. " +
                               "Run Tools > Beyond Time > Front End > Add Front-End Scenes To Build.");
                return;
            }

            loading = true;
            Time.timeScale = 1f;

            if (fade && isActiveAndEnabled) StartCoroutine(FadeThenLoad(sceneName));
            else SceneManager.LoadScene(sceneName);
        }

        IEnumerator FadeThenLoad(string sceneName)
        {
            yield return fade.FadeOut();
            SceneManager.LoadScene(sceneName);
        }

        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
