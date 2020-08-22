using System.Timers;
using UnityEngine;

public class Midair : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;
    private readonly Transform _transform;
    private Vector3 velocityCap;
    private Vector3 targetVelocity;

    public Midair(PlayerController controller, Rigidbody rb, Transform transform)
    {
        _controller = controller;
        _rb = rb;
        _transform = transform;
    }

    public void OnEnter()
    {
        velocityCap = _rb.velocity;
        velocityCap.x = Mathf.Max(Mathf.Abs(velocityCap.x), _controller.moveSpeed);
        velocityCap.z = Mathf.Max(Mathf.Abs(velocityCap.z), _controller.moveSpeed);
    }

    public void Tick()
    {
        _controller.ResetHasToJump(); //so jump inputs can't be queued midair
        if (_controller.Move != Vector2.zero)
        {
            targetVelocity.x = Mathf.Clamp(_rb.velocity.x +
                                           (_transform.forward * _controller.Move.y +
                                            _transform.right * _controller.Move.x).x * _controller.airControl * Time.deltaTime,
                                           -velocityCap.x, velocityCap.x);
            targetVelocity.z = Mathf.Clamp(_rb.velocity.z +
                                           (_transform.forward * _controller.Move.y +
                                            _transform.right * _controller.Move.x).z * _controller.airControl * Time.deltaTime,
                                           -velocityCap.z, velocityCap.z);
            _controller.SetAbsoluteVelocity(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
        }
        
        // targetVelocity.x = _controller.Move.x * _controller.airControl + initalVelocity.x;
        // targetVelocity.z = _controller.Move.y * _controller.airControl + initalVelocity.z;
        //
        // targetVelocity.x = Mathf.Min(Mathf.Abs(targetVelocity.x), _controller.moveSpeed) * Mathf.Sign(targetVelocity.x);
        // targetVelocity.z = Mathf.Min(Mathf.Abs(targetVelocity.z), _controller.moveSpeed) * Mathf.Sign(targetVelocity.z);
        //
        // _controller.SetRelativeVelocity(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
    }

    public void OnExit()
    {
    }
}