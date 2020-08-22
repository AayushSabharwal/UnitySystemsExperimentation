using UnityEngine;

public class Idle : IState
{
    private readonly PlayerController _controller;

    public Idle(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        _controller.SetRelativeVelocity(0f, 0f, 0f);
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}