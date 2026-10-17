using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>Put on someone who must survive. If they are defeated while their memory runs, the memory collapses.</summary>
    [RequireComponent(typeof(Fighter))]
    public class MemoryProtectee : MonoBehaviour
    {
        [TextArea] public string failText = "He died in the alley. Again.";

        Fighter fighter;
        MemoryZone zone;

        void Awake()
        {
            fighter = GetComponent<Fighter>();
            zone = GetComponentInParent<MemoryZone>();
        }

        void OnEnable()  { fighter.Defeated += OnDefeated; }
        void OnDisable() { fighter.Defeated -= OnDefeated; }

        void OnDefeated(Fighter f)
        {
            var z = zone ? zone : MemoryZone.Active;
            if (z && z.IsRunning) z.FailMemory(failText);
        }
    }
}
