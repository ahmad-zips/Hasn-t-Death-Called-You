using UnityEngine;

/// <summary>
/// Beyond Time - builds the hellfire atmosphere as the timer runs down.
/// Last 30s: light and fog blend toward red, distant fire appears.
/// Last 10s: close fire appears and the alarm plays.
/// On win: everything returns to calm and the fire is switched off.
/// </summary>
public class HellfireController : MonoBehaviour
{
    [Header("Lighting")]
    public Light sun;                                   // the Directional Light
    public Color calmLight = Color.white;
    public Color hellLight = new Color(1f, 0.25f, 0.05f);
    public Color calmFog = new Color(0.08f, 0.08f, 0.1f);
    public Color hellFog = new Color(0.45f, 0.05f, 0.02f);
    public float calmFogDensity = 0.004f;
    public float hellFogDensity = 0.03f;

    [Header("Fire objects (drag fire particles / red lights here)")]
    public GameObject[] pressureFire;   // switched on in the last 30s
    public GameObject[] finalFire;      // switched on in the last 10s

    [Header("Audio (optional)")]
    public AudioSource alarm;

    GameManager gm;
    bool won;

    void Start()
    {
        gm = GameManager.Instance;
        if (gm == null) { Debug.LogError("HellfireController: no GameManager in scene."); enabled = false; return; }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        SetFire(pressureFire, false);
        SetFire(finalFire, false);

        gm.OnPhaseChanged += OnPhase;
        gm.OnWin += OnWin;
    }

    void OnDestroy()
    {
        if (gm == null) return;
        gm.OnPhaseChanged -= OnPhase;
        gm.OnWin -= OnWin;
    }

    void Update()
    {
        // 0 = calm, 1 = fully consumed. Starts rising when the pressure phase begins.
        float t = won ? 0f : Mathf.InverseLerp(gm.pressureAt, 0f, gm.TimeLeft);

        if (sun) sun.color = Color.Lerp(calmLight, hellLight, t);
        RenderSettings.fogColor = Color.Lerp(calmFog, hellFog, t);
        RenderSettings.fogDensity = Mathf.Lerp(calmFogDensity, hellFogDensity, t);
    }

    void OnPhase(GameManager.Phase p)
    {
        if (won) return;
        SetFire(pressureFire, p == GameManager.Phase.Pressure || p == GameManager.Phase.Final);
        SetFire(finalFire, p == GameManager.Phase.Final);
        if (alarm && p == GameManager.Phase.Final) alarm.Play();
    }

    void OnWin()
    {
        won = true;
        SetFire(pressureFire, false);
        SetFire(finalFire, false);
        if (alarm) alarm.Stop();
    }

    static void SetFire(GameObject[] list, bool on)
    {
        if (list == null) return;
        foreach (var g in list) if (g) g.SetActive(on);
    }
}
