using System;
using UnityEngine;

namespace Game.Stage1.Camping.Interaction.Diary
{
    public class Diary : MonoBehaviour
    {
        [Range(0, .1f)] [SerializeField] private float disValue;

        [SerializeField] private CircleCollider2D fire;
        [SerializeField] private Animator diaryAnimator;
        
        public Action onOpen;
        public Action onFire;
        public Action onPickUp;

        private Vector2 _clickedPos;
        private bool _isDrag;
        
        private static readonly int IsOutHash = Animator.StringToHash("IsOut");

        // Out State에서 Drag해도 안움직일거임

        private void OnMouseDown()
        {
            if (!diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }

            _clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseDrag()
        {
            if (!diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }
            
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;

            if (_isDrag)
            {
                transform.position = pos;
            }
            else if (Vector2.Distance(_clickedPos, pos) > disValue)
            {
                transform.position = pos;
                _isDrag = true;
            }
        }

        private void OnMouseUp()
        {
            if (diaryAnimator.GetBool(IsOutHash))
            {
                if (_isDrag)
                {
                    _isDrag = false;
                    if (Vector3.Distance(fire.transform.position, transform.position) < fire.radius)
                    {
                        onFire?.Invoke();
                    }
                }
                else
                {
                    onOpen?.Invoke();
                }
            }
            else
            {
                diaryAnimator.SetBool(IsOutHash, true);
                onPickUp?.Invoke();
            }
        }

        public void Reset()
        {
            diaryAnimator.SetBool(IsOutHash, false);
            gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;
        }
    }
}