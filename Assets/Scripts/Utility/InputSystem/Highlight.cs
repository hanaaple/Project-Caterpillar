using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.InputSystem
{
    [Serializable]
    public abstract class Highlight
    {
        public Button button;

        public void Execute()
        {
            button.onClick?.Invoke();
        }
        
        public virtual void InitEventTrigger(UnityAction<BaseEventData> call)
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryPointerDown.callback.AddListener(call);
            eventTrigger.triggers.Add(entryPointerDown);
        }

        public abstract void SetDefault();

        public abstract void SetHighlight();
    }
}