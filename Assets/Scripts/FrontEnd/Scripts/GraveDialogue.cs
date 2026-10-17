using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace BeyondTime.FrontEnd
{
    /// <summary>
    /// Grave scene. Plays opening lines on wake-up, then the questioning when the player
    /// walks into this trigger, then reveals the memory portal. Press E to skip a line.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GraveDialogue : MonoBehaviour
    {
        [TextArea(2, 4)] public string[] openingLines;
        [TextArea(2, 4)] public string[] lines;
        public float secondsPerLine = 3.5f;
        public float openingDelay = 2.5f;

        public TMP_Text subtitleText;
        public TMP_Text objectiveText;
        public string objectiveBefore = "Approach the figures";
        public string objectiveAfter = "Step into the memory portal";

        public GameObject activateWhenDone;     // the portal, hidden until the questioning ends
        public Light[] figureLights;            // brighten while the figures speak
        public float speakingBoost = 2.5f;

        bool started;
        Coroutine opening;
        float[] baseIntensity;

        void Start()
        {
            GetComponent<Collider>().isTrigger = true;
            if (subtitleText) subtitleText.text = "";
            if (objectiveText) objectiveText.text = objectiveBefore;
            if (activateWhenDone) activateWhenDone.SetActive(false);

            if (figureLights != null)
            {
                baseIntensity = new float[figureLights.Length];
                for (int i = 0; i < figureLights.Length; i++)
                    if (figureLights[i]) baseIntensity[i] = figureLights[i].intensity;
            }

            if (openingLines != null && openingLines.Length > 0)
                opening = StartCoroutine(PlayLines(openingLines, openingDelay));
        }

        void OnTriggerEnter(Collider other)
        {
            if (started || !other.CompareTag("Player")) return;
            started = true;
            if (opening != null) StopCoroutine(opening);
            StartCoroutine(Questioning());
        }

        IEnumerator Questioning()
        {
            SetSpeaking(true);
            yield return PlayLines(lines, 0.3f);
            SetSpeaking(false);

            if (activateWhenDone) activateWhenDone.SetActive(true);
            if (objectiveText) objectiveText.text = objectiveAfter;
        }

        IEnumerator PlayLines(string[] list, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (list == null) yield break;

            foreach (var line in list)
            {
                if (subtitleText) subtitleText.text = line;
                yield return null;                       // so one E press skips only one line
                float t = 0f;
                while (t < secondsPerLine)
                {
                    t += Time.deltaTime;
                    if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) break;
                    yield return null;
                }
            }
            if (subtitleText) subtitleText.text = "";
        }

        void SetSpeaking(bool on)
        {
            if (figureLights == null || baseIntensity == null) return;
            for (int i = 0; i < figureLights.Length; i++)
                if (figureLights[i]) figureLights[i].intensity = baseIntensity[i] * (on ? speakingBoost : 1f);
        }
    }
}
