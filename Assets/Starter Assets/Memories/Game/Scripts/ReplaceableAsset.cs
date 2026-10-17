using UnityEngine;

namespace BeyondTime.Game
{
    /// <summary>
    /// Sits on every placeholder you can swap. When the game starts it looks for a replacement:
    ///   1. Override Prefab / Override Material on THIS component (replaces only this object), else
    ///   2. the matching slot in Resources/BeyondTimeAssets.asset (replaces every object of this kind).
    /// A prefab replaces the placeholder parts. A material repaints them. Nothing found = placeholder stays.
    /// Collisions stay on the placeholder by default, so gameplay never breaks.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ReplaceableAsset : MonoBehaviour
    {
        public AssetSlot slot;

        [Header("Replace only this object (optional)")]
        public GameObject overridePrefab;
        public Material overrideMaterial;

        [Header("How the replacement is placed")]
        public Vector3 offset = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
        [Tooltip("Stretch the replacement to the size of the placeholder (bridges, lava, walls).")]
        public bool fitToPlaceholder;
        [Tooltip("Keep the placeholder's invisible colliders. Turn off only if your prefab has its own colliders.")]
        public bool keepPlaceholderColliders = true;

        [Tooltip("Animator Controller used for a character model that has none (set by the builder to the Starter Assets controller).")]
        public RuntimeAnimatorController fallbackController;

        [Header("Placeholder parts (hidden when a prefab replaces them)")]
        public GameObject[] placeholderParts;

        public bool Replaced { get; private set; }
        public GameObject Replacement { get; private set; }

        void Awake() { Apply(); }

        public void Apply()
        {
            if (Replaced) return;
            var lib = BeyondTimeAssets.Load();
            GameObject prefab = overridePrefab ? overridePrefab : (lib ? lib.GetPrefab(slot) : null);
            Material mat = overrideMaterial ? overrideMaterial : (lib ? lib.GetMaterial(slot) : null);

            if (prefab) Swap(prefab, lib);
            else if (mat) Repaint(mat);
        }

        void Swap(GameObject prefab, BeyondTimeAssets lib)
        {
            bool character = BeyondTimeAssets.IsCharacter(slot);
            Bounds target = default(Bounds);
            bool fit = fitToPlaceholder && TryPlaceholderBounds(out target);

            RuntimeAnimatorController oldController = null;
            foreach (var part in Parts())
            {
                var a = part.GetComponentInChildren<Animator>(true);
                if (a && a.runtimeAnimatorController) { oldController = a.runtimeAnimatorController; break; }
            }

            var inst = Instantiate(prefab, transform);
            inst.name = prefab.name + "_REPLACEMENT";
            inst.transform.localPosition = offset;
            inst.transform.localRotation = Quaternion.Euler(rotation);
            inst.transform.localScale = scale;

            if (character)
            {
                var anim = inst.GetComponentInChildren<Animator>();
                if (!anim) anim = inst.AddComponent<Animator>();
                if (!anim.runtimeAnimatorController)
                {
                    var c = lib ? lib.GetController(slot) : null;
                    anim.runtimeAnimatorController = c ? c : (oldController ? oldController : fallbackController);
                }
                anim.applyRootMotion = false;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (!anim.GetComponent<BeyondTime.Memories.FighterAnimEvents>()) anim.gameObject.AddComponent<BeyondTime.Memories.FighterAnimEvents>();
            }

            foreach (var part in Parts())
            {
                if (character || !keepPlaceholderColliders) { part.SetActive(false); continue; }
                foreach (var r in part.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                foreach (var l in part.GetComponentsInChildren<Light>(true)) l.enabled = false;
            }

            if (fit) Fit(inst, target);
            Replacement = inst;
            Replaced = true;
        }

        void Repaint(Material mat)
        {
            foreach (var part in Parts())
                foreach (var r in part.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.gameObject.name.StartsWith("Eye")) continue;     // keep the glowing eyes on shadows
                    var arr = r.sharedMaterials;
                    for (int i = 0; i < arr.Length; i++) arr[i] = mat;
                    r.sharedMaterials = arr;
                }
        }

        GameObject[] Parts()
        {
            if (placeholderParts != null && placeholderParts.Length > 0) return placeholderParts;
            return new[] { gameObject };
        }

        bool TryPlaceholderBounds(out Bounds b)
        {
            b = default(Bounds);
            bool any = false;
            foreach (var part in Parts())
                foreach (var r in part.GetComponentsInChildren<Renderer>())
                {
                    if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
                }
            return any;
        }

        static void Fit(GameObject inst, Bounds target)
        {
            var rends = inst.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return;
            Bounds own = rends[0].bounds;
            foreach (var r in rends) own.Encapsulate(r.bounds);
            Vector3 s = inst.transform.localScale;
            inst.transform.localScale = new Vector3(
                own.size.x > 0.001f ? s.x * target.size.x / own.size.x : s.x,
                own.size.y > 0.001f ? s.y * target.size.y / own.size.y : s.y,
                own.size.z > 0.001f ? s.z * target.size.z / own.size.z : s.z);
            rends = inst.GetComponentsInChildren<Renderer>();
            own = rends[0].bounds;
            foreach (var r in rends) own.Encapsulate(r.bounds);
            inst.transform.position += target.center - own.center;
        }
    }
}
