using UnityEngine;
using Utility.Audio;

namespace Utility.Util
{
    public class AnimationTriggerAudioHelper : MonoBehaviour
    {
        [SerializeField] private AnimationTrigger[] animationTriggers;
        [SerializeField] private AudioClip audioClip;
        
        private void Start()
        {
            foreach (var animationTrigger in animationTriggers)
            {
                animationTrigger.onTriggerEnter = () =>
                {
                    AudioManager.Instance.PlaySfx(audioClip);
                };
            }
        }
    }
}
