using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BeyondTime.Memories
{
    /// <summary>
    /// PLACEHOLDER shadow enemy so memories are playable today.
    /// Replace it everywhere by dragging your real enemy prefab into MemorySettings > Shadow Prefab.
    /// Your enemy only needs to (1) chase the player, (2) be destroyed when it dies,
    /// (3) call MemoryEvents.RaisePlayerHit(seconds) when it hits, or use Integration/ShadowScriptBridge.
    /// </summary>
    public class MemoryShadow : MonoBehaviour, IMemoryDamageable
    {
        public int health = 3;
        public float speed = 3.2f;
        public float detectRange = 30f;
        public float attackRange = 1.6f;
        public float attackCooldown = 1.8f;
        public float timePenalty = 3f;
        public float knockback = 6f;

        [Tooltip("Set by escort steps so some shadows go for the person you protect.")]
        public Transform targetOverride;

        public bool IsDead { get; private set; }

        Transform player;
        NavMeshAgent agent;
        float nextAttack, stunUntil;
        Renderer[] rends;
        Vector3 baseScale;

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            agent = GetComponent<NavMeshAgent>();
            if (agent && !agent.isOnNavMesh) agent.enabled = false;
            rends = GetComponentsInChildren<Renderer>();
            baseScale = transform.localScale;
            nextAttack = Time.time + 1f;     // grace period after spawning
            StartCoroutine(Rise());
        }

        void Update()
        {
            if (IsDead) return;
            if (Time.time < stunUntil) { Halt(); return; }

            Transform target = targetOverride && targetOverride.gameObject.activeInHierarchy ? targetOverride : player;
            if (!target) return;

            Vector3 flat = target.position - transform.position; flat.y = 0f;
            float d = flat.magnitude;
            if (d > detectRange) { Halt(); return; }

            if (flat.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), Time.deltaTime * 10f);

            if (d > attackRange) { Chase(target, flat); return; }

            Halt();
            if (Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackCooldown;
                if (target == player)
                {
                    MemoryEvents.RaisePlayerHit(timePenalty);
                    StartCoroutine(Knock(player, flat.normalized));
                }
            }
        }

        void Chase(Transform target, Vector3 flat)
        {
            if (agent && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = speed; agent.isStopped = false; agent.SetDestination(target.position);
                return;
            }
            Vector3 next = transform.position + flat.normalized * speed * Time.deltaTime;
            if (GroundBelow(next, out float y)) { next.y = y; transform.position = next; }   // never walks off into lava
        }

        void Halt() { if (agent && agent.enabled && agent.isOnNavMesh) agent.isStopped = true; }

        public void Stun(float seconds) { stunUntil = Time.time + seconds; }

        public void TakeHit(int damage, float stunSeconds)
        {
            if (IsDead) return;
            health -= damage;
            Stun(stunSeconds);
            StartCoroutine(Flash());
            if (health <= 0) Die();
        }

        public void Die()
        {
            if (IsDead) return;
            IsDead = true;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            StartCoroutine(Dissolve());
        }

        IEnumerator Rise()
        {
            float t = 0f;
            while (t < 0.5f) { t += Time.deltaTime; transform.localScale = baseScale * Mathf.SmoothStep(0.2f, 1f, t / 0.5f); yield return null; }
            transform.localScale = baseScale;
        }

        IEnumerator Dissolve()
        {
            float t = 0f;
            Vector3 s = transform.localScale;
            while (t < 0.6f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(s, new Vector3(s.x * 1.6f, 0.05f, s.z * 1.6f), t / 0.6f); yield return null; }
            Destroy(gameObject);
        }

        IEnumerator Flash()
        {
            foreach (var r in rends) if (r) r.enabled = false;
            yield return new WaitForSeconds(0.08f);
            foreach (var r in rends) if (r) r.enabled = true;
        }

        IEnumerator Knock(Transform p, Vector3 dir)
        {
            var cc = p ? p.GetComponent<CharacterController>() : null;
            if (!cc) yield break;
            float t = 0f;
            while (t < 0.2f && cc && cc.enabled) { t += Time.deltaTime; cc.Move(dir * knockback * (1f - t / 0.2f) * Time.deltaTime); yield return null; }
        }

        bool GroundBelow(Vector3 p, out float y)
        {
            y = p.y;
            var hits = Physics.RaycastAll(p + Vector3.up * 1.5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue; bool found = false;
            foreach (var h in hits)
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.collider.GetComponentInParent<MemoryShadow>()) continue;
                if (h.distance < best) { best = h.distance; y = h.point.y; found = true; }
            }
            return found;
        }
    }
}
