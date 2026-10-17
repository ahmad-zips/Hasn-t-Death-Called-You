using UnityEngine;
using UnityEngine.UI;

namespace BeyondTime.Game
{
    /// <summary>Hook up sliders and the toggle; values load and save themselves.</summary>
    public class SettingsPanel : MonoBehaviour
    {
        public Slider masterSlider;
        public Slider musicSlider;
        public Toggle fullscreenToggle;

        void OnEnable()
        {
            if (masterSlider) { masterSlider.SetValueWithoutNotify(GameSettings.Master); masterSlider.onValueChanged.RemoveAllListeners(); masterSlider.onValueChanged.AddListener(v => GameSettings.Master = v); }
            if (musicSlider) { musicSlider.SetValueWithoutNotify(GameSettings.Music); musicSlider.onValueChanged.RemoveAllListeners(); musicSlider.onValueChanged.AddListener(v => GameSettings.Music = v); }
            if (fullscreenToggle) { fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen); fullscreenToggle.onValueChanged.RemoveAllListeners(); fullscreenToggle.onValueChanged.AddListener(v => GameSettings.Fullscreen = v); }
        }
    }
}
