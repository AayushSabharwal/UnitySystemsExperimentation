public class WallSlide : IState
{
    private readonly PlayerController _controller;

    public WallSlide(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
    }

    public void Tick()
    {
        _controller.SetRelativeVelocity(_controller.Move.x, -_controller.WallSlideSpeed, 0f);
    }

    public void OnExit()
    {
    }
}