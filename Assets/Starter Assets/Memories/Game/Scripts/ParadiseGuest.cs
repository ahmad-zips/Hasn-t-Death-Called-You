using UnityEngine;
using BeyondTime.Memories;

namespace BeyondTime.Game
{
    /// <summary>Someone you saved. Only appears in paradise if you redeemed their memory.</summary>
    public class ParadiseGuest : MonoBehaviour
    {
        public string memoryId = "memory_01_alley";
        public bool showWhenNotRedeemed = false;

        void Start()
        {
            bool redeemed = MemoryProgress.IsRedeemed(memoryId);
            if (!redeemed && !showWhenNotRedeemed) gameObject.SetActive(false);
        }
    }
}
