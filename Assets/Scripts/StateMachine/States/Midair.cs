using UnityEngine;

public class Midair : IState
{
    private readonly PlayerController _controller;
    private readonly Transform _transform;
    private Vector3 _velocityCap;
    private Vector3 _targetVelocity;
    private Vector3 _forward;
    private Vector3 _right;
    public Midair(PlayerController controller, Transform transform)
    {
        _controller = controller;
        _transform = transform;
    }

    public void OnEnter()
    {
        _velocityCap = _controller.Velocity;
        _velocityCap.x = Mathf.Max(Mathf.Abs(_velocityCap.x), _controller.MoveSpeed);
        _velocityCap.z = Mathf.Max(Mathf.Abs(_velocityCap.z), _controller.MoveSpeed);
        _targetVelocity = _controller.Velocity;
    }

    public void Tick()
    {
        _controller.ResetHasToJump(); //so jump inputs can't be queued midair
        if (_controller.Move != Vector2.zero)
        {
            _forward = _transform.forward;
            _right = _transform.right;
            _targetVelocity.x = Mathf.Clamp(_targetVelocity.x +
                                            (_forward * _controller.Move.y +
                                             _right * _controller.Move.x).x * _controller.AirControl *
                                            Time.fixedDeltaTime,
                                            -_velocityCap.x, _velocityCap.x);
            _targetVelocity.z = Mathf.Clamp(_targetVelocity.z +
                                           (_forward * _controller.Move.y +
                                            _right * _controller.Move.x).z * _controller.AirControl *
                                           Time.fixedDeltaTime,
                                           -_velocityCap.z, _velocityCap.z);
            _controller.SetAbsoluteVelocity(_targetVelocity.x, _controller.Velocity.y, _targetVelocity.z);
        }
    }

    public void OnExit()
    {
    }
}