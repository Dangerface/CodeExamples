using UnityEngine;

public class BodyAlignment : MonoBehaviour
{
[Header("References")]
public Rigidbody playerRigidbody;
public FootController leftFoot;
public FootController rightFoot;
public Transform leftFootRestTarget;
public Transform rightFootRestTarget;

[Header("Rotation Limit")]
public float maxAirRotationDegrees = 60f;

[Header("Alignment Speeds")]
public float alignToSlopeStrength = 20f;

[Header("Landing Prediction")]
public float predictionTimeStep = 0.1f;
public int predictionStepsInitial = 15;
public int predictionStepsMax = 60;
public LayerMask groundLayerMask;

[Header("Body Grounded")]
public float bodyGroundRaycastDistance = 1.5f;

private float _targetAngle = 0f;
private bool _hasTarget = false;
private bool _wasGrounded = false;

private bool DetectSlope(out float slopeAngle)
{
    Vector3 rayDir = -transform.up;
    Vector3[] origins = new Vector3[]
    {
        transform.position,
        transform.TransformPoint(leftFootRestTarget.localPosition),
        transform.TransformPoint(rightFootRestTarget.localPosition)
    };

    float closestDistance = float.MaxValue;
    slopeAngle = 0f;
    bool anyHit = false;

    foreach (Vector3 origin in origins)
    {
        if (Physics.Raycast(origin, rayDir, out RaycastHit hit, bodyGroundRaycastDistance, groundLayerMask))
        {
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                slopeAngle = Vector3.SignedAngle(Vector3.up, hit.normal, Vector3.forward);
                anyHit = true;
            }
        }
    }

    return anyHit;
}

private void FixedUpdate()
{
    float currentAngle = transform.eulerAngles.z;
    if (currentAngle > 180f) currentAngle -= 360f;

    bool bodyGrounded = DetectSlope(out _);
    bool feetGrounded = leftFoot.connected || rightFoot.connected;
    bool fullyAirborne = !bodyGrounded && !feetGrounded;

    if (bodyGrounded)
    {
        playerRigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
        return;
    }

    playerRigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

    if (feetGrounded)
    {
        AlignToAngle(currentAngle, 0f, alignToSlopeStrength);
        if (Mathf.Abs(currentAngle) < 2f)
            playerRigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }
    else if (fullyAirborne)
    {
        RunPrediction(playerRigidbody.linearVelocity);
        AirAlign(currentAngle);
    }
}

public void OnJump(Vector3 velocityAtJump)
{
    RunPrediction(velocityAtJump);
}

private void RunPrediction(Vector3 velocity)
{
    int steps = predictionStepsInitial;
    while (steps <= predictionStepsMax)
    {
        if (PredictLanding(velocity, steps, out RaycastHit hit))
        {
            float slopeAngle = Vector3.SignedAngle(Vector3.up, hit.normal, Vector3.forward);
            float absSlopeAngle = Mathf.Abs(slopeAngle);
            float sign = Mathf.Sign(slopeAngle);

            if (absSlopeAngle <= 60f)
                _targetAngle = slopeAngle;
            else if (absSlopeAngle <= 90f)
                _targetAngle = sign * (90f - absSlopeAngle);
            else
                _targetAngle = 0f;

            _targetAngle = Mathf.Clamp(_targetAngle, -maxAirRotationDegrees, maxAirRotationDegrees);
            _hasTarget = true;
            return;
        }

        steps *= 2;
    }

    _targetAngle = 0f;
    _hasTarget = false;
}

private void AirAlign(float currentAngle)
{
    float target = _hasTarget ? _targetAngle : 0f;
    AlignToAngle(currentAngle, target, alignToSlopeStrength);
}

private bool PredictLanding(Vector3 velocity, int steps, out RaycastHit landingHit)
{
    Vector3 pos = transform.position;
    Vector3 vel = velocity;

    for (int i = 0; i < steps; i++)
    {
        Vector3 nextPos = pos + vel * predictionTimeStep;
        vel += Physics.gravity * predictionTimeStep;

        // Cast from center, left and right offset
        Vector3[] origins = new Vector3[]
        {
            pos,
            pos + transform.right * 0.5f,
            pos - transform.right * 0.5f
        };

        RaycastHit flattest = default;
        float flattestAngle = float.MaxValue;
        bool anyHit = false;

        foreach (Vector3 origin in origins)
        {
            if (Physics.Raycast(origin, nextPos - pos, out RaycastHit hit, Vector3.Distance(pos, nextPos), groundLayerMask))
            {
                float angle = Mathf.Abs(Vector3.SignedAngle(Vector3.up, hit.normal, Vector3.forward));
                if (angle < flattestAngle)
                {
                    flattestAngle = angle;
                    flattest = hit;
                    anyHit = true;
                }
            }
        }

        if (anyHit)
        {
            landingHit = flattest;
            return true;
        }

        pos = nextPos;
    }

    landingHit = default;
    return false;
}
private void AlignToAngle(float currentAngle, float targetAngle, float springStrength)
{
    float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

    if (Mathf.Abs(angleDiff) < 2f)
        playerRigidbody.angularVelocity = Vector3.zero;
    else
        playerRigidbody.angularVelocity *= 0.85f;

    playerRigidbody.AddTorque(0f, 0f, angleDiff * springStrength);
}
}