using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.Game.Stage1
{
    public class Sign : MiniGame
    {
        [SerializeField] private EventTrigger eventTrigger;
        
        protected override void Init()
        {
            base.Init();
            EventTrigger.Entry pointerEvent;

            pointerEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };

            pointerEvent.callback.AddListener(_ =>
            {
                
                // End();
            });
            
            eventTrigger.triggers.Add(pointerEvent);
        }
    }
}