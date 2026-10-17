using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>Walk through to set where the player returns after falling into lava. Also holds the teleport helper.</summary>
    [RequireComponent(typeof(Collider))]
    public class MemoryCheckpoint : MonoBehaviour
    {
        public Transform spawnPoint;        // empty = this object

        static Transform current;
        public static Transform Current => current;
        public static void SetRespawn(Transform t) { if (t) current = t; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatic() { current = null; }

        void Awake() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) SetRespawn(spawnPoint ? spawnPoint : transform);
        }

        public static void Teleport(Transform player, Transform to)
        {
            if (!player || !to) return;
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.SetPositionAndRotation(to.position, to.rotation);
            if (cc) cc.enabled = true;
        }
    }
}
