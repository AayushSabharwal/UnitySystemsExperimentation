using UnityEngine;

public class WallSlide : IState
{
    private readonly PlayerController _controller;
    private readonly Rigidbody _rb;
    private Transform _transform;

    public WallSlide(PlayerController controller, Rigidbody rb, Transform transform)
    {
        _controller = controller;
        _rb = rb;
        _transform = transform;
    }

    public void OnEnter()
    {
    }

    public void Tick()
    {
        _rb.velocity = Vector3.down * _controller.wallSlideSpeed + _transform.right * _controller.Move.x;
    }

    public void OnExit()
    {
    }
}