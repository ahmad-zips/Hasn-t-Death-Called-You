using UnityEngine;

namespace BeyondTime.Memories
{
    /// <summary>
    /// OPTIONAL. Connects Ahmad's ShadowScript (Assets/Scripts/ShadowScript.cs) to the memories,
    /// so his shadows cost memory time when they hit the player.
    /// Put this on any one object in the hellfire scene (for example the MemoryUI).
    /// If your branch has no ShadowScript, delete this file.
    /// Do not keep Maryam's GameManager in the same scene as memories, or hits will be counted by both timers.
    /// </summary>
    public class ShadowScriptBridge : MonoBehaviour
    {
        void OnEnable()  { ShadowScript.OnPlayerTouched += Forward; }
        void OnDisable() { ShadowScript.OnPlayerTouched -= Forward; }
        void Forward(float seconds) { MemoryEvents.RaisePlayerHit(seconds); }
    }
}
