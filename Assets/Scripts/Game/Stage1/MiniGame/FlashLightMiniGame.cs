using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Audio;

namespace Game.Stage1.MiniGame
{
    public class FlashLightMiniGame : Default.MiniGame
    {
        [SerializeField] private EventTrigger[] eventTriggers;
        [SerializeField] private EventTrigger flashLight;
        
        [SerializeField] private AudioClip takeAudioClip;
        [SerializeField] private AudioClip dropAudioClip;
        
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
                    _offset = pointerEventData.position - (Vector2) ((RectTransform) eventTrigger.transform).position;
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

                var beginDrag = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.BeginDrag
                };

                beginDrag.callback.AddListener(_ => { AudioManager.Instance.PlaySfx(takeAudioClip); });

                var endDrag = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.EndDrag
                };

                endDrag.callback.AddListener(_ => { AudioManager.Instance.PlaySfx(dropAudioClip); });


                eventTrigger.triggers.Add(pointerDown);
                eventTrigger.triggers.Add(pointerEvent);
                eventTrigger.triggers.Add(beginDrag);
                eventTrigger.triggers.Add(endDrag);
            }

            flashLight.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
            pointerEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            pointerEvent.callback.AddListener(_ => { End(); });
            flashLight.triggers.Add(pointerEvent);
        }

        protected override void End(bool isClear = true)
        {
            foreach (var eventTrigger in eventTriggers)
            {
                eventTrigger.triggers.Clear();
            }

            flashLight.triggers.Clear();
            base.End(isClear);
        }
    }
}