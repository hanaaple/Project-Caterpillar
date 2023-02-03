using UnityEngine;
using UnityEngine.EventSystems;

public class CampingDropItem : DropItem
{
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;

    protected override void Start()
    {
        base.Start();
        if (dragItem)
        {
            CampingDragItem campingDragItem = dragItem.GetComponent<CampingDragItem>();
            if (campingDragItem)
            {
                campingDragItem.x = x;
                campingDragItem.y = y;
            }
        }
    }

    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);
        if (dragItem)
        {
            CampingDragItem campingDragItem = dragItem.GetComponent<CampingDragItem>();
            if (campingDragItem)
            {
                campingDragItem.x = x;
                campingDragItem.y = y;
            }
        }
    }
}