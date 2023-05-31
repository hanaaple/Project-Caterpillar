using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Drag_Drop;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingDropItem : DropItem
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        protected override void Start()
        {
            base.Start();

            if (dragItem && dragItem.TryGetComponent(out CampingDragItem campingDragItem))
            {
                campingDragItem.x = x;
                campingDragItem.y = y;
            }
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (!dragItem && eventData.pointerDrag &&
                eventData.pointerDrag.TryGetComponent(out CampingDragItem campingDragItem))
            {
                base.OnDrop(eventData);
                campingDragItem.x = x;
                campingDragItem.y = y;
            }
        }
    }
}