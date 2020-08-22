public class Sprinting : IState
{
    private readonly PlayerController _controller;
    private readonly CameraFXManager _cameraFx;

    public Sprinting(PlayerController controller, CameraFXManager cameraFx)
    {
        _controller = controller;
        _cameraFx = cameraFx;
    }

    public void OnEnter()
    {
        _cameraFx.SetFOV(true);
    }

    public void Tick()
    {
        // _rb.velocity = (_transform.forward * _controller.Move.y + _transform.right * _controller.Move.x) *
        //                _controller.sprintMultiplier +
        //                _transform.up * _rb.velocity.y;
        _controller.SetRelativeVelocity(_controller.Move.x * _controller.SprintMultiplier,
                                        _controller.Velocity.y,
                                        _controller.Move.y * _controller.SprintMultiplier);
    }

    public void OnExit()
    {
        _cameraFx.SetFOV(false);
    }
}