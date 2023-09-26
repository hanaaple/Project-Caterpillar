using System.Collections.Generic;
using UnityEngine;
using Utility.Audio;

namespace Utility.Util
{
    public static class TimeScaleHelper
    {
        private static readonly Stack<float> TimeScaleStack = new ();

        public static void Push(float timeScale)
        {
            TimeScaleStack.Push(timeScale);
            SetTimeScale(timeScale);
        }

        public static void Pop()
        {
            TimeScaleStack.Pop();
            if (TimeScaleStack.Count > 0)
            {
                var timeScale = TimeScaleStack.Peek();
                SetTimeScale(timeScale);
            }
            else
            {
                SetTimeScale(1f);
            }
        }

        private static void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            AudioManager.Instance.GetAudioSource(AudioSourceType.Sfx).pitch = timeScale;
            AudioManager.Instance.GetAudioSource(AudioSourceType.Bgm).pitch = timeScale;
        }
    }
}
