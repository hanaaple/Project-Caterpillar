using System;
using UnityEngine;

namespace Game.Stage1.Camping.Interaction.Diary
{
    [RequireComponent(typeof(Animator))]
    public class Diary : MonoBehaviour
    {
        [Range(0, .1f)] [SerializeField] private float disValue;

        [SerializeField] private CircleCollider2D fire;

        public Action onOpen;
        public Action onFire;
        public Action onPickUp;

        private Animator _diaryAnimator;
        private Vector3 _clickedPos;
        private bool _isDrag;
        private static readonly int IsOutHash = Animator.StringToHash("IsOut");

        // Out State에서 Drag해도 안움직일거임
        
        private void Awake()
        {
            _diaryAnimator = GetComponent<Animator>();
        }
        
        private void OnMouseDown()
        {
            if (!_diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }

            _clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _clickedPos = new Vector3(_clickedPos.x, _clickedPos.y, 0);
        }

        private void OnMouseDrag()
        {
            if (!_diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }

            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = new Vector3(pos.x, pos.y, 0);

            if (_isDrag)
            {
                transform.position = pos;
            }
            else if (Vector3.Distance(_clickedPos, pos) > disValue)
            {
                transform.position = pos;
                _isDrag = true;
            }
        }

        private void OnMouseUp()
        {
            if (_diaryAnimator.GetBool(IsOutHash))
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
                _diaryAnimator.SetBool(IsOutHash, true);
                onPickUp?.Invoke();
            }
        }

        public void Reset()
        {
            _diaryAnimator.SetBool(IsOutHash, false);
            gameObject.SetActive(false);
        }
    }
}