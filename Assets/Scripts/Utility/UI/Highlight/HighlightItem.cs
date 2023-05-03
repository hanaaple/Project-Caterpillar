using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.UI.Highlight
{
    [Serializable]
    public abstract class HighlightItem
    {
        public enum TransitionType
        {
            Highlight,
            Select
        }

        public Button button;

        protected List<TransitionType> TransitionTypes;

        public bool isEnable;

        public void Init()
        {
            isEnable = true;
            TransitionTypes = new List<TransitionType>();
            ClearEventTrigger();
            SetDefault();
        }

        public virtual void Reset()
        {
            TransitionTypes = new List<TransitionType>();
            SetDefault();
        }

        public void Push(TransitionType transitionType)
        {
            if (TransitionTypes.Contains(transitionType))
            {
                Debug.Log(transitionType + "이미 있음");
                return;
            }

            TransitionTypes.Add(transitionType);
            UpdateDisplay();
        }

        public virtual void Pop(TransitionType transitionType)
        {
            if (TransitionTypes.Contains(transitionType))
            {
                TransitionTypes.Remove(transitionType);
            }
            else
            {
                Debug.Log(transitionType + "없음");
            }
        }

        public void UpdateDisplay()
        {
            if (!isEnable)
            {
                return;
            }
            
            SetDefault();

            foreach (var transitionType in TransitionTypes)
            {
                if (transitionType.Equals(TransitionType.Select))
                {
                    SetSelect();
                }
                else if (transitionType.Equals(TransitionType.Highlight))
                {
                    EnterHighlight();
                }
            }
        }

        public void Execute()
        {
            button.onClick?.Invoke();
        }

        public abstract void SetDefault();

        public abstract void EnterHighlight();

        public abstract void SetSelect();

        public virtual void AddEventTrigger(EventTriggerType eventTriggerType, UnityAction<BaseEventData> onHighlight)
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();

            if (eventTrigger.triggers.Any(item => item.eventID == eventTriggerType))
            {
                return;
            }

            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry
            {
                eventID = eventTriggerType
            };
            entryPointerDown.callback.AddListener(onHighlight);
            eventTrigger.triggers.Add(entryPointerDown);
        }

        public virtual void ClearEventTrigger()
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
        }
    }
}