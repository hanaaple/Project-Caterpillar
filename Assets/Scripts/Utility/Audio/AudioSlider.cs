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
        
        private Animator _toggleAnimator;
        
        private void Awake()
        {
            _toggleAnimator = toggle.GetComponent<Animator>();
            
            slider.onValueChanged.AddListener(value =>
            {
                var slideRatio = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
                AudioManager.SetVolume(audioSourceName, slideRatio);
                text.text = $"{slideRatio * 100f:0}";
            });

            toggle.onValueChanged.AddListener(isOn =>
            {
                _toggleAnimator.SetBool("IsOn", isOn);
                Debug.Log(_toggleAnimator.GetBool("IsOn"));
                AudioManager.SetMute(audioSourceName, isOn);
            });
        }
 
        /// <summary>
        /// Audio Load When PreferenceManager Awake
        /// </summary>
        private void OnEnable()
        {
            var audioSource = AudioManager.GetAudioSource(audioSourceName);
            _toggleAnimator.SetBool("IsOn", audioSource.mute);
            toggle.isOn = audioSource.mute;
            
            var slideValue = Mathf.Lerp(slider.minValue, slider.maxValue, audioSource.volume);
            slider.value = slideValue;
            text.text = $"{audioSource.volume * 100f:0}";
        }
    }
}
