using UnityEngine;
using UnityEngine.UI;

public class VerticalSliderScrollPosition : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        slider.onValueChanged.AddListener(ChangeScrollPos);
        scrollRect.onValueChanged.AddListener(ChangeSliderPos);
    }

    public void ChangeScrollPos(float value)
    {
        scrollRect.verticalNormalizedPosition = value;
    }

    public void ChangeSliderPos(Vector2 vector)
    {
        slider.value = vector.y;
    }
}