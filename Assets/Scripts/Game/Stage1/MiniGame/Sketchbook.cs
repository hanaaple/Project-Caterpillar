using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Util;

namespace Game.Stage1.MiniGame
{
    public class Sketchbook : Default.MiniGame
    {
        [SerializeField] private EventTrigger button;
        [SerializeField] private Image fill;

        [SerializeField] private float degreeOffset;
        
        [SerializeField] private float minDegree;
        [SerializeField] private float maxDegree;

        [SerializeField] private float radiusOffset;
        
        [SerializeField] private AudioData drawAudioData;

        [Range(0f, 360f)] [SerializeField] private float angle;

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateDisplay();
            }
        }

        protected override void Init()
        {
            base.Init();

            angle = 0;
            
            EventTriggerHelper.AddEntry(button, EventTriggerType.Drag, _ =>
            {
                var pointerEventData = _ as PointerEventData;

                var orientation = ((Vector3)(pointerEventData.position * Operators.WindowToCanvasVector2) - fill.rectTransform.position).normalized;

                angle = Vector3.SignedAngle(orientation, -fill.rectTransform.up, Vector3.back) + 180;
                // if (fill.fillAmount * 360 < angle && )
                // {
                //     
                // }
                if (angle < 0)
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
        }

        protected override void End(bool isClear = true)
        {
            button.triggers.Clear();
            base.End(isClear);
        }

        private void UpdateDisplay()
        {
            angle = Mathf.Clamp(angle, minDegree, maxDegree);
            var radius = fill.rectTransform.rect.width / 2 + radiusOffset;
            var orientation = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (angle + 90 + degreeOffset)),
                Mathf.Sin(Mathf.Deg2Rad * (angle + 90 + degreeOffset))).normalized;
            fill.fillAmount = angle / 360;

            ((RectTransform) button.transform).anchoredPosition = radius * orientation + fill.rectTransform.anchoredPosition;
            
            Debug.Log(radius);
            
            if (Application.isPlaying)
            {
                drawAudioData.Play();
            }
        }
    }
}