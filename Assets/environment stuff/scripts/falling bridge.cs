
using UnityEngine;
using System.Collections;

public class FallingBridge : MonoBehaviour
{
    private Rigidbody rb;
    private bool triggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void TriggerFall()
    {
        if (!triggered)
        {
            triggered = true;
            StartCoroutine(FallAfterDelay());
        }
    }

    IEnumerator FallAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        rb.isKinematic = false;
        rb.useGravity = true;

        Debug.Log("Bridge is falling!");
    }
}
