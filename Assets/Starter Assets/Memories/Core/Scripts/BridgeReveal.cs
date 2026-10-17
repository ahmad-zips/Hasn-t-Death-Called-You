using System.Collections;
using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Bridges that rise out of the lava when a memory is completed.
    /// Leave the bridge INACTIVE in the scene and add it to the memory's Unlock On Complete list.
    /// Works with any bridge model.
    /// </summary>
    public class BridgeReveal : MonoBehaviour
    {
        public float riseFrom = -6f;     // metres below its final position
        public float duration = 2f;
        public AudioClip riseSound;

        Vector3 finalPos;
        bool hasPos;

        void Awake() { finalPos = transform.position; hasPos = true; }

        void OnEnable()
        {
            if (!hasPos) return;
            StopAllCoroutines();
            StartCoroutine(Rise());
        }

        IEnumerator Rise()
        {
            if (riseSound) AudioSource.PlayClipAtPoint(riseSound, finalPos);
            Vector3 from = finalPos + Vector3.up * riseFrom;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, finalPos, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            transform.position = finalPos;
        }
    }
}
