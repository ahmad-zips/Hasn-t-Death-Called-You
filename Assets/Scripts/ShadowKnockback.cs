using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ShadowKnockback : MonoBehaviour
{
    CharacterController cc;
    Vector3 vel;

    void Awake() { cc = GetComponent<CharacterController>(); }

    public void Push(Vector3 dir, float force)
    {
        dir.y = 0;
        vel = dir.normalized * force;
    }

    void Update()
    {
        if (vel.sqrMagnitude < 0.01f) return;
        cc.Move(vel * Time.deltaTime);
        vel = Vector3.Lerp(vel, Vector3.zero, 6f * Time.deltaTime);
    }
}