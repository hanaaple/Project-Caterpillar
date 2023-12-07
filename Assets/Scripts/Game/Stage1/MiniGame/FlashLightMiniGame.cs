using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Util;

namespace Game.Stage1.MiniGame
{
    public class FlashLightMiniGame : Default.MiniGame
    {
        [SerializeField] private EventTrigger[] eventTriggers;
        [SerializeField] private EventTrigger flashLight;
        [SerializeField] private EventTrigger wadOfPaper;
        [SerializeField] private EventTrigger paperPanelEventTrigger;
        [SerializeField] private GameObject paperPanel;
        
        [SerializeField] private AudioClip takeAudioClip;
        [SerializeField] private AudioClip dropAudioClip;
        
        private Vector2 _offset;
        private bool _isDrag;

        protected override void Init()
        {
            base.Init();
            foreach (var eventTrigger in eventTriggers)
            {
                eventTrigger.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

                EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.PointerDown, _ =>
                {
                    var pointerEventData = _ as PointerEventData;
                    _offset = pointerEventData.position - (Vector2) ((RectTransform) eventTrigger.transform).position;
                });

                EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.Drag, _ =>
                {
                    var pointerEventData = _ as PointerEventData;
                    // Debug.Log($"{pointerEventData.position}");
                    ((RectTransform) eventTrigger.transform).position = pointerEventData.position - _offset;
                });

                EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.BeginDrag, _ =>
                {
                    // Debug.LogWarning("Begin Drag");
                    _isDrag = true;
                    AudioManager.Instance.PlaySfx(takeAudioClip);
                });
                
                EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.EndDrag, _ =>
                {
                    // Debug.LogWarning("End Drag");
                    _isDrag = false;
                    AudioManager.Instance.PlaySfx(dropAudioClip);
                });
            }
            
            EventTriggerHelper.AddEntry(wadOfPaper, EventTriggerType.PointerClick, _ =>
            {
                if (!_isDrag)
                {
                    paperPanel.SetActive(true);
                }
            });

            EventTriggerHelper.AddEntry(paperPanelEventTrigger, EventTriggerType.PointerClick, _ =>
            {
                paperPanel.SetActive(false);
            });
            
            EventTriggerHelper.AddEntry(flashLight, EventTriggerType.PointerDown, _ =>
            {
                End();
            });
        }

        protected override void End(bool isClear = true)
        {
            foreach (var eventTrigger in eventTriggers)
            {
                eventTrigger.triggers.Clear();
            }

            flashLight.triggers.Clear();
            wadOfPaper.triggers.Clear();
            paperPanelEventTrigger.triggers.Clear();
            base.End(isClear);
        }
    }
}