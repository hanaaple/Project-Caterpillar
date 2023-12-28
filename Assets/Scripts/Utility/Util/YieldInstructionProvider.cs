using System.Collections.Generic;
using UnityEngine;

namespace Utility.Util
{
    public static class YieldInstructionProvider
    {
        private static readonly Dictionary<float, WaitForSeconds> TimeInterval = new ();
        private static readonly Dictionary<float, WaitForSecondsRealtime> RealTimeInterval = new ();
        private static readonly WaitForFixedUpdate FixedUpdate = new ();

        public static WaitForFixedUpdate WaitForFixedUpdate()
        {
            return FixedUpdate;
        }
        
        public static WaitForSeconds WaitForSeconds(float waitSec)
        {
            // Trim to 6 decimal place
            waitSec = Mathf.Round(waitSec * 1000000) / 1000000;
            if (TimeInterval.TryGetValue(waitSec, out var waitForSeconds))
            {
                return waitForSeconds;
            }

            var t = new WaitForSeconds(waitSec);
            TimeInterval.Add(waitSec, t);
            return null;
        }
        
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float waitSec)
        {
            // Trim to 6 decimal place
            waitSec = Mathf.Round(waitSec * 1000000) / 1000000;
            if (RealTimeInterval.TryGetValue(waitSec, out var waitForSeconds))
            {
                return waitForSeconds;
            }

            var t = new WaitForSecondsRealtime(waitSec);
            RealTimeInterval.Add(waitSec, t);
            return null;
        }
    }
}