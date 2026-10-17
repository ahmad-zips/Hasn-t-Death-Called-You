using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// ONE place to swap shared assets for every memory.
    /// Lives at Assets/Memories/Resources/MemorySettings.asset.
    /// Drag your real shadow enemy prefab into Shadow Prefab and every memory uses it.
    /// </summary>
    [CreateAssetMenu(menuName = "Beyond Time/Memory Settings", fileName = "MemorySettings")]
    public class MemorySettings : ScriptableObject
    {
        [Header("Swap these for your real assets")]
        [Tooltip("Spawned by every memory unless a memory sets its own override.")]
        public GameObject shadowPrefab;

        [Header("Rules")]
        public int maxAliveShadows = 8;
        [Tooltip("Off while developing: every Play starts fresh. Turn on for the final build so Continue remembers finished memories.")]
        public bool saveProgressBetweenSessions = false;

        [Header("Story text")]
        public float typewriterCharsPerSecond = 45f;

        static MemorySettings cached;
        public static MemorySettings Load()
        {
            if (cached == null) cached = Resources.Load<MemorySettings>("MemorySettings");
            return cached;
        }
    }
}
