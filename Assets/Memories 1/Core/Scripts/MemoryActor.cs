using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// A person inside a memory (brother, friend, old man ...).
    /// TO REPLACE THE LOOK: drop your character model as a child of this object and delete MODEL_REPLACE_ME.
    /// The Animator is found automatically. Type your animation state names into the steps (Actor State).
    /// </summary>
    public class MemoryActor : MonoBehaviour
    {
        public string displayName = "Person";
        public Animator animator;                       // empty = first Animator in children
        public string idleState = "";                   // played on start if set

        void Awake() { FindAnimator(); }
        void Start() { Play(idleState); }

        void FindAnimator() { if (!animator) animator = GetComponentInChildren<Animator>(); }

        public void Play(string state)
        {
            if (string.IsNullOrEmpty(state)) return;
            FindAnimator();
            if (!animator) return;
            int hash = Animator.StringToHash(state);
            if (animator.HasState(0, hash)) animator.CrossFade(hash, 0.15f);
            else Debug.LogWarning("MemoryActor '" + displayName + "': Animator has no state named '" + state + "'.", this);
        }

        public void Face(Vector3 worldPoint)
        {
            Vector3 d = worldPoint - transform.position; d.y = 0f;
            if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d);
        }
    }
}
