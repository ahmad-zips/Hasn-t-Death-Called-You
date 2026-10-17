using UnityEngine;
using UnityEngine.AI;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Brain for NPC fighters. Roles:
    ///   Attacker: menaces a victim, then hunts and attacks them. Turns on the player when hit or crowded,
    ///             goes back to the victim if the player backs off. Alerts its friends.
    ///   Guard:    patrols waypoints, spots the player (sight cone, hearing, or being alerted), then fights.
    ///   Victim:   panics, runs from the nearest threat, cowers when cornered, calms down when safe.
    /// Fighters circle, keep distance, attack in combos, block and dodge your strikes.
    /// Movement uses the CharacterController (slides along walls) and a NavMeshAgent for paths if one is baked.
    /// Drives the Starter Assets animator (Speed / Grounded / MotionSpeed) so idle, walk and run animate.
    /// </summary>
    [RequireComponent(typeof(Fighter))]
    public class BrawlerAI : MonoBehaviour, IMemoryResettable
    {
        public enum Role { Attacker, Guard, Victim }

        public Role role = Role.Guard;

        [Header("Movement")]
        public float walkSpeed = 1.7f;
        public float runSpeed = 4.6f;
        public float turnSpeed = 600f;

        [Header("Patrol (guards)")]
        public Transform[] patrolPoints;
        public float waitAtPoint = 1.5f;

        [Header("Senses")]
        public float sightRange = 12f;
        public float sightAngle = 130f;
        public float hearingRange = 4f;
        [Tooltip("Woken up when this one starts fighting.")]
        public BrawlerAI[] alertOnEngage;

        [Header("Fighting")]
        public float attackRange = 1.7f;
        [Range(0.1f, 1.5f)] public float aggression = 0.6f;
        public float minAttackGap = 0.7f;
        public float maxAttackGap = 1.7f;
        public int maxComboHits = 3;
        [Range(0f, 1f)] public float kickChance = 0.2f;
        [Range(0f, 1f)] public float blockChance = 0.35f;
        [Range(0f, 1f)] public float dodgeChance = 0.15f;

        [Header("Attacker")]
        public Fighter murderTarget;
        public float murderAttackGap = 2.4f;
        public float aggroMemory = 6f;
        [Tooltip("Before Engage() is called, stand over the victim threateningly.")]
        public bool menaceUntilEngaged = true;

        [Header("Victim")]
        public float panicRange = 7f;

        [Header("Arena")]
        [Tooltip("0 = off. Keeps this NPC within this many metres of where it started.")]
        public float leashRadius = 0f;

        public bool Engaged { get; private set; }

        enum State { Idle, Patrol, Fight, Flee, Cower, Calm, Menace, Out }
        State state;

        Fighter self, player, target;
        CharacterController cc;
        NavMeshAgent agent;
        Animator anim;
        int hSpeed, hGrounded, hMotion;
        bool hasSpeed, hasGrounded, hasMotion;

        Vector3 startPos;
        Quaternion startRot;
        float verticalVel, animSpeed;
        int patrolIndex;
        float waitUntil, nextAttack, reactUntil, blockUntil, aggroUntil, strafeSwitchAt, repathAt, stuckTime;
        int comboLeft;
        float strafeDir = 1f;
        Vector3 lastPos;

        // ------------------------------------------------------------ setup

        void Awake()
        {
            self = GetComponent<Fighter>();
            cc = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponentInChildren<Animator>();
            startPos = transform.position;
            startRot = transform.rotation;

            if (anim)
            {
                foreach (var p in anim.parameters)
                {
                    if (p.name == "Speed") { hasSpeed = true; hSpeed = p.nameHash; }
                    if (p.name == "Grounded") { hasGrounded = true; hGrounded = p.nameHash; }
                    if (p.name == "MotionSpeed") { hasMotion = true; hMotion = p.nameHash; }
                }
            }
            if (agent) { agent.updatePosition = false; agent.updateRotation = false; }
        }

        void OnEnable()  { self.Damaged += OnDamaged; }
        void OnDisable() { self.Damaged -= OnDamaged; }

        void Start()
        {
            if (agent && !agent.isOnNavMesh) agent.enabled = false;
            ResetState();
        }

        void ResetState()
        {
            Engaged = false;
            target = null;
            comboLeft = 0;
            patrolIndex = 0;
            aggroUntil = 0f;
            switch (role)
            {
                case Role.Attacker: state = menaceUntilEngaged ? State.Menace : State.Fight; Engaged = !menaceUntilEngaged; break;
                case Role.Guard: state = patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Idle; break;
                case Role.Victim: state = State.Cower; break;
            }
        }

        public void ResetForMemory() { ResetState(); verticalVel = 0f; animSpeed = 0f; }

        /// <summary>Start fighting now (memory steps call this). Attackers begin the murder.</summary>
        public void Engage()
        {
            if (self.IsDefeated || role == Role.Victim) return;
            if (Engaged) return;
            Engaged = true;
            state = State.Fight;
            foreach (var other in alertOnEngage) if (other) other.Engage();
        }

        void OnDamaged(Fighter attacker, float amount)
        {
            if (attacker && attacker.team == FighterTeam.Player)
            {
                aggroUntil = Time.time + aggroMemory;
                Engage();
            }
        }

        // ------------------------------------------------------------ loop

        void Update()
        {
            if (Time.deltaTime <= 0f) return;

            if (self.IsDefeated) { state = State.Out; self.inStance = false; self.cowering = false; Animate(0f); return; }
            ApplyGravity();
            if (!player) player = FindPlayer();

            if (self.IsDown || self.IsStaggered || self.IsDodging)
            {
                self.SetBlock(false);
                Animate(0f);
                return;
            }

            switch (role)
            {
                case Role.Attacker: AttackerBrain(); break;
                case Role.Guard: GuardBrain(); break;
                case Role.Victim: VictimBrain(); break;
            }

            // Stuck detection (used by the victim to decide to cower)
            float moved = (transform.position - lastPos).magnitude;
            stuckTime = moved < 0.01f ? stuckTime + Time.deltaTime : 0f;
            lastPos = transform.position;
        }

        // ------------------------------------------------------------ roles

        void AttackerBrain()
        {
            bool victimAlive = murderTarget && !murderTarget.IsDefeated;

            if (!Engaged)
            {
                self.inStance = true;
                if (victimAlive) Face(murderTarget.transform.position);
                if (player && Dist(player) < 3f) { aggroUntil = Time.time + aggroMemory; Engage(); }
                Animate(0f);
                return;
            }

            bool playerThreat = player && !player.IsDefeated && (Time.time < aggroUntil || Dist(player) < 2.4f);
            target = playerThreat || !victimAlive ? player : murderTarget;
            if (!target) { Animate(0f); return; }
            FightTarget(target, target == murderTarget);
        }

        void GuardBrain()
        {
            if (!Engaged)
            {
                self.inStance = false;
                if (player && CanSense(player)) { Engage(); return; }
                if (state == State.Patrol) Patrol(); else Animate(0f);
                return;
            }
            target = player;
            if (!target || target.IsDefeated) { Animate(0f); return; }
            FightTarget(target, false);
        }

        void VictimBrain()
        {
            Fighter threat = NearestHostile(panicRange, true);      // only run once the attack has started
            if (threat)
            {
                float d = Dist(threat);
                if (d < 1.8f || stuckTime > 0.8f)
                {
                    self.cowering = true;
                    Animate(0f);
                    if (stuckTime > 2.5f) stuckTime = 0f;   // try to run again after a while
                    return;
                }
                self.cowering = false;
                Vector3 away = transform.position - threat.transform.position; away.y = 0f;
                Vector3 side = Vector3.Cross(Vector3.up, away).normalized * Mathf.Sin(Time.time * 0.7f) * 0.6f;
                MoveDir((away.normalized + side).normalized, runSpeed);
                return;
            }

            bool danger = NearestHostile(5f, false) != null || NearestHostile(18f, true) != null;
            self.cowering = danger;
            Animate(0f);
            if (!danger && player) Face(player.transform.position);
        }

        // ------------------------------------------------------------ fighting

        void FightTarget(Fighter t, bool murdering)
        {
            self.inStance = true;
            self.focus = t;
            float d = Dist(t);

            if (d > attackRange + 0.35f && !self.IsAttacking)
            {
                self.SetBlock(false);
                MoveTo(t.transform.position, d > 4f ? runSpeed : walkSpeed);
                return;
            }

            Face(t.transform.position);

            // Defend against the player's attacks
            if (!murdering && t.IsAttacking && Time.time > reactUntil && IsFacingMe(t) && d < 2.6f)
            {
                reactUntil = Time.time + 0.9f;
                float r = Random.value;
                if (r < dodgeChance)
                {
                    Vector3 back = -transform.forward + transform.right * (Random.value < 0.5f ? -0.8f : 0.8f);
                    self.Dodge(back);
                }
                else if (r < dodgeChance + blockChance) blockUntil = Time.time + 0.65f;
            }
            self.SetBlock(Time.time < blockUntil);
            if (self.IsBlocking) { Animate(0f); return; }

            // Attack in combos
            if (self.IsAttacking)
            {
                if (comboLeft > 0 && self.Attack()) comboLeft--;
                Animate(0f);
                return;
            }

            if (Time.time >= nextAttack && self.CanAct && d <= attackRange + 0.2f)
            {
                if (!murdering && self.kick != null && self.kick.duration > 0f && Random.value < kickChance)
                {
                    self.Kick();
                    comboLeft = 0;
                }
                else
                {
                    comboLeft = murdering ? 0 : Random.Range(0, Mathf.Max(1, maxComboHits));
                    self.Attack();
                }
                float gap = murdering ? murderAttackGap : Random.Range(minAttackGap, maxAttackGap) / Mathf.Max(0.2f, aggression);
                nextAttack = Time.time + gap;
                Animate(0f);
                return;
            }

            // Footwork between attacks: back off if too close, otherwise circle
            if (murdering) { Animate(0f); return; }
            if (Time.time > strafeSwitchAt) { strafeDir = Random.value < 0.5f ? -1f : 1f; strafeSwitchAt = Time.time + Random.Range(1f, 2.5f); }
            Vector3 toT = t.transform.position - transform.position; toT.y = 0f;
            Vector3 dir = d < attackRange * 0.6f ? -toT.normalized : Vector3.Cross(Vector3.up, toT.normalized) * strafeDir;
            MoveDir(dir, walkSpeed * 0.55f, faceMove: false);
        }

        // ------------------------------------------------------------ patrol and senses

        void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) { Animate(0f); return; }
            var p = patrolPoints[patrolIndex % patrolPoints.Length];
            if (!p) { patrolIndex++; return; }

            Vector3 d = p.position - transform.position; d.y = 0f;
            if (d.magnitude < 0.6f)
            {
                if (waitUntil < Time.time - 0.1f) waitUntil = Time.time + waitAtPoint;
                if (Time.time >= waitUntil) patrolIndex++;
                Animate(0f);
                return;
            }
            MoveTo(p.position, walkSpeed);
        }

        bool CanSense(Fighter f)
        {
            if (f.IsDefeated) return false;
            Vector3 d = f.transform.position - transform.position; d.y = 0f;
            float dist = d.magnitude;
            if (dist <= hearingRange) return true;
            if (dist > sightRange) return false;
            if (Vector3.Angle(transform.forward, d) > sightAngle * 0.5f) return false;
            Vector3 eye = transform.position + Vector3.up * 1.6f;
            Vector3 to = f.transform.position + Vector3.up * 1.2f;
            foreach (var h in Physics.RaycastAll(eye, (to - eye).normalized, (to - eye).magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.transform.IsChildOf(transform) || h.collider.transform.IsChildOf(f.transform)) continue;
                if (h.collider.GetComponentInParent<Fighter>()) continue;
                return false;   // a wall is in the way
            }
            return true;
        }

        Fighter NearestHostile(float range, bool engagedOnly)
        {
            Fighter best = null; float bestD = range;
            foreach (var f in Fighter.All)
            {
                if (f.team != FighterTeam.Hostile || f.IsDefeated) continue;
                if (engagedOnly)
                {
                    var ai = f.GetComponent<BrawlerAI>();
                    if (ai && !ai.Engaged) continue;
                }
                float d = Dist(f);
                if (d < bestD) { bestD = d; best = f; }
            }
            return best;
        }

        static Fighter FindPlayer()
        {
            foreach (var f in Fighter.All) if (f.team == FighterTeam.Player) return f;
            return null;
        }

        bool IsFacingMe(Fighter f)
        {
            Vector3 d = transform.position - f.transform.position; d.y = 0f;
            return Vector3.Angle(f.transform.forward, d) < 60f;
        }

        // ------------------------------------------------------------ locomotion

        void MoveTo(Vector3 dest, float speed)
        {
            Vector3 dir;
            if (agent && agent.enabled && agent.isOnNavMesh)
            {
                agent.nextPosition = transform.position;
                if (Time.time >= repathAt) { agent.SetDestination(dest); repathAt = Time.time + 0.25f; }
                dir = agent.desiredVelocity; dir.y = 0f;
                if (dir.sqrMagnitude < 0.01f) { dir = dest - transform.position; dir.y = 0f; }
            }
            else { dir = dest - transform.position; dir.y = 0f; }

            if (dir.sqrMagnitude < 0.0004f) { Animate(0f); return; }
            MoveDir(Avoid(dir.normalized), speed);
        }

        void MoveDir(Vector3 dir, float speed, bool faceMove = true)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) { Animate(0f); return; }
            dir.Normalize();
            dir += Separation();
            dir = Leash(dir);
            dir = KeepOnGround(dir);
            if (dir.sqrMagnitude < 0.0001f) { Animate(0f); return; }
            if (faceMove) Face(transform.position + dir);
            if (cc && cc.enabled) cc.Move(dir * speed * Time.deltaTime);
            else transform.position += dir * speed * Time.deltaTime;
            Animate(speed);
        }

        Vector3 Leash(Vector3 dir)
        {
            if (leashRadius <= 0f) return dir;
            Vector3 fromHome = transform.position - startPos; fromHome.y = 0f;
            if (fromHome.magnitude < leashRadius * 0.9f) return dir;
            Vector3 outward = fromHome.normalized;
            float o = Vector3.Dot(dir, outward);
            if (o > 0f) dir -= outward * o * 1.2f;          // slide along the edge of the arena
            return dir;
        }

        /// <summary>Never step off an edge (islands float over lava).</summary>
        Vector3 KeepOnGround(Vector3 dir)
        {
            if (HasGround(transform.position + dir.normalized * 0.8f)) return dir;
            foreach (float a in new[] { 45f, -45f, 90f, -90f, 135f, -135f })
            {
                Vector3 alt = Quaternion.Euler(0f, a, 0f) * dir;
                if (HasGround(transform.position + alt.normalized * 0.8f)) return alt;
            }
            return Vector3.zero;
        }

        bool HasGround(Vector3 p)
        {
            foreach (var h in Physics.RaycastAll(p + Vector3.up * 1.2f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.collider.GetComponentInParent<Fighter>()) continue;
                return true;
            }
            return false;
        }

        Vector3 Avoid(Vector3 dir)
        {
            Vector3 origin = transform.position + Vector3.up * 0.9f;
            if (!Blocked(origin, dir)) return dir;
            foreach (float a in new[] { 45f, -45f, 90f, -90f })
            {
                Vector3 alt = Quaternion.Euler(0f, a, 0f) * dir;
                if (!Blocked(origin, alt)) return alt;
            }
            return dir;
        }

        bool Blocked(Vector3 origin, Vector3 dir)
        {
            foreach (var h in Physics.SphereCastAll(origin, 0.3f, dir, 1.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.collider.GetComponentInParent<Fighter>()) continue;
                return true;
            }
            return false;
        }

        Vector3 Separation()
        {
            Vector3 push = Vector3.zero;
            foreach (var f in Fighter.All)
            {
                if (f == self || f.IsDefeated) continue;
                Vector3 d = transform.position - f.transform.position; d.y = 0f;
                float m = d.magnitude;
                if (m > 0.01f && m < 0.9f) push += d / m * (0.9f - m);
            }
            return push;
        }

        void Face(Vector3 point)
        {
            Vector3 d = point - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), turnSpeed * Time.deltaTime);
        }

        void ApplyGravity()
        {
            if (!cc || !cc.enabled) return;
            if (cc.isGrounded && verticalVel < 0f) verticalVel = -2f;
            else verticalVel += Physics.gravity.y * Time.deltaTime;
            cc.Move(Vector3.up * verticalVel * Time.deltaTime);
        }

        float Dist(Fighter f)
        {
            Vector3 d = f.transform.position - transform.position; d.y = 0f;
            return d.magnitude;
        }

        /// <summary>Maps real speed onto the Starter Assets blend tree (0 idle, 2 walk, 6 run).</summary>
        void Animate(float speed)
        {
            if (!anim) return;
            float param = speed <= walkSpeed
                ? (walkSpeed > 0f ? speed / walkSpeed * 2f : 0f)
                : 2f + (speed - walkSpeed) / Mathf.Max(0.01f, runSpeed - walkSpeed) * 4f;
            animSpeed = Mathf.Lerp(animSpeed, param, 1f - Mathf.Exp(-10f * Time.deltaTime));
            if (hasSpeed) anim.SetFloat(hSpeed, animSpeed);
            if (hasMotion) anim.SetFloat(hMotion, 1f);
            if (hasGrounded) anim.SetBool(hGrounded, true);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, hearingRange);
        }
    }
}
