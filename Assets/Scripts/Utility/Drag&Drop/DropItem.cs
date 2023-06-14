using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.Drag_Drop
{
    public class DropItem : MonoBehaviour, IDropHandler
    {
        [SerializeField] protected DragItem dragItem;

        protected virtual void Start()
        {
            if (dragItem)
            {
                dragItem.OnBeginDragAction.RemoveAllListeners();
                dragItem.OnBeginDragAction.AddListener(ResetDropItem);
                dragItem.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;
            }
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag && !dragItem)
            {
                dragItem = eventData.pointerDrag.GetComponent<DragItem>();
                dragItem.OnBeginDragAction.AddListener(ResetDropItem);
                dragItem.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;
            }
        }

        private void ResetDropItem()
        {
            dragItem.OnBeginDragAction.RemoveListener(ResetDropItem);
            dragItem = null;
        }

        public bool HasItem()
        {
            return dragItem;
        }
    }
}