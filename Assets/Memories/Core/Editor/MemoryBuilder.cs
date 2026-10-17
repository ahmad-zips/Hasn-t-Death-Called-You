#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Memories.Building
{
    /// <summary>
    /// Tools > Beyond Time > Memories
    ///   1. Build Memory Kit        settings, placeholder shadow, MemoryUI prefab
    ///   2. Build All Memory Prefabs one prefab per memory folder
    ///   3. Build Test World        lava, islands, bridges, player, all memories, final gate
    /// Everything is written under Assets/Memories. No existing scene or asset is touched.
    /// </summary>
    public static class MemoryBuilder
    {
        const string Root = "Assets/Memories";
        const string CoreDir = Root + "/Core";
        const string PrefabDir = CoreDir + "/Prefabs";
        const string ResDir = Root + "/Resources";
        const string ShadowPath = PrefabDir + "/Shadow_PLACEHOLDER.prefab";
        const string UiPath = ResDir + "/MemoryUI.prefab";
        const string SettingsPath = ResDir + "/MemorySettings.asset";
        const string WorldPath = Root + "/Memories_TestWorld.unity";
        const string PlayerPrefabPath = "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/NestedParentArmature_Unpack.prefab";

        static readonly Color Bone = new Color(0.92f, 0.89f, 0.82f);
        static readonly Color Ash = new Color(0.62f, 0.6f, 0.58f);
        static readonly Color Ember = new Color(1f, 0.42f, 0.15f);
        static readonly Color Spirit = new Color(0.45f, 0.85f, 1f);

        // ================================================================== menu

        [MenuItem("Tools/Beyond Time/Memories/Build Everything (1 + 2 + 3)", false, 0)]
        public static void BuildEverything()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildKitInternal();
            BuildMemoriesInternal();
            BuildWorldInternal();
        }

        [MenuItem("Tools/Beyond Time/Memories/1. Build Memory Kit", false, 1)]
        public static void BuildKit()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildKitInternal();
        }

        [MenuItem("Tools/Beyond Time/Memories/2. Build All Memory Prefabs", false, 2)]
        public static void BuildMemories()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildMemoriesInternal();
        }

        [MenuItem("Tools/Beyond Time/Memories/3. Build Test World", false, 3)]
        public static void BuildWorld()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildWorldInternal();
        }

        [MenuItem("Tools/Beyond Time/Memories/Reset Saved Memory Progress", false, 30)]
        public static void ResetProgress()
        {
            foreach (var d in Definitions()) MemoryProgress.Forget(d.Id);
            PlayerPrefs.DeleteKey("BeyondTime_Ending");
            Debug.Log("Beyond Time: memory progress cleared.");
        }

        // ================================================================== 1. kit

        static void BuildKitInternal()
        {
            EnsureFolder("Assets", "Memories");
            EnsureFolder(Root, "Core");
            EnsureFolder(CoreDir, "Prefabs");
            EnsureFolder(CoreDir, "Art");
            EnsureFolder(Root, "Resources");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Placeholder shadow
            var shadow = new GameObject("Shadow_PLACEHOLDER");
            var model = new GameObject("MODEL_REPLACE_ME").transform;
            model.SetParent(shadow.transform, false);
            var body = MemoryKit.Mat("Shadow_Body", new Color(0.02f, 0.015f, 0.02f), new Color(0.12f, 0.0f, 0.02f));
            var eye = MemoryKit.Mat("Shadow_Eyes", Color.red, new Color(6f, 0.3f, 0.1f));
            MemoryKit.Prim(PrimitiveType.Capsule, "Body", new Vector3(0, 1f, 0), new Vector3(0.9f, 1f, 0.9f), body, model, false);
            MemoryKit.Prim(PrimitiveType.Sphere, "EyeL", new Vector3(-0.15f, 1.6f, 0.38f), Vector3.one * 0.12f, eye, model, false);
            MemoryKit.Prim(PrimitiveType.Sphere, "EyeR", new Vector3(0.15f, 1.6f, 0.38f), Vector3.one * 0.12f, eye, model, false);
            var cap = shadow.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0, 1f, 0); cap.height = 2f; cap.radius = 0.45f;
            shadow.AddComponent<MemoryShadow>();
            var shadowPrefab = PrefabUtility.SaveAsPrefabAsset(shadow, ShadowPath);
            UnityEngine.Object.DestroyImmediate(shadow);

            // Placeholder projectile (the Harrier's orb)
            var orb = new GameObject("Projectile_PLACEHOLDER");
            var orbModel = new GameObject("MODEL_REPLACE_ME").transform;
            orbModel.SetParent(orb.transform, false);
            MemoryKit.Prim(PrimitiveType.Sphere, "Orb", Vector3.zero, Vector3.one * 0.35f,
                           MemoryKit.Mat("Projectile_Orb", new Color(0.8f, 0.2f, 0.6f), new Color(5f, 1f, 3.5f)), orbModel, false);
            var orbLight = orb.AddComponent<Light>();
            orbLight.type = LightType.Point; orbLight.color = new Color(1f, 0.3f, 0.7f); orbLight.intensity = 4f; orbLight.range = 4f;
            orb.AddComponent<MemoryProjectile>();
            PrefabUtility.SaveAsPrefabAsset(orb, MemoryKit.ProjectilePath);
            UnityEngine.Object.DestroyImmediate(orb);

            // Settings
            var settings = AssetDatabase.LoadAssetAtPath<MemorySettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MemorySettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            if (settings.shadowPrefab == null) settings.shadowPrefab = shadowPrefab;   // never overwrite a real enemy someone already set
            EditorUtility.SetDirty(settings);

            // UI
            BuildUiPrefab();

            AssetDatabase.SaveAssets();
            Debug.Log("Beyond Time: memory kit built (settings, placeholder shadow, MemoryUI).");
        }

        // ================================================================== 2. memories

        static List<IMemoryDefinition> Definitions()
        {
            var list = new List<IMemoryDefinition>();
            foreach (var t in TypeCache.GetTypesDerivedFrom<IMemoryDefinition>())
                if (!t.IsAbstract && !t.IsInterface) list.Add((IMemoryDefinition)Activator.CreateInstance(t));
            return list.OrderBy(d => d.Folder).ToList();
        }

        static void BuildMemoriesInternal()
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(UiPath) || !AssetDatabase.LoadAssetAtPath<GameObject>(MemoryKit.ProjectilePath)) BuildKitInternal();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            int n = 0;
            foreach (var def in Definitions())
            {
                if (!AssetDatabase.IsValidFolder(def.Folder))
                {
                    string parent = System.IO.Path.GetDirectoryName(def.Folder).Replace("\\", "/");
                    AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(def.Folder));
                }
                try
                {
                    var kit = new MemoryKitAccess(def).Build();
                    kit.SaveTo(def.Folder, System.IO.Path.GetFileName(def.Folder));
                    n++;
                }
                catch (Exception e) { Debug.LogError("Beyond Time: failed to build " + def.Folder + "\n" + e); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Beyond Time: built " + n + " memory prefab(s).");
        }

        /// <summary>Small wrapper so a definition receives a kit with its own id and default size.</summary>
        class MemoryKitAccess
        {
            readonly IMemoryDefinition def;
            public MemoryKitAccess(IMemoryDefinition d) { def = d; }
            public MemoryKit Build()
            {
                var info = def as IMemoryLayout;
                float radius = info != null ? info.Radius : 14f;
                float time = info != null ? info.TimeLimit : 120f;
                string title = info != null ? info.Title : System.IO.Path.GetFileName(def.Folder);
                var kit = new MemoryKit(def.Id, title, radius, time);
                def.Build(kit);
                return kit;
            }
        }

        // ================================================================== 3. test world

        static void BuildWorldInternal()
        {
            var defs = Definitions();
            var prefabs = new List<GameObject>();
            foreach (var d in defs)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(d.Folder + "/" + System.IO.Path.GetFileName(d.Folder) + ".prefab");
                if (p && !d.Folder.Contains("Template")) prefabs.Add(p);
            }
            if (prefabs.Count == 0)
            {
                BuildMemoriesInternal();
                foreach (var d in defs)
                {
                    var p = AssetDatabase.LoadAssetAtPath<GameObject>(d.Folder + "/" + System.IO.Path.GetFileName(d.Folder) + ".prefab");
                    if (p && !d.Folder.Contains("Template")) prefabs.Add(p);
                }
                if (prefabs.Count == 0) { Debug.LogError("Beyond Time: no memory prefabs could be built. Check the Console for errors above."); return; }
            }

            BuildWorldFrom(prefabs, WorldPath);
        }

        /// <summary>Builds a playable scene for one memory only (start island, bridge, the memory, gate).</summary>
        public static void BuildSingleMemoryTest(string folder)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(UiPath)) BuildKitInternal();
            BuildMemoriesInternal();
            string name = System.IO.Path.GetFileName(folder);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/" + name + ".prefab");
            if (!prefab) { Debug.LogError("Beyond Time: could not build " + folder); return; }
            BuildWorldFrom(new List<GameObject> { prefab }, folder + "/" + name + "_Test.unity");
        }

        static void BuildWorldFrom(List<GameObject> prefabs, string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Atmosphere
            var sun = new GameObject("Directional Light", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(1f, 0.45f, 0.3f); sun.intensity = 0.55f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.16f, 0.04f, 0.02f);
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.09f, 0.07f);
            MakeVolume();

            // Lava (trigger) far below the islands
            int count = prefabs.Count;
            float spacing = 50f;
            float length = (count + 2) * spacing;
            var lava = MemoryKit.Prim(PrimitiveType.Plane, "LAVA_REPLACE_ME", new Vector3(0f, -3f, length * 0.5f - 25f),
                                      new Vector3(40f, 1f, length / 10f + 10f),
                                      MemoryKit.Mat("Lava", new Color(0.5f, 0.08f, 0.01f), new Color(3f, 0.6f, 0.05f)), null, false);
            var lavaBox = lava.AddComponent<BoxCollider>();
            lavaBox.isTrigger = true; lavaBox.center = new Vector3(0f, 0.5f, 0f); lavaBox.size = new Vector3(10f, 2f, 10f);
            var lz = lava.AddComponent<LavaZone>();

            var ground = MemoryKit.Mat("Ground_Ash", new Color(0.13f, 0.1f, 0.09f));
            var bridgeMat = MemoryKit.Mat("Bridge_Stone", new Color(0.2f, 0.17f, 0.15f));

            // Start island
            var startIsland = Island("StartIsland", new Vector3(0, 0, 0), 9f, ground);
            var spawn = new GameObject("PlayerStart").transform;
            spawn.position = new Vector3(0f, 0.1f, -4f);
            var cp = new GameObject("Checkpoint_Start", typeof(BoxCollider), typeof(MemoryCheckpoint));
            cp.transform.position = new Vector3(0f, 1.5f, -4f);
            var cpBox = cp.GetComponent<BoxCollider>(); cpBox.size = new Vector3(8f, 3f, 4f); cpBox.isTrigger = true;
            cp.GetComponent<MemoryCheckpoint>().spawnPoint = spawn;
            lz.fallbackRespawn = spawn;

            // Memories and bridges
            float prevEdge = 9f;
            var zones = new List<MemoryZone>();
            var bridges = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                float z = (i + 1) * spacing;
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i], scene);
                inst.transform.position = new Vector3(0f, 0f, z);
                var zone = inst.GetComponent<MemoryZone>();
                zones.Add(zone);

                float r = IslandRadius(inst);
                var bridge = Bridge("Bridge_" + i, prevEdge, z - r, bridgeMat);
                bridges.Add(bridge);
                prevEdge = z + r;

                var cpm = new GameObject("Checkpoint_" + (i + 1), typeof(BoxCollider), typeof(MemoryCheckpoint));
                cpm.transform.position = new Vector3(0f, 1.5f, z - r + 2f);
                var b = cpm.GetComponent<BoxCollider>(); b.size = new Vector3(6f, 3f, 2f); b.isTrigger = true;
                cpm.GetComponent<MemoryCheckpoint>().spawnPoint = zone.respawnPoint;
            }

            // Final island and gate
            float finalZ = (count + 1) * spacing;
            Island("FinalIsland", new Vector3(0, 0, finalZ), 9f, ground);
            var finalBridge = Bridge("Bridge_Final", prevEdge, finalZ - 9f, bridgeMat);
            bridges.Add(finalBridge);

            var gate = new GameObject("FinalGate", typeof(BoxCollider), typeof(MemoryFinalGate));
            gate.transform.position = new Vector3(0f, 2f, finalZ + 2f);
            var gb = gate.GetComponent<BoxCollider>(); gb.size = new Vector3(4f, 4f, 2f); gb.isTrigger = true;
            var gateVisual = Portal(gate.transform, Spirit);
            gate.GetComponent<MemoryFinalGate>().gateVisual = gateVisual;

            // Wire unlocks: memory i reveals the bridge after it
            for (int i = 0; i < zones.Count; i++)
            {
                var next = bridges[i + 1];
                next.AddComponent<BridgeReveal>();
                next.SetActive(false);
                zones[i].unlockOnComplete = new[] { next };
                PrefabUtility.RecordPrefabInstancePropertyModifications(zones[i]);
            }

            // Player
            var playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerAsset)
            {
                var rig = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset, scene);
                var arm = FindDeep(rig.transform, "PlayerArmature");
                if (arm)
                {
                    arm.position = spawn.position;
                    arm.rotation = Quaternion.identity;
                    SetupPlayerFighter(arm.gameObject);
                }
                foreach (var c in rig.GetComponentsInChildren<Camera>(true))
                    c.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }
            else Debug.LogWarning("Beyond Time: Starter Assets third-person prefab not found. Add your player (tagged Player) at PlayerStart.");

            // UI
            var ui = AssetDatabase.LoadAssetAtPath<GameObject>(UiPath);
            if (ui) PrefabUtility.InstantiatePrefab(ui, scene);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Beyond Time: test scene saved to " + scenePath + ". Press Play.");
        }

        /// <summary>Gives the Starter Assets player the full fighting kit. Safe to run on your own player too.</summary>
        public static void SetupPlayerFighter(GameObject player)
        {
            if (!player.GetComponent<CharacterController>()) { Debug.LogWarning("Beyond Time: player needs a CharacterController."); return; }
            if (!player.GetComponent<AudioSource>()) player.AddComponent<AudioSource>();
            var f = player.GetComponent<Fighter>();
            if (!f) f = player.AddComponent<Fighter>();
            FighterPresets.Player(f);
            if (!player.GetComponent<FighterPose>()) player.AddComponent<FighterPose>();
            var pb = player.GetComponent<PlayerBrawler>();
            if (!pb) pb = player.AddComponent<PlayerBrawler>();
            var test = player.GetComponent<MemoryTestCombat>();
            if (test) UnityEngine.Object.DestroyImmediate(test);

            if (!pb.repelFlash)
            {
                var l = new GameObject("RepelFlash", typeof(Light)).GetComponent<Light>();
                l.transform.SetParent(player.transform, false);
                l.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                l.type = LightType.Point; l.color = new Color(0.7f, 0.85f, 1f); l.intensity = 20f; l.range = 7f;
                l.enabled = false;
                pb.repelFlash = l;
            }
        }

        [MenuItem("Tools/Beyond Time/Memories/Add Fighting To Selected Player", false, 40)]
        static void AddFightingToSelection()
        {
            var go = Selection.activeGameObject;
            if (!go) { EditorUtility.DisplayDialog("Beyond Time", "Select your player object (the one with the CharacterController and tag Player).", "OK"); return; }
            SetupPlayerFighter(go);
            EditorUtility.SetDirty(go);
        }

        static float IslandRadius(GameObject memory)
        {
            var g = FindDeep(memory.transform, "Ground");
            return g ? g.lossyScale.x * 0.5f : 14f;
        }

        static GameObject Island(string name, Vector3 pos, float radius, Material mat)
        {
            var g = MemoryKit.Prim(PrimitiveType.Cylinder, name, pos + new Vector3(0, -0.5f, 0), new Vector3(radius * 2f, 0.5f, radius * 2f), mat, null, false);
            g.AddComponent<MeshCollider>();
            return g;
        }

        static GameObject Bridge(string name, float fromZ, float toZ, Material mat)
        {
            float len = Mathf.Max(1f, toZ - fromZ) + 1.5f;
            var root = new GameObject(name + "_REPLACE_ME");
            root.transform.position = new Vector3(0f, 0f, (fromZ + toZ) * 0.5f);
            MemoryKit.Prim(PrimitiveType.Cube, "Deck", new Vector3(0f, -0.2f, 0f), new Vector3(3.2f, 0.4f, len), mat, root.transform, true);
            MemoryKit.Prim(PrimitiveType.Cube, "RailL", new Vector3(-1.7f, 0.35f, 0f), new Vector3(0.2f, 0.7f, len), mat, root.transform, true);
            MemoryKit.Prim(PrimitiveType.Cube, "RailR", new Vector3(1.7f, 0.35f, 0f), new Vector3(0.2f, 0.7f, len), mat, root.transform, true);
            return root;
        }

        static GameObject Portal(Transform parent, Color color)
        {
            var root = new GameObject("PortalVisual");
            root.transform.SetParent(parent, false);
            var ring = MemoryKit.Mat("Portal_Ring", color * 0.4f, color * 4f);
            var disc = MemoryKit.Prim(PrimitiveType.Cylinder, "Ring", Vector3.zero, new Vector3(3.4f, 0.03f, 3.4f), ring, root.transform, false);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var l = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            l.transform.SetParent(root.transform, false); l.transform.localPosition = new Vector3(0, 0, -1f);
            l.type = LightType.Point; l.color = color; l.intensity = 8f; l.range = 12f;
            root.SetActive(false);
            return root;
        }

        static void MakeVolume()
        {
            string path = CoreDir + "/Art/HellfireVolume.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.8f); bloom.threshold.Override(0.9f);
            AssetDatabase.AddObjectToAsset(bloom, profile);
            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.38f);
            AssetDatabase.AddObjectToAsset(vig, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            var v = new GameObject("Global Volume").AddComponent<Volume>();
            v.isGlobal = true; v.sharedProfile = profile;
        }

        // ================================================================== UI prefab

        static void BuildUiPrefab()
        {
            var root = new GameObject("MemoryUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var t = root.transform;
            var ui = root.AddComponent<MemoryUI>();

            // Hit flash (bottom of the stack so text draws over it)
            var flash = Rect(t, "HitFlash", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            flash.gameObject.AddComponent<Image>().color = new Color(0.7f, 0.02f, 0f, 1f);
            flash.GetComponent<Image>().raycastTarget = false;
            ui.hitFlash = flash.gameObject.AddComponent<CanvasGroup>();
            ui.hitFlash.blocksRaycasts = false;

            // Story panel (bottom)
            var story = Rect(t, "StoryPanel", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 230));
            story.pivot = new Vector2(0.5f, 0f);
            story.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            var line = Rect(story, "EmberLine", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 3));
            line.pivot = new Vector2(0.5f, 1f);
            line.gameObject.AddComponent<Image>().color = Ember;
            ui.storyPanel = story.gameObject;
            ui.storyText = Label(story, "StoryText", "", 38, Bone, new Vector2(0, 0), new Vector2(1, 1), TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
            var st = (RectTransform)ui.storyText.transform; st.offsetMin = new Vector2(140, 30); st.offsetMax = new Vector2(-140, -30);
            var hint = Label(story, "ContinueHint", "E   continue", 22, Ember, new Vector2(1, 0), new Vector2(1, 0), TextAlignmentOptions.BottomRight, FontStyles.Bold);
            var hr = (RectTransform)hint.transform; hr.pivot = new Vector2(1, 0); hr.anchoredPosition = new Vector2(-40, 20); hr.sizeDelta = new Vector2(300, 40);
            ui.continueHint = hint.gameObject;

            // Objective (top left)
            var obj = Rect(t, "ObjectivePanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40), new Vector2(760, 130));
            obj.pivot = new Vector2(0, 1);
            obj.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            ui.objectivePanel = obj.gameObject;
            ui.titleText = Label(obj, "MemoryTitle", "MEMORY", 24, Ember, new Vector2(0, 1), new Vector2(1, 1), TextAlignmentOptions.TopLeft, FontStyles.Bold);
            var tt = (RectTransform)ui.titleText.transform; tt.pivot = new Vector2(0.5f, 1); tt.offsetMin = new Vector2(24, -52); tt.offsetMax = new Vector2(-24, -16);
            ui.objectiveText = Label(obj, "ObjectiveText", "", 30, Bone, new Vector2(0, 0), new Vector2(1, 1), TextAlignmentOptions.TopLeft, FontStyles.Normal);
            var ot = (RectTransform)ui.objectiveText.transform; ot.offsetMin = new Vector2(24, 12); ot.offsetMax = new Vector2(-24, -56);

            // Timer (top centre)
            ui.timerText = Label(t, "TimerText", "02:00", 72, Bone, new Vector2(0.5f, 1), new Vector2(0.5f, 1), TextAlignmentOptions.Center, FontStyles.Bold);
            var tr = (RectTransform)ui.timerText.transform; tr.pivot = new Vector2(0.5f, 1); tr.anchoredPosition = new Vector2(0, -30); tr.sizeDelta = new Vector2(400, 90);
            ui.penaltyText = Label(t, "PenaltyText", "-3s", 40, new Color(1f, 0.25f, 0.2f), new Vector2(0.5f, 1), new Vector2(0.5f, 1), TextAlignmentOptions.Center, FontStyles.Bold);
            var pr = (RectTransform)ui.penaltyText.transform; pr.pivot = new Vector2(0.5f, 1); pr.anchoredPosition = new Vector2(0, -120); pr.sizeDelta = new Vector2(300, 60);

            // Ledger (top right)
            ui.ledgerText = Label(t, "LedgerText", "", 24, Ash, new Vector2(1, 1), new Vector2(1, 1), TextAlignmentOptions.TopRight, FontStyles.Bold);
            var lr = (RectTransform)ui.ledgerText.transform; lr.pivot = new Vector2(1, 1); lr.anchoredPosition = new Vector2(-40, -40); lr.sizeDelta = new Vector2(400, 80);

            // Hold panel
            var hold = Rect(t, "HoldPanel", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 300), new Vector2(620, 110));
            hold.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            ui.holdPanel = hold.gameObject;
            ui.holdText = Label(hold, "HoldText", "Hold E", 30, Bone, new Vector2(0, 0.45f), new Vector2(1, 1), TextAlignmentOptions.Center, FontStyles.Bold);
            var barBg = Rect(hold, "BarBackground", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(520, 16));
            barBg.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.22f, 0.2f);
            ui.holdFill = FilledBar(barBg, "BarFill", Ember);

            // Choice panel
            var choice = Rect(t, "ChoicePanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            choice.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.06f, 0.09f, 0.78f);
            ui.choicePanel = choice.gameObject;
            var frozen = Label(choice, "TimeFrozen", "TIME STANDS STILL", 24, Spirit, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAlignmentOptions.Center, FontStyles.Bold);
            var fr = (RectTransform)frozen.transform; fr.anchoredPosition = new Vector2(0, 300); fr.sizeDelta = new Vector2(900, 40);
            ui.choicePrompt = Label(choice, "Prompt", "What do you do?", 48, Bone, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAlignmentOptions.Center, FontStyles.Bold);
            var cpr = (RectTransform)ui.choicePrompt.transform; cpr.anchoredPosition = new Vector2(0, 210); cpr.sizeDelta = new Vector2(1500, 120);
            var buttons = new List<Button>(); var labels = new List<TMP_Text>();
            for (int i = 0; i < 3; i++)
            {
                var b = MakeButton(choice, "Choice" + (i + 1), new Vector2(0, 70 - i * 115), new Vector2(1000, 96));
                buttons.Add(b); labels.Add(b.GetComponentInChildren<TMP_Text>());
            }
            ui.choiceButtons = buttons.ToArray();
            ui.choiceLabels = labels.ToArray();
            var cbg = Rect(choice, "DecideBarBackground", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(700, 10));
            cbg.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.25f);
            ui.choiceTimerFill = FilledBar(cbg, "DecideBarFill", Spirit);

            // Banner
            var banner = Rect(t, "Banner", new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 60), new Vector2(0, 300));
            banner.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            ui.banner = banner.gameObject.AddComponent<CanvasGroup>();
            ui.banner.blocksRaycasts = false;
            ui.bannerTitle = Label(banner, "Title", "MEMORY REDEEMED", 84, Bone, new Vector2(0, 0.45f), new Vector2(1, 1), TextAlignmentOptions.Center, FontStyles.Bold);
            ui.bannerBody = Label(banner, "Body", "", 32, Ash, new Vector2(0.1f, 0), new Vector2(0.9f, 0.48f), TextAlignmentOptions.Top, FontStyles.Italic);

            // Marker
            var marker = Rect(t, "ObjectiveMarker", new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(34, 34));
            var mImg = marker.gameObject.AddComponent<Image>(); mImg.color = Ember; mImg.raycastTarget = false;
            marker.localRotation = Quaternion.Euler(0, 0, 45f);
            ui.marker = marker;
            var md = Label(marker, "Distance", "0 m", 20, Bone, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAlignmentOptions.Center, FontStyles.Bold);
            var mdr = (RectTransform)md.transform; mdr.localRotation = Quaternion.Euler(0, 0, -45f); mdr.anchoredPosition = new Vector2(0, -38); mdr.sizeDelta = new Vector2(140, 30);
            ui.markerDistance = md;

            PrefabUtility.SaveAsPrefabAsset(root, UiPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        static RectTransform Rect(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = aMin; r.anchorMax = aMax; r.anchoredPosition = pos; r.sizeDelta = size;
            return r;
        }

        static TMP_Text Label(Transform parent, string name, string text, float size, Color color, Vector2 aMin, Vector2 aMax,
                              TextAlignmentOptions align, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = aMin; r.anchorMax = aMax; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color; tmp.alignment = align; tmp.fontStyle = style; tmp.raycastTarget = false;
            return tmp;
        }

        static Image FilledBar(RectTransform parent, string name, Color color)
        {
            var r = Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var r = Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var img = r.gameObject.AddComponent<Image>(); img.color = Color.white;
            var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            cb.highlightedColor = new Color(0.42f, 0.14f, 0.06f, 1f);
            cb.pressedColor = new Color(0.65f, 0.22f, 0.08f, 1f);
            cb.selectedColor = cb.normalColor;
            b.colors = cb;
            var l = Label(r, "Label", "[1]   Choice", 34, Bone, Vector2.zero, Vector2.one, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            var lr = (RectTransform)l.transform; lr.offsetMin = new Vector2(40, 0); lr.offsetMax = new Vector2(-40, 0);
            return b;
        }

        // ================================================================== misc

        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t) { var r = FindDeep(c, name); if (r) return r; }
            return null;
        }
    }

    /// <summary>Optional: memory definitions can implement this to set their title, island size and time limit.</summary>
    public interface IMemoryLayout
    {
        string Title { get; }
        float Radius { get; }
        float TimeLimit { get; }
    }
}
#endif
