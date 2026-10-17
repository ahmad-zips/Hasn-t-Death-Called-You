using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Plays the grave video, then loads the hellfire world. Hold Space, E or Esc for 1 second to skip.
    /// Setup: empty scene, a Camera, an object with Video Player (Render Mode = Camera Far Plane, Camera = your camera,
    /// Video Clip = the grave video, untick Loop) and this script. Add the scene to Build Profiles before the world.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class IntroVideo : MonoBehaviour
    {
        public string nextScene = "Memories_TestWorld";
        public bool allowSkip = true;
        public float skipHoldSeconds = 1f;

        VideoPlayer player;
        bool loading;
        float held;

        void Start()
        {
            player = GetComponent<VideoPlayer>();
            player.isLooping = false;
            player.loopPointReached += OnVideoEnd;
            player.Play();
        }

        void OnDestroy() { if (player) player.loopPointReached -= OnVideoEnd; }

        void OnVideoEnd(VideoPlayer source) { Next(); }

        void Update()
        {
            if (!allowSkip || loading) return;
            var k = Keyboard.current;
            bool holding = k != null && (k.spaceKey.isPressed || k.eKey.isPressed || k.escapeKey.isPressed);
            held = holding ? held + Time.unscaledDeltaTime : 0f;
            if (held >= skipHoldSeconds) Next();
        }

        void Next()
        {
            if (loading) return;
            loading = true;
            if (Application.CanStreamedLevelBeLoaded(nextScene)) SceneManager.LoadScene(nextScene);
            else Debug.LogError("IntroVideo: scene '" + nextScene + "' is not in Build Profiles.");
        }
    }
}
