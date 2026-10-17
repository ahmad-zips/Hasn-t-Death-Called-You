#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Beyond Time - builds the whole HUD + GameManager + Hellfire in the open scene with one click.
/// Put this file in Assets/Editor/, then use the menu: Tools > Beyond Time > Build HUD
/// </summary>
public static class BuildBeyondTimeHUD
{
    [MenuItem("Tools/Beyond Time/Build HUD")]
    public static void Build()
    {
        if (Object.FindFirstObjectByType<GameManager>() != null)
        {
            EditorUtility.DisplayDialog("Beyond Time",
                "This scene already has a GameManager. Delete it (and the Canvas) first if you want to rebuild.", "OK");
            return;
        }

        // ---------- Canvas ----------
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build HUD");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // ---------- HUD texts (created first so panels draw on top) ----------
        var top = new Vector2(0.5f, 1f);
        var mid = new Vector2(0.5f, 0.5f);

        var timer = MakeText(canvasGO.transform, "TimerText", "01:30", 72, Color.white,
                             top, new Vector2(0, -60), new Vector2(400, 100));
        var penalty = MakeText(canvasGO.transform, "PenaltyText", "-3s", 40, new Color32(255, 60, 60, 255),
                               top, new Vector2(0, -150), new Vector2(300, 60));
        penalty.gameObject.SetActive(false);

        // ---------- Panels ----------
        var winPanel = MakePanel(canvasGO.transform, "WinPanel", new Color32(0, 0, 0, 170));
        MakeText(winPanel.transform, "Title", "MEMORY STABILIZED", 90, Color.white,
                 mid, new Vector2(0, 120), new Vector2(1200, 150));

        var failPanel = MakePanel(canvasGO.transform, "FailPanel", new Color32(120, 10, 0, 220));
        MakeText(failPanel.transform, "Title", "CONSUMED BY HELLFIRE", 90, Color.white,
                 mid, new Vector2(0, 120), new Vector2(1200, 150));
        var retryBtn = MakeButton(failPanel.transform, "RETRY");

        var pausePanel = MakePanel(canvasGO.transform, "PausePanel", new Color32(0, 0, 0, 200));
        MakeText(pausePanel.transform, "Title", "PAUSED", 90, Color.white,
                 mid, new Vector2(0, 120), new Vector2(1200, 150));
        var resumeBtn = MakeButton(pausePanel.transform, "RESUME");

        winPanel.SetActive(false);
        failPanel.SetActive(false);
        pausePanel.SetActive(false);

        // ---------- EventSystem (needed for buttons) ----------
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            Undo.RegisterCreatedObjectUndo(es, "Build HUD");
        }

        // ---------- GameManager ----------
        var gmGO = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(gmGO, "Build HUD");
        var gm = gmGO.AddComponent<GameManager>();
        gm.totalTime = 90f;
        gm.pressureAt = 30f;
        gm.finalAt = 10f;
        gm.timerText = timer;
        gm.penaltyText = penalty;
        gm.winPanel = winPanel;
        gm.failPanel = failPanel;
        gm.pausePanel = pausePanel;

        UnityEventTools.AddPersistentListener(retryBtn.onClick, new UnityAction(gm.Retry));
        UnityEventTools.AddPersistentListener(resumeBtn.onClick, new UnityAction(gm.Resume));

        // ---------- Hellfire ----------
        var hfGO = new GameObject("Hellfire");
        Undo.RegisterCreatedObjectUndo(hfGO, "Build HUD");
        var hf = hfGO.AddComponent<HellfireController>();

        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) { hf.sun = l; break; }

        Vector3 origin = Vector3.zero, fwd = Vector3.forward;
        var player = GameObject.FindWithTag("Player");
        if (player) { origin = player.transform.position; fwd = player.transform.forward; }

        var pressureFire = MakeFireLight("PressureFire", origin + fwd * 20f + Vector3.up * 4f,
                                         new Color32(255, 80, 0, 255), 30f, 40f);
        var finalFire = MakeFireLight("FinalFire", origin + fwd * 8f + Vector3.up * 2f,
                                      new Color32(255, 60, 0, 255), 60f, 25f);
        hf.pressureFire = new[] { pressureFire };
        hf.finalFire = new[] { finalFire };

        // ---------- Add this scene to Build Settings (Retry needs it) ----------
        var scene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(scene.path))
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!list.Exists(s => s.path == scene.path))
            {
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }
        else
        {
            Debug.LogWarning("Beyond Time: save the scene first (File > Save As), then run this again or add it in Build Profiles.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = gmGO;
        Debug.Log("Beyond Time: HUD built. Press Ctrl+S, then commit AND push right away.");
    }

    // ---------- helpers ----------
    static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size, Color color,
                                    Vector2 anchor, Vector2 pos, Vector2 dim)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;

        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static GameObject MakePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static Button MakeButton(Transform parent, string label)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -80);
        rt.sizeDelta = new Vector2(400, 100);

        var img = go.GetComponent<Image>();
        img.color = new Color32(40, 40, 40, 235);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        var txt = MakeText(go.transform, "Text", label, 48, Color.white,
                           new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 100));
        var trt = (RectTransform)txt.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        return btn;
    }

    static GameObject MakeFireLight(string name, Vector3 pos, Color color, float intensity, float range)
    {
        var go = new GameObject(name, typeof(Light));
        Undo.RegisterCreatedObjectUndo(go, "Build HUD");
        go.transform.position = pos;
        var l = go.GetComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        return go;
    }
}
#endif
