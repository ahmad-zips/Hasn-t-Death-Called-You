using UnityEngine;

namespace BeyondTime.Game
{
    /// <summary>Uses the hell or paradise skybox from the asset library (or the override) when the scene starts.</summary>
    public class SkyboxFromLibrary : MonoBehaviour
    {
        public enum Which { Hell, Paradise }
        public Which sky = Which.Hell;
        public Material overrideSkybox;

        void Awake()
        {
            var lib = BeyondTimeAssets.Load();
            var m = overrideSkybox ? overrideSkybox : (lib ? (sky == Which.Hell ? lib.hellSkybox : lib.paradiseSkybox) : null);
            if (!m) return;
            RenderSettings.skybox = m;
            foreach (var c in Camera.allCameras) c.clearFlags = CameraClearFlags.Skybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
