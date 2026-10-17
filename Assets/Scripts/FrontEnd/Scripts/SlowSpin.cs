using UnityEngine;

namespace BeyondTime.FrontEnd
{
    /// <summary>Spins an object and optionally pulses a light. Used on the portals.</summary>
    public class SlowSpin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 0f, 40f);
        public Light pulseLight;
        public float pulseAmount = 0.35f;   // 0.35 = intensity swings 35% either way
        public float pulseSpeed = 2f;

        float baseIntensity;

        void Start() { if (pulseLight) baseIntensity = pulseLight.intensity; }

        void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
            if (pulseLight)
                pulseLight.intensity = baseIntensity * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
        }
    }
}
