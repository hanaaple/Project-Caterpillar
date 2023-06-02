using UnityEngine;
using UnityEngine.UI;

namespace Utility.UI.Util
{
    public class VerticalSliderScrollPosition : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private ScrollRect scrollRect;

        private void Awake()
        {
            slider.onValueChanged.AddListener(ChangeScrollPos);
            scrollRect.onValueChanged.AddListener(ChangeSliderPos);
        }

        private void ChangeScrollPos(float value)
        {
            scrollRect.verticalNormalizedPosition = value;
        }   

        private void ChangeSliderPos(Vector2 vector)
        {
            slider.value = vector.y;
        }
    }
}