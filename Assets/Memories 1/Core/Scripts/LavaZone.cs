using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Put on the lava (a trigger collider under the bridges).
    /// Player: sent back to the last checkpoint (or the running memory's respawn point) and loses time.
    /// Placeholder shadows that fall in are destroyed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LavaZone : MonoBehaviour
    {
        public float timePenalty = 5f;
        public Transform fallbackRespawn;
        public AudioClip sizzleSound;

        void Awake() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Transform to = MemoryCheckpoint.Current ? MemoryCheckpoint.Current : fallbackRespawn;
                MemoryCheckpoint.Teleport(other.transform, to);
                if (sizzleSound) AudioSource.PlayClipAtPoint(sizzleSound, other.transform.position);
                MemoryEvents.RaisePlayerHit(timePenalty);
                return;
            }

            var fighter = other.GetComponentInParent<Fighter>();
            if (fighter) { fighter.ReturnToStart(); return; }

            var shadow = other.GetComponentInParent<MemoryShadow>();
            if (shadow) shadow.Die();
        }
    }
}
