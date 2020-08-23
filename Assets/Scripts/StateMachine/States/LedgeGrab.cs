public class LedgeGrab : IState
{
    private readonly PlayerController _controller;
    private bool _hasGrabbed;

    public LedgeGrab(PlayerController controller)
    {
        _controller = controller;
        _hasGrabbed = false;
    }

    public void OnEnter()
    {
        _hasGrabbed = false;
    }

    public void Tick()
    {
        if (!_hasGrabbed)
        {
            float deltaY = _controller.VaultCheckHit.point.y - _controller.transform.position.y;
            if (deltaY > 0.5f && deltaY < _controller.VaultHeight)
            {
                _hasGrabbed = true;
                _controller.DisableGravity();
            }
        }


        _controller.SetRelativeVelocity(_controller.Move.x/_controller.MoveSpeed * _controller.HorizontalWallSlideSpeed,
                                        _hasGrabbed
                                            ? 0f
                                            : _controller.Velocity.y, 0f);
    }

    public void OnExit()
    {
        _controller.EnableGravity();
        _hasGrabbed = false;
    }
}