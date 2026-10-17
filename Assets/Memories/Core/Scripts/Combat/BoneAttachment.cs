using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>Snaps this object (for example a knife) into a humanoid bone when the game starts.</summary>
    public class BoneAttachment : MonoBehaviour
    {
        public HumanBodyBones bone = HumanBodyBones.RightHand;
        public Vector3 localPosition = new Vector3(0f, 0.08f, 0.03f);
        public Vector3 localEuler = new Vector3(0f, 0f, 90f);

        void Start()
        {
            Animator anim = null;
            var fighter = GetComponentInParent<Fighter>();
            if (fighter) anim = fighter.GetComponentInChildren<Animator>();
            if (!anim) anim = GetComponentInParent<Animator>();
            if (!anim || !anim.isHuman) return;

            var b = anim.GetBoneTransform(bone);
            if (!b) return;
            transform.SetParent(b, false);
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.Euler(localEuler);
        }
    }
}
