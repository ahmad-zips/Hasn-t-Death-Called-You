using System.Collections;
using UnityEngine;
using TMPro;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>
    /// The ending. Fades in on a peaceful paradise, tells the last lines of the story, lets the player walk freely.
    /// Reaching the shrine (or waiting) shows the final screen: ending, score breakdown, best score.
    /// A condemned ending dims the light: same place, less warmth.
    /// </summary>
    public class ParadiseController : MonoBehaviour
    {
        [TextArea(2, 3)] public string[] redeemedLines = {
            "The fire is gone. The air smells of rain and grass.",
            "The ones you went back for are here. They are waiting for you.",
            "Walk to the light when you are ready."
        };
        [TextArea(2, 3)] public string[] condemnedLines = {
            "The fire is gone, but the light is thin here.",
            "Some of the people you left behind are missing.",
            "Walk to the light when you are ready."
        };
        public float secondsPerLine = 5f;
        public GameObject storyPanel;
        public TMP_Text storyText;

        [Header("Ending")]
        public Transform shrine;
        public float shrineRadius = 4f;
        public float autoEndAfter = 150f;
        public GameObject endPanel;
        public TMP_Text endTitle;
        public TMP_Text endBody;
        public TMP_Text scoreText;
        public GameObject creditsPanel;

        [Header("Mood")]
        public Light sun;
        public Color condemnedSunColor = new Color(0.75f, 0.72f, 0.7f);
        public float condemnedSunIntensity = 0.6f;

        Transform player;
        bool ended;
        float start;

        void Start()
        {
            start = Time.time;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            if (endPanel) endPanel.SetActive(false);
            if (creditsPanel) creditsPanel.SetActive(false);
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;

            bool good = GameScore.Ending != "Condemned";
            if (!good && sun) { sun.color = condemnedSunColor; sun.intensity = condemnedSunIntensity; }
            StartCoroutine(Tell(good ? redeemedLines : condemnedLines));
        }

        IEnumerator Tell(string[] lines)
        {
            yield return new WaitForSeconds(1.5f);
            if (storyPanel) storyPanel.SetActive(true);
            foreach (var l in lines) { if (storyText) storyText.text = l; yield return new WaitForSeconds(secondsPerLine); }
            if (storyPanel) storyPanel.SetActive(false);
        }

        void Update()
        {
            if (ended) return;
            bool atShrine = shrine && player && Vector3.Distance(player.position, shrine.position) < shrineRadius;
            if (atShrine || Time.time - start > autoEndAfter) ShowEnd();
        }

        void ShowEnd()
        {
            ended = true;
            StopAllCoroutines();
            if (storyPanel) storyPanel.SetActive(false);
            bool good = GameScore.Ending != "Condemned";
            bool best = GameScore.SubmitFinal();

            if (endTitle) endTitle.text = good ? "PEACE" : "REST";
            if (endBody) endBody.text = good
                ? "Every wrong you faced, you chose to make right."
                : "You reached the end, but not every wrong was made right.";
            if (scoreText) scoreText.text =
                "Memories redeemed   " + GameScore.MemoriesRedeemed + "      condemned   " + GameScore.MemoriesCondemned +
                "\nEnemies defeated   " + GameScore.EnemiesDefeated + "      damage taken   " + GameScore.DamageTaken +
                "\nSouls left   " + GameScore.Souls +
                "\n\nFINAL SCORE   " + GameScore.Score.ToString("N0") +
                (best ? "\nNEW BEST SCORE" : "\nBEST   " + GameScore.HighScore.ToString("N0"));
            if (endPanel) endPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        public void MainMenu() { SceneFlow.Load(SceneFlow.MainMenu); }
        public void ToggleCredits() { if (creditsPanel) creditsPanel.SetActive(!creditsPanel.activeSelf); }
        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
