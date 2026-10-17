using System.Collections;
using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Brain for a ranged shadow (the Harrier). Stays on its perch, turns to the player, and when it has a clear
    /// line of sight it winds up (hand glows) and throws a projectile. Cover blocks both its view and its shots.
    /// Up close it shoves. Reach it and knock it out to stop the shooting.
    /// </summary>
    [RequireComponent(typeof(Fighter))]
    public class RangedAI : MonoBehaviour, IMemoryResettable
    {
        public bool startEngaged = false;
        public float range = 24f;
        public float fireInterval = 2.3f;
        public float firstShotDelay = 1.2f;
        public float projectileSpeed = 13f;
        public float projectileDamage = 9f;
        [Tooltip("Projectile model. Empty = the built-in glowing orb.")]
        public GameObject projectilePrefab;
        public float meleeRange = 1.8f;
        public float turnSpeed = 360f;
        [Tooltip("The throw animation. Uses the Cast pose by default.")]
        public AttackMove castMove = new AttackMove { name = "Cast", pose = PoseMove.Cast, duration = 0.9f, hitStart = 0.62f, hitEnd = 0.7f,
                                                      damage = 0f, reach = 0f, radius = 0.01f, lunge = 0f, knockback = 0f, stagger = 0f, telegraph = true };

        public bool Engaged { get; private set; }

        Fighter self, player;
        CharacterController cc;
        Animator anim;
        float nextShot, verticalVel;

        void Awake()
        {
            self = GetComponent<Fighter>();
            cc = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
        }

        void Start() { if (!Engaged) Engaged = startEngaged; }

        public void Engage() { if (Engaged) return; Engaged = true; nextShot = Time.time + firstShotDelay; }

        public void ResetForMemory() { StopAllCoroutines(); Engaged = startEngaged; verticalVel = 0f; }

        void Update()
        {
            if (Time.deltaTime <= 0f || self.IsDefeated) return;
            Gravity();
            if (!player) foreach (var f in Fighter.All) if (f.team == FighterTeam.Player) { player = f; break; }
            if (!player || player.IsDefeated || !Engaged) { self.inStance = false; return; }
            if (self.IsDown || self.IsStaggered) return;

            self.inStance = true;
            self.focus = player;
            Vector3 d = player.transform.position - transform.position; d.y = 0f;
            if (d.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), turnSpeed * Time.deltaTime);

            float dist = d.magnitude;
            if (dist < meleeRange) { if (self.CanAct) self.Attack(); return; }

            if (Time.time >= nextShot && self.CanAct && dist <= range && HasLineOfSight())
            {
                nextShot = Time.time + fireInterval;
                if (self.Perform(castMove)) StartCoroutine(Release());
            }
        }

        IEnumerator Release()
        {
            yield return new WaitForSeconds(castMove.duration * castMove.hitStart);
            if (self.IsDefeated || !self.IsAttacking || !player) yield break;

            Vector3 from = Muzzle();
            Vector3 to = player.transform.position + Vector3.up * 1.1f;
            MemoryProjectile.Launch(projectilePrefab, from, to - from, projectileSpeed, projectileDamage, self);
        }

        Vector3 Muzzle()
        {
            if (anim && anim.isHuman)
            {
                var hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
                if (hand) return hand.position + transform.forward * 0.3f;
            }
            return transform.position + Vector3.up * 1.5f * transform.lossyScale.y + transform.forward * 0.6f;
        }

        bool HasLineOfSight()
        {
            Vector3 eye = transform.position + Vector3.up * 1.6f * transform.lossyScale.y;
            Vector3 to = player.transform.position + Vector3.up * 1.1f;
            Vector3 dir = to - eye;
            foreach (var h in Physics.RaycastAll(eye, dir.normalized, dir.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                var f = h.collider.GetComponentInParent<Fighter>();
                if (f) continue;
                return false;
            }
            return true;
        }

        void Gravity()
        {
            if (!cc || !cc.enabled) return;
            if (cc.isGrounded && verticalVel < 0f) verticalVel = -2f;
            else verticalVel += Physics.gravity.y * Time.deltaTime;
            cc.Move(Vector3.up * verticalVel * Time.deltaTime);
        }
    }
}
