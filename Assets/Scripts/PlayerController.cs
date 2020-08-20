using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Serializable]
    private struct SphereCastParams
    {
        public float sphereCastRadius;
        public float sphereCastDistance;
        public LayerMask groundMask;
    }

    [Header("References")]
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private SphereCastParams groundCheckParams;
    
    [Header("Input Modifiers")]
    [SerializeField]
    [Range(0f, 1f)]
    private float lookSensitivity = 0.01f;
    [SerializeField]
    private float verticalLookAngle = 90f;
    
    [Header("Movement Parameters")]
    [SerializeField]
    private float moveSpeed = 4f;
    [SerializeField]
    private float jumpSpeed = 100f;
    [SerializeField]
    [Range(1f, 2f)]
    private float sprintMultiplier = 1.5f;
    [SerializeField]
    private float sprintDuration = 2f;
    [SerializeField]
    private float sprintRegenerationDelay = 1f;

    private Controls _controls;
    private Vector2 _move;
    private Vector2 _look;
    private Rigidbody _rb;
    private bool _isHoldingSprint;
    private float _sprintDurationLeft;
    private float _sprintRegenerationDelayLeft;
    private bool IsGrounded =>
        Physics.SphereCast(new Ray(transform.position, Vector3.down), groundCheckParams.sphereCastRadius,
                           groundCheckParams.sphereCastDistance, groundCheckParams.groundMask,
                           QueryTriggerInteraction.Ignore);
    public bool IsSprinting => _isHoldingSprint && IsMoving && IsGrounded && CanSprint;
    private bool IsMoving => _rb.velocity.sqrMagnitude > 0f;
    private bool CanSprint => SprintDurationLeft > 0f;
    private float SprintDurationLeft
    {
        get => _sprintDurationLeft;
        set
        {
            _sprintDurationLeft = value;
            OnSprintValueChanged?.Invoke(value / sprintDuration);
        }
    }

    public delegate void OnPercentageValueChangedHandler(float percent);

    public event OnPercentageValueChangedHandler OnSprintValueChanged;

    private void Awake()
    {
        _isHoldingSprint = false;
        SprintDurationLeft = sprintDuration;
        _sprintRegenerationDelayLeft = sprintRegenerationDelay;
        _controls = new Controls();
    }

    private void OnEnable()
    {
        _controls.Player.Move.performed += MoveInput;
        _controls.Player.Move.canceled += MoveInput;
        _controls.Player.Move.Enable();

        _controls.Player.Look.performed += LookInput;
        _controls.Player.Look.canceled += LookInput;
        _controls.Player.Look.Enable();

        _controls.Player.Jump.performed += JumpInput;
        _controls.Player.Jump.Enable();

        _controls.Player.Sprint.performed += SprintInput;
        _controls.Player.Sprint.canceled += SprintInput;
        _controls.Player.Sprint.Enable();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (IsGrounded)
        {
            _rb.velocity = (transform.forward * _move.y + transform.right * _move.x) *
                           (CanSprint ? sprintMultiplier : 1f) +
                           transform.up * _rb.velocity.y;
        }

        transform.Rotate(Vector3.up, _look.x);
        cameraTransform.Rotate(Vector3.right, _look.y);

        HandleSprintDuration();
        ClampCameraRotation();
    }

    private void OnDisable()
    {
        _controls.Player.Move.performed -= MoveInput;
        _controls.Player.Look.canceled -= MoveInput;
        _controls.Player.Move.Disable();

        _controls.Player.Look.performed -= LookInput;
        _controls.Player.Look.canceled -= LookInput;
        _controls.Player.Look.Disable();

        _controls.Player.Jump.performed -= JumpInput;
        _controls.Player.Jump.Disable();

        _controls.Player.Sprint.performed -= SprintInput;
        _controls.Player.Sprint.canceled -= SprintInput;
        _controls.Player.Sprint.Disable();
    }

    private void HandleSprintDuration()
    {
        if (IsSprinting)
        {
            _sprintRegenerationDelayLeft = sprintRegenerationDelay;
            SprintDurationLeft = Mathf.Max(SprintDurationLeft - Time.deltaTime, 0f);
        }
        else if (SprintDurationLeft < sprintDuration)
        {
            if (_sprintRegenerationDelayLeft > 0f)
                _sprintRegenerationDelayLeft -= Time.deltaTime;
            else
            {
                _sprintRegenerationDelayLeft = 0f;
                SprintDurationLeft = Mathf.Min(SprintDurationLeft + Time.deltaTime, sprintDuration);
            }
        }
    }

    private void ClampCameraRotation()
    {
        Vector3 euler = cameraTransform.localRotation.eulerAngles;
        euler.y = 0f;
        euler.z = 0f;

        if (euler.x > verticalLookAngle && euler.x < 180f)
            euler.x = verticalLookAngle;
        else if (euler.x < 360f - verticalLookAngle && euler.x > 180f)
            euler.x = 360f - verticalLookAngle;

        cameraTransform.localRotation = Quaternion.Euler(euler);
    }

    private void MoveInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>() * moveSpeed;
    }

    private void LookInput(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>() * lookSensitivity;
        _look.y *= -1f;
    }

    private void JumpInput(InputAction.CallbackContext context)
    {
        if (IsGrounded) _rb.velocity += Vector3.up * jumpSpeed;
    }

    private void SprintInput(InputAction.CallbackContext context)
    {
        _isHoldingSprint = context.ReadValueAsButton();
    }
}