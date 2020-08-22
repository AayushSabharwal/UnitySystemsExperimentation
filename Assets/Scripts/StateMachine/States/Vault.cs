using UnityEngine;

public class Vault : IState
{
    private readonly PlayerController _controller;
    private readonly Transform _transform;

    public Vault(PlayerController controller, Transform transform)
    {
        _controller = controller;
        _transform = transform;
    }

    public void OnEnter()
    {
        if (_controller.IsFacingWall)
        {
            _transform.position = new Vector3(_controller.transform.position.x, _controller.VaultCheckHit.point.y + 1f,
                                              _transform.position.z);
            return;
        }

        Vector3 wallPoint = _controller.WallCheckHit.point;
        Vector3 transformPosition = _transform.position;
        wallPoint.y = transformPosition.y;
        float distance = (wallPoint - transformPosition).magnitude;
        float time = distance / (_controller.MoveSpeed * 1.5f);
        float vel = (_controller.VaultCheckHit.point.y - transformPosition.y + 1f) / time -
                    Physics.gravity.y * time * 0.5f;
        _controller.SetAbsoluteVelocity(_controller.Velocity.x, vel, _controller.Velocity.z);
        _controller.ResetYVelocityAfter(time);
    }

    public void Tick()
    {
    }

    public void OnExit()
    {
    }
}