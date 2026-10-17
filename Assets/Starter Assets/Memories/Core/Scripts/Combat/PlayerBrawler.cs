using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Player fighting controls on top of the Starter Assets third-person controller.
    ///   Left mouse        punch combo: jab, cross, hook (keep clicking)
    ///   F                 front kick (breaks guards); F during a punch combo = roundhouse (knockdown)
    ///   Right mouse       block; press it just before a hit lands to PARRY (attacker is stunned)
    ///   Space             dodge (invulnerable while dodging; the only way past a knife stab)
    ///   Q                 repel: stuns everything nearby (cooldown)
    /// Movement is frozen only while you are committed to a move, then your WASD input is restored.
    /// Needs Fighter (Player preset) and FighterPose on the same object (PlayerArmature).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Fighter))]
    public class PlayerBrawler : MonoBehaviour
    {
        public float repelRadius = 6f;
        public float repelStun = 2.5f;
        public float repelCooldown = 8f;
        public Light repelFlash;            // optional
        public float stanceRange = 9f;      // raise the guard when enemies are this close

        public float RepelReady01 => repelCooldown <= 0f ? 1f : Mathf.Clamp01(1f - (repelReadyAt - Time.time) / repelCooldown);

        Fighter fighter;
        Component inputs;
        FieldInfo moveField, jumpField, sprintField;
        bool locked;
        float repelReadyAt, flashOffAt;

        void Start()
        {
            fighter = GetComponent<Fighter>();
            inputs = GetComponent("StarterAssetsInputs");
            if (inputs)
            {
                var t = inputs.GetType();
                moveField = t.GetField("move");
                jumpField = t.GetField("jump");
                sprintField = t.GetField("sprint");
            }
            if (repelFlash) repelFlash.enabled = false;
        }

        void Update()
        {
            if (repelFlash && repelFlash.enabled && Time.time > flashOffAt) repelFlash.enabled = false;
            if (Time.timeScale < 0.01f) return;                 // a choice is open, time is frozen

            var k = Keyboard.current;
            var m = Mouse.current;
            if (k == null || m == null) return;

            if (!fighter.IsDefeated)
            {
                fighter.SetBlock(m.rightButton.isPressed);
                if (m.leftButton.wasPressedThisFrame) fighter.Attack();
                if (k.fKey.wasPressedThisFrame) fighter.Kick();
                if (k.spaceKey.wasPressedThisFrame) { fighter.Dodge(InputDirection(k)); ClearJump(); }
                if (k.qKey.wasPressedThisFrame && Time.time >= repelReadyAt) Repel();
            }

            fighter.inStance = HostileNear();
            if (fighter.IsBlocking) FaceNearestHostile(4.5f);   // blocks only work facing the attack

            bool lockNow = fighter.IsAttacking || fighter.IsDodging || fighter.IsStaggered || fighter.IsDown || fighter.IsBlocking || fighter.IsDefeated;
            if (lockNow)
            {
                SetMove(Vector2.zero);
                ClearJump();
                if (sprintField != null) sprintField.SetValue(inputs, false);
                locked = true;
            }
            else if (locked)
            {
                locked = false;
                SetMove(WASD(k));
            }
        }

        void Repel()
        {
            repelReadyAt = Time.time + repelCooldown;
            foreach (var col in Physics.OverlapSphere(transform.position, repelRadius))
            {
                var f = col.GetComponentInParent<Fighter>();
                if (f && f.team == FighterTeam.Hostile) { f.Stagger(repelStun); continue; }
                var s = col.GetComponentInParent<MemoryShadow>();
                if (s) s.Stun(repelStun);
            }
            if (repelFlash) { repelFlash.enabled = true; flashOffAt = Time.time + 0.15f; }
        }

        void FaceNearestHostile(float range)
        {
            Fighter best = null; float bestD = range * range;
            foreach (var f in Fighter.All)
            {
                if (f.team != FighterTeam.Hostile || f.IsDefeated) continue;
                float d = (f.transform.position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = f; }
            }
            if (!best) return;
            Vector3 dir = best.transform.position - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 720f * Time.deltaTime);
        }

        bool HostileNear()
        {
            foreach (var f in Fighter.All)
            {
                if (f.team != FighterTeam.Hostile || f.IsDefeated) continue;
                if ((f.transform.position - transform.position).sqrMagnitude < stanceRange * stanceRange) return true;
            }
            return false;
        }

        Vector3 InputDirection(Keyboard k)
        {
            Vector2 v = WASD(k);
            if (v.sqrMagnitude < 0.01f) return -transform.forward;
            Transform cam = Camera.main ? Camera.main.transform : transform;
            Vector3 f = cam.forward; f.y = 0f; f.Normalize();
            Vector3 r = cam.right; r.y = 0f; r.Normalize();
            return f * v.y + r * v.x;
        }

        static Vector2 WASD(Keyboard k)
        {
            float x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
            float y = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
            var v = new Vector2(x, y);
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        void SetMove(Vector2 v) { if (moveField != null) moveField.SetValue(inputs, v); }
        void ClearJump() { if (jumpField != null) jumpField.SetValue(inputs, false); }
    }
}
