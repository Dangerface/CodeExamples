using UnityEngine;

public class FrogGrip : MonoBehaviour
{
    [Header("References")]
    public Rigidbody playerRigidbody;
    public FootController leftFoot;
    public FootController rightFoot;

    [Header("Grip Settings")]
    public float gripForce = 150f;
    public float maxBodySpeedToGrip = 5f;

    private void FixedUpdate()
    {
        ApplyGrip(leftFoot);
        ApplyGrip(rightFoot);
    }

    private void ApplyGrip(FootController foot)
    {
        if (!foot.connected) return;
        if (foot.ExtensionAmount < 0.1f) return;

        float bodySpeed = playerRigidbody.linearVelocity.magnitude;
        if (bodySpeed > maxBodySpeedToGrip) return;

        // Direction from body toward foot
        Vector3 toFoot = foot.transform.position - playerRigidbody.position;
        toFoot.z = 0f;

        if (toFoot.sqrMagnitude < 0.01f) return;

        playerRigidbody.AddForce(toFoot.normalized * gripForce, ForceMode.Force);
    }
}