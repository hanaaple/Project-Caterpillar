using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Stage1.Camping.Interaction.Diary
{
    [RequireComponent(typeof(Animator))]
    public class Bag : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private GameObject diary;
        [SerializeField] private GameObject diaryMask;
        
        [NonSerialized] public bool IsInteractable;

        private Animator _bagAnimator;
        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

        public void Init()
        {
            _bagAnimator = GetComponent<Animator>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var isOpened = diaryMask.activeSelf;
            _bagAnimator.SetBool(IsOpenHash, !isOpened);
            
            if (IsInteractable)
            {
                diary.SetActive(!isOpened);
            }
        }
        
        public void Reset()
        {
            _bagAnimator.SetBool(IsOpenHash, false);
            IsInteractable = true;
        }
    }
}
