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
        Vector3 velocity = _rb.velocity;
        velocity.y += _controller.jumpSpeed;
        _rb.velocity = velocity;

        _controller.NotifyHasJumped();
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}