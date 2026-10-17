using UnityEngine;
using UnityEngine.InputSystem;

namespace BeyondTime.Memories
{
    /// <summary>
    /// TESTING ONLY. Lets you fight placeholder shadows before the real combat is merged.
    /// LMB strike, Q repel (stun). Remove this component once the real PlayerCombat is on the player.
    /// </summary>
    public class MemoryTestCombat : MonoBehaviour
    {
        public float reach = 2.2f, radius = 1.3f, cooldown = 0.45f, stun = 0.5f;
        public int damage = 1;
        public float repelRadius = 10f, repelStun = 3f, repelCooldown = 6f;

        float nextHit, nextRepel;

        void Update()
        {
            if (Time.timeScale == 0f) return;
            var m = Mouse.current; var k = Keyboard.current;

            if (m != null && m.leftButton.wasPressedThisFrame && Time.time >= nextHit)
            {
                nextHit = Time.time + cooldown;
                Vector3 c = transform.position + Vector3.up + transform.forward * reach * 0.5f;
                foreach (var col in Physics.OverlapSphere(c, radius, ~0, QueryTriggerInteraction.Ignore))
                {
                    var d = col.GetComponentInParent<IMemoryDamageable>();
                    if (d != null) d.TakeHit(damage, stun);
                }
            }

            if (k != null && k.qKey.wasPressedThisFrame && Time.time >= nextRepel)
            {
                nextRepel = Time.time + repelCooldown;
                foreach (var col in Physics.OverlapSphere(transform.position, repelRadius))
                {
                    var s = col.GetComponentInParent<MemoryShadow>();
                    if (s) s.Stun(repelStun);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + Vector3.up + transform.forward * reach * 0.5f, radius);
        }
    }
}
