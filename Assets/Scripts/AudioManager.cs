using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    
    [Header("슬라이더")]
    public Slider volumeSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("오디오 소스")]
    public AudioSource bgm;
    public AudioSource sfx;
    
    [Header("오디오 믹서")]
    public AudioMixer audioMixer;

    private bool _isPaused = false;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        volumeSlider.onValueChanged.AddListener(value =>
        {
            audioMixer.SetFloat("Volume", (value <= volumeSlider.minValue) ? -80f : volumeSlider.value);
        });
        
        sfxSlider.onValueChanged.AddListener(value =>
        {
            audioMixer.SetFloat("Sfx", (value <= sfxSlider.minValue) ? -80f : sfxSlider.value);
        });
        
        bgmSlider.onValueChanged.AddListener(value =>
        {
            audioMixer.SetFloat("Bgm", (value <= bgmSlider.minValue) ? -80f : bgmSlider.value);
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
