using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] protected bool isInteractable;
        
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;

        internal Action OnBeginDragAction;

        
        public AudioData takeAudioData;
        public AudioData dropAudioData;
        
        [Header("For Debug")]
        public int x;
        public int y;
        
        protected void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                OnBeginDragAction?.Invoke();
                _canvasGroup.alpha = .6f;
                _canvasGroup.blocksRaycasts = false;
                
                takeAudioData.Play();
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                
                dropAudioData.Play();
            }
        }
    }
}
