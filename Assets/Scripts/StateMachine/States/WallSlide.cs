using UnityEngine;

public class WallSlide : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;

    public WallSlide(PlayerController controller, Rigidbody rb)
    {
        _controller = controller;
        _rb = rb;
    }

    public void OnEnter()
    {
    }

    public void Tick()
    {
        // _rb.velocity = Vector3.down * _controller.wallSlideSpeed + _transform.right * _controller.Move.x;
        _controller.SetRelativeVelocity(_controller.Move.x, -_controller.wallSlideSpeed, 0f);
    }

    public void OnExit()
    {
    }
}