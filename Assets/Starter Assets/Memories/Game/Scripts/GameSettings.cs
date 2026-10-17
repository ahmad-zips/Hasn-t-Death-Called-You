using UnityEngine;
using UnityEngine.UI;

namespace BeyondTime.Game
{
    /// <summary>Saved settings (volume, fullscreen) applied at startup and by the Settings panel.</summary>
    public static class GameSettings
    {
        public static float Master { get => PlayerPrefs.GetFloat("BT_Master", 1f); set { PlayerPrefs.SetFloat("BT_Master", value); AudioListener.volume = value; } }
        public static float Music  { get => PlayerPrefs.GetFloat("BT_Music", 0.6f); set { PlayerPrefs.SetFloat("BT_Music", value); MusicPlayer.ApplyVolume(); } }
        public static bool Fullscreen { get => PlayerPrefs.GetInt("BT_Fullscreen", 1) == 1; set { PlayerPrefs.SetInt("BT_Fullscreen", value ? 1 : 0); Screen.fullScreen = value; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ApplyAtStart() { AudioListener.volume = Master; }
    }


}
