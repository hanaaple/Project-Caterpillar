using System;
using UnityEngine;

namespace Game.Stage1.Camping.Interaction.Diary
{
    [RequireComponent(typeof(Animator))]
    public class Bag : MonoBehaviour
    {
        [SerializeField] private GameObject diary;
        [SerializeField] private GameObject diaryMask;

        [NonSerialized] public bool IsOut;

        private Animator _bagAnimator;
        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

        public void Init()
        {
            _bagAnimator = GetComponent<Animator>();
        }

        private void OnMouseDown()
        {
            var isOpened = diaryMask.activeSelf;
            _bagAnimator.SetBool(IsOpenHash, !isOpened);
            
            if (!IsOut)
            {
                diary.SetActive(!isOpened);
            }
        }
        
        public void Reset()
        {
            _bagAnimator.SetBool(IsOpenHash, false);
            IsOut = false;
        }
    }
}
