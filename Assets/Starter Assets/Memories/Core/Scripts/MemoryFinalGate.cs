using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Opens when every memory in the scene is finished. Walking in gives the ending
    /// (based on how many memories were redeemed) and loads the next scene if it is in the build.
    /// The ending is saved in PlayerPrefs "BeyondTime_Ending" = Redeemed or Condemned.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MemoryFinalGate : MonoBehaviour
    {
        public GameObject gateVisual;                   // hidden until all memories are done
        public string sceneToLoad = "BT_Paradise";

        /// <summary>good ending?, memories redeemed, memories total</summary>
        public static event System.Action<bool, int, int> Finished;
        [Tooltip("How many memories must be redeemed for the good ending.")]
        public int redeemedNeeded = 3;
        [TextArea] public string redeemedEnding = "You faced every wrong and chose to make it right. The fire lets you pass.";
        [TextArea] public string condemnedEnding = "You faced every wrong, but too often you chose it again. The fire remembers.";

        bool open, used;

        void Start()
        {
            GetComponent<Collider>().isTrigger = true;
            MemoryProgress.Changed += Refresh;
            Refresh();
        }

        void OnDestroy() { MemoryProgress.Changed -= Refresh; }

        void Refresh()
        {
            open = MemoryZone.All.Count > 0 && MemoryZone.All.TrueForAll(z => z.IsCompleted);
            if (gateVisual) gateVisual.SetActive(open);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!open || used || !other.CompareTag("Player")) return;
            used = true;
            StartCoroutine(Finish());
        }

        IEnumerator Finish()
        {
            int redeemed = 0;
            foreach (var z in MemoryZone.All) if (MemoryProgress.IsRedeemed(z.memoryId)) redeemed++;
            bool good = redeemed >= Mathf.Min(redeemedNeeded, MemoryZone.All.Count);

            PlayerPrefs.SetString("BeyondTime_Ending", good ? "Redeemed" : "Condemned");
            PlayerPrefs.Save();
            Finished?.Invoke(good, redeemed, MemoryZone.All.Count);

            var ui = MemoryUI.Ensure();
            if (ui) ui.ShowBanner(good ? "REDEEMED" : "CONDEMNED",
                                  (good ? redeemedEnding : condemnedEnding) + "\n\n" + redeemed + " of " + MemoryZone.All.Count + " memories redeemed", 5f);

            yield return new WaitForSecondsRealtime(5f);
            if (!string.IsNullOrEmpty(sceneToLoad) && Application.CanStreamedLevelBeLoaded(sceneToLoad))
                BeyondTime.Game.SceneFlow.Load(sceneToLoad);
        }
    }
}
