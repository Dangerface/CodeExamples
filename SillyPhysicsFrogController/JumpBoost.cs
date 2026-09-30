using UnityEngine;

public class JumpBoost : MonoBehaviour
{
    [Header("References")]
    public FootController leftFoot;
    public FootController rightFoot;
    public Rigidbody playerRigidbody;

    [Header("Trigger Settings")]
    [Range(0f, 1f)]
    public float extensionThreshold = 0.9f;
    public float simultaneousLeaveWindow = 0.15f;

    [Header("Boost Settings")]
    public float boostForce = 10f;
    public float boostDuration = 1f;
    [Header("Body Alignment")]
    private BodyAlignment _bodyAlignment;

    private float _leftLiftoffTime = -999f;
    private float _rightLiftoffTime = -999f;
    private bool _leftWasExtended = false;
    private bool _rightWasExtended = false;
    private bool _leftWasGrounded = false;
    private bool _rightWasGrounded = false;

    private bool _boosting = false;
    private float _boostTimer = 0f;
    private Vector3 _boostDirection;

private void Awake()
{
    _bodyAlignment = GetComponent<BodyAlignment>();
}

private void Update()
{
    bool leftExtended = leftFoot.ExtensionAmount >= extensionThreshold;
    bool rightExtended = rightFoot.ExtensionAmount >= extensionThreshold;

    // Detect left foot liftoff
    if (_leftWasGrounded && !leftFoot.connected)
    {
        if (leftExtended || _leftWasExtended)
        {
            _leftLiftoffTime = Time.time;
        }
    }

    // Detect right foot liftoff
    if (_rightWasGrounded && !rightFoot.connected)
    {
        if (rightExtended || _rightWasExtended)
        {
            _rightLiftoffTime = Time.time;
        }
    }

    // Check if both lifted off within time window
    if (!_boosting &&
        _leftLiftoffTime > 0 && _rightLiftoffTime > 0 &&
        Mathf.Abs(_leftLiftoffTime - _rightLiftoffTime) <= simultaneousLeaveWindow)
    {
        _boosting = true;
        _boostTimer = 0f;
        _boostDirection = playerRigidbody.linearVelocity.normalized;
        _leftLiftoffTime = -999f;
        _rightLiftoffTime = -999f;

        Vector3 estimatedVelocity = playerRigidbody.linearVelocity + _boostDirection * boostForce;
        _bodyAlignment.OnJump(estimatedVelocity);
    }

    // Track extended state just before leaving ground
    if (leftFoot.connected) _leftWasExtended = leftExtended;
    if (rightFoot.connected) _rightWasExtended = rightExtended;

    _leftWasGrounded = leftFoot.connected;
    _rightWasGrounded = rightFoot.connected;
}

private void FixedUpdate()
{
    if (_boosting)
    {
        _boostTimer += Time.fixedDeltaTime;
        playerRigidbody.AddForce(_boostDirection * boostForce, ForceMode.Force);
        

        if (_boostTimer >= boostDuration)
            _boosting = false;
    }
}
}