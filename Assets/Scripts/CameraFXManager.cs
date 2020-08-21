using UnityEngine;

public class CameraFXManager : MonoBehaviour
{
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

    private void Awake()
    {
        _lastTween = -1;
        _isTweening = false;
    }

    public void SetFOV(bool isSprinting)
    {
        if (_isTweening)
            LeanTween.cancel(_lastTween);
        _isTweening = true;
        _lastTween = LeanTween.value(gameObject,
                                     fov => cam.fieldOfView = fov,
                                     isSprinting ? normalFOV : sprintFOV,
                                     isSprinting ? sprintFOV : normalFOV,
                                     tweenDuration)
                              .setOnComplete(() => _isTweening = false)
                              .id;
        
    }
}