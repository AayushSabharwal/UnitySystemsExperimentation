using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Serializable]
    private struct SpherecastParams
    {
        public float spherecastRadius;
        public float spherecastDistance;
        public LayerMask mask;
    }

    [Serializable]
    private struct RaycastParams
    {
        public float raycastDistance;
        public LayerMask mask;
    }

    [Header("References")]
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private SpherecastParams groundCheckParams;
    [SerializeField]
    private CameraFXManager cameraFx;

    [Header("Input Modifiers")]
    [SerializeField]
    [Range(0f, 1f)]
    private float lookSensitivity = 0.01f;
    [SerializeField]
    private float verticalLookAngle = 90f;

    [Header("Basic Movement Parameters")]
    [SerializeField]
    public float moveSpeed = 4f;
    [SerializeField]
    public float jumpSpeed = 100f;
    [SerializeField]
    public float airControl = 0.8f;
    [Range(1f, 2f)]
    public float sprintMultiplier = 1.5f;
    [SerializeField]
    private float sprintDuration = 2f;
    [SerializeField]
    private float sprintRegenerationDelay = 1f;
    [SerializeField]
    private float wallJumpDuration = 0.4f;
    [SerializeField]
    [Tooltip("X is perpendicular out, Y is tangential up")]
    public Vector2 wallJumpSpeed;

    [Header("Wall Checking")]
    [SerializeField]
    private RaycastParams wallCheckRaycastParams;
    public float wallSlideSpeed;

    [Header("Debugging")]
    [SerializeField]
    private bool showGroundCheck;
    [SerializeField]
    private bool showWallCheck;

    private Controls _controls;
    private Vector2 _look;
    private float _sprintDurationLeft;
    private float _sprintRegenerationDelayLeft;
    private bool _hasToJump;
    private bool _isHoldingSprint;
    private bool _isGrounded;
    private bool _isFacingWall;
    private bool _isWallJumping;
    private StateMachine _stateMachine;
    private Rigidbody _rb;

    public Vector2 Move { get; private set; }

    private bool IsSprinting => _isHoldingSprint && IsMoving && _isGrounded && CanSprint;
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
        _isFacingWall = false;
        _hasToJump = false;
        SprintDurationLeft = sprintDuration;
        _sprintRegenerationDelayLeft = sprintRegenerationDelay;
        _controls = new Controls();
        _rb = GetComponent<Rigidbody>();

        _stateMachine = new StateMachine();
        Walking walking = new Walking(this, _rb);
        Sprinting sprinting = new Sprinting(this, _rb, cameraFx);
        Jump jump = new Jump(this, _rb);
        Midair midair = new Midair(this, _rb, transform);
        WallSlide wallSlide = new WallSlide(this, _rb);
        WallJump wallJump = new WallJump(this, _rb);
        Idle idle = new Idle(this);

        bool IsMoving() => Move != Vector2.zero;
        bool NotMoving() => Move == Vector2.zero;
        bool Grounded() => _isGrounded;
        bool NotGrounded() => !_isGrounded;
        bool SprintButtonHeldAndCanSprint() => _isHoldingSprint && CanSprint;
        bool SprintButtonLeft() => !_isHoldingSprint;
        bool CannotSprint() => !CanSprint;
        bool JumpButtonPressed() => _hasToJump;
        bool FacingWallAndHoldingForward() => _isFacingWall && Move.y > 0f;
        bool NotFacingWall() => !_isFacingWall;
        bool NotHoldingForward() => Move.y < moveSpeed;

        _stateMachine.AddTransition(idle, walking, IsMoving);
        _stateMachine.AddTransition(idle, midair, NotGrounded);
        _stateMachine.AddTransition(idle, jump, JumpButtonPressed);

        _stateMachine.AddTransition(walking, idle, NotMoving);
        _stateMachine.AddTransition(walking, midair, NotGrounded);
        _stateMachine.AddTransition(walking, sprinting, SprintButtonHeldAndCanSprint);
        _stateMachine.AddTransition(walking, jump, JumpButtonPressed);

        _stateMachine.AddTransition(sprinting, idle, NotMoving);
        _stateMachine.AddTransition(sprinting, walking, SprintButtonLeft);
        _stateMachine.AddTransition(sprinting, walking, CannotSprint);
        _stateMachine.AddTransition(sprinting, midair, NotGrounded);
        _stateMachine.AddTransition(sprinting, jump, JumpButtonPressed);

        _stateMachine.AddTransition(jump, midair, NotGrounded);
        // _stateMachine.AddTransition(jump, idle, Grounded);

        _stateMachine.AddTransition(midair, idle, Grounded);
        _stateMachine.AddTransition(midair, wallSlide, FacingWallAndHoldingForward);

        _stateMachine.AddTransition(wallSlide, midair, NotHoldingForward);
        _stateMachine.AddTransition(wallSlide, idle, Grounded);
        _stateMachine.AddTransition(wallSlide, wallJump, JumpButtonPressed);
        _stateMachine.AddTransition(wallSlide, midair, NotFacingWall);
        
        _stateMachine.AddTransition(wallJump, midair, NotGrounded);

        _stateMachine.ChangeState(idle);
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

    private void Update()
    {
        _stateMachine.Tick();

        transform.Rotate(Vector3.up, _look.x);
        cameraTransform.Rotate(Vector3.right, _look.y);

        HandleSprintDuration();
        ClampCameraRotation();
        CheckGrounded();
        CheckFacingWall();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (showGroundCheck)
        {
            Gizmos.DrawWireSphere(transform.position - Vector3.up * groundCheckParams.spherecastDistance,
                                  groundCheckParams.spherecastRadius);
        }

        if (showWallCheck)
            Gizmos.DrawLine(transform.position,
                            transform.position + transform.forward * wallCheckRaycastParams.raycastDistance);
    }

    public void SetRelativeVelocity(float x, float y, float z)
    {
        if (_isWallJumping)
            return;

        _rb.velocity = transform.right * x + transform.up * y + transform.forward * z;
    }

    public void SetAbsoluteVelocity(float x, float y, float z)
    {
        if (_isWallJumping)
            return;
        _rb.velocity = Vector3.right * x + Vector3.up * y + Vector3.forward * z;
    }

    public void ResetHasToJump()
    {
        _hasToJump = false;
    }

    public void OnWallJump()
    {
        _isWallJumping = true;
        Timing.RunCoroutine(ReEnableControls());
    }

    private IEnumerator<float> ReEnableControls()
    {
        yield return Timing.WaitForSeconds(wallJumpDuration);
        _isWallJumping = false;
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.SphereCast(new Ray(transform.position, Vector3.down), groundCheckParams.spherecastRadius,
                                         groundCheckParams.spherecastDistance, groundCheckParams.mask,
                                         QueryTriggerInteraction.Ignore);
    }

    private void CheckFacingWall()
    {
        _isFacingWall = Physics.Raycast(new Ray(transform.position, transform.forward),
                                        wallCheckRaycastParams.raycastDistance, wallCheckRaycastParams.mask,
                                        QueryTriggerInteraction.Ignore);
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
        Move = context.ReadValue<Vector2>() * moveSpeed;
    }

    private void LookInput(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>() * lookSensitivity;
        _look.y *= -1f;
    }

    private void JumpInput(InputAction.CallbackContext context)
    {
        // if (IsGrounded) Rb.velocity += Vector3.up * jumpSpeed;
        _hasToJump = true;
    }

    private void SprintInput(InputAction.CallbackContext context)
    {
        _isHoldingSprint = context.ReadValueAsButton();
    }
}