using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public enum TouchState
    {
        None,
        Pending,
        Holding,
        Swiping,
        DoubleTapped
    }

    public enum SwipeDirection
    {
        Left, Right, Up, Down,
        UpLeft, UpRight, DownLeft, DownRight,
        None
    }

    public static event Action OnSingleTapDetected;
    public static event Action OnQuickTapDetected;
    public static event Action OnHoldDetected;
    public static event Action OnHoldCanceled;
    public static event Action OnEndTouchDetected;

    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;
    public static event Action OnSwipeUp;
    public static event Action OnSwipeDown;
    public static event Action OnSwipeUpLeft;
    public static event Action OnSwipeUpRight;
    public static event Action OnSwipeDownLeft;
    public static event Action OnSwipeDownRight;

    public delegate void FirstTouchDetectedEventHandler(Vector2 startPos);
    public static event FirstTouchDetectedEventHandler OnFirstTouchDetected;

    public static InputManager Instance { get; private set; }

    [Header("Thresholds")]
    [Tooltip("Normalized distance (0-1) required to register a swipe.")]
    [SerializeField] private float minSwipeDistance = 0.1f; 
    
    [Tooltip("Time in seconds before a stationary touch becomes a Hold.")]
    [SerializeField] private float holdTimeThreshold = 0.15f;
    
    [Tooltip("Max time to be considered a Tap.")]
    [SerializeField] private float maxTapTime = 0.15f;
    
    [SerializeField] private float doubleTapInterval = 0.2f;

    private static float _touchStartTime;

    private float editorSwipeSpeedMultiplier = 1.0f;
    private float androidSwipeSpeedMultiplier = 4.8f;
    private float iosSwipeSpeedMultiplier = 1.0f;
    private float _cachedPlatformSwipeSpeedMultiplier;

    private PlayerControls playercontrols;
    private TouchState currentState = TouchState.None;
    
    private Vector2 startPos;
    private Vector2 currentPos;
    private Vector2 lastPosition;
    private Vector2 firstTapPosition = Vector2.zero;
    
    private double swipeStartTime;
    private float lastTapTime = 0f;
    
    private float effectiveScreenWidth;
    private float cachedDPI;
    
    private float _swipeSpeed;
    private float _lastCompletedSwipeTime;
    private float _lastCompletedSwipeDistanceValue;
    
    public Vector2 PixelDelta { get; private set; }
    public Vector2 endPos;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); }

        playercontrols = new PlayerControls();

        #if UNITY_EDITOR
            _cachedPlatformSwipeSpeedMultiplier = editorSwipeSpeedMultiplier;
        #elif UNITY_ANDROID
            _cachedPlatformSwipeSpeedMultiplier = androidSwipeSpeedMultiplier;
        #elif UNITY_IOS
            _cachedPlatformSwipeSpeedMultiplier = iosSwipeSpeedMultiplier;
        #else
            _cachedPlatformSwipeSpeedMultiplier = 1.0f;
        #endif
    }

    private void OnEnable()
    {
        playercontrols.Enable();
        playercontrols.Touch.TouchPress.started += OnTouchStarted;
        playercontrols.Touch.TouchPress.canceled += OnTouchCanceled;
    }

    private void OnDisable()
    {
        playercontrols.Touch.TouchPress.started -= OnTouchStarted;
        playercontrols.Touch.TouchPress.canceled -= OnTouchCanceled;
        playercontrols.Disable();
    }

    void Start()
    {
        StartCoroutine(DelayedCacheScreenDPI());
        CalculateEffectiveScreenWidth();
    }

    void Update()
    {
        if (currentState == TouchState.None)
        {
            ResetFrameData();
            return;
        }

        UpdatePositionData();
        HandleStateMachine();
    }

    private void UpdatePositionData()
    {
        currentPos = playercontrols.Touch.TouchPosition.ReadValue<Vector2>();
        Vector2 rawDelta = currentPos - lastPosition;
        PixelDelta = rawDelta;

        float timeDelta = Time.unscaledDeltaTime;
        if (timeDelta > 0)
        {
            Vector2 normalizedPixelDelta = new Vector2(
                rawDelta.x / GetSafeScreenWidth(),
                rawDelta.y / Screen.height
            );
            _swipeSpeed = (normalizedPixelDelta.magnitude / timeDelta) * _cachedPlatformSwipeSpeedMultiplier;
        }

        lastPosition = currentPos;
    }

    private void HandleStateMachine()
    {
        float duration = (float)(Time.timeAsDouble - swipeStartTime);
        float currentDistanceNormalized = GetSwipeDistance();

        switch (currentState)
        {
            case TouchState.Pending:
                if (currentDistanceNormalized >= minSwipeDistance)
                {
                    StartSwipe();
                }
                else if (duration >= holdTimeThreshold)
                {
                    StartHold();
                }
                break;

            case TouchState.Holding:
                if (currentDistanceNormalized >= minSwipeDistance)
                {
                    OnHoldCanceled?.Invoke();
                    StartSwipe();
                }
                break;

            case TouchState.Swiping:
                break;

            case TouchState.DoubleTapped:
                if (currentDistanceNormalized >= minSwipeDistance)
                {
                    StartSwipe();
                }
                break;
        }
    }

    private void StartHold()
    {
        currentState = TouchState.Holding;
        OnHoldDetected?.Invoke();
    }

    private void StartSwipe()
    {
        currentState = TouchState.Swiping;
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        _touchStartTime = (float)Time.time;
        Vector2 startingRead = playercontrols.Touch.TouchPosition.ReadValue<Vector2>();
        
        float currentTime = (float)Time.time; 
        float timeSinceLastTap = currentTime - lastTapTime;
        float distFromLastTap = Vector2.Distance(firstTapPosition, startingRead);
        
        bool isDoubleTapSequence = timeSinceLastTap < doubleTapInterval && distFromLastTap < 50f;

        if (isDoubleTapSequence)
        {
            currentState = TouchState.DoubleTapped;
            lastTapTime = 0; 
        }
        else
        {
            currentState = TouchState.Pending;
        }

        startPos = startingRead;
        currentPos = startingRead;
        lastPosition = startingRead;
        swipeStartTime = Time.timeAsDouble;
        
        PixelDelta = Vector2.zero;
        _swipeSpeed = 0f;
        _lastCompletedSwipeTime = 0f;
        _lastCompletedSwipeDistanceValue = 0f;

        OnFirstTouchDetected?.Invoke(startPos);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        endPos = playercontrols.Touch.TouchPosition.ReadValue<Vector2>();
        lastTapTime = (float)Time.time; 
        firstTapPosition = endPos;
        
        float finalDuration = (float)(Time.timeAsDouble - swipeStartTime);
        float finalDistance = GetSwipeDistance();

        _lastCompletedSwipeTime = finalDuration;
        _lastCompletedSwipeDistanceValue = finalDistance;

        if (currentState == TouchState.Pending)
        {
            if (finalDuration <= maxTapTime)
            {
                OnSingleTapDetected?.Invoke();
                OnQuickTapDetected?.Invoke(); 
            }
        }
        else if (currentState == TouchState.Swiping)
        {
            TriggerSwipeDirectionEvent();
        }

        OnEndTouchDetected?.Invoke();
        currentState = TouchState.None;
        PixelDelta = Vector2.zero;
    }

    private void ResetFrameData()
    {
        _swipeSpeed = 0f;
        PixelDelta = Vector2.zero;
    }

    private void TriggerSwipeDirectionEvent()
    {
        SwipeDirection dir = GetSwipeDirectionEnum();
        switch (dir)
        {
            case SwipeDirection.Left: OnSwipeLeft?.Invoke(); break;
            case SwipeDirection.Right: OnSwipeRight?.Invoke(); break;
            case SwipeDirection.Up: OnSwipeUp?.Invoke(); break;
            case SwipeDirection.Down: OnSwipeDown?.Invoke(); break;
            case SwipeDirection.UpLeft: OnSwipeUpLeft?.Invoke(); break;
            case SwipeDirection.UpRight: OnSwipeUpRight?.Invoke(); break;
            case SwipeDirection.DownLeft: OnSwipeDownLeft?.Invoke(); break;
            case SwipeDirection.DownRight: OnSwipeDownRight?.Invoke(); break;
        }
    }

    public SwipeDirection GetSwipeDirectionEnum()
    {
        Vector2 vector = endPos - startPos;
        if (vector == Vector2.zero) return SwipeDirection.None;

        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return SwipeDirection.Right;
        if (angle >= 22.5f && angle < 67.5f) return SwipeDirection.UpRight;
        if (angle >= 67.5f && angle < 112.5f) return SwipeDirection.Up;
        if (angle >= 112.5f && angle < 157.5f) return SwipeDirection.UpLeft;
        if (angle >= 157.5f && angle < 202.5f) return SwipeDirection.Left;
        if (angle >= 202.5f && angle < 247.5f) return SwipeDirection.DownLeft;
        if (angle >= 247.5f && angle < 292.5f) return SwipeDirection.Down;
        if (angle >= 292.5f && angle < 337.5f) return SwipeDirection.DownRight;

        return SwipeDirection.None;
    }

    private int GetSafeScreenWidth()
    {
        return Screen.width > 0 ? Screen.width : 1080;
    }

    private void CalculateEffectiveScreenWidth()
    {
        effectiveScreenWidth = Mathf.Max(Screen.width, Screen.height);
        if (effectiveScreenWidth <= 0) effectiveScreenWidth = 1080f;
    }

    private IEnumerator DelayedCacheScreenDPI() 
    {
        yield return new WaitForSeconds(0.1f); 
        cachedDPI = (Screen.dpi > 0 && !float.IsNaN(Screen.dpi)) ? Screen.dpi : 300f;
    }

    public bool IsTouching() => currentState != TouchState.None;
    public bool IsSwiping() => currentState == TouchState.Swiping;
    public bool IsHolding() => currentState == TouchState.Holding;
    public bool IsPending() => currentState == TouchState.Pending;
    public bool IsDoubleTap() => currentState == TouchState.DoubleTapped;

    public Vector2 GetStartPos() => startPos;
    public Vector2 GetCurrentPos() => currentPos;
    public Vector2 GetPixelDelta() => PixelDelta;
    public float GetStartTime() => (float)swipeStartTime;
    public float GetSwipeSpeed() => _swipeSpeed;

    public float GetSwipeDistance()
    {
        if (currentState != TouchState.None)
            return Vector2.Distance(startPos, currentPos) / GetSafeScreenWidth();
        return _lastCompletedSwipeDistanceValue;
    }

    public float GetSwipeTime()
    {
        if (currentState != TouchState.None)
            return (float)(Time.timeAsDouble - swipeStartTime);
        return _lastCompletedSwipeTime;
    }

    public float GetCacheScreenDPI() => cachedDPI;
    public Vector2 GetSwipeDirection() => (currentPos - startPos).normalized;

    public static float GetTouchStartTime() => _touchStartTime;

    public static void ResetTouchStartTime()
    {
        _touchStartTime = (float)Time.time;
    }
    
    public Vector2 GetSwipeDistancePixel() => currentPos - startPos;
    public float GetCurrentSwipeNormalizedDistance() => GetSwipeDistance();
    public Vector2 GetCurrentSwipeStartPos() => startPos;
    public Vector2 GetCurrentSwipeEndPos() => currentPos;
}