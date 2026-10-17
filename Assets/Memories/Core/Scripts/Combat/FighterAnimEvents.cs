using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Receives the footstep and landing events baked into the Starter Assets animations,
    /// so NPCs using those animations do not spam "AnimationEvent has no receiver" errors.
    /// Put on the object that has the Animator. Optional footstep sounds.
    /// </summary>
    public class FighterAnimEvents : MonoBehaviour
    {
        public AudioClip[] footsteps;
        [Range(0f, 1f)] public float volume = 0.4f;

        void OnFootstep(AnimationEvent e)
        {
            if (e.animatorClipInfo.weight < 0.5f || footsteps == null || footsteps.Length == 0) return;
            AudioSource.PlayClipAtPoint(footsteps[Random.Range(0, footsteps.Length)], transform.position, volume);
        }

        void OnLand(AnimationEvent e) { }
    }
}
