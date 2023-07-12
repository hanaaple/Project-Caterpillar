using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.Game.Stage1
{
    public class FlashLightMiniGame : MiniGame
    {
        [SerializeField] private EventTrigger[] eventTriggers;
        [SerializeField] private EventTrigger flashLight;
        
        private Vector2 _offset;

        protected override void Init()
        {
            base.Init();
            EventTrigger.Entry pointerEvent;
            foreach (var eventTrigger in eventTriggers)
            {
                eventTrigger.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                
                var pointerDown = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };

                pointerDown.callback.AddListener(_ =>
                {
                    var pointerEventData = _ as PointerEventData;
                    _offset = pointerEventData.position - (Vector2)((RectTransform) eventTrigger.transform).position;
                });
                
                pointerEvent = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Drag
                };

                pointerEvent.callback.AddListener(_ =>
                {
                    var pointerEventData = _ as PointerEventData;
                    Debug.Log($"{pointerEventData.position}");
                    ((RectTransform) eventTrigger.transform).position = pointerEventData.position - _offset;
                });


                eventTrigger.triggers.Add(pointerDown);
                eventTrigger.triggers.Add(pointerEvent);
            }

            flashLight.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
            pointerEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            pointerEvent.callback.AddListener(_ => { End(); });
            flashLight.triggers.Add(pointerEvent);
        }

        protected override void End()
        {
            foreach (var eventTrigger in eventTriggers)
            {
                eventTrigger.triggers.Clear();
            }

            flashLight.triggers.Clear();
            base.End();
        }
    }
}