public class LedgeClimb : IState
{
    private readonly PlayerController _controller;

    public LedgeClimb(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        _controller.SetAbsoluteVelocity(_controller.Velocity.x, _controller.LedgeClimbVelocity, _controller.Velocity.z);
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}