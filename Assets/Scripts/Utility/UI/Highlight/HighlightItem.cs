using System;
using System.Collections.Generic;
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
            Highlight, Select 
        } 
        
        public Button button;
        
        protected List<TransitionType> transitionTypes;
        
        public void Init()
        {
            transitionTypes = new List<TransitionType>();
            ClearEventTrigger();
            SetDefault();
        }
        
        public void Reset()
        {
            transitionTypes = new List<TransitionType>();
            SetDefault();
        }

        public void Add(TransitionType transitionType)
        {
            if (transitionTypes.Contains(transitionType))
            {
                Debug.Log(transitionType + "이미 있음");
                return;
            }
            transitionTypes.Add(transitionType);
            Highlight();
        }

        public virtual void Remove(TransitionType transitionType)
        {
            if (transitionTypes.Contains(transitionType))
            {
                transitionTypes.Remove(transitionType);
            }
            else
            {
                Debug.Log(transitionType + "없음");
            }

            Highlight();
        }

        protected void Highlight()
        {
            if (transitionTypes.Count > 0)
            {
                foreach (var transitionType in transitionTypes)
                {
                    if(transitionType.Equals(TransitionType.Select))
                    {
                        SetSelect();
                    }
                    else if(transitionType.Equals(TransitionType.Highlight))
                    {
                        EnterHighlight();
                    }   
                }
            }
            else
            {
                SetDefault();
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