public class WallJump : IState
{
    private readonly PlayerController _controller;

    public WallJump(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        _controller.SetRelativeVelocity(0f, _controller.WallJumpSpeed.y, -_controller.WallJumpSpeed.x);
        _controller.OnWallJump();
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}