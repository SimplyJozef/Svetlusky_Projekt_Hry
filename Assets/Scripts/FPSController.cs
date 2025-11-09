using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 3.0f;
    [SerializeField] private float _jumpHeight = 1.0f;

    [Header("Look Settings")]
    [SerializeField] private float _lookSensitivity = 0.4f; // ↓ znížené z 2f na 0.4f
    [SerializeField] private float _verticalLookClamp = 89f;
    [SerializeField] private float _lookSmoothTime = 0.05f; // pridané – jemné vyhladzovanie

    [SerializeField] private bool _bCanMove = true;

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

    private void Awake()
    {
        _mainInputActions = new IA_Main();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _mainInputActions.AM_Player.A_Move.performed += OnReadMovementVector;
        _mainInputActions.AM_Player.A_Move.canceled += OnReadMovementVector;

        _mainInputActions.AM_Player.A_Look.performed += OnReadLookVector;
        _mainInputActions.AM_Player.A_Look.canceled += OnReadLookVector;

        _mainInputActions.AM_Player.A_Sprint.performed += OnSprintButton;
        _mainInputActions.AM_Player.A_Sprint.canceled += OnSprintButton;

        _mainInputActions.AM_Player.A_Jump.performed += OnJumpButton;
        _mainInputActions.AM_Player.A_Jump.canceled += OnJumpButton;

        _mainInputActions.AM_Player.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
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
        // nová verzia: plynulé vyhladzovanie pohľadu
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
}