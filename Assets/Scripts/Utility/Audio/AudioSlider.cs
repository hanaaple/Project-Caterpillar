using UnityEngine;
using UnityEngine.UI;

namespace Utility.Audio
{
    public class AudioSlider : MonoBehaviour
    {
        [Header("오디오 소스")]
        [SerializeField] private Slider slider;
        [SerializeField] private string slideName;

        private void Start()
        {
            slider.onValueChanged.AddListener(value =>
            {
                AudioManager.Instance.audioMixer.SetFloat(slideName, value <= slider.minValue ? -80f : slider.value);
            });
        }
    }
}
