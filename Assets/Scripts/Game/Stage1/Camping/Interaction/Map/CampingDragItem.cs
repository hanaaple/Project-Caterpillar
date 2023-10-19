using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;
using Utility.Drag_Drop;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingDragItem : DragItem
    {
        public AudioClip takeAudioClip;
        public AudioClip dropAudioClip;
        
        [Header("For Debug")]
        public int x;
        public int y;
        
        
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            
            if (isInteractable)
            {
                AudioManager.Instance.PlaySfx(takeAudioClip);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            
            if (isInteractable)
            {
                AudioManager.Instance.PlaySfx(dropAudioClip);
            }
        }
    }
}
