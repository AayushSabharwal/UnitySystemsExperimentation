using UnityEngine;

public class Sprinting : IState
{
    private readonly PlayerController _controller;
    private readonly Transform _transform;
    private readonly Rigidbody _rb;
    private readonly CameraFXManager _cameraFx;

    public Sprinting(PlayerController controller, Transform transform, Rigidbody rb, CameraFXManager cameraFx)
    {
        _controller = controller;
        _transform = transform;
        _rb = rb;
        _cameraFx = cameraFx;
    }

    public void OnEnter()
    {
        _cameraFx.SetFOV(true);
    }

    public void Tick()
    {
        _rb.velocity = (_transform.forward * _controller.Move.y + _transform.right * _controller.Move.x) *
                       _controller.sprintMultiplier +
                       _transform.up * _rb.velocity.y;
    }

    public void OnExit()
    {
        _cameraFx.SetFOV(false);
    }
}