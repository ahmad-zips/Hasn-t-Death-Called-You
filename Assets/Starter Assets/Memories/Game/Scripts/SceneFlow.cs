using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeyondTime.Game
{
    /// <summary>Scene names and fade-to-black loading for the whole game.</summary>
    public static class SceneFlow
    {
        public const string MainMenu = "BT_MainMenu";
        public const string Intro = "BT_GraveIntro";
        public const string World = "BT_HellfireWorld";
        public const string Paradise = "BT_Paradise";

        static Fader fader;

        public static void Load(string scene)
        {
            Time.timeScale = 1f;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError("Beyond Time: scene '" + scene + "' is not in Build Profiles. Run Tools > Beyond Time > Game > Build Whole Game.");
                return;
            }
            if (!fader) fader = Fader.Create();
            fader.LoadWithFade(scene);
        }

        public class Fader : MonoBehaviour
        {
            CanvasGroup group;
            bool busy;

            public static Fader Create()
            {
                var go = new GameObject("SceneFader", typeof(Canvas), typeof(CanvasGroup));
                DontDestroyOnLoad(go);
                var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 999;
                var img = new GameObject("Black", typeof(RectTransform), typeof(Image));
                img.transform.SetParent(go.transform, false);
                var rt = (RectTransform)img.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
                img.GetComponent<Image>().color = Color.black;
                var f = go.AddComponent<Fader>();
                f.group = go.GetComponent<CanvasGroup>();
                f.group.alpha = 0f; f.group.blocksRaycasts = false;
                return f;
            }

            public void LoadWithFade(string scene) { if (!busy) StartCoroutine(Run(scene)); }

            IEnumerator Run(string scene)
            {
                busy = true;
                group.blocksRaycasts = true;
                for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime) { group.alpha = t / 0.5f; yield return null; }
                group.alpha = 1f;
                var op = SceneManager.LoadSceneAsync(scene);
                while (!op.isDone) yield return null;
                Time.timeScale = 1f;
                for (float t = 0; t < 0.6f; t += Time.unscaledDeltaTime) { group.alpha = 1f - t / 0.6f; yield return null; }
                group.alpha = 0f;
                group.blocksRaycasts = false;
                busy = false;
            }
        }
    }
}
