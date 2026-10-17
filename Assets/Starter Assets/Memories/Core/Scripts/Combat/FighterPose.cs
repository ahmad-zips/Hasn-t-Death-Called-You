using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Animates punches, kicks, knife attacks, guard stance, blocking, flinching and cowering on ANY humanoid model
    /// by bending the real arm and leg bones toward target points every frame, on top of the normal walk/idle animation.
    /// No animation clips needed. If a move has an Anim State set on the Fighter, that clip is used instead
    /// and this layer stays out of the way for that move.
    /// </summary>
    [RequireComponent(typeof(Fighter))]
    public class FighterPose : MonoBehaviour
    {
        [Range(0f, 1f)] public float stanceWeight = 0.65f;
        public float poseSmoothing = 14f;

        Fighter fighter;
        Animator anim;
        Transform lUpper, lLower, lHand, rUpper, rLower, rHand, rThigh, rShin, rFoot, chest, head;
        float scale = 1f;
        float stanceW, blockW, cowerW, flinchW;
        bool ready;

        void Start()
        {
            fighter = GetComponent<Fighter>();
            anim = GetComponentInChildren<Animator>();
            if (!anim || !anim.isHuman) { enabled = false; return; }

            lUpper = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            lLower = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            lHand  = anim.GetBoneTransform(HumanBodyBones.LeftHand);
            rUpper = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rLower = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rHand  = anim.GetBoneTransform(HumanBodyBones.RightHand);
            rThigh = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rShin  = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rFoot  = anim.GetBoneTransform(HumanBodyBones.RightFoot);
            chest  = anim.GetBoneTransform(HumanBodyBones.Chest);
            if (!chest) chest = anim.GetBoneTransform(HumanBodyBones.Spine);
            head   = anim.GetBoneTransform(HumanBodyBones.Head);

            if (head) scale = Mathf.Clamp((head.position.y - transform.position.y) / 1.6f, 0.5f, 1.6f);
            ready = lUpper && lLower && lHand && rUpper && rLower && rHand;
        }

        void LateUpdate()
        {
            if (!ready || fighter.IsDefeated || fighter.IsDown) return;
            float dt = Time.deltaTime;
            float k = 1f - Mathf.Exp(-poseSmoothing * Mathf.Max(dt, 0.0001f));

            var move = fighter.CurrentMove;
            bool clipMove = move != null && !string.IsNullOrEmpty(move.animState);
            bool attacking = move != null && !clipMove && move.pose != PoseMove.None;

            stanceW = Mathf.Lerp(stanceW, fighter.inStance && !attacking ? stanceWeight : 0f, k);
            blockW  = Mathf.Lerp(blockW, fighter.IsBlocking ? 1f : 0f, k * 1.5f);
            cowerW  = Mathf.Lerp(cowerW, fighter.cowering ? 1f : 0f, k);
            float flinchTarget = fighter.IsStaggered ? Mathf.Clamp01(1f - (Time.time - fighter.StaggerStarted) * 2.5f) : 0f;
            flinchW = Mathf.Lerp(flinchW, flinchTarget, k * 2f);

            // Base layers (guard / cower / block) on the arms
            if (!attacking)
            {
                if (cowerW > 0.01f) { Cower(cowerW); }
                else if (blockW > 0.01f) { Block(blockW); }
                else if (stanceW > 0.01f) { Guard(stanceW); }
            }

            if (attacking) Strike(move, fighter.AttackProgress);
            if (flinchW > 0.01f) Flinch(flinchW);
        }

        // ------------------------------------------------------------ poses

        void Guard(float w)
        {
            Arm(lUpper, lLower, lHand, P(-0.28f, 1.15f, 0.15f), P(-0.14f, 1.50f, 0.34f), w);
            Arm(rUpper, rLower, rHand, P(0.28f, 1.12f, 0.10f),  P(0.15f, 1.45f, 0.30f),  w);
        }

        void Block(float w)
        {
            Arm(lUpper, lLower, lHand, P(-0.25f, 1.30f, 0.25f), P(-0.08f, 1.65f, 0.33f), w);
            Arm(rUpper, rLower, rHand, P(0.25f, 1.30f, 0.25f),  P(0.08f, 1.65f, 0.33f),  w);
            Bend(chest, 10f * w);
        }

        void Cower(float w)
        {
            Bend(chest, 35f * w);
            Arm(lUpper, lLower, lHand, P(-0.25f, 1.25f, 0.35f), P(-0.06f, 1.55f, 0.45f), w);
            Arm(rUpper, rLower, rHand, P(0.25f, 1.25f, 0.35f),  P(0.06f, 1.55f, 0.45f),  w);
        }

        void Flinch(float w)
        {
            Bend(chest, -22f * w);
            Bend(head, -15f * w);
        }

        void Strike(AttackMove m, float t)
        {
            // Phase weights: wind up, snap to the strike, recover
            float h0 = Mathf.Max(0.05f, m.hitStart), h1 = Mathf.Max(h0, m.hitEnd);
            bool striking = t >= h0;
            float w = !striking ? Mathf.SmoothStep(0f, 0.85f, t / h0)
                    : t <= h1 ? 1f
                    : 1f - Mathf.SmoothStep(0f, 1f, (t - h1) / Mathf.Max(0.01f, 1f - h1));

            switch (m.pose)
            {
                case PoseMove.JabLeft:
                    Guard(0.6f);
                    if (striking) Arm(lUpper, lLower, lHand, P(-0.12f, 1.48f, 0.50f), P(-0.10f, 1.50f, 0.88f), w);
                    else          Arm(lUpper, lLower, lHand, P(-0.30f, 1.20f, 0.05f), P(-0.20f, 1.45f, 0.25f), w);
                    Twist(chest, 15f * w);
                    break;

                case PoseMove.CrossRight:
                    Arm(lUpper, lLower, lHand, P(-0.28f, 1.15f, 0.15f), P(-0.14f, 1.52f, 0.32f), 0.8f);
                    if (striking) Arm(rUpper, rLower, rHand, P(0.08f, 1.48f, 0.50f), P(0.05f, 1.50f, 0.92f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.32f, 1.15f, 0.00f), P(0.20f, 1.45f, 0.20f), w);
                    Twist(chest, -25f * w);
                    break;

                case PoseMove.HookRight:
                    Arm(lUpper, lLower, lHand, P(-0.28f, 1.15f, 0.15f), P(-0.14f, 1.52f, 0.32f), 0.8f);
                    if (striking) Arm(rUpper, rLower, rHand, P(0.35f, 1.48f, 0.45f), P(-0.05f, 1.50f, 0.65f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.50f, 1.30f, -0.05f), P(0.55f, 1.45f, 0.20f), w);
                    Twist(chest, (striking ? -35f : 20f) * w);
                    break;

                case PoseMove.Uppercut:
                    if (striking) Arm(rUpper, rLower, rHand, P(0.15f, 1.25f, 0.40f), P(0.05f, 1.70f, 0.55f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.30f, 1.10f, 0.00f), P(0.20f, 1.00f, 0.25f), w);
                    Bend(chest, (striking ? -10f : 15f) * w);
                    break;

                case PoseMove.FrontKick:
                    Guard(0.8f);
                    if (striking) Leg(P(0.12f, 1.00f, 0.55f), P(0.12f, 1.05f, 1.10f), w);
                    else          Leg(P(0.12f, 0.95f, 0.45f), P(0.12f, 0.50f, 0.35f), w);
                    Bend(chest, -12f * w);
                    break;

                case PoseMove.Roundhouse:
                    Guard(0.8f);
                    if (striking) Leg(P(0.45f, 1.05f, 0.40f), P(0.05f, 1.30f, 1.00f), w);
                    else          Leg(P(0.35f, 0.95f, 0.30f), P(0.45f, 0.55f, 0.10f), w);
                    Twist(chest, -30f * w);
                    Bend(chest, -15f * w);
                    break;

                case PoseMove.KnifeSlash:
                    Arm(lUpper, lLower, lHand, P(-0.30f, 1.20f, 0.10f), P(-0.25f, 1.35f, 0.30f), 0.6f);
                    if (striking) Arm(rUpper, rLower, rHand, P(0.10f, 1.35f, 0.40f), P(-0.45f, 1.20f, 0.60f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.40f, 1.50f, -0.10f), P(0.55f, 1.60f, 0.00f), w);
                    Twist(chest, (striking ? -30f : 25f) * w);
                    break;

                case PoseMove.KnifeStab:
                    Arm(lUpper, lLower, lHand, P(-0.30f, 1.20f, 0.20f), P(-0.15f, 1.40f, 0.45f), 0.7f);
                    if (striking) Arm(rUpper, rLower, rHand, P(0.10f, 1.25f, 0.50f), P(0.05f, 1.20f, 0.95f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.30f, 1.10f, -0.20f), P(0.25f, 1.15f, -0.15f), w);
                    Twist(chest, (striking ? -20f : 30f) * w);
                    Bend(chest, (striking ? 12f : -8f) * w);
                    break;

                case PoseMove.HeavySlam:
                    if (striking)
                    {
                        Arm(lUpper, lLower, lHand, P(-0.25f, 1.20f, 0.60f), P(-0.15f, 0.60f, 0.90f), w);
                        Arm(rUpper, rLower, rHand, P(0.25f, 1.20f, 0.60f),  P(0.15f, 0.60f, 0.90f),  w);
                    }
                    else
                    {
                        Arm(lUpper, lLower, lHand, P(-0.30f, 1.80f, 0.00f), P(-0.15f, 2.15f, 0.10f), w);
                        Arm(rUpper, rLower, rHand, P(0.30f, 1.80f, 0.00f),  P(0.15f, 2.15f, 0.10f),  w);
                    }
                    Bend(chest, (striking ? 30f : -18f) * w);
                    break;

                case PoseMove.Sweep:
                    Arm(lUpper, lLower, lHand, P(-0.30f, 1.20f, 0.10f), P(-0.25f, 1.35f, 0.30f), 0.6f);
                    if (striking) Arm(rUpper, rLower, rHand, P(0.30f, 1.20f, 0.50f), P(-0.70f, 1.00f, 0.60f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.50f, 1.30f, -0.10f), P(0.70f, 1.20f, -0.30f), w);
                    Twist(chest, (striking ? -45f : 40f) * w);
                    break;

                case PoseMove.Cast:
                    if (striking) Arm(rUpper, rLower, rHand, P(0.15f, 1.50f, 0.50f), P(0.10f, 1.50f, 0.90f), w);
                    else          Arm(rUpper, rLower, rHand, P(0.35f, 1.60f, -0.10f), P(0.20f, 1.75f, -0.20f), w);
                    Arm(lUpper, lLower, lHand, P(-0.30f, 1.25f, 0.15f), P(-0.20f, 1.40f, 0.35f), 0.5f);
                    Twist(chest, (striking ? -25f : 20f) * w);
                    break;

                case PoseMove.Shove:
                    if (striking)
                    {
                        Arm(lUpper, lLower, lHand, P(-0.25f, 1.35f, 0.40f), P(-0.20f, 1.40f, 0.75f), w);
                        Arm(rUpper, rLower, rHand, P(0.25f, 1.35f, 0.40f),  P(0.20f, 1.40f, 0.75f),  w);
                    }
                    else
                    {
                        Arm(lUpper, lLower, lHand, P(-0.28f, 1.20f, 0.05f), P(-0.20f, 1.35f, 0.25f), w);
                        Arm(rUpper, rLower, rHand, P(0.28f, 1.20f, 0.05f),  P(0.20f, 1.35f, 0.25f),  w);
                    }
                    Bend(chest, (striking ? 12f : -5f) * w);
                    break;
            }
        }

        // ------------------------------------------------------------ bone helpers

        Vector3 P(float x, float y, float z) => transform.TransformPoint(new Vector3(x, y, z) * scale);

        void Arm(Transform upper, Transform lower, Transform end, Vector3 elbowTarget, Vector3 handTarget, float w)
        {
            AimBone(upper, lower.position, elbowTarget, w);
            AimBone(lower, end.position, handTarget, w);
        }

        void Leg(Vector3 kneeTarget, Vector3 footTarget, float w)
        {
            if (!rThigh || !rShin || !rFoot) return;
            AimBone(rThigh, rShin.position, kneeTarget, w);
            AimBone(rShin, rFoot.position, footTarget, w);
        }

        static void AimBone(Transform bone, Vector3 childPos, Vector3 target, float w)
        {
            if (!bone || w <= 0.001f) return;
            Vector3 from = childPos - bone.position;
            Vector3 to = target - bone.position;
            if (from.sqrMagnitude < 1e-6f || to.sqrMagnitude < 1e-6f) return;
            Quaternion q = Quaternion.FromToRotation(from, to);
            bone.rotation = Quaternion.Slerp(Quaternion.identity, q, Mathf.Clamp01(w)) * bone.rotation;
        }

        void Twist(Transform bone, float degrees)
        {
            if (bone && Mathf.Abs(degrees) > 0.01f) bone.rotation = Quaternion.AngleAxis(degrees, transform.up) * bone.rotation;
        }

        void Bend(Transform bone, float degrees)   // positive = lean forward
        {
            if (bone && Mathf.Abs(degrees) > 0.01f) bone.rotation = Quaternion.AngleAxis(degrees, transform.right) * bone.rotation;
        }
    }
}
