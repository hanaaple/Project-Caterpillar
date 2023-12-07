using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Util;

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

        public Action onHighlight;
        public Action onDeHighlight;
        public Action onExecute;
        public Action onSelect;
        public Action onDeSelect;

        public void Init()
        {
            isEnable = true;
            TransitionTypes = new List<TransitionType>();
            ClearEventTrigger();
            SetDefault();
        }

        public virtual void Reset()
        {
            TransitionTypes.Clear();
            SetDefault();
        }

        public void Push(TransitionType transitionType)
        {
            if (TransitionTypes.Contains(transitionType))
            {
                Debug.Log(transitionType + "이미 있음");
                return;
            }

            Debug.Log($"Push {transitionType}");
            
            if (transitionType == TransitionType.Select)
            {
                Select();
            }
            else if (transitionType == TransitionType.Highlight)
            {
                Highlight();
            }

            // if (transitionType == TransitionType.Highlight)
            // {
            //     // Audio UI Highlgiht
            // }

            TransitionTypes.Add(transitionType);
            UpdateDisplay();
        }

        public virtual void Pop(TransitionType transitionType)
        {
            if (TransitionTypes.Contains(transitionType))
            {
                if (transitionType == TransitionType.Select)
                {
                    DeSelect();
                }
                else if (transitionType == TransitionType.Highlight)
                {
                    DeHighlight();
                }

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

            ResetUpdate();

            foreach (var transitionType in TransitionTypes)
            {
                // Debug.Log($"{button.gameObject} - {transitionType}");
                if (transitionType.Equals(TransitionType.Select))
                {
                    SelectUpdate();
                }
                else if (transitionType.Equals(TransitionType.Highlight))
                {
                    HighlightUpdate();
                }
            }
        }

        public void Execute()
        {
            onExecute?.Invoke();
            button.onClick?.Invoke();
        }

        /// <summary>
        /// Reset Update (sprite, color, etc...)
        /// </summary>
        public virtual void ResetUpdate()
        {
        }

        /// <summary>
        /// Use For Display (sprite, color, etc...) It works several times
        /// </summary>
        public virtual void HighlightUpdate()
        {
        }

        /// <summary>
        /// Use For Display (sprite, color, etc...) It works several times
        /// </summary>
        public virtual void SelectUpdate()
        {
        }

        /// <summary>
        /// Use for Reset event
        /// </summary>
        public virtual void SetDefault()
        {

        }

        /// <summary>
        /// Use For Select Event
        /// </summary>
        public virtual void Highlight()
        {
            onHighlight?.Invoke();
        }

        /// <summary>
        /// Use For Select Event
        /// </summary>
        public virtual void DeHighlight()
        {
            onDeHighlight?.Invoke();
        }

        /// <summary>
        /// Use For Select Event
        /// </summary>
        public virtual void Select()
        {
            onSelect?.Invoke();
        }

        /// <summary>
        /// Use For Select Event
        /// </summary>
        public virtual void DeSelect()
        {
            onDeSelect?.Invoke();
        }

        public virtual void AddEventTrigger(EventTriggerType eventTriggerType, UnityAction<BaseEventData> action)
        {
            var eventTrigger = button.GetComponent<EventTrigger>();
            
            EventTriggerHelper.CreateOrAddEntry(eventTrigger, eventTriggerType, _ =>
            {
                if (_ is not PointerEventData {button: PointerEventData.InputButton.Left} pointerEventData)
                    return;

                action(_);
            });
        }

        public virtual void ClearEventTrigger()
        {
            var eventTrigger = button.GetComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
        }
    }
}