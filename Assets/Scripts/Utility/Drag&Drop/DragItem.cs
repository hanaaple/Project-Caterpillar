using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.Drag_Drop
{
    public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] protected bool isInteractable;
        
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;

        internal Action OnBeginDragAction;

        protected void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                OnBeginDragAction?.Invoke();
                _canvasGroup.alpha = .6f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isInteractable)
            {
                _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }
    }
}