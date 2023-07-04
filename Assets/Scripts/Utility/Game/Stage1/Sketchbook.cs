using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.Game.Stage1
{
    public class Sketchbook : MiniGame
    {
        [SerializeField] private EventTrigger button;
        [SerializeField] private Image fill;
        
        [SerializeField] private float maxFill;

        [SerializeField] private float radiusOffset;

        protected override void Init()
        {
            base.Init();
            var onPointerDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };
            
            onPointerDrag.callback.AddListener(_ =>
            {
                var pointerEventData = _ as PointerEventData;

                var orientation = ((Vector3)pointerEventData.position - fill.rectTransform.position).normalized;
                var radius = fill.rectTransform.rect.width / 2 - radiusOffset;

                var angle = Vector3.SignedAngle(orientation, -fill.rectTransform.up, Vector3.back) + 180;
                Debug.Log(angle);
                // if (fill.fillAmount * 360 < angle && )
                // {
                //     
                // }
                if (angle <= 0)
                {
                    return;
                }
                
                // 오른쪽으로 돌리면서 커진 경우 스탑
                
                if (angle >= maxFill)
                {
                    angle = maxFill;
                    End();
                }
                
                fill.fillAmount = angle / 360;
                
                button.transform.position = radius * orientation + fill.rectTransform.position;
            });
            
            button.triggers.Add(onPointerDrag);
        }

        protected override void End()
        {
            button.triggers.Clear();
            base.End();
        }
    }
}