using UnityEngine;
using UnityEngine.AI;

namespace BeyondTime.Memories
{
    /// <summary>
    /// A person the player must protect and lead (escort steps).
    /// Follows the player when close. Loses health while shadows are near (handled by MemoryZone).
    /// Works with or without a NavMeshAgent. Add one for smarter pathing once the world has a baked NavMesh.
    /// </summary>
    [RequireComponent(typeof(MemoryActor))]
    public class MemoryNPC : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float followDistance = 2.2f;     // stops this close to the player
        public float followRange = 14f;         // waits if the player is further than this
        public float speed = 3.6f;
        public string walkState = "";           // animation state names on your model (optional)
        public string idleState = "";
        public string hurtState = "";

        public float Health { get; private set; }
        public float Health01 => maxHealth <= 0f ? 0f : Health / maxHealth;
        public bool IsDead => Health <= 0f;
        public string DisplayName => actor ? actor.displayName : name;

        MemoryActor actor;
        NavMeshAgent agent;
        Transform leader;
        Vector3 startPos;
        Quaternion startRot;
        bool following, walking;
        float hurtCooldown;

        void Awake()
        {
            actor = GetComponent<MemoryActor>();
            agent = GetComponent<NavMeshAgent>();
            startPos = transform.position;
            startRot = transform.rotation;
            Health = maxHealth;
        }

        void Start() { if (agent && !agent.isOnNavMesh) agent.enabled = false; }

        public void BeginFollow(Transform target) { leader = target; following = true; Health = maxHealth; }
        public void StopFollow() { following = false; Stop(); }

        public void Damage(float amount)
        {
            if (IsDead) return;
            Health = Mathf.Max(0f, Health - amount);
            if (Time.time >= hurtCooldown) { actor.Play(hurtState); hurtCooldown = Time.time + 1f; }
        }

        public void ResetNPC()
        {
            following = false;
            Health = maxHealth;
            if (agent && agent.enabled) agent.Warp(startPos); else transform.position = startPos;
            transform.rotation = startRot;
            Stop();
        }

        void Update()
        {
            if (!following || IsDead || !leader) return;

            Vector3 flat = leader.position - transform.position; flat.y = 0f;
            float d = flat.magnitude;

            if (d > followDistance && d < followRange) Move(flat);
            else Stop();
        }

        void Move(Vector3 flatDir)
        {
            if (agent && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = speed;
                agent.isStopped = false;
                agent.SetDestination(leader.position);
            }
            else
            {
                Vector3 next = transform.position + flatDir.normalized * speed * Time.deltaTime;
                if (GroundBelow(next, out float y)) { next.y = y; transform.position = next; }
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flatDir), Time.deltaTime * 8f);
            }
            if (!walking) { walking = true; actor.Play(walkState); }
        }

        void Stop()
        {
            if (agent && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            if (walking) { walking = false; actor.Play(idleState); }
        }

        bool GroundBelow(Vector3 p, out float y)
        {
            y = p.y;
            var hits = Physics.RaycastAll(p + Vector3.up * 1.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue; bool found = false;
            foreach (var h in hits)
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.distance < best) { best = h.distance; y = h.point.y; found = true; }
            }
            return found;
        }
    }
}
