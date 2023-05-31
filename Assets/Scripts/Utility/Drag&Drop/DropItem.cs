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
                dragItem.OnBeginDragAction.AddListener(Reset);
                dragItem.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;
            }
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag && !dragItem)
            {
                dragItem = eventData.pointerDrag.GetComponent<DragItem>();
                dragItem.OnBeginDragAction.AddListener(Reset);
                dragItem.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;
            }
        }

        private void Reset()
        {
            dragItem.OnBeginDragAction.RemoveListener(Reset);
            dragItem = null;
        }

        public bool HasItem()
        {
            return dragItem;
        }
    }
}