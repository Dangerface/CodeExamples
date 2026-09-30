using UnityEngine;

public class FrogLegController : MonoBehaviour
{
    [Header("Right Leg IK")]
    public Transform rightHip;
    public Transform rightFoot;
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    [Range(-180f, 180f)]
    public float rightKneeBendAngle = -90f;

    [Header("Left Leg IK")]
    public Transform leftHip;
    public Transform leftFoot;
    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    [Range(-180f, 180f)]
    public float leftKneeBendAngle = 90f;

    [Header("Leg Lengths")]
    public float upperLegLength = 1f;
    public float lowerLegLength = 1f;

    private Vector3 _rightUpperLegOriginalScale;
    private Vector3 _leftUpperLegOriginalScale;
    private Vector3 _rightLowerLegOriginalScale;
    private Vector3 _leftLowerLegOriginalScale;

    private float segmentOverlap = 0.5f;

    void Start()
{
    _rightUpperLegOriginalScale = rightUpperLeg.localScale;
    _leftUpperLegOriginalScale = leftUpperLeg.localScale;
    _rightLowerLegOriginalScale = rightLowerLeg.localScale;
    _leftLowerLegOriginalScale = leftLowerLeg.localScale;
}

    void LateUpdate()
{
    SolveTwoBoneIK(rightHip, rightFoot, rightUpperLeg, rightLowerLeg, rightKneeBendAngle, _rightUpperLegOriginalScale, _rightLowerLegOriginalScale);
    SolveTwoBoneIK(leftHip, leftFoot, leftUpperLeg, leftLowerLeg, leftKneeBendAngle, _leftUpperLegOriginalScale, _leftLowerLegOriginalScale);
}


    void SolveTwoBoneIK(Transform hip, Transform foot, Transform upperLeg, Transform lowerLeg, float kneeBendAngle, Vector3 upperOriginalScale, Vector3 lowerOriginalScale)
{
    Vector3 hipPos = hip.position;
    Vector3 footPos = foot.position;

    float a = upperLegLength;
    float b = lowerLegLength;
    float c = Mathf.Clamp(Vector3.Distance(hipPos, footPos), Mathf.Abs(a - b) + 0.001f, a + b - 0.001f);

    float cosA = (a * a + c * c - b * b) / (2f * a * c);
    cosA = Mathf.Clamp(cosA, -1f, 1f);
    float angleA = Mathf.Acos(cosA) * Mathf.Rad2Deg;

    Vector3 hipToFoot = (footPos - hipPos).normalized;
    Vector3 poleDir = Quaternion.AngleAxis(kneeBendAngle, Vector3.forward) * hipToFoot;
    float poleSign = Mathf.Sign(Vector3.Cross(hipToFoot, poleDir).z);

    Vector3 kneeDir = Quaternion.AngleAxis(-angleA * poleSign, Vector3.forward) * hipToFoot;
    Vector3 kneePos = hipPos + kneeDir * a;

    float upperDist = Vector3.Distance(hipPos, kneePos);
    upperLeg.localScale = new Vector3(upperOriginalScale.x, upperOriginalScale.y * ((upperDist + segmentOverlap) / upperLegLength), upperOriginalScale.z);
    PointSegmentAt(upperLeg, hipPos, kneePos, 90f);

    float lowerDist = Vector3.Distance(kneePos, footPos);
    lowerLeg.localScale = new Vector3(lowerOriginalScale.x, lowerOriginalScale.y * ((lowerDist + segmentOverlap) / lowerLegLength), lowerOriginalScale.z);

    Vector3 lowerDir = (footPos - kneePos).normalized;
    lowerLeg.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(lowerDir.y, lowerDir.x) * Mathf.Rad2Deg + 90f);
    lowerLeg.position = footPos - lowerDir * (lowerDist * 0.5f);
}

    void PointSegmentAt(Transform segment, Vector3 from, Vector3 to, float rotOffset)
    {
        Vector3 dir = (to - from).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        segment.rotation = Quaternion.Euler(0f, 0f, angle + rotOffset);

        float length = Vector3.Distance(from, to);
        segment.position = from + dir * (length * 0.5f);
    }
}