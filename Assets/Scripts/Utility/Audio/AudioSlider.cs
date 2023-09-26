using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Audio
{
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private AudioSourceType audioSourceType;
        [SerializeField] private TMP_Text text;
        [Header("음소거")] [SerializeField] private Toggle toggle;

        private Animator _toggleAnimator;

        private static readonly int IsOnHash = Animator.StringToHash("IsOn");

        private void Awake()
        {
            _toggleAnimator = toggle.GetComponent<Animator>();

            slider.onValueChanged.AddListener(value =>
            {
                var slideRatio = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
                AudioManager.Instance.SetVolume(audioSourceType, slideRatio);
                text.text = $"{slideRatio * 100f:0}";
            });

            toggle.onValueChanged.AddListener(isOn =>
            {
                _toggleAnimator.SetBool(IsOnHash, isOn);
                AudioManager.Instance.SetMute(audioSourceType, !isOn);
            });
        }

        /// <summary>
        /// Audio Load When PreferenceManager Awake
        /// </summary>
        private void OnEnable()
        {
            var audioSource = AudioManager.Instance.GetAudioSource(audioSourceType);
            toggle.isOn = !audioSource.mute;
            _toggleAnimator.SetBool(IsOnHash, toggle.isOn);
            
            var slideValue = Mathf.Lerp(slider.minValue, slider.maxValue, AudioManager.Instance.GetBgmVolume(audioSourceType));
            slider.value = slideValue;
        }
    }
}