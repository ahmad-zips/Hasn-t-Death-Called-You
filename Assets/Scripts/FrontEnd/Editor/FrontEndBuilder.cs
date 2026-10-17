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
using TMPro;

namespace BeyondTime.FrontEnd.Building
{
    /// <summary>
    /// Beyond Time - builds the front-end scenes (MainMenu, Grave, Briefing1, EndScreen) in Assets/FrontEnd/Scenes.
    /// Menu: Tools > Beyond Time > Front End.
    /// Touches no existing scene or script until you run the merge step (3 and 4).
    /// </summary>
    public static class FrontEndBuilder
    {
        const string Root = "Assets/FrontEnd";
        const string SceneDir = Root + "/Scenes";
        const string ArtDir = Root + "/Art";
        const string PlayerPrefabPath =
            "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/NestedParentArmature_Unpack.prefab";

        // Palette
        static readonly Color Bone   = new Color(0.92f, 0.89f, 0.82f);
        static readonly Color Ash    = new Color(0.62f, 0.60f, 0.58f);
        static readonly Color Ember  = new Color(1.00f, 0.42f, 0.15f);
        static readonly Color Spirit = new Color(0.45f, 0.85f, 1.00f);
        static readonly Color Night  = new Color(0.03f, 0.025f, 0.04f);

        // ================================================================== MENU ITEMS

        [MenuItem("Tools/Beyond Time/Front End/1. Build All Front-End Scenes", false, 1)]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var existing = new List<string>();
            foreach (var n in new[] { FrontEndScenes.MainMenu, FrontEndScenes.Grave, FrontEndScenes.Briefing1, FrontEndScenes.EndScreen })
                if (System.IO.File.Exists(ScenePath(n))) existing.Add(n);

            if (existing.Count > 0 && !EditorUtility.DisplayDialog("Beyond Time",
                    "These scenes already exist and will be rebuilt from scratch:\n\n" + string.Join(", ", existing) +
                    "\n\nManual changes inside them will be lost. Continue?", "Rebuild", "Cancel"))
                return;

            BuildMainMenu();
            BuildGrave();
            BuildBriefing();
            BuildEndScreen();

            EditorSceneManager.OpenScene(ScenePath(FrontEndScenes.MainMenu));
            Debug.Log("Beyond Time: front-end scenes built in " + SceneDir +
                      ". Next: Tools > Beyond Time > Front End > 2. Add Front-End Scenes To Build (for local testing).");
        }

        [MenuItem("Tools/Beyond Time/Front End/Rebuild Only/Main Menu", false, 20)]
        static void RebuildMenu()     { if (Confirm(FrontEndScenes.MainMenu))  BuildMainMenu(); }
        [MenuItem("Tools/Beyond Time/Front End/Rebuild Only/Grave", false, 21)]
        static void RebuildGrave()    { if (Confirm(FrontEndScenes.Grave))     BuildGrave(); }
        [MenuItem("Tools/Beyond Time/Front End/Rebuild Only/Briefing 1", false, 22)]
        static void RebuildBriefing() { if (Confirm(FrontEndScenes.Briefing1)) BuildBriefing(); }
        [MenuItem("Tools/Beyond Time/Front End/Rebuild Only/End Screen", false, 23)]
        static void RebuildEnd()      { if (Confirm(FrontEndScenes.EndScreen)) BuildEndScreen(); }

