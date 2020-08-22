using UnityEngine;

public class Midair : IState
{
    private readonly PlayerController _controller;
    private readonly Transform _transform;
    private Vector3 velocityCap;
    private Vector3 targetVelocity;

    public Midair(PlayerController controller, Transform transform)
    {
        _controller = controller;
        _transform = transform;
    }

    public void OnEnter()
    {
        velocityCap = _controller.Velocity;
        velocityCap.x = Mathf.Max(Mathf.Abs(velocityCap.x), _controller.MoveSpeed);
        velocityCap.z = Mathf.Max(Mathf.Abs(velocityCap.z), _controller.MoveSpeed);
        targetVelocity = _controller.Velocity;
    }

    public void Tick()
    {
        _controller.ResetHasToJump(); //so jump inputs can't be queued midair
        if (_controller.Move != Vector2.zero)
        {
            targetVelocity.x = Mathf.Clamp(targetVelocity.x +
                                           (_transform.forward * _controller.Move.y +
                                            _transform.right * _controller.Move.x).x * _controller.AirControl *
                                           Time.deltaTime,
                                           -velocityCap.x, velocityCap.x);
            targetVelocity.z = Mathf.Clamp(targetVelocity.z +
                                           (_transform.forward * _controller.Move.y +
                                            _transform.right * _controller.Move.x).z * _controller.AirControl *
                                           Time.deltaTime,
                                           -velocityCap.z, velocityCap.z);
            _controller.SetAbsoluteVelocity(targetVelocity.x, _controller.Velocity.y, targetVelocity.z);
        }
    }

    public void OnExit()
    {
    }
}