using System.Collections;
using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Opens something with a smooth move and turn: gates rising, levers pulled, rubble shifting.
    /// Memory steps open it (Open On Start); it closes again when the memory restarts.
    /// Works on any model: put the model under this object.
    /// </summary>
    public class MemoryMover : MonoBehaviour, IMemoryResettable
    {
        public Vector3 openOffset = new Vector3(0f, 4f, 0f);
        public Vector3 openEuler = Vector3.zero;
        public float duration = 1.5f;
        public AudioClip sound;

        public bool IsOpen { get; private set; }

        Vector3 closedPos;
        Quaternion closedRot;

        void Awake() { closedPos = transform.localPosition; closedRot = transform.localRotation; }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (sound) AudioSource.PlayClipAtPoint(sound, transform.position);
            if (!gameObject.activeInHierarchy) { Snap(1f); return; }
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        public void ResetForMemory()
        {
            StopAllCoroutines();
            IsOpen = false;
            Snap(0f);
        }

        IEnumerator Animate()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                Snap(Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            Snap(1f);
        }

        void Snap(float k)
        {
            transform.localPosition = closedPos + openOffset * k;
            transform.localRotation = closedRot * Quaternion.Euler(openEuler * k);
        }
    }
}
