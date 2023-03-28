using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Audio
{
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private string audioSourceName;
        [SerializeField] private TMP_Text text;
        [Header("음소거")] [SerializeField] private Toggle toggle;

        private void Start()
        {
            slider.onValueChanged.AddListener(value =>
            {
                var slideRatio = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
                AudioManager.SetVolume(audioSourceName, slideRatio);
                text.text = $"{slideRatio * 100f:0}";
            });

            toggle.onValueChanged.AddListener(value => { AudioManager.SetMute(audioSourceName, value); });
        }

        private void OnEnable()
        {
            var audioSource = AudioManager.GetAudioSource(audioSourceName);
            toggle.isOn = audioSource.mute;
            
            var slideValue = Mathf.Lerp(slider.minValue, slider.maxValue, audioSource.volume);
            slider.value = slideValue;
            text.text = $"{audioSource.volume * 100f:0}";
        }
    }
}
