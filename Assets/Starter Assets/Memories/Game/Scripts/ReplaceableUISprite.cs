using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Game
{
    /// <summary>Swaps this Image's sprite for the matching UI slot in BeyondTimeAssets (or the override).</summary>
    [RequireComponent(typeof(Image))]
    public class ReplaceableUISprite : MonoBehaviour
    {
        public UISlot slot;
        public Sprite overrideSprite;
        public bool preserveAspect;
        [Tooltip("Hide this image entirely when no sprite is provided (for example the logo).")]
        public bool hideIfNoSprite;
        [Tooltip("Hidden when a sprite IS provided (for example the text title behind the logo).")]
        public GameObject[] hideWhenReplaced;

        void Awake()
        {
            var lib = BeyondTimeAssets.Load();
            var s = overrideSprite ? overrideSprite : (lib ? lib.GetSprite(slot) : null);
            if (!s) { if (hideIfNoSprite) gameObject.SetActive(false); return; }
            if (hideWhenReplaced != null) foreach (var g in hideWhenReplaced) if (g) g.SetActive(false);
            var img = GetComponent<Image>();
            img.sprite = s;
            img.color = Color.white;
            img.preserveAspect = preserveAspect;
            img.type = s.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        }
    }

}
