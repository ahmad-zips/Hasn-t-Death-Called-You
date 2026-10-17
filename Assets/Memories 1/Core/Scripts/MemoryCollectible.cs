using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// A pickup for Collect steps (letter pieces, photos, keys).
    /// TO REPLACE THE LOOK: put your model under this object and delete MODEL_REPLACE_ME. Keep the trigger collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MemoryCollectible : MonoBehaviour
    {
        public float spinSpeed = 90f;
        public float bobHeight = 0.25f;
        public AudioClip pickupSound;

        public bool Collected { get; private set; }

        Vector3 basePos;

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            basePos = transform.localPosition;
        }

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * 2f) * bobHeight;
        }

        void OnTriggerEnter(Collider other)
        {
            if (Collected || !other.CompareTag("Player")) return;
            Collected = true;
            if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            gameObject.SetActive(false);
        }

        public void ResetItem()
        {
            Collected = false;
            transform.localPosition = basePos;
            gameObject.SetActive(true);
        }
    }
}
