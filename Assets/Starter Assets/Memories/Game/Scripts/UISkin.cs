using UnityEngine;
using TMPro;

namespace BeyondTime.Game
{
    /// <summary>Put on a Canvas: applies the library font to every text under it.</summary>
    public class UISkin : MonoBehaviour
    {
        public TMP_FontAsset overrideFont;

        void Awake()
        {
            var lib = BeyondTimeAssets.Load();
            var f = overrideFont ? overrideFont : (lib ? lib.font : null);
            if (!f) return;
            foreach (var t in GetComponentsInChildren<TMP_Text>(true)) t.font = f;
        }
    }
}
