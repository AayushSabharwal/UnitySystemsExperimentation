using UnityEngine;

// ReSharper disable CheckNamespace

public class Walking : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;

    public Walking(PlayerController controller, Rigidbody rb)
    {
        _controller = controller;
        _rb = rb;
    }

    public void OnEnter()
    {
    }

    public void Tick()
    {
        _controller.SetRelativeVelocity(_controller.Move.x, _rb.velocity.y, _controller.Move.y);
        // _rb.velocity = _transform.forward * _controller.Move.y + _transform.right * _controller.Move.x +
        //                _transform.up * _rb.velocity.y;
    }

    public void OnExit()
    {
    }
}