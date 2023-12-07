using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingDropItem : MonoBehaviour, IDropHandler
    {
        [SerializeField] protected CampingDragItem dragItem;
        
        [SerializeField] private int x;
        [SerializeField] private int y;

        private bool _isInit;
        private CampingDragItem _originDragItem;
        
        protected void Start()
        {
            if (dragItem)
            {
                dragItem.OnBeginDragAction = ResetDropItem;
                var rectTransform = dragItem.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = rectTransform.anchoredPosition;
                
                if (dragItem.TryGetComponent(out CampingDragItem campingDragItem))
                {
                    campingDragItem.x = x;
                    campingDragItem.y = y;
                }
            }

            if (!_isInit)
            {
                _originDragItem = dragItem;
                _isInit = true;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!dragItem && eventData.pointerDrag &&
                eventData.pointerDrag.TryGetComponent(out CampingDragItem campingDragItem))
            {
                dragItem = eventData.pointerDrag.GetComponent<CampingDragItem>();
                dragItem.OnBeginDragAction = ResetDropItem;
                dragItem.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;

                campingDragItem.x = x;
                campingDragItem.y = y;
            }
        }

        public void ResetItem()
        {
            dragItem = _originDragItem;
            
            Start();
        }
        
        private void ResetDropItem()
        {
            dragItem.OnBeginDragAction = () => {};
            dragItem = null;
        }

        public bool HasItem()
        {
            return dragItem != null;
        }
    }
}