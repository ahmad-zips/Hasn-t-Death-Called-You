using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// A section of fire that bursts on and off in a rhythm. Small embers warn just before it ignites.
    /// Standing in it while it burns costs time, staggers you and pushes you back. Time your crossing.
    /// TO REPLACE THE LOOK: put your fire particle systems under Flames (and Warning) and delete the placeholders.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FireHazard : MonoBehaviour
    {
        public float onTime = 1.5f;
        public float offTime = 2.3f;
        public float warnTime = 0.6f;
        [Tooltip("Offsets the rhythm so several hazards do not burn at the same moment.")]
        public float phase = 0f;
        public float damage = 8f;
        public float knockback = 4.5f;
        public float hurtInterval = 0.8f;

        public GameObject flames;
        public GameObject warning;

        public bool IsBurning { get; private set; }

        Fighter playerFighter;
        Transform playerInside;
        float nextHurt;
        AttackMove burn;

        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            burn = new AttackMove { name = "Fire", pose = PoseMove.None, damage = damage, knockback = knockback, stagger = 0.4f, unblockable = true };
        }

        void Update()
        {
            float cycle = onTime + offTime;
            float t = Mathf.Repeat(Time.time + phase, cycle);
            IsBurning = t < onTime;
            bool warn = !IsBurning && t > cycle - warnTime;

            if (flames && flames.activeSelf != IsBurning) flames.SetActive(IsBurning);
            if (warning && warning.activeSelf != warn) warning.SetActive(warn);

            if (IsBurning && playerInside && Time.time >= nextHurt)
            {
                nextHurt = Time.time + hurtInterval;
                Vector3 push = playerInside.position - transform.position; push.y = 0f;
                Vector3 axis = transform.forward;
                Vector3 dir = Vector3.Dot(push, axis) >= 0f ? axis : -axis;   // push out along the walkway, never off the side
                if (playerFighter) playerFighter.TakeHit(null, burn, dir);
                else MemoryEvents.RaisePlayerHit(damage * 0.25f);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInside = other.transform;
            playerFighter = other.GetComponent<Fighter>();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) playerInside = null;
        }

        void OnDisable() { playerInside = null; }
    }
}
