using System.Collections;
using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// A projectile fired by a ranged enemy. Flies straight, is stopped by walls and cover,
    /// hurts the player (blockable, dodgeable), and flashes on impact.
    /// Any model works as the projectile prefab; this script is added at launch if missing.
    /// </summary>
    public class MemoryProjectile : MonoBehaviour
    {
        public float speed = 13f;
        public float radius = 0.25f;
        public float life = 5f;
        public Color flashColor = new Color(1f, 0.3f, 0.6f);

        Vector3 dir;
        Fighter owner;
        AttackMove hit;
        float dieAt;

        public static MemoryProjectile Launch(GameObject prefab, Vector3 pos, Vector3 dir, float speed, float damage, Fighter owner)
        {
            GameObject go = prefab ? Instantiate(prefab, pos, Quaternion.LookRotation(dir)) : CreateDefault(pos, dir);
            var p = go.GetComponent<MemoryProjectile>();
            if (!p) p = go.AddComponent<MemoryProjectile>();
            foreach (var c in go.GetComponentsInChildren<Collider>()) c.enabled = false;   // movement uses sphere casts
            p.dir = dir.normalized;
            p.speed = speed;
            p.owner = owner;
            p.hit = new AttackMove { name = "Projectile", pose = PoseMove.None, damage = damage, knockback = 2.5f, stagger = 0.35f, guardDamage = 25f };
            p.dieAt = Time.time + p.life;
            return p;
        }

        static GameObject CreateDefault(Vector3 pos, Vector3 dir)
        {
            var go = new GameObject("Projectile");
            go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = new Color(1f, 0.3f, 0.6f); l.intensity = 4f; l.range = 4f;
            return go;
        }

        void Update()
        {
            if (Time.time >= dieAt) { Destroy(gameObject); return; }
            float step = speed * Time.deltaTime;
            Vector3 pos = transform.position;

            RaycastHit best = default(RaycastHit);
            float bestDist = float.MaxValue;
            foreach (var h in Physics.SphereCastAll(pos, radius, dir, step, ~0, QueryTriggerInteraction.Ignore))
            {
                if (owner && h.collider.transform.IsChildOf(owner.transform)) continue;
                var f = h.collider.GetComponentInParent<Fighter>();
                if (f && f.team != FighterTeam.Player) continue;      // flies past other enemies and the people you protect
                if (h.distance < bestDist) { bestDist = h.distance; best = h; }
            }

            if (best.collider)
            {
                var target = best.collider.GetComponentInParent<Fighter>();
                if (target) target.TakeHit(owner, hit, dir);
                StartCoroutine(Impact(best.point));
                return;
            }
            transform.position = pos + dir * step;
        }

        IEnumerator Impact(Vector3 at)
        {
            enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            transform.position = at;
            var l = GetComponent<Light>();
            if (!l) l = gameObject.AddComponent<Light>();
            l.type = LightType.Point; l.color = flashColor; l.range = 5f;
            float t = 0f;
            while (t < 0.2f) { t += Time.deltaTime; l.intensity = Mathf.Lerp(10f, 0f, t / 0.2f); yield return null; }
            Destroy(gameObject);
        }
    }
}
