using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Utility.Game.Stage1
{
    public class Sketchbook : MiniGame
    {
        [SerializeField] private EventTrigger button;
        [SerializeField] private Image fill;

        [SerializeField] private float degreeOffset;
        
        [SerializeField] private float minDegree;
        [SerializeField] private float maxDegree;

        [SerializeField] private float radiusOffset;

        [Range(0f, 360f)] [SerializeField] private float angle;

        // private void OnValidate()
        // {
        //     UpdateDisplay();
        // }

        protected override void Init()
        {
            base.Init();

            angle = 0;
            
            var onPointerDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };

            onPointerDrag.callback.AddListener(_ =>
            {
                var pointerEventData = _ as PointerEventData;

                var orientation = ((Vector3)pointerEventData.position - fill.rectTransform.position).normalized;

                angle = Vector3.SignedAngle(orientation, -fill.rectTransform.up, Vector3.back) + 180;
                // if (fill.fillAmount * 360 < angle && )
                // {
                //     
                // }
                if (angle <= 0)
                {
                    return;
                }

                // 오른쪽으로 돌리면서 커진 경우 스탑

                if (angle >= maxDegree)
                {
                    angle = maxDegree;
                    End();
                }


                UpdateDisplay();
            });

            button.triggers.Add(onPointerDrag);
        }

        protected override void End()
        {
            button.triggers.Clear();
            base.End();
        }

        private void UpdateDisplay()
        {
            angle = Mathf.Clamp(angle, minDegree, maxDegree);
            var radius = fill.rectTransform.rect.width / 2 - radiusOffset;
            var orientation = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (angle + 90 + degreeOffset)),
                Mathf.Sin(Mathf.Deg2Rad * (angle + 90 + degreeOffset))).normalized;
            fill.fillAmount = angle / 360;

            button.transform.position = radius * orientation + fill.rectTransform.position;
        }
    }
}