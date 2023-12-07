using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Property;
using Object = UnityEngine.Object;

namespace Utility.Audio
{
    public enum AudioSourceType
    {
        Bgm,
        Sfx
    }

    [Serializable]
    public class AudioData
    {
        [SerializeField] public Object audioObject;
        [Range(0, 1)] [SerializeField] private float volume;

        [SerializeField] internal AudioSourceType audioSourceType;
        
        [ConditionalHideInInspector("audioSourceType", AudioSourceType.Sfx)] [SerializeField]
        internal bool isLoop;

        [ConditionalHideInInspector("audioSourceType", AudioSourceType.Sfx)] [SerializeField]
        private bool isOneShot;

        [FormerlySerializedAs("isWithOutTimeScale")] [ConditionalHideInInspector("audioSourceType", AudioSourceType.Sfx)] [SerializeField]
        private bool ignoreTimeScale;

        [ConditionalHideInInspector("audioSourceType", AudioSourceType.Sfx)] [SerializeField]
        private bool isFade;

        /// <summary>
        /// sfx loop or bgm
        /// </summary>
        [SerializeField] private bool isCustomFade;

        [ConditionalHideInInspector("isCustomFade")] [SerializeField]
        private float fadeSec;

        [ConditionalHideInInspector("isCustomFade")] [SerializeField]
        private AnimationCurve animationCurve;

        public void Play()
        {
            if (audioSourceType == AudioSourceType.Bgm)
            {
                if (isCustomFade)
                {
                    AudioManager.Instance.PlayBgmWithFade(audioObject, volume, fadeSec, animationCurve);
                }
                else
                {
                    AudioManager.Instance.PlayBgmWithFade(audioObject, volume);
                }
            }
            else if (audioSourceType == AudioSourceType.Sfx)
            {
                if (isLoop)
                {
                    if (isCustomFade)
                    {
                        AudioManager.Instance.PlaySfxAsBgm(audioObject, volume, isFade, ignoreTimeScale, fadeSec, animationCurve);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySfxAsBgm(audioObject, volume, isFade, ignoreTimeScale);
                    }
                }
                else
                {
                    AudioManager.Instance.PlaySfx(audioObject, volume, isOneShot, ignoreTimeScale);
                }
            }
        }

        public void Stop()
        {
            if (audioSourceType == AudioSourceType.Bgm)
            {
                AudioManager.Instance.StopBgm();
            }
            else if (audioSourceType == AudioSourceType.Sfx)
            {
                if (isLoop)
                {
                    AudioManager.Instance.StopSfxAsBgm(audioObject, isFade);
                }
                else
                {
                    Debug.LogError("Stop Audio Error - OneShot을 멈추는 기능은 구현되어 있지 않습니다.");
                }
            }
        }
    }
}