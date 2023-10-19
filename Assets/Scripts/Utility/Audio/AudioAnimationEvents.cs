using UnityEngine;
using UnityEngine.Timeline;

namespace Utility.Audio
{
    public class AudioAnimationEvents : MonoBehaviour
    {
        public void PlayOneShot(AudioClip audioClip)
        {
            AudioManager.Instance.PlaySfx(audioClip);
        }
        
        public void PlayOneShotTimeline(TimelineAsset timelineAsset)
        {
            AudioManager.Instance.PlaySfx(timelineAsset);
        }
        
        public void PlayBgm(AudioClip audioClip)
        {
            AudioManager.Instance.PlayBgmWithFade(audioClip);
        }
    }
}
