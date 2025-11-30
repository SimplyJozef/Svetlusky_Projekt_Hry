using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 3.0f;
    [SerializeField] private float _jumpHeight = 1.0f;

    [Header("Look Settings")]
    [SerializeField] private float _lookSensitivity = 0.4f;
    [SerializeField] private float _verticalLookClamp = 89f;
    [SerializeField] private float _lookSmoothTime = 0.05f;

    [Header("Flashlight Settings")]
    [SerializeField] private Light _flashlight;
    private bool _isFlashlightOn = true;
    [SerializeField] private float _flashlightOverdriveBattery = 3.0f;

    [SerializeField] private bool _bCanMove = true;

    [SerializeField] private bool _bRecordLogs = true;

    private Vector2 _movementVector;
    private float _lookHorizontal;
    private float _lookVertical;

    private Vector2 _smoothedLookInput;
    private Vector2 _currentLookVelocity;

    private IA_Main _mainInputActions;
    private CharacterController _characterController;
    private Vector3 _velocity;

    private bool _bIsSprinting;
    private bool _bWantsToJump;

    private Coroutine _flashlightOverdriveCoroutine;

    private bool bIsInOverdrive = false;

    private string _userId;

    private Coroutine _logCoroutine;
    private Coroutine _postCoroutine;

    private string _logs;

    private void Awake()
    {
        _mainInputActions = new IA_Main();
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _userId = PlayerPrefs.GetString("UserName");
        SendLog("[LogBegin]");
        _logCoroutine = StartCoroutine(LogMovementCoroutine());
        _postCoroutine = StartCoroutine(PeriodicLogSendCoroutine());
    }

    private void OnEnable()
    {
        // Pohyb
        _mainInputActions.AM_Player.A_Move.performed += OnReadMovementVector;
        _mainInputActions.AM_Player.A_Move.canceled += OnReadMovementVector;

        // Pohľad
        _mainInputActions.AM_Player.A_Look.performed += OnReadLookVector;
        _mainInputActions.AM_Player.A_Look.canceled += OnReadLookVector;

        // Beh
        _mainInputActions.AM_Player.A_Sprint.performed += OnSprintButton;
        _mainInputActions.AM_Player.A_Sprint.canceled += OnSprintButton;

        // Skok
        _mainInputActions.AM_Player.A_Jump.performed += OnJumpButton;
        _mainInputActions.AM_Player.A_Jump.canceled += OnJumpButton;

        // 🔦 Svetlo (B)
        _mainInputActions.AM_Player.A_Flashlight.performed += OnFlashlightToggle;

        _mainInputActions.AM_Player.A_FlashlightOverdrive.performed += OnFlashlightOverdrive;

        _mainInputActions.AM_Player.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _mainInputActions.AM_Player.A_Move.performed -= OnReadMovementVector;
        _mainInputActions.AM_Player.A_Move.canceled -= OnReadMovementVector;

        _mainInputActions.AM_Player.A_Look.performed -= OnReadLookVector;
        _mainInputActions.AM_Player.A_Look.canceled -= OnReadLookVector;

        _mainInputActions.AM_Player.A_Sprint.performed -= OnSprintButton;
        _mainInputActions.AM_Player.A_Sprint.canceled -= OnSprintButton;

        _mainInputActions.AM_Player.A_Jump.performed -= OnJumpButton;
        _mainInputActions.AM_Player.A_Jump.canceled -= OnJumpButton;

        _mainInputActions.AM_Player.A_Flashlight.performed -= OnFlashlightToggle;
        
        _mainInputActions.AM_Player.A_FlashlightOverdrive.performed -= OnFlashlightOverdrive;

        _mainInputActions.AM_Player.Disable();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnReadMovementVector(InputAction.CallbackContext context)
    {
        _movementVector = context.ReadValue<Vector2>();
    }

    private void OnReadLookVector(InputAction.CallbackContext context)
    {
        var targetLook = context.ReadValue<Vector2>() * _lookSensitivity;
        _smoothedLookInput = Vector2.SmoothDamp(
            _smoothedLookInput,
            targetLook,
            ref _currentLookVelocity,
            _lookSmoothTime
        );

        _lookHorizontal += _smoothedLookInput.x;
        _lookVertical += _smoothedLookInput.y;
        _lookVertical = Mathf.Clamp(_lookVertical, -_verticalLookClamp, _verticalLookClamp);
    }

    private void OnSprintButton(InputAction.CallbackContext context)
    {
        _bIsSprinting = context.performed;
    }

    private void OnJumpButton(InputAction.CallbackContext context)
    {
        _bWantsToJump = context.performed;
    }

    private void OnFlashlightToggle(InputAction.CallbackContext context)
    {
        if (context.performed && _flashlight != null)
        {
            _isFlashlightOn = !_isFlashlightOn;
            _flashlight.enabled = _isFlashlightOn;
        }
    }
    
    private void OnFlashlightOverdrive(InputAction.CallbackContext context)
    {
        if (!bIsInOverdrive && _flashlightOverdriveBattery > 0.0f)
        {
            StartCoroutine(FlashlightOverdrive());
        }
    }

    IEnumerator FlashlightOverdrive()
    {
        bIsInOverdrive = true;
        _flashlightOverdriveBattery -= 1;

        const float enterTime = 0.25f;
        yield return StartCoroutine(AnimateFlashlight(
            duration: enterTime,
            startSpot: _flashlight.spotAngle,
            endSpot: 10f,
            startInner: _flashlight.innerSpotAngle,
            endInner: 5f,
            startIntensity: _flashlight.intensity,
            endIntensity: 32000f
        ));

        const float overdriveDuration = 2f;
        var elapsed = 0f;

        while (elapsed < overdriveDuration)
        {
            elapsed += Time.deltaTime;

            var flicker = Mathf.PerlinNoise(Time.time * 20f, 0f) * 25000;

            _flashlight.intensity = 32000f + flicker;

            yield return null;
        }

        const float exitTime = 0.25f;
        yield return StartCoroutine(AnimateFlashlight(
            duration: exitTime,
            startSpot: _flashlight.spotAngle,
            endSpot: 33.23819f,
            startInner: _flashlight.innerSpotAngle,
            endInner: 14.76275f,
            startIntensity: _flashlight.intensity,
            endIntensity: 4000f
        ));

        bIsInOverdrive = false;
    }
    
    IEnumerator AnimateFlashlight(
        float duration,
        float startSpot, float endSpot,
        float startInner, float endInner,
        float startIntensity, float endIntensity)
    {
        var time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            var t = time / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            _flashlight.spotAngle = Mathf.Lerp(startSpot, endSpot, t);
            _flashlight.innerSpotAngle = Mathf.Lerp(startInner, endInner, t);
            _flashlight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

            yield return null;
        }

        _flashlight.spotAngle = endSpot;
        _flashlight.innerSpotAngle = endInner;
        _flashlight.intensity = endIntensity;
    }

    void Update()
    {
        if (!_bCanMove)
            return;

        MoveCharacter();
        transform.rotation = Quaternion.Euler(0, _lookHorizontal, 0);
    }

    private void LateUpdate()
    {
        _playerCamera.transform.localRotation = Quaternion.Euler(-_lookVertical, 0, 0);
    }

    private void MoveCharacter()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
            _velocity.y = 0f;

        var moveDir = new Vector3(_movementVector.x, 0, _movementVector.y);
        moveDir = transform.TransformDirection(moveDir);

        if (_bWantsToJump && _characterController.isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2.0f * Physics.gravity.y);

        _velocity.y += Physics.gravity.y * Time.deltaTime;
        var speed = _bIsSprinting ? _runSpeed : _walkSpeed;
        var finalMove = (moveDir * speed) + (_velocity.y * Vector3.up);

        _characterController.Move(finalMove * Time.deltaTime);
    }

    private IEnumerator PeriodicLogSendCoroutine()
    {
        while (_bRecordLogs)
        {
            yield return new WaitForSeconds(10f);
            SendLog(_logs);
            _logs = "";
        }
    }

    private IEnumerator LogMovementCoroutine()
    {
        while (_bRecordLogs)
        {
            LogMovement(transform.position, transform.forward);
            yield return new WaitForSeconds(1f);
        }
    }

    private void LogMovement(Vector3 position, Vector3 fwd)
    {
        _logs += $"|Move;{position.x};{position.y};{position.z};{fwd.x};{fwd.y};{fwd.z};{Time.timeSinceLevelLoad}";
    }

    private void SendLog(string data)
    {
        var apiUrl = "https://game-log-server-w2i98.ondigitalocean.app/recordGameLog";
        var log = new LogEntry
        {
            User = _userId,
            Data = data
        };
        var json = JsonUtility.ToJson(log);
        StartCoroutine(SendPostRequest(apiUrl, json));
    }

    private IEnumerator SendPostRequest(string apiUrl, string json)
    {
        var req = new UnityWebRequest(apiUrl, "POST");
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
        }
    }
}