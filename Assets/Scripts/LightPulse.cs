using UnityEngine;
using UnityEngine.InputSystem;

public class LightPulse : MonoBehaviour
{
    public float radius = 12f;
    public float stunTime = 3f;
    public float cooldown = 6f;
    public Light flash;            // optional: a bright light on the player, off by default

    float ready;

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && Time.time >= ready)
        {
            ready = Time.time + cooldown;
            foreach (var c in Physics.OverlapSphere(transform.position, radius))
            {
                var m = c.GetComponentInParent<ShadowScript>();
                if (m) m.Stun(stunTime);
            }
            if (flash) StartCoroutine(Flash());
        }
    }

    System.Collections.IEnumerator Flash()
    {
        flash.enabled = true;
        yield return new WaitForSeconds(0.15f);
        flash.enabled = false;
    }
}