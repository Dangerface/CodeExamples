using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInputManager : MonoBehaviour
{
    [Header("Right Leg")]
    [SerializeField] private InputActionReference _rightStickAction;
    [SerializeField] private FootController _rightFoot;

    [Header("Left Leg")]
    [SerializeField] private InputActionReference _leftStickAction;
    [SerializeField] private FootController _leftFoot;

    [SerializeField] private float _deadzone = 0.1f;
    [SerializeField] private bool _invertX = true;
    [SerializeField] private bool _invertY = true;

    private void OnEnable()
    {
        if (_rightStickAction != null)
            _rightStickAction.action.Enable();

        if (_leftStickAction != null)
            _leftStickAction.action.Enable();
    }

    private void OnDisable()
    {
        if (_rightStickAction != null)
            _rightStickAction.action.Disable();

        if (_leftStickAction != null)
            _leftStickAction.action.Disable();
    }

    private void Update()
    {
        HandleStick(_rightStickAction, _rightFoot);
        HandleStick(_leftStickAction, _leftFoot);
    }

    private void HandleStick(InputActionReference actionRef, FootController foot)
    {
        if (actionRef == null || foot == null)
            return;

        Vector2 stick = actionRef.action.ReadValue<Vector2>();

        if (_invertX)
            stick.x *= -1f;

        if (_invertY)
            stick.y *= -1f;

        if (stick.magnitude < _deadzone)
            stick = Vector2.zero;

        foot.SetLegInput(stick);
    }
}
