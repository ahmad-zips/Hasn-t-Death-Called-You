#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using BeyondTime.Memories;
using BeyondTime.Memories.Building;

namespace BeyondTime.Game.Building
{
    /// <summary>
    /// Tools > Beyond Time > Game > Build Whole Game
    /// Builds the asset library, both memories, and the four scenes of the full game, wires every button,
    /// and puts the scenes in Build Profiles: BT_MainMenu, BT_GraveIntro, BT_HellfireWorld, BT_Paradise.
    /// </summary>
    public static class GameBuilder
    {
        const string Root = "Assets/Memories/Game";
        const string SceneDir = Root + "/Scenes";
        const string LibraryPath = "Assets/Memories/Resources/BeyondTimeAssets.asset";
        const string PlayerPrefabPath = "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/NestedParentArmature_Unpack.prefab";

        static readonly Color Bone = new Color(0.92f, 0.89f, 0.82f);
        static readonly Color Ash = new Color(0.62f, 0.6f, 0.58f);
        static readonly Color Ember = new Color(1f, 0.42f, 0.15f);
        static readonly Color Blood = new Color(0.75f, 0.08f, 0.05f);
        static readonly Color Spirit = new Color(0.45f, 0.85f, 1f);
        static readonly Color Night = new Color(0.03f, 0.025f, 0.04f);
        static readonly Color Gold = new Color(1f, 0.85f, 0.5f);

        static string ScenePath(string n) => SceneDir + "/" + n + ".unity";

        // ================================================================== menu

        [MenuItem("Tools/Beyond Time/Game/Build Whole Game", false, 0)]
        public static void BuildWholeGame()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolders();
            EnsureLibrary();
            MemoryBuilder.BuildKitInternal();
            var settings = AssetDatabase.LoadAssetAtPath<MemorySettings>("Assets/Memories/Resources/MemorySettings.asset");
            if (settings) { settings.saveProgressBetweenSessions = true; EditorUtility.SetDirty(settings); }
            MemoryBuilder.BuildMemoriesInternal();

            BuildWorld();
            BuildParadise();
            BuildIntro();
            BuildMainMenu();
            SetBuildOrder();
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene(ScenePath(SceneFlow.MainMenu));
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<BeyondTimeAssets>(LibraryPath);
            EditorUtility.DisplayDialog("Beyond Time",
                "The whole game is built.\n\n" +
                "Press Play in BT_MainMenu to play from the start.\n\n" +
                "The asset library is now selected: drag your models, materials, effects, music, sprites and the intro video into it.",
                "OK");
        }

        [MenuItem("Tools/Beyond Time/Game/Rebuild Only/Main Menu", false, 20)] static void OnlyMenu() { if (Save()) { EnsureFolders(); EnsureLibrary(); BuildMainMenu(); } }
        [MenuItem("Tools/Beyond Time/Game/Rebuild Only/Grave Intro", false, 21)] static void OnlyIntro() { if (Save()) { EnsureFolders(); EnsureLibrary(); BuildIntro(); } }
        [MenuItem("Tools/Beyond Time/Game/Rebuild Only/Hellfire World", false, 22)] static void OnlyWorld() { if (Save()) { EnsureFolders(); EnsureLibrary(); MemoryBuilder.BuildMemoriesInternal(); BuildWorld(); } }
        [MenuItem("Tools/Beyond Time/Game/Rebuild Only/Paradise", false, 23)] static void OnlyParadise() { if (Save()) { EnsureFolders(); EnsureLibrary(); BuildParadise(); } }

        [MenuItem("Tools/Beyond Time/Game/Select Asset Library", false, 1)]
        static void SelectLibrary()
        {
            EnsureLibrary();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<BeyondTimeAssets>(LibraryPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        [MenuItem("Tools/Beyond Time/Game/Set Build Order", false, 2)]
        static void SetBuildOrder()
        {
            var ours = new[] { SceneFlow.MainMenu, SceneFlow.Intro, SceneFlow.World, SceneFlow.Paradise };
            var list = new List<EditorBuildSettingsScene>();
            foreach (var n in ours) if (System.IO.File.Exists(ScenePath(n))) list.Add(new EditorBuildSettingsScene(ScenePath(n), true));
            foreach (var s in EditorBuildSettings.scenes)
                if (!s.path.StartsWith(SceneDir + "/")) list.Add(new EditorBuildSettingsScene(s.path, false));   // keep others, switched off
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("Beyond Time: Build Profiles = BT_MainMenu, BT_GraveIntro, BT_HellfireWorld, BT_Paradise (other scenes kept but unticked).");
        }

        static bool Save() => EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets/Memories", "Game");
            if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder(Root, "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Memories/Resources")) AssetDatabase.CreateFolder("Assets/Memories", "Resources");
            if (!AssetDatabase.IsValidFolder(Root + "/Art")) AssetDatabase.CreateFolder(Root, "Art");
        }

