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
    public struct RaycastParams
    {
        public Vector3 positionOffset;
        public float raycastDistance;
        public LayerMask mask;
    }

    [Header("References")]
    [SerializeField]
    private Transform cameraTransform;
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
    private float moveSpeed = 4f;
    [SerializeField]
    private float jumpSpeed = 100f;
    [SerializeField]
    private float airControl = 0.8f;
    [SerializeField]
    [Range(1f, 2f)]
    private float sprintMultiplier = 1.5f;
    [SerializeField]
    private float sprintDuration = 2f;
    [SerializeField]
    private float sprintRegenerationDelay = 1f;
    [SerializeField]
    private float wallJumpDuration = 0.4f;
    [SerializeField]
    [Tooltip("X is perpendicular out, Y is tangential up")]
    private Vector2 wallJumpSpeed;
    [SerializeField]
    private float wallSlideSpeed;
    [SerializeField]
    private float vaultHeight;
    [SerializeField]
    private float vaultDistance;
    [SerializeField]
    private LayerMask vaultMask;
    
    [Header("Raycast Checking")]
    [SerializeField]
    private SpherecastParams groundCheckParams;
    [SerializeField]
    private RaycastParams wallCheckRaycastParams;
    [SerializeField]
    private float maxDistanceForFacingWall = 0.6f;

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
    private bool _wallAhead;
    private bool _canVault;
    private bool _isWallJumping;
    private RaycastHit _wallCheckHit;
    private RaycastHit _vaultCheckHit;
    private StateMachine _stateMachine;
    private Transform _transform; //caching to prevent repeated property access. Apparently this is noticeable
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
    public float MoveSpeed => moveSpeed;
    public float JumpSpeed => jumpSpeed;
    public float AirControl => airControl;
    public float SprintMultiplier => sprintMultiplier;
    public RaycastHit WallCheckHit => _wallCheckHit;
    public Vector2 WallJumpSpeed => wallJumpSpeed;
    public float WallSlideSpeed => wallSlideSpeed;
    public Vector3 Velocity => _rb.velocity;
    public RaycastHit VaultCheckHit => _vaultCheckHit;
    public bool IsFacingWall => _isFacingWall;

    public delegate void OnPercentageValueChangedHandler(float percent);

    public event OnPercentageValueChangedHandler OnSprintValueChanged;

    private void Awake()
    {
        _transform = transform;
        _isHoldingSprint = false;
        _isFacingWall = false;
        _hasToJump = false;
        SprintDurationLeft = sprintDuration;
        _sprintRegenerationDelayLeft = sprintRegenerationDelay;
        _controls = new Controls();
        _rb = GetComponent<Rigidbody>();

        _stateMachine = new StateMachine();
        Walking walking = new Walking(this);
        Sprinting sprinting = new Sprinting(this, cameraFx);
        Jump jump = new Jump(this);
        Midair midair = new Midair(this, _transform);
        WallSlide wallSlide = new WallSlide(this);
        WallJump wallJump = new WallJump(this);
        Idle idle = new Idle(this);
        Vault vault = new Vault(this, _transform);
        
        bool IsMoving() => Move != Vector2.zero;
        bool NotMoving() => Move == Vector2.zero;
        bool Grounded() => _isGrounded;
        bool NotGrounded() => !_isGrounded;
        bool SprintButtonHeldAndCanSprint() => _isHoldingSprint && CanSprint;
        bool SprintButtonLeft() => !_isHoldingSprint;
        bool CannotSprint() => !CanSprint;
        bool JumpButtonPressed() => _hasToJump;
        bool FacingWallAndHoldingForward() => IsFacingWall && Move.y > 0f;
        bool NotFacingWall() => !IsFacingWall;
        bool NotHoldingForward() => Move.y < MoveSpeed;
        bool CanVaultAndHoldingForward() => _canVault && Move.y > 0f;
        
        _stateMachine.AddTransition(idle, walking, IsMoving);
        _stateMachine.AddTransition(idle, midair, NotGrounded);
        _stateMachine.AddTransition(idle, jump, JumpButtonPressed);

        _stateMachine.AddTransition(walking, idle, NotMoving);
        _stateMachine.AddTransition(walking, midair, NotGrounded);
        _stateMachine.AddTransition(walking, sprinting, SprintButtonHeldAndCanSprint);
        _stateMachine.AddTransition(walking, jump, JumpButtonPressed);
        _stateMachine.AddTransition(walking, vault, CanVaultAndHoldingForward);
        
        _stateMachine.AddTransition(sprinting, idle, NotMoving);
        _stateMachine.AddTransition(sprinting, walking, SprintButtonLeft);
        _stateMachine.AddTransition(sprinting, walking, CannotSprint);
        _stateMachine.AddTransition(sprinting, midair, NotGrounded);
        _stateMachine.AddTransition(sprinting, jump, JumpButtonPressed);
        _stateMachine.AddTransition(sprinting, vault, CanVaultAndHoldingForward);
        
        _stateMachine.AddTransition(jump, midair, NotGrounded);

        _stateMachine.AddTransition(midair, idle, Grounded);
        _stateMachine.AddTransition(midair, wallSlide, FacingWallAndHoldingForward);

        _stateMachine.AddTransition(wallSlide, midair, NotHoldingForward);
        _stateMachine.AddTransition(wallSlide, idle, Grounded);
        _stateMachine.AddTransition(wallSlide, wallJump, JumpButtonPressed);
        _stateMachine.AddTransition(wallSlide, midair, NotFacingWall);

        _stateMachine.AddTransition(wallJump, midair, NotGrounded);
        
        _stateMachine.AddTransition(vault, midair, NotGrounded);
        _stateMachine.AddTransition(vault, idle, Grounded);
        
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
        _transform.Rotate(Vector3.up, _look.x);
        cameraTransform.Rotate(Vector3.right, _look.y);

        HandleSprintDuration();
        ClampCameraRotation();
        CheckGrounded();
        CheckFacingWall();
        CheckCanVault();    //Must come after CheckFacingWall
    }

    private void FixedUpdate()
    {
        _stateMachine.Tick();
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
        // ReSharper disable Unity.InefficientPropertyAccess
        Gizmos.color = Color.green;
        if (showGroundCheck)
        {
            Gizmos.DrawWireSphere(transform.position - Vector3.up * groundCheckParams.spherecastDistance,
                                  groundCheckParams.spherecastRadius);
        }

        if (showWallCheck)
        {
            Gizmos.DrawLine(transform.position + wallCheckRaycastParams.positionOffset,
                            transform.position + wallCheckRaycastParams.positionOffset +
                            transform.forward * wallCheckRaycastParams.raycastDistance);
        }
        // ReSharper restore Unity.InefficientPropertyAccess
    }

    public void SetRelativeVelocity(float x, float y, float z)
    {
        if (_isWallJumping)
            return;

        _rb.velocity = _transform.right * x + _transform.up * y + _transform.forward * z;
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

    public void ResetYVelocityAfter(float delay)
    {
        Timing.RunCoroutine(ResetYVelocityAfterDelay(delay));
    }

    private IEnumerator<float> ResetYVelocityAfterDelay(float delay)
    {
        yield return Timing.WaitForSeconds(delay);
        Vector3 velocity = _rb.velocity;
        velocity = new Vector3(velocity.x, 0f, velocity.z);
        _rb.velocity = velocity;
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
        _isGrounded = Physics.SphereCast(new Ray(_transform.position, Vector3.down), groundCheckParams.spherecastRadius,
                                         groundCheckParams.spherecastDistance, groundCheckParams.mask,
                                         QueryTriggerInteraction.Ignore);
    }

    private void CheckFacingWall()
    {
        _wallAhead = Physics.Raycast(_transform.position + wallCheckRaycastParams.positionOffset, _transform.forward,
                                     out _wallCheckHit, wallCheckRaycastParams.raycastDistance,
                                     wallCheckRaycastParams.mask, QueryTriggerInteraction.Ignore);
        if (_wallAhead)
        {
            Vector3 point = _wallCheckHit.point;
            point.y = _transform.position.y;
            if ((point - _transform.position).sqrMagnitude <=
                maxDistanceForFacingWall * maxDistanceForFacingWall)
            {
                _isFacingWall = true;
            }
            else
            {
                _isFacingWall = false;
            }
        }
        else
        {
            _isFacingWall = false;
        }
    }

    private void CheckCanVault()
    {
        if (!_wallAhead)
        {
            _canVault = false;
            return;
        }

        Vector3 wallCheckHitPoint = _wallCheckHit.point;
        wallCheckHitPoint.y = _transform.position.y;
        float sqrDistance = (wallCheckHitPoint - transform.position).sqrMagnitude;
        if (sqrDistance <= vaultDistance * vaultDistance &&
            Physics.SphereCast(_wallCheckHit.point + Vector3.up * (vaultHeight + 0.5f), 0.25f, Vector3.down,
                               out _vaultCheckHit, vaultHeight + 1f, vaultMask,
                               QueryTriggerInteraction.Ignore))
        {
            _canVault = true;
        }
        else
        {
            _canVault = false;
        }
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
        Move = context.ReadValue<Vector2>() * MoveSpeed;
    }

    private void LookInput(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>() * lookSensitivity;
        _look.y *= -1f;
    }

    private void JumpInput(InputAction.CallbackContext context)
    {
        _hasToJump = true;
    }

    private void SprintInput(InputAction.CallbackContext context)
    {
        _isHoldingSprint = context.ReadValueAsButton();
    }
}