using System.Collections.Generic;
using UnityEngine;

namespace Utility.Util
{
    public static class TimeScaleHelper
    {
        private static readonly Stack<float> TimeScaleStack = new ();

        public static void Push(float timeScale)
        {
            TimeScaleStack.Push(timeScale);
            Time.timeScale = timeScale;
        }

        public static void Pop()
        {
            TimeScaleStack.Pop();
            if (TimeScaleStack.Count > 0)
            {
                var timeScale = TimeScaleStack.Peek();
                Time.timeScale = timeScale;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
}