        static void EnsureLibrary()
        {
            EnsureFolders();
            if (AssetDatabase.LoadAssetAtPath<BeyondTimeAssets>(LibraryPath)) return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BeyondTimeAssets>(), LibraryPath);
            AssetDatabase.SaveAssets();
        }

        // ================================================================== MAIN MENU

        static void BuildMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera(Night);
            EventSystemIfMissing();
            Music(MusicSlot.Menu);

            var canvas = Canvas("MenuCanvas", 0);
            var bg = FullImage(canvas, "Background", Night, UISlot.MenuBackground);
            Glow(canvas, "EmberGlow", new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(1800, 500), new Color(0.6f, 0.12f, 0.02f, 0.35f));

            var tag = Label(canvas, "Tagline", "A GAME ABOUT THE CHOICES WE MAKE", 26, Ash, Top, new Vector2(0, -150), new Vector2(1200, 50), FontStyles.Bold);
            var title = Label(canvas, "Title", "BEYOND TIME", 150, Bone, Top, new Vector2(0, -270), new Vector2(1600, 190), FontStyles.Bold);
            var sub = Label(canvas, "Subtitle", "Every second is a chance to make things right.", 34, Ember, Top, new Vector2(0, -385), new Vector2(1400, 60), FontStyles.Italic);
            var logo = Image(canvas, "Logo", Top, new Vector2(0, -260), new Vector2(900, 300), Color.white);
            var logoSlot = logo.gameObject.AddComponent<ReplaceableUISprite>();
            logoSlot.slot = UISlot.Logo; logoSlot.preserveAspect = true; logoSlot.hideIfNoSprite = true;
            logoSlot.hideWhenReplaced = new[] { title.gameObject };

            var ctrl = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            string[] labels = { "NEW GAME", "CONTINUE", "SETTINGS", "CONTROLS", "CREDITS", "QUIT" };
            UnityAction[] actions = { ctrl.NewGame, ctrl.Continue, ctrl.Settings, ctrl.Controls, ctrl.Credits, ctrl.Quit };
            for (int i = 0; i < labels.Length; i++)
            {
                var b = Button(canvas, labels[i].Replace(" ", "") + "Button", labels[i], new Vector2(0, -20 - i * 92), new Vector2(460, 78), 34);
                UnityEventTools.AddPersistentListener(b.onClick, actions[i]);
                if (i == 1) ctrl.continueButton = b;
            }
            ctrl.bestScoreText = Label(canvas, "BestScore", "", 24, Ash, new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(800, 40), FontStyles.Bold);

            ctrl.settingsPanel = SettingsPanelUI(canvas, ctrl.Back);
            ctrl.controlsPanel = ControlsPanelUI(canvas, ctrl.Back);
            ctrl.creditsPanel = CreditsPanelUI(canvas, ctrl.Back);

            canvas.gameObject.AddComponent<UISkin>();
            EditorSceneManager.SaveScene(scene, ScenePath(SceneFlow.MainMenu));
        }

        // ================================================================== GRAVE INTRO (video placeholder)

        static void BuildIntro()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = Camera(Color.black);
            EventSystemIfMissing();

            var go = new GameObject("GraveIntroVideo", typeof(VideoPlayer));
            var vp = go.GetComponent<VideoPlayer>();
            vp.playOnAwake = false; vp.isLooping = false;
            vp.renderMode = VideoRenderMode.CameraFarPlane; vp.targetCamera = cam;
            vp.aspectRatio = VideoAspectRatio.FitInside;
            var intro = go.AddComponent<GraveIntro>();

            var canvas = Canvas("IntroCanvas", 10);
            var card = new GameObject("PLACEHOLDER_CARD", typeof(RectTransform));
            card.transform.SetParent(canvas, false);
            Stretch((RectTransform)card.transform);
            FullImage(card.transform, "Background", new Color(0.02f, 0.015f, 0.015f), UISlot.VideoPlaceholder);
            Glow(card.transform, "Candle", new Vector2(0.5f, 0.5f), new Vector2(0, 160), new Vector2(60, 140), new Color(1f, 0.7f, 0.3f, 0.9f));
            Glow(card.transform, "CandleHalo", new Vector2(0.5f, 0.5f), new Vector2(0, 170), new Vector2(380, 380), new Color(1f, 0.5f, 0.15f, 0.12f));
            Label(card.transform, "Title", "GRAVE INTRO VIDEO PLACEHOLDER", 56, Bone, Mid, new Vector2(0, -10), new Vector2(1600, 80), FontStyles.Bold);
            Label(card.transform, "Hint", "Drag your video into  Resources / BeyondTimeAssets  >  Intro Video  (or onto GraveIntroVideo > Video Clip)", 24, Ash, Mid, new Vector2(0, -70), new Vector2(1600, 40), FontStyles.Italic);
            var barBg = Image(card.transform, "ProgressBackground", Mid, new Vector2(0, -140), new Vector2(700, 6), new Color(0.2f, 0.18f, 0.16f));
            intro.placeholderProgress = Filled(barBg.transform, "Progress", Ember);
            intro.placeholderCard = card;

            var skip = Label(canvas, "SkipHint", "Hold SPACE to skip", 22, Ash, new Vector2(1, 0), new Vector2(-170, 50), new Vector2(300, 36), FontStyles.Bold);
            var skipBg = Image(canvas, "SkipBackground", new Vector2(1, 0), new Vector2(-170, 26), new Vector2(240, 4), new Color(0.25f, 0.25f, 0.25f));
            intro.skipFill = Filled(skipBg.transform, "SkipFill", Bone);
            intro.skipFill.fillAmount = 0f;
            intro.skipHint = skip;

            canvas.gameObject.AddComponent<UISkin>();
            EditorSceneManager.SaveScene(scene, ScenePath(SceneFlow.Intro));
        }

        // ================================================================== HELLFIRE WORLD

        static void BuildWorld()
        {
            var prefabs = new List<GameObject>();
            foreach (var d in MemoryBuilder.Definitions())
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(d.Folder + "/" + System.IO.Path.GetFileName(d.Folder) + ".prefab");
                if (p) prefabs.Add(p);
            }
            if (prefabs.Count == 0) { Debug.LogError("Beyond Time: no memory prefabs. Check the Console."); return; }

            string path = ScenePath(SceneFlow.World);
            MemoryBuilder.BuildWorldFrom(prefabs, path);     // leaves the scene open
            var scene = SceneManager.GetActiveScene();

            foreach (var gate in Object.FindObjectsByType<MemoryFinalGate>(FindObjectsSortMode.None)) gate.sceneToLoad = SceneFlow.Paradise;
            new GameObject("Sky").AddComponent<SkyboxFromLibrary>().sky = SkyboxFromLibrary.Which.Hell;
            EventSystemIfMissing();

            var session = Object.FindFirstObjectByType<GameSession>();
            var sys = session ? session.gameObject : new GameObject("GameSystems");
            if (!session) session = sys.AddComponent<GameSession>();
            if (!sys.GetComponent<AudioSource>()) sys.AddComponent<AudioSource>();
            if (!sys.GetComponent<MusicPlayer>()) sys.AddComponent<MusicPlayer>().slot = MusicSlot.World;

            // ---------- HUD ----------
            var hudCanvas = Canvas("GameHUD", 40);
            hudCanvas.SetParent(sys.transform, false);
            var hud = hudCanvas.gameObject.AddComponent<GameHUD>();

            var frame = Image(hudCanvas, "HealthFrame", TopLeft, new Vector2(40, -190), new Vector2(560, 92), new Color(0f, 0f, 0f, 0.55f));
            ((RectTransform)frame.transform).pivot = new Vector2(0, 1);
            AddSlot(frame.gameObject, UISlot.HudFrame);
            Label(frame.transform, "SoulLabel", "SOUL", 22, Ember, TopLeft, new Vector2(20, -12), new Vector2(120, 30), FontStyles.Bold).alignment = TextAlignmentOptions.Left;
            var barBg = Image(frame.transform, "HealthBackground", TopLeft, new Vector2(20, -48), new Vector2(380, 20), new Color(0.15f, 0.05f, 0.04f));
            ((RectTransform)barBg.transform).pivot = new Vector2(0, 1);
            hud.healthLag = Filled(barBg.transform, "HealthLag", new Color(1f, 0.95f, 0.9f, 0.6f));
            hud.healthFill = Filled(barBg.transform, "HealthFill", new Color(0.9f, 0.25f, 0.1f));
            hud.healthText = Label(frame.transform, "HealthText", "100 / 100", 18, Bone, TopLeft, new Vector2(20, -70), new Vector2(380, 22), FontStyles.Bold);
            hud.healthText.alignment = TextAlignmentOptions.Left;
            var souls = new List<GameObject>();
            for (int i = 0; i < GameScore.StartSouls; i++)
            {
                var flame = Image(frame.transform, "Soul" + (i + 1), TopLeft, new Vector2(430 + i * 40, -40), new Vector2(24, 24), Ember);
                flame.transform.localRotation = Quaternion.Euler(0, 0, 45);
                souls.Add(flame.gameObject);
            }
            hud.soulIcons = souls.ToArray();

            hud.scoreText = Label(hudCanvas, "ScoreText", "SCORE  0", 30, Gold, new Vector2(1, 1), new Vector2(-40, -140), new Vector2(500, 44), FontStyles.Bold);
            ((RectTransform)hud.scoreText.transform).pivot = new Vector2(1, 1);
            hud.scoreText.alignment = TextAlignmentOptions.TopRight;

            var toast = Image(hudCanvas, "Toast", Top, new Vector2(0, -230), new Vector2(900, 110), new Color(0.1f, 0f, 0f, 0.75f));
            hud.toast = toast.gameObject.AddComponent<CanvasGroup>();
            hud.toast.blocksRaycasts = false;
            hud.toastText = Label(toast.transform, "Text", "", 30, Bone, Mid, Vector2.zero, new Vector2(860, 100), FontStyles.Bold);

            var card = Image(hudCanvas, "ResultCard", new Vector2(1, 0.5f), new Vector2(-300, 20), new Vector2(520, 360), new Color(0.05f, 0.03f, 0.03f, 0.9f));
            AddSlot(card.gameObject, UISlot.ResultCard);
            hud.resultCard = card.gameObject.AddComponent<CanvasGroup>();
            hud.resultCard.blocksRaycasts = false;
            hud.resultTitle = Label(card.transform, "Title", "MEMORY REDEEMED", 36, Gold, Top, new Vector2(0, -40), new Vector2(480, 50), FontStyles.Bold);
            hud.resultBody = Label(card.transform, "Body", "", 24, Bone, Mid, new Vector2(0, -30), new Vector2(460, 260), FontStyles.Normal);
            hud.resultBody.alignment = TextAlignmentOptions.TopLeft;
            hudCanvas.gameObject.AddComponent<UISkin>();

            // ---------- MENUS ----------
            var menuCanvas = Canvas("GameMenus", 80);
            menuCanvas.SetParent(sys.transform, false);
            var menus = menuCanvas.gameObject.AddComponent<GameMenus>();

            var pause = new GameObject("PausePanel", typeof(RectTransform));
            pause.transform.SetParent(menuCanvas, false); Stretch((RectTransform)pause.transform);
            FullImage(pause.transform, "Background", new Color(0.02f, 0.06f, 0.09f, 0.88f), UISlot.PauseBackground);
            Label(pause.transform, "Title", "TIME STANDS STILL", 80, Spirit, Top, new Vector2(0, -170), new Vector2(1400, 110), FontStyles.Bold);
            menus.pauseScoreText = Label(pause.transform, "Score", "", 28, Bone, Top, new Vector2(0, -250), new Vector2(1200, 40), FontStyles.Bold);
            string[] pl = { "RESUME", "RESTART MEMORY", "SETTINGS", "CONTROLS", "MAIN MENU", "QUIT" };
            UnityAction[] pa = { menus.Resume, menus.RestartMemory, menus.OpenSettings, menus.OpenControls, menus.MainMenu, menus.Quit };
            for (int i = 0; i < pl.Length; i++)
                UnityEventTools.AddPersistentListener(Button(pause.transform, pl[i].Replace(" ", "") + "Button", pl[i], new Vector2(0, 90 - i * 92), new Vector2(480, 78), 32).onClick, pa[i]);
            menus.pausePanel = pause;

            menus.settingsPanel = SettingsPanelUI(menuCanvas, menus.Back);
            menus.controlsPanel = ControlsPanelUI(menuCanvas, menus.Back);

            var over = new GameObject("GameOverPanel", typeof(RectTransform));
            over.transform.SetParent(menuCanvas, false); Stretch((RectTransform)over.transform);
            FullImage(over.transform, "Background", new Color(0.18f, 0.02f, 0.01f, 0.94f), UISlot.GameOverBackground);
            Label(over.transform, "Title", "CONSUMED BY HELLFIRE", 90, Bone, Top, new Vector2(0, -200), new Vector2(1600, 120), FontStyles.Bold);
            Label(over.transform, "Line", "The flames took the memory. Your soul is spent.", 32, Ember, Top, new Vector2(0, -290), new Vector2(1400, 50), FontStyles.Italic);
            menus.gameOverScoreText = Label(over.transform, "Score", "", 32, Bone, Mid, new Vector2(0, 40), new Vector2(1200, 220), FontStyles.Bold);
            UnityEventTools.AddPersistentListener(Button(over.transform, "TryAgainButton", "TRY AGAIN", new Vector2(0, -170), new Vector2(460, 80), 34).onClick, menus.TryAgain);
            UnityEventTools.AddPersistentListener(Button(over.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -265), new Vector2(460, 80), 34).onClick, menus.MainMenu);
            menus.gameOverPanel = over;
            menuCanvas.gameObject.AddComponent<UISkin>();

            session.hud = hud;
            session.menus = menus;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        // ================================================================== PARADISE

        static void BuildParadise()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(1f, 0.93f, 0.78f); sun.intensity = 1.35f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(28f, -40f, 0f);
            var sky = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            if (sky) RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.8f, 0.88f, 0.95f); RenderSettings.fogDensity = 0.006f;
            new GameObject("Sky").AddComponent<SkyboxFromLibrary>().sky = SkyboxFromLibrary.Which.Paradise;
            Volume("ParadiseVolume", 0.9f, 0.18f, 1.1f);

            var env = new GameObject("PARADISE_ENVIRONMENT").transform;
            var grass = MemoryKit.Mat("Paradise_Grass", new Color(0.32f, 0.6f, 0.25f));
            var ground = MemoryKit.Prim(PrimitiveType.Cylinder, "Ground", new Vector3(0, -0.5f, 0), new Vector3(100f, 0.5f, 100f), grass, env, false);
            ground.AddComponent<MeshCollider>();
            MemoryKit.MarkReplaceable(ground, AssetSlot.ParadiseGround, false);

            var water = MemoryKit.Prim(PrimitiveType.Plane, "Water", new Vector3(0, -0.6f, 0), new Vector3(60f, 1f, 60f),
                                       MemoryKit.Mat("Paradise_Water", new Color(0.25f, 0.55f, 0.75f), new Color(0.05f, 0.15f, 0.25f)), env, false);
            MemoryKit.MarkReplaceable(water, AssetSlot.ParadiseWater, false);
            var edge = new GameObject("IslandEdge");   // invisible ring so the player stays on the island
            edge.transform.SetParent(env, false);
            for (int i = 0; i < 36; i++)
            {
                float a = i * Mathf.PI * 2f / 36f;
                var w = MemoryKit.Prim(PrimitiveType.Cube, "Edge" + i, new Vector3(Mathf.Sin(a) * 49f, 1.5f, Mathf.Cos(a) * 49f), new Vector3(9f, 3f, 0.5f), null, edge.transform, true);
                w.transform.localRotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
                Object.DestroyImmediate(w.GetComponent<MeshRenderer>());
            }

            var stone = MemoryKit.Mat("Paradise_Stone", new Color(0.85f, 0.82f, 0.75f));
            for (int i = 0; i < 14; i++)
                MemoryKit.Prim(PrimitiveType.Cube, "Path" + i, new Vector3(Mathf.Sin(i * 0.5f) * 1.2f, 0.02f, -30f + i * 4.4f), new Vector3(2.4f, 0.06f, 3.2f), stone, env, true)
                    .transform.localRotation = Quaternion.Euler(0, i * 7f, 0);

            var rng = new System.Random(42);
            var trunk = MemoryKit.Mat("Paradise_Trunk", new Color(0.4f, 0.28f, 0.18f));
            var leaves = MemoryKit.Mat("Paradise_Leaves", new Color(0.3f, 0.62f, 0.28f));
            var blossom = MemoryKit.Mat("Paradise_Blossom", new Color(1f, 0.8f, 0.88f), new Color(0.15f, 0.08f, 0.1f));
            for (int i = 0; i < 28; i++)
            {
                Vector3 p = RandomOnIsland(rng, 6f, 42f);
                var tree = new GameObject("Tree_REPLACE_ME_" + i);
                tree.transform.SetParent(env, false);
                tree.transform.localPosition = p;
                tree.transform.localRotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
                float h = 3f + (float)rng.NextDouble() * 2.5f;
                var t = MemoryKit.Prim(PrimitiveType.Cylinder, "Trunk", new Vector3(0, h * 0.5f, 0), new Vector3(0.35f, h * 0.5f, 0.35f), trunk, tree.transform, true);
                var c1 = MemoryKit.Prim(PrimitiveType.Sphere, "Canopy", new Vector3(0, h + 0.6f, 0), Vector3.one * (2.6f + (float)rng.NextDouble()), i % 3 == 0 ? blossom : leaves, tree.transform, false);
                var c2 = MemoryKit.Prim(PrimitiveType.Sphere, "Canopy2", new Vector3(0.8f, h, 0.3f), Vector3.one * 1.8f, i % 3 == 0 ? blossom : leaves, tree.transform, false);
                MemoryKit.MarkReplaceable(tree, AssetSlot.ParadiseTree, false, t, c1, c2);
            }

            Color[] petal = { new Color(1f, 1f, 1f), new Color(1f, 0.75f, 0.85f), new Color(1f, 0.9f, 0.4f), new Color(0.7f, 0.75f, 1f) };
            for (int i = 0; i < 45; i++)
            {
                Vector3 p = RandomOnIsland(rng, 3.5f, 44f);
                var flower = new GameObject("Flower_REPLACE_ME_" + i);
                flower.transform.SetParent(env, false);
                flower.transform.localPosition = p;
                var col = petal[i % petal.Length];
                var f = MemoryKit.Prim(PrimitiveType.Sphere, "Bloom", new Vector3(0, 0.25f, 0), new Vector3(0.35f, 0.2f, 0.35f),
                                       MemoryKit.Mat("Paradise_Petal_" + ColorUtility.ToHtmlStringRGB(col), col, col * 0.15f), flower.transform, false);
                var st = MemoryKit.Prim(PrimitiveType.Cylinder, "Stem", new Vector3(0, 0.12f, 0), new Vector3(0.04f, 0.12f, 0.04f), leaves, flower.transform, false);
                MemoryKit.MarkReplaceable(flower, AssetSlot.ParadiseFlower, false, f, st);
            }

            var rockMat = MemoryKit.Mat("Paradise_Rock", new Color(0.6f, 0.6f, 0.58f));
            for (int i = 0; i < 10; i++)
            {
                Vector3 p = RandomOnIsland(rng, 8f, 45f);
                var rock = new GameObject("Rock_REPLACE_ME_" + i);
                rock.transform.SetParent(env, false);
                rock.transform.localPosition = p;
                float s = 0.8f + (float)rng.NextDouble() * 1.6f;
                var r = MemoryKit.Prim(PrimitiveType.Sphere, "Rock", new Vector3(0, s * 0.25f, 0), new Vector3(s, s * 0.6f, s * 1.2f), rockMat, rock.transform, true);
                MemoryKit.MarkReplaceable(rock, AssetSlot.ParadiseRock, false, r);
            }

            // Shrine at the end of the path
            var shrine = new GameObject("SHRINE_REPLACE_ME");
            shrine.transform.SetParent(env, false);
            shrine.transform.localPosition = new Vector3(0, 0, 33f);
            var marble = MemoryKit.Mat("Paradise_Marble", new Color(0.95f, 0.94f, 0.9f), new Color(0.08f, 0.08f, 0.07f));
            var parts = new List<GameObject>();
            parts.Add(MemoryKit.Prim(PrimitiveType.Cylinder, "Base", new Vector3(0, 0.15f, 0), new Vector3(8f, 0.15f, 8f), marble, shrine.transform, true));
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                parts.Add(MemoryKit.Prim(PrimitiveType.Cylinder, "Pillar" + i, new Vector3(Mathf.Sin(a) * 3.2f, 2.3f, Mathf.Cos(a) * 3.2f), new Vector3(0.45f, 2.1f, 0.45f), marble, shrine.transform, true));
            }
            parts.Add(MemoryKit.Prim(PrimitiveType.Cylinder, "Roof", new Vector3(0, 4.55f, 0), new Vector3(7.6f, 0.2f, 7.6f), marble, shrine.transform, false));
            parts.Add(MemoryKit.Prim(PrimitiveType.Sphere, "Light Orb", new Vector3(0, 1.8f, 0), Vector3.one * 0.9f,
                                     MemoryKit.Mat("Paradise_Orb", Gold, Gold * 3f), shrine.transform, false));
            var orbLight = new GameObject("OrbLight", typeof(Light)).GetComponent<Light>();
            orbLight.transform.SetParent(shrine.transform, false); orbLight.transform.localPosition = new Vector3(0, 2f, 0);
            orbLight.type = LightType.Point; orbLight.color = Gold; orbLight.intensity = 6f; orbLight.range = 14f;
            MemoryKit.MarkReplaceable(shrine, AssetSlot.ParadiseShrine, false, parts.ToArray());

            // Floating motes
            var moteMat = MemoryKit.Mat("Paradise_Mote", Gold, Gold * 4f);
            for (int i = 0; i < 30; i++)
            {
                Vector3 p = RandomOnIsland(rng, 2f, 35f) + Vector3.up * (1f + (float)rng.NextDouble() * 2.5f);
                var m = MemoryKit.Prim(PrimitiveType.Sphere, "Mote" + i, p, Vector3.one * 0.08f, moteMat, env, false);
                m.AddComponent<FloatingMote>();
            }

            // The people you saved (only appear if you redeemed their memory)
            Guest("TheManYouSaved", new Vector3(-3.5f, 0, 28f), 160f, new Color(0.65f, 0.72f, 0.82f), AssetSlot.HumanVictim, "memory_01_alley");
            Guest("YourFriend", new Vector3(3.5f, 0, 28f), 200f, new Color(0.85f, 0.78f, 0.6f), AssetSlot.HumanFriend, "memory_02_siege");

            // Player
            var playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerAsset)
            {
                var rig = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset, scene);
                var arm = Find(rig.transform, "PlayerArmature");
                if (arm) { arm.position = new Vector3(0, 0.05f, -36f); arm.rotation = Quaternion.identity; }
                foreach (var c in rig.GetComponentsInChildren<UnityEngine.Camera>(true)) c.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }
            EventSystemIfMissing();

            // UI
            var canvas = Canvas("ParadiseCanvas", 20);
            var ctrl = new GameObject("ParadiseController").AddComponent<ParadiseController>();
            ctrl.sun = sun;
            ctrl.shrine = shrine.transform;
            ctrl.shrineRadius = 5f;

            var story = Image(canvas, "StoryPanel", new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(1500, 130), new Color(1f, 1f, 1f, 0.2f));
            ctrl.storyPanel = story.gameObject;
            ctrl.storyText = Label(story.transform, "Text", "", 36, Color.white, Mid, Vector2.zero, new Vector2(1440, 120), FontStyles.Italic);
            story.gameObject.SetActive(false);

            var end = new GameObject("EndPanel", typeof(RectTransform));
            end.transform.SetParent(canvas, false); Stretch((RectTransform)end.transform);
            FullImage(end.transform, "Background", new Color(1f, 0.96f, 0.88f, 0.82f), UISlot.VictoryBackground);
            ctrl.endTitle = Label(end.transform, "Title", "PEACE", 130, new Color(0.45f, 0.32f, 0.15f), Top, new Vector2(0, -170), new Vector2(1400, 160), FontStyles.Bold);
            ctrl.endBody = Label(end.transform, "Body", "", 36, new Color(0.35f, 0.3f, 0.25f), Top, new Vector2(0, -280), new Vector2(1500, 60), FontStyles.Italic);
            ctrl.scoreText = Label(end.transform, "Score", "", 30, new Color(0.3f, 0.25f, 0.2f), Mid, new Vector2(0, 20), new Vector2(1400, 280), FontStyles.Bold);
            UnityEventTools.AddPersistentListener(Button(end.transform, "MainMenuButton", "MAIN MENU", new Vector2(-300, -330), new Vector2(380, 80), 32).onClick, ctrl.MainMenu);
            UnityEventTools.AddPersistentListener(Button(end.transform, "CreditsButton", "CREDITS", new Vector2(100, -330), new Vector2(380, 80), 32).onClick, ctrl.ToggleCredits);
            UnityEventTools.AddPersistentListener(Button(end.transform, "QuitButton", "QUIT", new Vector2(500, -330), new Vector2(380, 80), 32).onClick, ctrl.Quit);
            ctrl.endPanel = end;
            ctrl.creditsPanel = CreditsPanelUI(canvas, ctrl.ToggleCredits);

            canvas.gameObject.AddComponent<UISkin>();
            var music = new GameObject("Music", typeof(AudioSource));
            music.AddComponent<MusicPlayer>().slot = MusicSlot.Paradise;
            EditorSceneManager.SaveScene(scene, ScenePath(SceneFlow.Paradise));
        }

        static Vector3 RandomOnIsland(System.Random rng, float pathClear, float maxR)
        {
            for (int tries = 0; tries < 40; tries++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float r = Mathf.Sqrt((float)rng.NextDouble()) * maxR;
                var p = new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
                if (Mathf.Abs(p.x) > pathClear && new Vector2(p.x, p.z - 33f).magnitude > 7f) return p;
            }
            return new Vector3(pathClear + 2f, 0f, 0f);
        }

        static void Guest(string name, Vector3 pos, float yaw, Color clothes, AssetSlot slot, string memoryId)
        {
            var root = new GameObject(name);
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(MemoryKit.ArmaturePath);
            if (model)
            {
                var m = (GameObject)PrefabUtility.InstantiatePrefab(model);
                m.name = "MODEL_REPLACE_ME";
                m.transform.SetParent(root.transform, false);
                var anim = m.GetComponent<Animator>(); if (!anim) anim = m.AddComponent<Animator>();
                anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(MemoryKit.ControllerPath);
                anim.applyRootMotion = false;
                var mat = MemoryKit.Mat("Paradise_Clothes_" + ColorUtility.ToHtmlStringRGB(clothes), clothes);
                foreach (var r in m.GetComponentsInChildren<Renderer>(true))
                {
                    var arr = new Material[Mathf.Max(1, r.sharedMaterials.Length)];
                    for (int i = 0; i < arr.Length; i++) arr[i] = mat;
                    r.sharedMaterials = arr;
                }
                m.AddComponent<FighterAnimEvents>();
                MemoryKit.MarkReplaceable(root, slot, false, m);
            }
            root.AddComponent<MemoryActor>().displayName = name;
            root.AddComponent<ParadiseGuest>().memoryId = memoryId;
            var col = root.AddComponent<CapsuleCollider>(); col.center = new Vector3(0, 0.9f, 0); col.height = 1.8f; col.radius = 0.3f;
        }

        // ================================================================== shared panels

        static GameObject SettingsPanelUI(Transform canvas, UnityAction back)
        {
            var panel = new GameObject("SettingsPanel", typeof(RectTransform));
            panel.transform.SetParent(canvas, false); Stretch((RectTransform)panel.transform);
            FullImage(panel.transform, "Background", new Color(0f, 0f, 0f, 0.92f), UISlot.Panel);
            Label(panel.transform, "Title", "SETTINGS", 80, Bone, Top, new Vector2(0, -150), new Vector2(1000, 100), FontStyles.Bold);
            var sp = panel.AddComponent<SettingsPanel>();
            sp.masterSlider = Slider(panel.transform, "Master Volume", new Vector2(0, 120));
            sp.musicSlider = Slider(panel.transform, "Music Volume", new Vector2(0, 30));
            sp.fullscreenToggle = Toggle(panel.transform, "Fullscreen", new Vector2(0, -60));
            UnityEventTools.AddPersistentListener(Button(panel.transform, "BackButton", "BACK", new Vector2(0, -330), new Vector2(360, 78), 32).onClick, back);
            panel.SetActive(false);
            return panel;
        }

        static GameObject ControlsPanelUI(Transform canvas, UnityAction back)
        {
            var panel = new GameObject("ControlsPanel", typeof(RectTransform));
            panel.transform.SetParent(canvas, false); Stretch((RectTransform)panel.transform);
            FullImage(panel.transform, "Background", new Color(0f, 0f, 0f, 0.92f), UISlot.Panel);
            Label(panel.transform, "Title", "CONTROLS", 80, Bone, Top, new Vector2(0, -130), new Vector2(1000, 100), FontStyles.Bold);
            var list = Label(panel.transform, "List",
                "WASD                Move\nMouse               Look\nShift               Sprint\nLeft mouse          Punch combo\nF                   Kick  (during a combo: roundhouse)\n" +
                "Right mouse         Block  (just before a hit: parry)\nSpace               Dodge\nQ                   Repel nearby enemies\nE                   Continue story, hold to interact\n1  2  3             Choices\nEsc                 Pause",
                32, Bone, Mid, new Vector2(0, 20), new Vector2(1000, 560), FontStyles.Normal);
            list.alignment = TextAlignmentOptions.Left;
            UnityEventTools.AddPersistentListener(Button(panel.transform, "BackButton", "BACK", new Vector2(0, -360), new Vector2(360, 78), 32).onClick, back);
            panel.SetActive(false);
            return panel;
        }

        static GameObject CreditsPanelUI(Transform canvas, UnityAction back)
        {
            var panel = new GameObject("CreditsPanel", typeof(RectTransform));
            panel.transform.SetParent(canvas, false); Stretch((RectTransform)panel.transform);
            FullImage(panel.transform, "Background", new Color(0f, 0f, 0f, 0.92f), UISlot.Panel);
            Label(panel.transform, "Title", "CREDITS", 80, Bone, Top, new Vector2(0, -150), new Vector2(1000, 100), FontStyles.Bold);
            Label(panel.transform, "Names", "BEYOND TIME\n\nMade by\nADD TEAM NAMES HERE\n\nMusic and sound\nADD CREDITS HERE\n\nThank you for playing.",
                  34, Bone, Mid, new Vector2(0, 0), new Vector2(1200, 520), FontStyles.Normal);
            UnityEventTools.AddPersistentListener(Button(panel.transform, "BackButton", "BACK", new Vector2(0, -360), new Vector2(360, 78), 32).onClick, back);
            panel.SetActive(false);
            return panel;
        }

        // ================================================================== scene helpers

        static UnityEngine.Camera Camera(Color bg)
        {
            var go = new GameObject("Main Camera", typeof(UnityEngine.Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var cam = go.GetComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            return cam;
        }

        static void Music(MusicSlot slot)
        {
            var go = new GameObject("Music", typeof(AudioSource));
            go.AddComponent<MusicPlayer>().slot = slot;
        }

        static void EventSystemIfMissing()
        {
            if (Object.FindFirstObjectByType<EventSystem>()) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        static void Volume(string name, float bloom, float vignette, float postExposure)
        {
            string path = Root + "/Art/" + name + ".asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var b = profile.Add<Bloom>(true); b.intensity.Override(bloom); b.threshold.Override(0.95f);
            AssetDatabase.AddObjectToAsset(b, profile);
            var v = profile.Add<Vignette>(true); v.intensity.Override(vignette);
            AssetDatabase.AddObjectToAsset(v, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            var vol = new GameObject("Global Volume").AddComponent<Volume>();
            vol.isGlobal = true; vol.sharedProfile = profile;
        }

        static Transform Find(Transform t, string n)
        {
            if (t.name == n) return t;
            foreach (Transform c in t) { var r = Find(c, n); if (r) return r; }
            return null;
        }

        // ================================================================== UI helpers

        static readonly Vector2 Top = new Vector2(0.5f, 1f);
        static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);
        static readonly Vector2 TopLeft = new Vector2(0f, 1f);

        static Sprite UISprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        static Transform Canvas(string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = order;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1920, 1080); s.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static Image FullImage(Transform parent, string name, Color color, UISlot slot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            var img = go.GetComponent<Image>(); img.color = color;
            if (slot != UISlot.None) AddSlot(go, slot);
            return img;
        }

        static void AddSlot(GameObject go, UISlot slot) { go.AddComponent<ReplaceableUISprite>().slot = slot; }

        static Image Image(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = anchor; r.anchoredPosition = pos; r.sizeDelta = size;
            var img = go.GetComponent<Image>(); img.color = color; img.raycastTarget = false;
            return img;
        }

        static void Glow(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var img = Image(parent, name, anchor, pos, size, color);
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        static Image Filled(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform);
            var img = go.GetComponent<Image>();
            img.sprite = UISprite; img.type = UnityEngine.UI.Image.Type.Filled; img.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            img.fillAmount = 1f; img.color = color; img.raycastTarget = false;
            return img;
        }

        static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color, Vector2 anchor, Vector2 pos, Vector2 dim, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = anchor; r.anchoredPosition = pos; r.sizeDelta = dim;
            if (anchor == TopLeft) r.pivot = new Vector2(0f, 1f);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
            return t;
        }

        static Button Button(Transform parent, string name, string label, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = Mid; r.anchoredPosition = pos; r.sizeDelta = size;
            var img = go.GetComponent<Image>(); img.color = Color.white;
            AddSlot(go, UISlot.Button);
            var b = go.GetComponent<Button>(); b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = new Color(0.1f, 0.09f, 0.1f, 0.92f);
            cb.highlightedColor = new Color(0.45f, 0.15f, 0.06f, 1f);
            cb.pressedColor = new Color(0.7f, 0.25f, 0.08f, 1f);
            cb.selectedColor = cb.normalColor;
            cb.disabledColor = new Color(0.1f, 0.09f, 0.1f, 0.35f);
            b.colors = cb;
            var t = Label(go.transform, "Label", label, fontSize, Bone, Mid, Vector2.zero, size, FontStyles.Bold);
            Stretch((RectTransform)t.transform);
            return b;
        }

        static Slider Slider(Transform parent, string label, Vector2 pos)
        {
            Label(parent, label.Replace(" ", "") + "Label", label.ToUpperInvariant(), 28, Bone, Mid, pos + new Vector2(-330, 0), new Vector2(380, 40), FontStyles.Bold)
                .alignment = TextAlignmentOptions.Right;
            var res = new DefaultControls.Resources
            {
                standard = UISprite,
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd")
            };
            var go = DefaultControls.CreateSlider(res);
            go.name = label.Replace(" ", "") + "Slider";
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = Mid; r.anchoredPosition = pos + new Vector2(120, 0); r.sizeDelta = new Vector2(460, 24);
            var fill = go.transform.Find("Fill Area/Fill");
            if (fill) fill.GetComponent<Image>().color = Ember;
            var s = go.GetComponent<Slider>(); s.minValue = 0f; s.maxValue = 1f; s.value = 1f;
            return s;
        }

        static Toggle Toggle(Transform parent, string label, Vector2 pos)
        {
            Label(parent, label + "Label", label.ToUpperInvariant(), 28, Bone, Mid, pos + new Vector2(-330, 0), new Vector2(380, 40), FontStyles.Bold)
                .alignment = TextAlignmentOptions.Right;
            var res = new DefaultControls.Resources
            {
                standard = UISprite,
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd")
            };
            var go = DefaultControls.CreateToggle(res);
            go.name = label + "Toggle";
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = Mid; r.anchoredPosition = pos + new Vector2(-90, 0); r.sizeDelta = new Vector2(40, 40);
            var legacyLabel = go.transform.Find("Label");
            if (legacyLabel) Object.DestroyImmediate(legacyLabel.gameObject);
            var bgTr = go.transform.Find("Background");
            if (bgTr) { var br = (RectTransform)bgTr; br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.offsetMin = br.offsetMax = Vector2.zero; }
            return go.GetComponent<Toggle>();
        }
    }
}
#endif
