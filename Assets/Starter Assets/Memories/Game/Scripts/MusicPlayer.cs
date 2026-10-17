using UnityEngine;
using UnityEngine.UI;

namespace BeyondTime.Game
{
    /// <summary>Plays this scene's music from the asset library (or the override clip), looping.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        public MusicSlot slot;
        public AudioClip overrideClip;

        static MusicPlayer current;
        AudioSource src;

        void Start()
        {
            current = this;
            src = GetComponent<AudioSource>();
            var lib = BeyondTimeAssets.Load();
            var clip = overrideClip ? overrideClip : (lib ? lib.GetMusic(slot) : null);
            if (!clip) return;
            src.clip = clip; src.loop = true; src.spatialBlend = 0f; src.ignoreListenerPause = true;
            ApplyVolume();
            src.Play();
        }

        public static void ApplyVolume() { if (current && current.src) current.src.volume = GameSettings.Music; }

        public static void PlaySting(AudioClip c)
        {
            if (!c) return;
            if (current && current.src) current.src.PlayOneShot(c, 1f);
            else AudioSource.PlayClipAtPoint(c, Camera.main ? Camera.main.transform.position : Vector3.zero);
        }
    }
}
