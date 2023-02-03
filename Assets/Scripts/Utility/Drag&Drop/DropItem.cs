using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour, IDropHandler
{
    [SerializeField]
    protected DragItem dragItem;

    protected virtual void Start()
    {
        if (dragItem)
        {
            dragItem.onBeginDrag.AddListener(Reset);
            dragItem.GetComponent<RectTransform>().anchoredPosition =
                GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag && !dragItem)
        {
            dragItem = eventData.pointerDrag.GetComponent<DragItem>();
            dragItem.onBeginDrag.AddListener(Reset);
            dragItem.GetComponent<RectTransform>().anchoredPosition =
                GetComponent<RectTransform>().anchoredPosition;
        }
    }

    private void Reset()
    {
        dragItem.onBeginDrag.RemoveListener(Reset);
        dragItem = null;
    }

    public bool HasItem()
    {
        return dragItem;
    }
}
