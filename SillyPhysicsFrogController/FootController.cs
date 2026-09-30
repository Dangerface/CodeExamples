using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(Rigidbody))]
public class FootController : MonoBehaviour
{
    public float ExtensionAmount { get; private set; }
    private const float MAX_EXTENSION_DISTANCE = 4f;
    private const float AIR_MASS = 0.5f;

    private ConfigurableJoint _joint;
    private Rigidbody _rb;

    public bool connected;
    public bool isLeftLeg;

    private int _contactCount;
    private float _originalMass;

    private Vector3 _currentTarget;

    public Transform hip;
    public Transform footRestTarget;

    private float minAngle = 40f;
    private float maxAngle = 280f;

    [Header("Grip Settings")]
    public float footLateralDamping = 80f;

private void Awake()
{
    _joint = GetComponent<ConfigurableJoint>();
    _rb = GetComponent<Rigidbody>();
    _originalMass = _rb.mass;
}

private void FixedUpdate()
{
    if (!connected)
    {
        if (_rb.mass != AIR_MASS)
            _rb.mass = AIR_MASS;
    }
    else
    {
        if (_rb.mass != _originalMass)
            _rb.mass = _originalMass;
    }

    if (connected && ExtensionAmount > 0.1f)
    {
        Vector3 lateralVelocity = new Vector3(_rb.linearVelocity.x, 0f, 0f);
        _rb.AddForce(-lateralVelocity * footLateralDamping, ForceMode.Force);
    }
}

private void OnCollisionEnter(Collision collision)
{
    _contactCount++;
    connected = _contactCount > 0;
}

private void OnCollisionExit(Collision collision)
{
    _contactCount = Mathf.Max(0, _contactCount - 1);
    connected = _contactCount > 0;
}

public void SetLegInput(Vector2 input)
{
    if (_joint == null)
        return;

    Vector3 restLocalPos = -_joint.connectedBody.transform.InverseTransformPoint(footRestTarget.position);

    if (input.sqrMagnitude < 0.0001f)
    {
        _joint.targetPosition = restLocalPos;
        ExtensionAmount = 0f;
        return;
    }

    Vector2 clamped = Vector2.ClampMagnitude(input, 1f);
    ExtensionAmount = clamped.magnitude;

    if (isLeftLeg)
        clamped.x *= -1f;

    float angle = Mathf.Atan2(clamped.y, clamped.x) * Mathf.Rad2Deg;
    if (angle < 0) angle += 360f;

    float extensionMultiplier = 1f;
    float clampedAngle = angle;

    if (!isLeftLeg)
    {
        bool inFreeZone = angle >= minAngle && angle <= maxAngle;
        if (!inFreeZone)
        {
            if (angle < minAngle)
            {
                clampedAngle = minAngle;
                extensionMultiplier = Mathf.Lerp(0.05f, 1f, (angle - minAngle) / (60f - minAngle));
            }
            else if (angle > maxAngle)
            {
                clampedAngle = maxAngle;
                extensionMultiplier = Mathf.Lerp(1f, 0.05f, (angle - maxAngle) / (80f - maxAngle));
            }
        }
    }
    else
    {
        bool inFreeZone = angle >= minAngle && angle <= maxAngle;
        if (!inFreeZone)
        {
            if (angle < minAngle)
            {
                clampedAngle = minAngle;
                extensionMultiplier = Mathf.Lerp(0.05f, 1f, (angle - minAngle) / (60f - minAngle));
            }
            else if (angle > maxAngle)
            {
                clampedAngle = maxAngle;
                extensionMultiplier = Mathf.Lerp(1f, 0.05f, (angle - maxAngle) / (80f - maxAngle));
            }
        }
    }

    Vector2 finalClamped = new Vector2(
        Mathf.Cos(clampedAngle * Mathf.Deg2Rad),
        Mathf.Sin(clampedAngle * Mathf.Deg2Rad)
    ) * clamped.magnitude * extensionMultiplier;

    float xDir = isLeftLeg ? finalClamped.x : -finalClamped.x;
    Vector3 extendDirWorld = new Vector3(xDir, -finalClamped.y, 0f).normalized;
    Vector3 targetWorld = hip.position + extendDirWorld * (finalClamped.magnitude * MAX_EXTENSION_DISTANCE);
    Vector3 extendTarget = -_joint.connectedBody.transform.InverseTransformPoint(targetWorld);

    _currentTarget = Vector3.Lerp(restLocalPos, extendTarget, finalClamped.magnitude);
    _joint.targetPosition = _currentTarget;
}
}