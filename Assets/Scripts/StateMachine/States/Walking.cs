// ReSharper disable CheckNamespace

public class Walking : IState
{
    private readonly PlayerController _controller;

    public Walking(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
    }

    public void Tick()
    {
        _controller.SetRelativeVelocity(_controller.Move.x, _controller.Velocity.y, _controller.Move.y);
    }

    public void OnExit()
    {
    }
}