using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Utility.Timeline
{
    public class SignalReceiverBgm : MonoBehaviour, INotificationReceiver
    {
        public SignalAssetEventPair[] signalAssetEventPairs;

        [Serializable]
        public class SignalAssetEventPair
        {
            public SignalAsset signalAsset;
            public ParameterizedEvent events;

            [Serializable]
            public class ParameterizedEvent : UnityEvent<Object>
            {
            }
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is ParameterizedEmitter<Object> audioEmitter)
            {
                var matches = signalAssetEventPairs.Where(x => ReferenceEquals(x.signalAsset, audioEmitter.asset));
                foreach (var m in matches)
                {
                    m.events.Invoke(audioEmitter.parameter);
                }
            }
        }
    }
}