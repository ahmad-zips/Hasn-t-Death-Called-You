using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace BeyondTime.Game
{
    /// <summary>
    /// Plays the grave intro video, then loads the hellfire world.
    /// VIDEO SLOT: drag your clip into Video Clip here, or into BeyondTimeAssets > Intro Video.
    /// With no clip, a placeholder title card plays for Placeholder Seconds instead.
    /// Hold Space, E or Esc to skip.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class GraveIntro : MonoBehaviour
    {
        public VideoClip videoClip;
        public float placeholderSeconds = 6f;
        public GameObject placeholderCard;
        public Image placeholderProgress;
        public TMP_Text skipHint;
        public float skipHoldSeconds = 1f;
        public Image skipFill;

        VideoPlayer player;
        bool loading, placeholderMode;
        float held, started;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            player = GetComponent<VideoPlayer>();
            var lib = BeyondTimeAssets.Load();
            var clip = videoClip ? videoClip : (lib ? lib.introVideo : null);
            started = Time.time;

            if (clip)
            {
                placeholderMode = false;
                if (placeholderCard) placeholderCard.SetActive(false);
                player.clip = clip;
                player.isLooping = false;
                player.loopPointReached += _ => Next();
                player.Play();
            }
            else
            {
                placeholderMode = true;
                player.enabled = false;
                if (placeholderCard) placeholderCard.SetActive(true);
            }
        }

        void Update()
        {
            if (loading) return;
            if (placeholderMode)
            {
                float t = (Time.time - started) / Mathf.Max(0.1f, placeholderSeconds);
                if (placeholderProgress) placeholderProgress.fillAmount = Mathf.Clamp01(t);
                if (t >= 1f) { Next(); return; }
            }

            var k = Keyboard.current;
            bool holding = k != null && (k.spaceKey.isPressed || k.eKey.isPressed || k.escapeKey.isPressed);
            held = holding ? held + Time.unscaledDeltaTime : 0f;
            if (skipFill) skipFill.fillAmount = held / skipHoldSeconds;
            if (held >= skipHoldSeconds) Next();
        }

        void Next()
        {
            if (loading) return;
            loading = true;
            GameScore.CameFromMenu = true;
            SceneFlow.Load(SceneFlow.World);
        }
    }
}
