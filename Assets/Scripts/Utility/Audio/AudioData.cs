using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
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
        [SerializeField] private Object audioObject;
        [Range(0, 1)] [SerializeField] private float volume;

        [SerializeField] private AudioSourceType audioSourceType;
        
        [ConditionalHideInInspector("audioSourceType", AudioSourceType.Sfx)] [SerializeField]
        private bool isLoop;

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
        
        public AudioSourceType AudioSourceType => audioSourceType;
        public bool IsLoop => isLoop;
        public Object AudioObject => audioObject;

        public float Length {
            get
            {
                return audioObject switch
                {
                    AudioClip audioClip => audioClip.length,
                    TimelineAsset timelineAsset => (float)timelineAsset.duration,
                    _ => 0f
                };
            }
        }

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