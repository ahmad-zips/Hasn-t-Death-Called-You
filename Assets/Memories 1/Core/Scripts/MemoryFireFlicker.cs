using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>Flickers a light like fire. Used on the placeholder fires.</summary>
    [RequireComponent(typeof(Light))]
    public class MemoryFireFlicker : MonoBehaviour
    {
        public float min = 2f, max = 6f, speed = 7f;
        Light lt; float seed;
        void Awake() { lt = GetComponent<Light>(); seed = Random.value * 100f; }
        void Update() { lt.intensity = Mathf.Lerp(min, max, Mathf.PerlinNoise(seed, Time.time * speed)); }
    }
}
