using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Utility.Audio
{
    public class AudioManagerTest : MonoBehaviour
    {
        public Button sfxClipPlay;
        public Button sfxTimelinePlay;

        public Button bgmClipPlay;
        public Button bgmTimelinePlay;
        public Button bgmStop;
        public Button bgmReturn;
        
        public AudioClip audioClip;
        public AudioClip bgm;
        public TimelineAsset timelineAudioClip;

        private void Start()
        {
            Debug.LogWarning("Start Teststtt");
            sfxClipPlay.onClick.AddListener(PlaySfxClip);
            sfxTimelinePlay.onClick.AddListener(PlaySfxTimeline);
            bgmClipPlay.onClick.AddListener(PlayBgmClip);
            bgmTimelinePlay.onClick.AddListener(PlayBgmTimeline);
            bgmStop.onClick.AddListener(ReduceBgmVolume);
            bgmReturn.onClick.AddListener(ReturnBgmVolume);
        }

        public void PlaySfxTimeline()
        {
            AudioManager.Instance.PlaySfx(timelineAudioClip, 1f, false);
        }
        
        public void PlaySfxClip()
        {
            AudioManager.Instance.PlaySfx(audioClip, 1f, false);
        }
        
        public void PlayBgmTimeline()
        {
            AudioManager.Instance.PlayBgmWithFade(timelineAudioClip);
        }
        
        public void PlayBgmClip()
        {
            AudioManager.Instance.PlayBgmWithFade(bgm);
        }
        
        public void ReduceBgmVolume()
        {
            AudioManager.Instance.ReduceBgmVolume();
        }
        
        public void ReturnBgmVolume()
        {
            AudioManager.Instance.ReturnBgmVolume();
        }
    }
}