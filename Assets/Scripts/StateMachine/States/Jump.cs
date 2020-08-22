using UnityEngine;

public class Jump : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;

    public Jump(PlayerController controller, Rigidbody rb)
    {
        _controller = controller;
        _rb = rb;
    }

    public void OnEnter()
    {
        _controller.SetAbsoluteVelocity(_rb.velocity.x, _rb.velocity.y + _controller.jumpSpeed, _rb.velocity.z);
        _controller.ResetHasToJump();
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}