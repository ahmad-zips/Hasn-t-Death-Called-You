using System.Collections;
using UnityEngine;

namespace BeyondTime.FrontEnd
{
    /// <summary>Full-screen black image that fades in when a scene opens and fades out before a scene change.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFade : MonoBehaviour
    {
        public bool fadeInOnStart = true;
        public float holdBlackFor = 0f;     // seconds of black before fading in (grave wake-up)
        public float duration = 1f;

        CanvasGroup cg;

        void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;      // never blocks menu clicks
            cg.interactable = false;
            cg.alpha = fadeInOnStart ? 1f : 0f;
        }

        IEnumerator Start()
        {
            if (!fadeInOnStart) yield break;
            if (holdBlackFor > 0f) yield return new WaitForSecondsRealtime(holdBlackFor);
            yield return Fade(1f, 0f);
        }

        public IEnumerator FadeOut() { yield return Fade(cg.alpha, 1f); }

        IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;   // works even if timeScale is 0 (fail panel)
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
        }
    }
}
