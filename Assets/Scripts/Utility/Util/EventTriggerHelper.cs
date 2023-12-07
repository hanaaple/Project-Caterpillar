using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Utility.Util
{
    public static class EventTriggerHelper
    {
        public static void CreateOrAddEntry(EventTrigger eventTrigger, EventTriggerType eventID,
            UnityAction<BaseEventData> call)
        {
            var entry = eventTrigger.triggers.Find(item => item.eventID == eventID);

            if (entry == null)
            {
                entry = new EventTrigger.Entry
                {
                    eventID = eventID
                };
                eventTrigger.triggers.Add(entry);
            }

            entry.callback.AddListener(call);
        }
    
        public static void CreateOrAddEntry(EventTrigger eventTrigger, EventTriggerType eventID,
            UnityAction call)
        {
            var entry = eventTrigger.triggers.Find(item => item.eventID == eventID);

            if (entry == null)
            {
                entry = new EventTrigger.Entry
                {
                    eventID = eventID
                };
                eventTrigger.triggers.Add(entry);
            }

            entry.callback.AddListener(_ =>
            {
                call();
            });
        }
        
        public static void AddEntry(EventTrigger eventTrigger, EventTriggerType eventID,
            UnityAction<BaseEventData> call)
        {
            var pointerEntry = new EventTrigger.Entry
            {
                eventID = eventID
            };
            pointerEntry.callback.AddListener(call);

            eventTrigger.triggers.Add(pointerEntry);
        }
        
        public static void AddEntry(EventTrigger eventTrigger, EventTriggerType eventID,
            UnityAction call)
        {
            var pointerEntry = new EventTrigger.Entry
            {
                eventID = eventID
            };
            pointerEntry.callback.AddListener(_ =>
            {
                call();
            });

            eventTrigger.triggers.Add(pointerEntry);
        }
    }
}