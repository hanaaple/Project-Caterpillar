using UnityEngine;
using UnityEngine.Timeline;
using Utility.Audio;
using Utility.Core;

namespace Utility.Timeline
{
    public class SignalReceiveInterface : MonoBehaviour
    {
        public void FocusLetterBox()
        {
            PlayUIManager.Instance.dialogueController.SetFocusMode(false);
        }
        
        public void PlayOneShot(Object obj)
        {
            if (obj is AudioClip audioClip)
            {
                AudioManager.Instance.PlaySfx(audioClip);
            }
            else if (obj is TimelineAsset timelineAsset)
            {
                AudioManager.Instance.PlaySfx(timelineAsset);
            }
        }
        
        public void PlayBgm(Object obj)
        {
            if (obj is AudioClip audioClip)
            {
                AudioManager.Instance.PlayBgmWithFade(audioClip);
            }
            else if (obj is TimelineAsset timelineAsset)
            {
                AudioManager.Instance.PlayBgmWithFade(timelineAsset);
            }
        }
    }
}
