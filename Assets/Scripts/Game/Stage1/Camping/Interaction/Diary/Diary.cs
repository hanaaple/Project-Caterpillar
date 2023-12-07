using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Diary
{
    public class Diary : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Range(0, .1f)] [SerializeField] private float disValue;

        [SerializeField] private CircleCollider2D fire;
        [SerializeField] private Animator diaryAnimator;

        [SerializeField] private AudioData takeAudioData;
        [SerializeField] private AudioData dropAudioData;

        public Action onOpen;
        public Action onFire;
        public Action onPickUp;

        private Vector2 _clickedPos;
        private bool _isDrag;

        private static readonly int IsOutHash = Animator.StringToHash("IsOut");

        // Out State에서 Drag해도 안움직일거임
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }

            _clickedPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            takeAudioData.Play();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!diaryAnimator.GetBool(IsOutHash))
            {
                return;
            }

            var pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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

        public void OnPointerUp(PointerEventData eventData)
        {
            if (diaryAnimator.GetBool(IsOutHash))
            {
                if (_isDrag)
                {
                    dropAudioData.Play();
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