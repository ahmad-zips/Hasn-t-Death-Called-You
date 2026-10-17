using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>Health (soul) bar, soul flames, score, toasts and the per-memory result card.</summary>
    public class GameHUD : MonoBehaviour
    {
        public Image healthFill;
        public Image healthLag;               // white bar that drains behind the red one
        public TMP_Text healthText;
        public GameObject[] soulIcons;
        public TMP_Text scoreText;
        public CanvasGroup toast;
        public TMP_Text toastText;
        public CanvasGroup resultCard;
        public TMP_Text resultTitle;
        public TMP_Text resultBody;

        Fighter player;
        float shownScore, lag = 1f;
        Coroutine toastRoutine, cardRoutine;

        void Start()
        {
            if (toast) toast.alpha = 0f;
            if (resultCard) resultCard.alpha = 0f;
        }

        void Update()
        {
            if (!player) foreach (var f in Fighter.All) if (f.team == FighterTeam.Player) { player = f; break; }

            float h = player ? player.Health01 : 1f;
            if (healthFill) healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, h, Time.unscaledDeltaTime * 3f);
            lag = h > lag ? h : Mathf.MoveTowards(lag, h, Time.unscaledDeltaTime * 0.6f);
            if (healthLag) healthLag.fillAmount = lag;
            if (healthText && player) healthText.text = Mathf.CeilToInt(player.Health) + " / " + Mathf.RoundToInt(player.maxHealth);

            if (soulIcons != null)
                for (int i = 0; i < soulIcons.Length; i++) if (soulIcons[i]) soulIcons[i].SetActive(i < GameScore.Souls);

            shownScore = Mathf.MoveTowards(shownScore, GameScore.Score, Mathf.Max(50f, Mathf.Abs(GameScore.Score - shownScore) * 4f) * Time.unscaledDeltaTime);
            if (scoreText) scoreText.text = "SCORE  " + Mathf.RoundToInt(shownScore).ToString("N0");
        }

        public void Toast(string text, float seconds = 2.5f)
        {
            if (!toast) return;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(Show(toast, toastText, text, seconds, 0f));
        }

        public void Result(string title, string body, float delay = 2.5f, float seconds = 5f)
        {
            if (!resultCard) return;
            if (resultTitle) resultTitle.text = title;
            if (cardRoutine != null) StopCoroutine(cardRoutine);
            cardRoutine = StartCoroutine(Show(resultCard, resultBody, body, seconds, delay));
        }

        static IEnumerator Show(CanvasGroup g, TMP_Text label, string text, float seconds, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            if (label) label.text = text;
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime) { g.alpha = t / 0.3f; yield return null; }
            g.alpha = 1f;
            yield return new WaitForSecondsRealtime(seconds);
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime) { g.alpha = 1f - t / 0.5f; yield return null; }
            g.alpha = 0f;
        }
    }
}
