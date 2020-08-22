using UnityEngine;

public class WallJump : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;

    public WallJump(PlayerController controller, Rigidbody rb)
    {
        _controller = controller;
        _rb = rb;
    }

    public void OnEnter()
    {
        _controller.SetRelativeVelocity(0f, _controller.wallJumpSpeed.y, -_controller.wallJumpSpeed.x);
        _controller.OnWallJump();
        // _rb.velocity = -_transform.forward * _controller.wallJumpSpeed.x + _transform.up * _controller.wallJumpSpeed.y;
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}