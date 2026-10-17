using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// The easy way out. Walking through it during the memory ends it immediately as CONDEMNED:
    /// you abandoned them again. Switch it on from a step's Activate list.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MemoryAbandonExit : MonoBehaviour
    {
        [TextArea(2, 4)] public string abandonText = "You ran for the open door. Behind you, the calling stopped. Just like last time.";

        MemoryZone zone;

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            zone = GetComponentInParent<MemoryZone>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var z = zone ? zone : MemoryZone.Active;
            if (z && z.IsRunning) z.EndCondemned(abandonText);
        }
    }
}
