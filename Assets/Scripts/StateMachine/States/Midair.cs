using UnityEngine;

public class Midair : IState
{
    private readonly PlayerController _controller;
    private readonly Transform _transform;
    private readonly Rigidbody _rb;

    public Midair(PlayerController controller, Transform transform, Rigidbody rb)
    {
        _controller = controller;
        _transform = transform;
        _rb = rb;
    }
    
    public void OnEnter()
    {
    }

    public void Tick()
    {
        _controller.ResetHasToJump();
        _rb.velocity = _transform.forward * _controller.Move.y + _transform.right * _controller.Move.x +
                       _transform.up * _rb.velocity.y;
    }

    public void OnExit()
    {
    }
}