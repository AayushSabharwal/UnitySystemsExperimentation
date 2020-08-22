public class Jump : IState
{
    private readonly PlayerController _controller;

    public Jump(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        _controller.SetAbsoluteVelocity(_controller.Velocity.x,
                                        _controller.Velocity.y + _controller.JumpSpeed,
                                        _controller.Velocity.z);
        _controller.ResetHasToJump();
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}