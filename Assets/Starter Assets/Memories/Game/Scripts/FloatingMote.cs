using UnityEngine;

namespace BeyondTime.Game
{
    /// <summary>Gentle drifting light (fireflies, petals, motes) for paradise.</summary>
    public class FloatingMote : MonoBehaviour
    {
        public float radius = 1.5f;
        public float speed = 0.3f;
        Vector3 home; float seed;
        void Start() { home = transform.position; seed = Random.value * 100f; }
        void Update()
        {
            float t = Time.time * speed + seed;
            transform.position = home + new Vector3(Mathf.Sin(t) * radius, Mathf.Sin(t * 1.7f) * 0.4f, Mathf.Cos(t * 0.8f) * radius);
        }
    }
}
