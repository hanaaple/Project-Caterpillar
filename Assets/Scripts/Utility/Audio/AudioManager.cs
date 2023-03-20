using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Utility.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<AudioManager>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
    
        [Header("슬라이더")]
        public Slider bgmSlider;
        public Slider sfxSlider;

        [Header("오디오 소스")]
        public AudioSource bgm;
        public AudioSource sfx;
    
        [Header("오디오 믹서")]
        public AudioMixer audioMixer;

        private bool _isPaused;

        private static AudioManager Create()
        {
            var sceneLoaderPrefab = Resources.Load<AudioManager>("AudioManager");
            return Instantiate(sceneLoaderPrefab);
        }

        private void Start()
        {
            sfxSlider?.onValueChanged.AddListener(value =>
            {
                audioMixer.SetFloat("Sfx", value <= sfxSlider.minValue ? -80f : sfxSlider.value);
            });

            bgmSlider?.onValueChanged.AddListener(value =>
            {
                audioMixer.SetFloat("Bgm", value <= bgmSlider.minValue ? -80f : bgmSlider.value);
            });
        }

        public void PlaySfx(AudioClip audioClip)
        {
            sfx.PlayOneShot(audioClip);
        }
    
    
        public void PlayBgm(AudioClip audioClip)
        {
            if (bgm.clip != audioClip)
            {
                bgm.clip = audioClip;   
            }
            if (_isPaused)
            {
                bgm.UnPause();   
            }
            else
            {
                bgm.Play();   
            }
        }    
    
        public void PauseBgm()
        {
            _isPaused = true;
            bgm.Pause();
        }
    
        public void StopBgm()
        {
            _isPaused = false;
            bgm.clip = null; 
            bgm.Stop();
        }
    }
}
