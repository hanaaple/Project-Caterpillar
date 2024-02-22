using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;
using Utility.Util;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected bool isInteractable;
        
        public AudioData takeAudioData;
        public AudioData dropAudioData;
        
        [Header("For Debug")]
        public int x;
        public int y;
        
        internal Action OnBeginDragAction;
        
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        protected void Start()
        {
            if (!TryGetComponent(out _canvasGroup))
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

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
                _rectTransform.anchoredPosition += eventData.delta * Operators.WindowToCanvasVector2;
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
