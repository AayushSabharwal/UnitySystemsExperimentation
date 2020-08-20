using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private Image playerSprintSlider;

    private void Awake()
    {
        playerController.OnSprintValueChanged += UpdateSprintUI;
    }

    private void UpdateSprintUI(float percent)
    {
        playerSprintSlider.fillAmount = percent;
    }
}