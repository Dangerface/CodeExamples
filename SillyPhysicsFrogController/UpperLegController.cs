using UnityEngine;

public class UpperLegController : MonoBehaviour
{
    public Transform hip;
    public Transform knee;

    public Transform upperLeg;
    public Transform upperLegStart; // hip marker
    public Transform upperLegEnd;   // knee marker

    void LateUpdate()
    {
        UpdateUpperLeg();
    }

    void UpdateUpperLeg()
    {
        // Direction from hip to knee
        Vector3 rawDir = knee.position - hip.position;

        if (rawDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 dir = rawDir.normalized;

        // Store hip world position BEFORE rotation
        Vector3 hipWorldBefore = upperLegStart.position;

        // Rotate upper leg to face knee
        Quaternion targetRot =
            Quaternion.LookRotation(Vector3.forward, dir);

        upperLeg.rotation = targetRot;

        // Correct position so hip stays glued
        Vector3 hipWorldAfter = upperLegStart.position;
        upperLeg.position += (hipWorldBefore - hipWorldAfter);
    }
}