        [MenuItem("Tools/Beyond Time/Front End/2. Add Front-End Scenes To Build", false, 2)]
        public static void AddToBuild()
        {
            var ours = new[] { FrontEndScenes.MainMenu, FrontEndScenes.Grave, FrontEndScenes.Briefing1 };
            var result = new List<EditorBuildSettingsScene>();

            foreach (var n in ours)
                if (System.IO.File.Exists(ScenePath(n))) result.Add(new EditorBuildSettingsScene(ScenePath(n), true));

            string endPath = ScenePath(FrontEndScenes.EndScreen);
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.path.StartsWith(SceneDir + "/")) continue;   // ours are re-added in the right order
                result.Add(s);
            }
            if (System.IO.File.Exists(endPath)) result.Add(new EditorBuildSettingsScene(endPath, true));

            EditorBuildSettings.scenes = result.ToArray();

            var order = new List<string>();
            for (int i = 0; i < result.Count; i++) order.Add(i + ": " + System.IO.Path.GetFileNameWithoutExtension(result[i].path));
            Debug.Log("Beyond Time: build list is now\n" + string.Join("\n", order));
        }

        [MenuItem("Tools/Beyond Time/Front End/3. Connect Open Trial Scene (merge step)", false, 3)]
        public static void ConnectTrialScene()
        {
            var gm = Object.FindFirstObjectByType<GameManager>();
            if (gm == null)
            {
                EditorUtility.DisplayDialog("Beyond Time",
                    "Open the trial scene (Trial1) first. It needs a GameManager (Maryam's Build HUD).", "OK");
                return;
            }

            var loader = Object.FindFirstObjectByType<FlowLoader>();
            if (loader == null)
            {
                var go = new GameObject("FlowLoader");
                Undo.RegisterCreatedObjectUndo(go, "Connect Trial");
                loader = go.AddComponent<FlowLoader>();
                loader.cursorOnStart = CursorOnStart.LeaveAlone;   // GameManager controls the cursor in trials
            }

            int added = 0;
            if (gm.winPanel && !gm.winPanel.transform.Find("NextButton"))
            {
                var b = MakeButton(gm.winPanel.transform, "NextButton", "NEXT", new Vector2(0, -210), new Vector2(400, 100), 44);
                UnityEventTools.AddStringPersistentListener(b.onClick, new UnityAction<string>(loader.SaveProgress), FrontEndScenes.EndScreen);
                UnityEventTools.AddStringPersistentListener(b.onClick, new UnityAction<string>(loader.LoadScene), FrontEndScenes.EndScreen);
                added++;
            }
            foreach (var panel in new[] { gm.failPanel, gm.pausePanel })
            {
                if (!panel || panel.transform.Find("MainMenuButton")) continue;
                var b = MakeButton(panel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -210), new Vector2(400, 100), 44);
                UnityEventTools.AddStringPersistentListener(b.onClick, new UnityAction<string>(loader.LoadScene), FrontEndScenes.MainMenu);
                added++;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Beyond Time",
                added + " button(s) added and wired.\n\nWin panel: NEXT goes to " + FrontEndScenes.EndScreen +
                ".\nFail and pause panels: MAIN MENU.\n\nPress Ctrl+S to save the scene.", "OK");
        }

        // ================================================================== MAIN MENU

        static void BuildMainMenu()
        {
            var scene = NewScene();

            MakeCamera(new Vector3(0, 1.6f, -7), new Vector3(4, 0, 0), Night);
            MakeDirectional(new Vector3(35, 150, 0), new Color(0.5f, 0.55f, 0.75f), 0.25f);
            SetFog(new Color(0.05f, 0.03f, 0.04f), 0.05f, new Color(0.06f, 0.05f, 0.06f));
            MakeVolume("MenuVolume", 1.6f, 0.4f);

            // Backdrop: dark ground, an ember glow and a slow portal on the right
            Prim(PrimitiveType.Plane, "Ground", new Vector3(0, 0, 4), new Vector3(4, 1, 4), Mat("Soil", new Color(0.06f, 0.05f, 0.05f)), null, true);
            var portal = MakePortal("Ember", new Vector3(3.6f, 1.9f, 3f), 2.2f, Ember, 8f);
            portal.SetActive(true);
            MakePointLight("EmberGlow", new Vector3(-4, 0.5f, 3), Ember, 3f, 9f);

            // UI
            var canvas = MakeCanvas("MenuCanvas", 0);
            MakeEventSystemIfMissing();

            Label(canvas, "Tagline", "A GAME ABOUT THE CHOICES WE MAKE", 26, Ash, Top, new Vector2(0, -170), new Vector2(1200, 50), FontStyles.Bold);
            Label(canvas, "Title", "BEYOND TIME", 150, Bone, Top, new Vector2(0, -290), new Vector2(1600, 190), FontStyles.Bold);
            Label(canvas, "Subtitle", "Every second is a chance to make things right.", 34, Ember, Top, new Vector2(0, -410), new Vector2(1400, 60), FontStyles.Italic);

            var newGame  = MakeButton(canvas, "NewGameButton",  "NEW GAME", new Vector2(0, -30),  new Vector2(460, 86), 38);
            var cont     = MakeButton(canvas, "ContinueButton", "CONTINUE", new Vector2(0, -135), new Vector2(460, 86), 38);
            var controls = MakeButton(canvas, "ControlsButton", "CONTROLS", new Vector2(0, -240), new Vector2(460, 86), 38);
            var quit     = MakeButton(canvas, "QuitButton",     "QUIT",     new Vector2(0, -345), new Vector2(460, 86), 38);

            // Controls panel
            var panel = Panel(canvas, "ControlsPanel", new Color(0, 0, 0, 0.92f));
            Label(panel.transform, "Title", "CONTROLS", 72, Bone, Top, new Vector2(0, -140), new Vector2(1000, 100), FontStyles.Bold);
            var list = Label(panel.transform, "List",
                "WASD          Move\n" +
                "Mouse         Rotate camera\n" +
                "Shift         Sprint\n" +
                "Left mouse    Attack\n" +
                "Right mouse   Block\n" +
                "Space         Dodge\n" +
                "Q             Repel nearby shadows\n" +
                "E (hold)      Interact with objectives\n" +
                "Esc           Pause",
                36, Bone, Mid, new Vector2(0, 30), new Vector2(900, 520), FontStyles.Normal);
            list.alignment = TextAlignmentOptions.Left;
            list.lineSpacing = 18;
            var back = MakeButton(panel.transform, "BackButton", "BACK", new Vector2(0, -380), new Vector2(360, 80), 34);
            panel.SetActive(false);

            var fade = MakeFade(0f, 1.2f);

            var loaderGO = new GameObject("FlowLoader");
            var loader = loaderGO.AddComponent<FlowLoader>();
            loader.firstScene = FrontEndScenes.Grave;
            loader.cursorOnStart = CursorOnStart.Show;
            loader.continueButton = cont;
            loader.fade = fade;

            UnityEventTools.AddPersistentListener(newGame.onClick, new UnityAction(loader.NewGame));
            UnityEventTools.AddPersistentListener(cont.onClick, new UnityAction(loader.Continue));
            UnityEventTools.AddBoolPersistentListener(controls.onClick, new UnityAction<bool>(panel.SetActive), true);
            UnityEventTools.AddBoolPersistentListener(back.onClick, new UnityAction<bool>(panel.SetActive), false);
            UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(loader.Quit));

            Save(scene, FrontEndScenes.MainMenu);
        }

        // ================================================================== GRAVE

        static void BuildGrave()
        {
            var scene = NewScene();

            MakeDirectional(new Vector3(55, -30, 0), new Color(0.55f, 0.62f, 0.85f), 0.35f);
            SetFog(new Color(0.035f, 0.045f, 0.07f), 0.035f, new Color(0.07f, 0.08f, 0.11f));
            MakeVolume("GraveVolume", 1.8f, 0.45f);

            // The pit: 14 m wide, 24 m long, 4 m walls
            var soil = Mat("Soil", new Color(0.09f, 0.07f, 0.06f));
            var stone = Mat("Stone", new Color(0.16f, 0.16f, 0.17f));
            var env = new GameObject("Environment").transform;
            Prim(PrimitiveType.Plane, "Floor", new Vector3(0, 0, 0), new Vector3(1.4f, 1, 2.4f), soil, env, true);
            Prim(PrimitiveType.Cube, "WallLeft",  new Vector3(-7.3f, 2, 0),  new Vector3(0.6f, 4, 24.6f), stone, env, true);
            Prim(PrimitiveType.Cube, "WallRight", new Vector3(7.3f, 2, 0),   new Vector3(0.6f, 4, 24.6f), stone, env, true);
            Prim(PrimitiveType.Cube, "WallBack",  new Vector3(0, 2, -12.3f), new Vector3(15.2f, 4, 0.6f), stone, env, true);
            Prim(PrimitiveType.Cube, "WallFront", new Vector3(0, 2, 12.3f),  new Vector3(15.2f, 4, 0.6f), stone, env, true);

            var rocks = new[] {
                new Vector4(-5.5f, -8f, 1.2f, 0), new Vector4(5.8f, -5f, 0.9f, 0), new Vector4(-6f, 2f, 1.5f, 0),
                new Vector4(6.2f, 6f, 1.1f, 0), new Vector4(-4.5f, 9.5f, 0.8f, 0), new Vector4(4.8f, 10.5f, 1.3f, 0) };
            for (int i = 0; i < rocks.Length; i++)
                Prim(PrimitiveType.Sphere, "Rock" + i, new Vector3(rocks[i].x, rocks[i].z * 0.3f, rocks[i].y),
                     new Vector3(rocks[i].z, rocks[i].z * 0.7f, rocks[i].z * 1.1f), stone, env, true);

            // Player (third-person Starter Assets rig)
            var playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerAsset)
            {
                var rig = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset, scene);
                var armature = FindDeep(rig.transform, "PlayerArmature");
                if (armature) { armature.position = new Vector3(0, 0, -9); armature.rotation = Quaternion.identity; }
                foreach (var c in rig.GetComponentsInChildren<Camera>(true))
                    c.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }
            else
            {
                Debug.LogWarning("Beyond Time: Starter Assets third-person prefab not found at " + PlayerPrefabPath + ". Add a player tagged Player by hand.");
                MakeCamera(new Vector3(0, 3, -12), new Vector3(15, 0, 0), Night);
            }

            // Two figures: abstract shrouded shapes with a halo and a rim light. No faces by design.
            var robe = Mat("FigureRobe", new Color(0.02f, 0.02f, 0.03f), new Color(0.02f, 0.03f, 0.08f));
            var halo = Mat("FigureHalo", Bone, Bone * 2.5f);
            var figureLights = new List<Light>();
            foreach (float x in new[] { -2.2f, 2.2f })
            {
                var fig = new GameObject(x < 0 ? "FigureLeft" : "FigureRight").transform;
                fig.position = new Vector3(x, 0, 3.5f);
                Prim(PrimitiveType.Capsule, "Robe", fig.position + new Vector3(0, 1.7f, 0), new Vector3(1.1f, 1.7f, 1.1f), robe, fig, true);
                var h = Prim(PrimitiveType.Cylinder, "Halo", fig.position + new Vector3(0, 3.75f, 0), new Vector3(0.9f, 0.015f, 0.9f), halo, fig, false);
                h.AddComponent<SlowSpin>().degreesPerSecond = new Vector3(0, 25f, 0);
                var rim = MakePointLight("RimLight", fig.position + new Vector3(0, 2.6f, 1.4f), new Color(0.85f, 0.9f, 1f), 4f, 6f);
                rim.transform.SetParent(fig, true);
                figureLights.Add(rim);
            }

            // Portal (hidden until the questioning ends)
            var portal = MakePortal("Spirit", new Vector3(0, 1.9f, 9.5f), 2.6f, Spirit, 10f);
            var trigger = portal.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(3.2f, 3.8f, 2f);
            var portalTrigger = portal.AddComponent<PortalTrigger>();
            portalTrigger.sceneToLoad = FrontEndScenes.Briefing1;

            // UI
            var canvas = MakeCanvas("GraveCanvas", 0);
            MakeEventSystemIfMissing();

            var bar = Panel(canvas, "SubtitleBar", new Color(0, 0, 0, 0.55f));
            var barRt = (RectTransform)bar.transform;
            barRt.anchorMin = new Vector2(0, 0); barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0.5f, 0); barRt.sizeDelta = new Vector2(0, 210); barRt.anchoredPosition = Vector2.zero;

            var subtitle = Label(canvas, "Subtitle", "", 40, Bone, Bottom, new Vector2(0, 105), new Vector2(1500, 160), FontStyles.Normal);

            var objLabel = Label(canvas, "ObjectiveLabel", "OBJECTIVE", 22, Ember, TopLeft, new Vector2(60, -50), new Vector2(700, 36), FontStyles.Bold);
            objLabel.alignment = TextAlignmentOptions.Left;
            var objective = Label(canvas, "ObjectiveText", "Approach the figures", 32, Bone, TopLeft, new Vector2(60, -88), new Vector2(900, 50), FontStyles.Normal);
            objective.alignment = TextAlignmentOptions.Left;

            var hint = Label(canvas, "ControlsHint", "WASD move     Mouse look     Shift sprint     E skip line", 22, Ash,
                            new Vector2(1, 1), new Vector2(-60, -50), new Vector2(900, 36), FontStyles.Normal);
            ((RectTransform)hint.transform).pivot = new Vector2(1, 1);
            hint.alignment = TextAlignmentOptions.Right;

            var fade = MakeFade(1.0f, 2.5f);

            var loaderGO = new GameObject("FlowLoader");
            var loader = loaderGO.AddComponent<FlowLoader>();
            loader.cursorOnStart = CursorOnStart.Lock;
            loader.fade = fade;
            portalTrigger.loader = loader;

            // Dialogue zone between the player and the figures
            var zone = new GameObject("QuestionZone");
            zone.transform.position = new Vector3(0, 1.5f, 0.5f);
            var zc = zone.AddComponent<BoxCollider>();
            zc.isTrigger = true;
            zc.size = new Vector3(13.5f, 3f, 3f);
            var dlg = zone.AddComponent<GraveDialogue>();
            dlg.subtitleText = subtitle;
            dlg.objectiveText = objective;
            dlg.activateWhenDone = portal;
            dlg.figureLights = figureLights.ToArray();
            dlg.openingLines = new[] {
                "Where am I?",
                "The earth is cold. It is so quiet.",
                "Someone is waiting for me."
            };
            dlg.lines = new[] {
                "You have arrived where every soul must answer.",
                "Your deeds are not finished with you.",
                "There are moments you left broken.",
                "Return to them. Make them right.",
                "Go, before the fire takes them."
            };

            portal.SetActive(false);
            Save(scene, FrontEndScenes.Grave);
        }

        // ================================================================== BRIEFING 1

        static void BuildBriefing()
        {
            var scene = NewScene();
            MakeCamera(new Vector3(0, 1, -10), Vector3.zero, new Color(0.06f, 0.02f, 0.02f));
            MakeVolume("BriefingVolume", 1.2f, 0.45f);

            var canvas = MakeCanvas("BriefingCanvas", 0);
            MakeEventSystemIfMissing();

            Label(canvas, "TrialNumber", "TRIAL 01", 30, Ember, Top, new Vector2(0, -110), new Vector2(800, 50), FontStyles.Bold);
            Label(canvas, "Fragment", "MEMORY FRAGMENT", 24, Ash, Top, new Vector2(0, -155), new Vector2(800, 40), FontStyles.Bold);
            Label(canvas, "MissionTitle", "The Moment of Anger", 96, Bone, Top, new Vector2(0, -250), new Vector2(1600, 130), FontStyles.Bold);

            Card(canvas, "ObjectiveCard", new Vector2(-560, 20), "MISSION OBJECTIVE",
                 "Reach the injured person and prevent the confrontation from becoming violent.");
            Card(canvas, "TimeCard", new Vector2(0, 20), "TIME LIMIT: 90 SECONDS",
                 "Complete the corrective action before the memory collapses into hellfire.");
            Card(canvas, "ThreatCard", new Vector2(560, 20), "THREAT: SHADOW CREATURES",
                 "Fight or dodge past them. Every hit you take costs you time.");

            Label(canvas, "HowToWin", "Reach the person, then hold E to intervene. A hit interrupts you.", 28, Ash,
                            Mid, new Vector2(0, -230), new Vector2(1500, 50), FontStyles.Italic);

            var enter = MakeButton(canvas, "EnterButton", "ENTER MEMORY", new Vector2(0, -350), new Vector2(520, 100), 42);

            var fade = MakeFade(0f, 0.8f);
            var loader = new GameObject("FlowLoader").AddComponent<FlowLoader>();
            loader.cursorOnStart = CursorOnStart.Show;
            loader.fade = fade;

            UnityEventTools.AddStringPersistentListener(enter.onClick, new UnityAction<string>(loader.SaveProgress), FrontEndScenes.Briefing1);
            UnityEventTools.AddStringPersistentListener(enter.onClick, new UnityAction<string>(loader.LoadScene), FrontEndScenes.Trial1);

            Save(scene, FrontEndScenes.Briefing1);
        }

        // ================================================================== END SCREEN

        static void BuildEndScreen()
        {
            var scene = NewScene();
            MakeCamera(new Vector3(0, 1.6f, -7), new Vector3(4, 0, 0), Night);
            MakeDirectional(new Vector3(35, 150, 0), new Color(0.6f, 0.65f, 0.8f), 0.2f);
            SetFog(new Color(0.03f, 0.04f, 0.06f), 0.04f, new Color(0.06f, 0.07f, 0.09f));
            MakeVolume("EndVolume", 1.6f, 0.4f);
            Prim(PrimitiveType.Plane, "Ground", new Vector3(0, 0, 4), new Vector3(4, 1, 4), Mat("Soil", new Color(0.06f, 0.05f, 0.05f)), null, true);
            MakePortal("Spirit", new Vector3(0, 2.2f, 5f), 3f, Spirit, 6f).SetActive(true);

            var canvas = MakeCanvas("EndCanvas", 0);
            MakeEventSystemIfMissing();

            Label(canvas, "Title", "THE MEMORY IS AT PEACE", 96, Bone, Top, new Vector2(0, -200), new Vector2(1700, 130), FontStyles.Bold);
            Label(canvas, "Line", "One wrong has been made right. Others are still waiting.", 36, Spirit, Top, new Vector2(0, -310), new Vector2(1500, 60), FontStyles.Italic);
            Label(canvas, "Thanks", "Thank you for playing Beyond Time", 34, Ash, Mid, new Vector2(0, 40), new Vector2(1400, 60), FontStyles.Normal);
            Label(canvas, "Credits", "Made by: ADD TEAM NAMES HERE", 28, Ash, Mid, new Vector2(0, -20), new Vector2(1400, 50), FontStyles.Normal);

            var menu = MakeButton(canvas, "MainMenuButton", "MAIN MENU", new Vector2(0, -220), new Vector2(420, 86), 38);
            var quit = MakeButton(canvas, "QuitButton", "QUIT", new Vector2(0, -325), new Vector2(420, 86), 38);

            var fade = MakeFade(0.3f, 1.5f);
            var loader = new GameObject("FlowLoader").AddComponent<FlowLoader>();
            loader.cursorOnStart = CursorOnStart.Show;
            loader.fade = fade;

            UnityEventTools.AddStringPersistentListener(menu.onClick, new UnityAction<string>(loader.LoadScene), FrontEndScenes.MainMenu);
            UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(loader.Quit));

            Save(scene, FrontEndScenes.EndScreen);
        }

        // ================================================================== SCENE HELPERS

        static string ScenePath(string name) => SceneDir + "/" + name + ".unity";

        static bool Confirm(string name)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (!System.IO.File.Exists(ScenePath(name))) return true;
            return EditorUtility.DisplayDialog("Beyond Time", name + " already exists and will be rebuilt from scratch. Continue?", "Rebuild", "Cancel");
        }

        static Scene NewScene()
        {
            EnsureFolder("Assets", "FrontEnd");
            EnsureFolder(Root, "Scenes");
            EnsureFolder(Root, "Art");
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        static void Save(Scene scene, string name)
        {
            EditorSceneManager.SaveScene(scene, ScenePath(name));
            AssetDatabase.SaveAssets();
            Debug.Log("Beyond Time: saved " + ScenePath(name));
        }

        static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var r = FindDeep(c, name);
                if (r) return r;
            }
            return null;
        }

        static Camera MakeCamera(Vector3 pos, Vector3 euler, Color bg)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(euler));
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            return cam;
        }

        static Light MakeDirectional(Vector3 euler, Color color, float intensity)
        {
            var go = new GameObject("Directional Light", typeof(Light));
            go.transform.rotation = Quaternion.Euler(euler);
            var l = go.GetComponent<Light>();
            l.type = LightType.Directional;
            l.color = color;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
            return l;
        }

        static Light MakePointLight(string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = new GameObject(name, typeof(Light));
            go.transform.position = pos;
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            return l;
        }

        static void SetFog(Color fog, float density, Color ambient)
        {
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = density;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
        }

        static void MakeVolume(string profileName, float bloomIntensity, float vignetteIntensity)
        {
            string path = ArtDir + "/" + profileName + ".asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(bloomIntensity);
            bloom.threshold.Override(0.85f);
            AssetDatabase.AddObjectToAsset(bloom, profile);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(vignetteIntensity);
            vignette.smoothness.Override(0.5f);
            AssetDatabase.AddObjectToAsset(vignette, profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var go = new GameObject("Global Volume");
            var v = go.AddComponent<Volume>();
            v.isGlobal = true;
            v.sharedProfile = profile;
        }

        static Material Mat(string name, Color baseColor, Color? emission = null)
        {
            string path = ArtDir + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader != null ? shader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat, Transform parent, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent) go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (mat) go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject MakePortal(string key, Vector3 pos, float size, Color color, float lightIntensity)
        {
            var root = new GameObject("Portal");
            root.transform.position = pos;

            var ring = Mat("PortalRing_" + key, color * 0.4f, color * 4f);
            var core = Mat("PortalCore_" + key, Color.black, color * 0.8f);

            var outer = Prim(PrimitiveType.Cylinder, "Ring", pos, new Vector3(size, 0.03f, size), ring, root.transform, false);
            outer.transform.rotation = Quaternion.Euler(90, 0, 0);
            var inner = Prim(PrimitiveType.Cylinder, "Core", pos + new Vector3(0, 0, -0.12f), new Vector3(size * 0.82f, 0.03f, size * 0.82f), core, root.transform, false);
            inner.transform.rotation = Quaternion.Euler(90, 0, 0);

            var light = MakePointLight("PortalLight", pos + new Vector3(0, 0, -1f), color, lightIntensity, size * 4f);
            light.transform.SetParent(root.transform, true);

            var spin = outer.AddComponent<SlowSpin>();
            spin.degreesPerSecond = new Vector3(0, 60f, 0);
            spin.pulseLight = light;
            return root;
        }

        // ================================================================== UI HELPERS

        static readonly Vector2 Top = new Vector2(0.5f, 1f);
        static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
        static readonly Vector2 TopLeft = new Vector2(0f, 1f);

        static Transform MakeCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static void MakeEventSystemIfMissing()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        static ScreenFade MakeFade(float holdBlack, float duration)
        {
            var canvas = MakeCanvas("FadeCanvas", 100);
            var go = new GameObject("Fade", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(canvas, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;
            go.GetComponent<CanvasGroup>().alpha = 0f;     // invisible while editing; ScreenFade sets it at runtime
            var fade = go.AddComponent<ScreenFade>();
            fade.holdBlackFor = holdBlack;
            fade.duration = duration;
            return fade;
        }

        static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color,
                                    Vector2 anchor, Vector2 pos, Vector2 dim, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor == TopLeft ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dim;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        static GameObject Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        static void Card(Transform parent, string name, Vector2 pos, string title, string body)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Mid;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(500, 300);
            go.GetComponent<Image>().color = new Color(0.08f, 0.05f, 0.05f, 0.92f);

            var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(go.transform, false);
            var art = (RectTransform)accent.transform;
            art.anchorMin = new Vector2(0, 1); art.anchorMax = new Vector2(1, 1);
            art.pivot = new Vector2(0.5f, 1); art.sizeDelta = new Vector2(0, 6); art.anchoredPosition = Vector2.zero;
            accent.GetComponent<Image>().color = Ember;

            Label(go.transform, "Title", title, 28, Ember, Top, new Vector2(0, -50), new Vector2(440, 50), FontStyles.Bold);
            Label(go.transform, "Body", body, 28, Bone, Mid, new Vector2(0, -30), new Vector2(440, 180), FontStyles.Normal);
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(go, "Beyond Time button");
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Mid;
            rt.pivot = Mid;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = Color.white;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor      = new Color(0.10f, 0.09f, 0.10f, 0.92f);
            cb.highlightedColor = new Color(0.42f, 0.14f, 0.06f, 1f);
            cb.pressedColor     = new Color(0.65f, 0.22f, 0.08f, 1f);
            cb.selectedColor    = cb.normalColor;
            cb.disabledColor    = new Color(0.10f, 0.09f, 0.10f, 0.35f);
            cb.colorMultiplier  = 1f;
            btn.colors = cb;

            var t = Label(go.transform, "Text", label, fontSize, Bone, Mid, Vector2.zero, size, FontStyles.Bold);
            var trt = (RectTransform)t.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            return btn;
        }
    }
}
#endif
