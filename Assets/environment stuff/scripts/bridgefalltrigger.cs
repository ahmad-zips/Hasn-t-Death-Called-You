
using UnityEngine;

public class BridgeFallTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Trigger touched by: " + other.name);

            if (other.CompareTag("Player"))
            {
                Debug.Log("PLAYER DETECTED!");

                FallingBridge bridge =
                    GetComponentInParent<FallingBridge>();

                if (bridge != null)
                {
                    bridge.TriggerFall();
                }
                else
                {
                    Debug.LogError("FallingBridge not found in parent!");
                }
            }
        }
    }

