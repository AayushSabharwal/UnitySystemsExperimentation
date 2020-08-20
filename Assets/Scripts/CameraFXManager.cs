using UnityEngine;

public class CameraFXManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private int normalFOV = 60;
    [SerializeField]
    private int sprintFOV = 65;
    [SerializeField]
    private float tweenDuration = 0.2f;

    private int _lastTween;
    private bool _isTweening;
    private int TargetFOV => playerController.IsSprinting ? sprintFOV : normalFOV;

    private void Awake()
    {
        _lastTween = -1;
        _isTweening = false;
    }

    private void Update()
    {
        if (!Mathf.Approximately(cam.fieldOfView, TargetFOV) && !_isTweening)
            SetFOV();
    }

    private void SetFOV()
    {
        if (_isTweening)
            LeanTween.cancel(_lastTween);
        _isTweening = true;
        _lastTween = LeanTween.value(gameObject,
                                     fov => cam.fieldOfView = fov,
                                     playerController.IsSprinting ? normalFOV : sprintFOV,
                                     TargetFOV,
                                     tweenDuration)
                              .setOnComplete(() => _isTweening = false)
                              .id;
        //cam.fieldOfView = playerController.IsSprinting ? sprintFOV : normalFOV;
    }
}