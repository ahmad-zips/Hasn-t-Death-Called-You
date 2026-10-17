#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Memories.Building
{
    /// <summary>One memory = one class implementing this, inside its own folder's Editor subfolder.</summary>
    public interface IMemoryDefinition
    {
        string Id { get; }          // unique, used for saving progress, e.g. "memory_01_anger"
        string Folder { get; }      // e.g. "Assets/Memories/Memory_01_Anger"
        void Build(MemoryKit kit);
    }

    /// <summary>
    /// Helpers that memory definitions use to lay out an island and write its steps.
    /// Every placeholder is named *_REPLACE_ME so artists can find and swap it.
    /// </summary>
    public class MemoryKit
    {
        public const string ArtDir = "Assets/Memories/Core/Art";

        public GameObject Root { get; private set; }
        public MemoryZone Zone { get; private set; }
        public Transform Art, Actors, Targets, Spawns, Collect, Fires, Hazards;
        public float Radius { get; private set; }

        readonly List<GameObject> fires = new List<GameObject>();

        public MemoryKit(string id, string title, float radius, float timeLimit)
        {
            Radius = radius;
            Root = new GameObject("Memory_" + title.Replace(" ", ""));
            var box = Root.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 3f, 1f);
            box.size = new Vector3(radius * 2f - 2f, 6f, radius * 2f - 4f);

            Zone = Root.AddComponent<MemoryZone>();
            Zone.memoryId = id;
            Zone.memoryTitle = title;
            Zone.timeLimit = timeLimit;

            Art = Child("ART_REPLACE_ME");
            Actors = Child("Actors");
            Targets = Child("Targets");
            Spawns = Child("SpawnPoints");
            Collect = Child("Collectibles");
            Fires = Child("FIRE_REPLACE_ME");
            Hazards = Child("StepObjects");

            // Island ground (top surface at y = 0)
            var ground = Prim(PrimitiveType.Cylinder, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(radius * 2f, 0.5f, radius * 2f),
                              Mat("Ground_Ash", new Color(0.13f, 0.1f, 0.09f)), Art, false);
            ground.AddComponent<MeshCollider>();
            var rim = Prim(PrimitiveType.Cylinder, "MoltenRim", new Vector3(0f, -0.62f, 0f), new Vector3(radius * 2f + 0.6f, 0.5f, radius * 2f + 0.6f),
                           Mat("Molten_Rim", new Color(0.3f, 0.06f, 0.02f), new Color(1.6f, 0.35f, 0.05f)), Art, false);

            // Respawn just inside the entrance (bridges arrive at -Z)
            var spawn = new GameObject("RespawnPoint").transform;
            spawn.SetParent(Root.transform, false);
            spawn.localPosition = new Vector3(0f, 0.1f, -radius + 1.5f);   // just outside the start trigger
            Zone.respawnPoint = spawn;

            // Default spawn ring
            var ring = new List<Transform>();
            for (int i = 0; i < 6; i++)
            {
                float a = (i / 6f) * Mathf.PI * 2f;
                ring.Add(Point(Spawns, "Spawn_" + i, new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius * 0.65f));
            }
            Zone.defaultSpawnPoints = ring.ToArray();
        }

        // ------------------------------------------------------------ layout

        Transform Child(string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(Root.transform, false);
            return t;
        }

        public Transform Point(string name, Vector3 local) => Point(Targets, name, local);

        public Transform Point(Transform parent, string name, Vector3 local)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = local;
            return t;
        }

        public Transform[] SpawnSet(string name, params Vector3[] locals)
        {
            var group = new GameObject(name).transform;
            group.SetParent(Spawns, false);
            var list = new Transform[locals.Length];
            for (int i = 0; i < locals.Length; i++) list[i] = Point(group, name + "_" + i, locals[i]);
            return list;
        }

        /// <summary>A person. Placeholder: coloured capsule body + head under MODEL_REPLACE_ME.</summary>
        public MemoryActor Actor(string displayName, Vector3 local, Color color, float yaw, bool lowered = false)
        {
            var root = new GameObject(displayName.Replace(" ", ""));
            root.transform.SetParent(Actors, false);
            root.transform.localPosition = local;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var model = new GameObject("MODEL_REPLACE_ME").transform;
            model.SetParent(root.transform, false);
            var mat = Mat("Actor_" + displayName.Replace(" ", ""), color, color * 0.25f);
            if (lowered)
            {
                var body = Prim(PrimitiveType.Capsule, "Body", Vector3.zero, new Vector3(0.7f, 0.55f, 0.7f), mat, model, false);
                body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                var head = Prim(PrimitiveType.Sphere, "Head", Vector3.zero, Vector3.one * 0.45f, mat, model, false);
                head.transform.localPosition = new Vector3(0f, 1.25f, 0.15f);
            }
            else
            {
                var body = Prim(PrimitiveType.Capsule, "Body", Vector3.zero, new Vector3(0.7f, 0.85f, 0.7f), mat, model, false);
                body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                var head = Prim(PrimitiveType.Sphere, "Head", Vector3.zero, Vector3.one * 0.45f, mat, model, false);
                head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            }

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, lowered ? 0.6f : 1f, 0f);
            col.height = lowered ? 1.2f : 2f;
            col.radius = 0.35f;

            // Soft light so the person reads against the dark
            var glow = new GameObject("Glow", typeof(Light));
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            var l = glow.GetComponent<Light>();
            l.type = LightType.Point; l.color = Color.Lerp(color, Color.white, 0.5f); l.intensity = 2.5f; l.range = 5f;

            var actor = root.AddComponent<MemoryActor>();
            actor.displayName = displayName;
            return actor;
        }

        public MemoryNPC Npc(string displayName, Vector3 local, Color color, float yaw)
        {
            var actor = Actor(displayName, local, color, yaw);
            return actor.gameObject.AddComponent<MemoryNPC>();
        }

        /// <summary>Pickups. Returns the root to put in a Collect step.</summary>
        public Transform Collectibles(string name, Color color, params Vector3[] locals)
        {
            var root = new GameObject(name).transform;
            root.SetParent(Collect, false);
            var mat = Mat("Pickup_" + name, color, color * 3f);
            for (int i = 0; i < locals.Length; i++)
            {
                var item = new GameObject(name + "_" + (i + 1));
                item.transform.SetParent(root, false);
                item.transform.localPosition = locals[i] + Vector3.up * 1f;
                var sc = item.AddComponent<SphereCollider>(); sc.radius = 1.1f; sc.isTrigger = true;
                var model = new GameObject("MODEL_REPLACE_ME").transform;
                model.SetParent(item.transform, false);
                var vis = Prim(PrimitiveType.Cube, "Shard", Vector3.zero, new Vector3(0.5f, 0.7f, 0.08f), mat, model, false);
                vis.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                var l = new GameObject("Glow", typeof(Light)).GetComponent<Light>();
                l.transform.SetParent(item.transform, false);
                l.type = LightType.Point; l.color = color; l.intensity = 3f; l.range = 4f;
                item.AddComponent<MemoryCollectible>();
            }
            return root;
        }

        public GameObject Prop(PrimitiveType type, string name, Vector3 local, Vector3 scale, Color color, float yaw = 0f)
        {
            var g = Prim(type, name, Vector3.zero, scale, Mat("Prop_" + ColorKey(color), color), Art, true);
            g.transform.localPosition = local;
            g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            if (type == PrimitiveType.Cylinder) { Object.DestroyImmediate(g.GetComponent<Collider>()); g.AddComponent<MeshCollider>().convex = true; }
            return g;
        }

        /// <summary>Island fire. Goes out when the memory is completed.</summary>
        public GameObject Fire(Vector3 local, float size = 1f) => MakeFire(Fires, "Fire", local, size, true);

        /// <summary>A group of fires that starts hidden; switch it on from a step's Activate list (escape sequences).</summary>
        public GameObject FireWall(string name, params Vector3[] locals)
        {
            var group = new GameObject(name);
            group.transform.SetParent(Hazards, false);
            foreach (var p in locals) MakeFire(group.transform, "Fire", p, 1.3f, false);
            group.SetActive(false);
            fires.Add(group);
            return group;
        }

        GameObject MakeFire(Transform parent, string name, Vector3 local, float size, bool track)
        {
            var root = new GameObject(name + "_REPLACE_ME");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = local;
            var core = Mat("Fire_Core", new Color(1f, 0.45f, 0.1f), new Color(4f, 1.2f, 0.2f));
            var outer = Mat("Fire_Outer", new Color(0.6f, 0.08f, 0.02f), new Color(2f, 0.25f, 0.03f));
            Prim(PrimitiveType.Sphere, "Flame", Vector3.zero, new Vector3(0.6f, 1.1f, 0.6f) * size, core, root.transform, false).transform.localPosition = new Vector3(0f, 0.55f * size, 0f);
            Prim(PrimitiveType.Sphere, "Glow", Vector3.zero, new Vector3(1f, 0.6f, 1f) * size, outer, root.transform, false).transform.localPosition = new Vector3(0f, 0.25f * size, 0f);
            var l = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            l.transform.SetParent(root.transform, false);
            l.transform.localPosition = new Vector3(0f, 1.2f * size, 0f);
            l.type = LightType.Point; l.color = new Color(1f, 0.45f, 0.15f); l.range = 7f * size; l.intensity = 4f;
            l.gameObject.AddComponent<MemoryFireFlicker>();
            if (track) fires.Add(root);
            return root;
        }

        // ------------------------------------------------------------ fighters (real NPCs)

        public const string ArmaturePath = "Assets/Starter Assets/Runtime/ThirdPersonController/Character/Models/Armature.fbx";
        public const string ControllerPath = "Assets/Starter Assets/Runtime/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller";

        /// <summary>
        /// A real NPC: humanoid body (Starter Assets armature with idle/walk/run), tinted clothes,
        /// Fighter with a preset, FighterPose for punches and kicks, BrawlerAI brain, floating health bar.
        /// TO REPLACE THE LOOK: swap MODEL_REPLACE_ME for any humanoid model with an Animator.
        /// </summary>
        public BrawlerAI Brawler(string displayName, Vector3 local, float yaw, BrawlerAI.Role role, Color clothes,
                                 System.Action<Fighter> preset, bool alwaysShowBar = false)
        {
            var root = FighterBody(displayName, local, yaw, clothes, null, preset, 1f,
                                   role == BrawlerAI.Role.Victim ? new Color(0.45f, 0.85f, 0.5f) : new Color(0.85f, 0.18f, 0.12f), alwaysShowBar);
            var ai = root.AddComponent<BrawlerAI>();
            ai.role = role;
            return ai;
        }

        /// <summary>
        /// A shadow creature: the same animated humanoid body, pitch black with a dark red glow and burning eyes.
        /// Scale above 1 makes a giant (the Gatekeeper).
        /// </summary>
        public BrawlerAI ShadowBrawler(string displayName, Vector3 local, float yaw, System.Action<Fighter> preset, float scale = 1f)
        {
            var root = FighterBody(displayName, local, yaw, new Color(0.02f, 0.015f, 0.02f), new Color(0.25f, 0.02f, 0.03f),
                                   preset, scale, new Color(0.85f, 0.18f, 0.12f), false);
            ShadowEyes(root);
            var ai = root.AddComponent<BrawlerAI>();
            ai.role = BrawlerAI.Role.Guard;
            return ai;
        }

        /// <summary>A ranged shadow that throws projectiles (the Harrier).</summary>
        public RangedAI Harrier(string displayName, Vector3 local, float yaw)
        {
            var root = FighterBody(displayName, local, yaw, new Color(0.03f, 0.01f, 0.04f), new Color(0.3f, 0.03f, 0.25f),
                                   FighterPresets.Harrier, 1f, new Color(0.85f, 0.18f, 0.12f), false);
            ShadowEyes(root);
            var ai = root.AddComponent<RangedAI>();
            ai.projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePath);

            var glow = new GameObject("HAND_GLOW", typeof(Light));
            glow.transform.SetParent(root.transform, false);
            var l = glow.GetComponent<Light>();
            l.type = LightType.Point; l.color = new Color(1f, 0.3f, 0.7f); l.intensity = 6f; l.range = 3f; l.enabled = false;
            var att = glow.AddComponent<BoneAttachment>();
            att.localPosition = Vector3.zero; att.localEuler = Vector3.zero;
            root.GetComponent<Fighter>().telegraphLight = l;
            return ai;
        }

        public const string ProjectilePath = "Assets/Memories/Core/Prefabs/Projectile_PLACEHOLDER.prefab";

        GameObject FighterBody(string displayName, Vector3 local, float yaw, Color clothes, Color? emission,
                               System.Action<Fighter> preset, float scale, Color barColor, bool alwaysShowBar)
        {
            var root = new GameObject(displayName.Replace(" ", ""));
            root.transform.SetParent(Actors, false);
            root.transform.localPosition = local + Vector3.up * 0.05f;
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            root.transform.localScale = Vector3.one * scale;

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.center = new Vector3(0f, 0.93f, 0f); cc.radius = 0.28f; cc.skinWidth = 0.02f; cc.stepOffset = 0.3f;

            var mat = Mat("Body_" + ColorKey(clothes) + (emission.HasValue ? "_glow" : ""), clothes, emission);
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ArmaturePath);
            if (modelAsset)
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                model.name = "MODEL_REPLACE_ME";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;

                var animator = model.GetComponent<Animator>();
                if (!animator) animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                foreach (var r in model.GetComponentsInChildren<Renderer>(true))
                {
                    var arr = new Material[Mathf.Max(1, r.sharedMaterials.Length)];
                    for (int i = 0; i < arr.Length; i++) arr[i] = mat;
                    r.sharedMaterials = arr;
                }
                model.AddComponent<FighterAnimEvents>();
            }
            else
            {
                Debug.LogWarning("Beyond Time: Starter Assets armature not found; " + displayName + " uses a capsule placeholder.");
                var holder = new GameObject("MODEL_REPLACE_ME").transform;
                holder.SetParent(root.transform, false);
                Prim(PrimitiveType.Capsule, "Body", new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f), mat, holder, false);
            }

            root.AddComponent<AudioSource>().spatialBlend = 1f;
            var fighter = root.AddComponent<Fighter>();
            preset(fighter);
            fighter.displayName = displayName;
            root.AddComponent<FighterPose>();
            HealthBar(root.transform, fighter, alwaysShowBar, barColor, scale);
            return root;
        }

        void ShadowEyes(GameObject root)
        {
            var model = root.transform.Find("MODEL_REPLACE_ME");
            var parent = model ? model : root.transform;
            var eye = Mat("Shadow_Eyes", Color.red, new Color(6f, 0.4f, 0.1f));
            Prim(PrimitiveType.Sphere, "EyeL", new Vector3(-0.045f, 1.62f, 0.11f), Vector3.one * 0.035f, eye, parent, false);
            Prim(PrimitiveType.Sphere, "EyeR", new Vector3(0.045f, 1.62f, 0.11f), Vector3.one * 0.035f, eye, parent, false);
        }

        /// <summary>A knife in the right hand. Glows red during unblockable stabs.</summary>
        public GameObject Knife(BrawlerAI owner)
        {
            var knife = new GameObject("KNIFE_REPLACE_ME");
            knife.transform.SetParent(owner.transform, false);
            knife.transform.localPosition = new Vector3(0.3f, 1f, 0.2f);
            var steel = Mat("Knife_Steel", new Color(0.75f, 0.76f, 0.8f), new Color(0.15f, 0.15f, 0.17f));
            var grip = Mat("Knife_Grip", new Color(0.08f, 0.06f, 0.05f));
            Prim(PrimitiveType.Cube, "Blade", new Vector3(0f, 0.17f, 0f), new Vector3(0.03f, 0.22f, 0.008f), steel, knife.transform, false);
            Prim(PrimitiveType.Cube, "Grip", new Vector3(0f, 0.02f, 0f), new Vector3(0.035f, 0.1f, 0.03f), grip, knife.transform, false);
            knife.AddComponent<BoneAttachment>();

            var glow = new GameObject("TelegraphLight", typeof(Light)).GetComponent<Light>();
            glow.transform.SetParent(knife.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            glow.type = LightType.Point; glow.color = new Color(1f, 0.1f, 0.05f); glow.intensity = 6f; glow.range = 3f;
            glow.enabled = false;
            owner.GetComponent<Fighter>().telegraphLight = glow;
            return knife;
        }

        void HealthBar(Transform owner, Fighter fighter, bool always, Color color, float scale = 1f)
        {
            var go = new GameObject("HealthBar", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            go.transform.SetParent(owner, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(120f, 34f);
            rt.localPosition = new Vector3(0f, 2.25f, 0f);
            rt.localScale = Vector3.one * (0.01f / Mathf.Max(0.01f, scale));

            var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            MakeBar(go.transform, sprite, "Background", new Vector2(0f, -4f), new Vector2(110f, 12f), new Color(0f, 0f, 0f, 0.7f), false);
            var fill = MakeBar(go.transform, sprite, "Fill", new Vector2(0f, -4f), new Vector2(106f, 8f), color, true);
            var guardFill = MakeBar(go.transform, sprite, "Guard", new Vector2(0f, -12f), new Vector2(106f, 3f), new Color(0.75f, 0.8f, 0.9f), true);

            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(go.transform, false);
            var nr = (RectTransform)nameObj.transform; nr.anchoredPosition = new Vector2(0f, 10f); nr.sizeDelta = new Vector2(200f, 18f);
            var tmp = nameObj.GetComponent<TextMeshProUGUI>();
            tmp.text = fighter.displayName; tmp.fontSize = 14f; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; tmp.raycastTarget = false;

            var bar = go.AddComponent<FighterHealthBar>();
            bar.fighter = fighter; bar.fill = fill; bar.guardFill = guardFill; bar.nameText = tmp;
            bar.group = go.GetComponent<CanvasGroup>(); bar.group.alpha = always ? 1f : 0f;
            bar.alwaysVisible = always;
            bar.height = 2.25f * scale;
        }

        // ------------------------------------------------------------ siege pieces

        /// <summary>Fire that bursts on and off across a path. Width along X, depth along Z (local to the island).</summary>
        public FireHazard FireHazardAt(string name, Vector3 local, Vector3 size, float phase, float onTime = 1.5f, float offTime = 2.3f)
        {
            var go = new GameObject(name + "_REPLACE_ME_FX");
            go.transform.SetParent(Hazards, false);
            go.transform.localPosition = local + Vector3.up * size.y * 0.5f;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true; box.size = size;

            var flames = new GameObject("Flames").transform;
            flames.SetParent(go.transform, false);
            var core = Mat("Fire_Core", new Color(1f, 0.45f, 0.1f), new Color(4f, 1.2f, 0.2f));
            int n = Mathf.Max(2, Mathf.RoundToInt(size.x / 0.8f));
            for (int i = 0; i < n; i++)
            {
                float x = Mathf.Lerp(-size.x * 0.4f, size.x * 0.4f, n == 1 ? 0.5f : i / (float)(n - 1));
                var f = Prim(PrimitiveType.Sphere, "Flame" + i, new Vector3(x, -size.y * 0.5f + 0.7f, 0f), new Vector3(0.6f, 1.4f, 0.6f), core, flames, false);
            }
            var l = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            l.transform.SetParent(flames, false); l.transform.localPosition = Vector3.up * 0.5f;
            l.type = LightType.Point; l.color = new Color(1f, 0.45f, 0.15f); l.intensity = 6f; l.range = 6f;

            var warning = new GameObject("Warning").transform;
            warning.SetParent(go.transform, false);
            var ember = Mat("Fire_Ember", new Color(0.6f, 0.1f, 0.02f), new Color(2f, 0.3f, 0.05f));
            for (int i = 0; i < n; i++)
            {
                float x = Mathf.Lerp(-size.x * 0.4f, size.x * 0.4f, n == 1 ? 0.5f : i / (float)(n - 1));
                Prim(PrimitiveType.Sphere, "Ember" + i, new Vector3(x, -size.y * 0.5f + 0.15f, 0f), Vector3.one * 0.25f, ember, warning, false);
            }

            var hz = go.AddComponent<FireHazard>();
            hz.flames = flames.gameObject; hz.warning = warning.gameObject;
            hz.phase = phase; hz.onTime = onTime; hz.offTime = offTime;
            return hz;
        }

        /// <summary>An empty pivot with a MemoryMover; put the visible parts under it.</summary>
        public MemoryMover Mover(string name, Transform parent, Vector3 local, Vector3 openOffset, Vector3 openEuler, float duration)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            var m = go.AddComponent<MemoryMover>();
            m.openOffset = openOffset; m.openEuler = openEuler; m.duration = duration;
            return m;
        }

        /// <summary>A sloped ramp from one point to another (both local, on the ground surface).</summary>
        public GameObject Ramp(string name, Transform parent, Vector3 from, Vector3 to, float width, Color color)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            var g = Prim(PrimitiveType.Cube, name, Vector3.zero, new Vector3(width, 0.3f, len), Mat("Prop_" + ColorKey(color), color), parent, true);
            g.transform.localPosition = (from + to) * 0.5f - Vector3.up * 0.15f;
            g.transform.localRotation = Quaternion.LookRotation(d.normalized, Vector3.up);
            return g;
        }

        /// <summary>A hidden group of fires (for MemoryZone pressure / final objects).</summary>
        public GameObject FireGroup(string name, float size, params Vector3[] locals)
        {
            var group = new GameObject(name);
            group.transform.SetParent(Hazards, false);
            foreach (var p in locals) MakeFire(group.transform, "Fire", p, size, false);
            group.SetActive(false);
            return group;
        }

        /// <summary>Box prop placed in a named section group (keeps environment replaceable by area).</summary>
        public GameObject Block(Transform section, string name, Vector3 local, Vector3 scale, Color color, float yaw = 0f)
        {
            var g = Prim(PrimitiveType.Cube, name, Vector3.zero, scale, Mat("Prop_" + ColorKey(color), color), section, true);
            g.transform.localPosition = local;
            g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return g;
        }

        public Transform Section(string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(Art, false);
            return t;
        }

        static Image MakeBar(Transform parent, Sprite sprite, string n, Vector2 pos, Vector2 size, Color c, bool filled)
        {
            var r = new GameObject(n, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchoredPosition = pos; r.sizeDelta = size;
            var img = r.GetComponent<Image>();
            img.color = c; img.raycastTarget = false;
            if (filled) { img.sprite = sprite; img.type = Image.Type.Filled; img.fillMethod = Image.FillMethod.Horizontal; img.fillAmount = 1f; }
            return img;
        }

        public MemoryStep DefeatFighters(string objective, string story, Fighter protect, BrawlerAI[] engage, params Fighter[] fighters)
        {
            var s = Add(StepType.DefeatFighters, "Defeat fighters", story, objective);
            s.fighters = fighters; s.protect = protect; s.engageOnStart = engage;
            return s;
        }

        // ------------------------------------------------------------ steps

        public int NextIndex => Zone.steps.Count;

        MemoryStep Add(StepType type, string label, string story, string objective)
        {
            var s = new MemoryStep { type = type, label = Zone.steps.Count + " " + label, story = story, objective = objective };
            Zone.steps.Add(s);
            return s;
        }

        public MemoryStep Narrate(string story) => Add(StepType.Narrate, "Narrate", story, "");

        public MemoryStep Defeat(string objective, string story, int shadows, Transform[] points = null)
        {
            var s = Add(StepType.DefeatShadows, "Defeat", story, objective);
            s.shadowsOnStart = shadows; s.spawnPoints = points;
            return s;
        }

        public MemoryStep Reach(Transform target, float radius, string objective, string story, int shadows = 0, Transform[] points = null)
        {
            var s = Add(StepType.ReachPoint, "Reach " + target.name, story, objective);
            s.target = target; s.radius = radius; s.shadowsOnStart = shadows; s.spawnPoints = points;
            return s;
        }

        public MemoryStep Hold(Transform target, float radius, float seconds, string holdText, string objective, string story)
        {
            var s = Add(StepType.HoldInteract, "Hold E at " + target.name, story, objective);
            s.target = target; s.radius = radius; s.holdTime = seconds; s.holdText = holdText;
            return s;
        }

        public MemoryStep Survive(float seconds, string objective, string story, int keepAlive, Transform stayNear = null, float radius = 4f)
        {
            var s = Add(StepType.SurviveTime, "Survive " + seconds + "s", story, objective);
            s.surviveSeconds = seconds; s.keepShadowsAlive = keepAlive;
            if (stayNear) { s.target = stayNear; s.mustStayNearTarget = true; s.radius = radius; }
            return s;
        }

        public MemoryStep Escort(MemoryNPC npc, Transform destination, float radius, string objective, string story, int shadows)
        {
            var s = Add(StepType.Escort, "Escort " + npc.name, story, objective);
            s.escortNpc = npc; s.target = destination; s.radius = radius; s.shadowsOnStart = shadows;
            return s;
        }

        public MemoryStep CollectStep(Transform root, string objective, string story, int shadows)
        {
            var s = Add(StepType.Collect, "Collect " + root.name, story, objective);
            s.collectRoot = root; s.shadowsOnStart = shadows;
            return s;
        }

        public MemoryStep Choice(string prompt, float timeout, int defaultChoice, params MemoryChoice[] choices)
        {
            var s = Add(StepType.Choice, "Choice", "", "Choose");
            s.choicePrompt = prompt; s.choiceTimeout = timeout; s.defaultChoice = defaultChoice; s.choices = choices;
            return s;
        }

        public static MemoryChoice Right(string label, string result, bool endMemory = true, int goToStep = -1) =>
            new MemoryChoice { label = label, resultText = result, righteous = true, endMemory = endMemory, goToStep = goToStep };

        public static MemoryChoice Wrong(string label, string result, float penalty, int extraShadows, int goToStep = -1) =>
            new MemoryChoice { label = label, resultText = result, righteous = false, timePenalty = penalty, extraShadows = extraShadows, goToStep = goToStep };

        public void Endings(string redeemed, string condemned)
        {
            Zone.redeemedText = redeemed;
            Zone.condemnedText = condemned;
        }

        // ------------------------------------------------------------ save

        public GameObject SaveTo(string folder, string fileName)
        {
            Zone.disableOnComplete = fires.ToArray();      // island fires and fire walls all go out when the memory is done
            string path = folder + "/" + fileName + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(Root, path);
            Object.DestroyImmediate(Root);
            return prefab;
        }

        // ------------------------------------------------------------ primitives and materials

        public static GameObject Prim(PrimitiveType type, string name, Vector3 local, Vector3 scale, Material mat, Transform parent, bool keepCollider)
        {
            var g = GameObject.CreatePrimitive(type);
            g.name = name;
            if (parent) g.transform.SetParent(parent, false);
            g.transform.localPosition = local;
            g.transform.localScale = scale;
            if (mat) g.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider) Object.DestroyImmediate(g.GetComponent<Collider>());
            return g;
        }

        static string ColorKey(Color c) => ColorUtility.ToHtmlStringRGB(c);

        public static Material Mat(string name, Color baseColor, Color? emission = null)
        {
            if (!AssetDatabase.IsValidFolder(ArtDir)) AssetDatabase.CreateFolder("Assets/Memories/Core", "Art");
            string path = ArtDir + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader != null ? shader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", baseColor);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
#endif
