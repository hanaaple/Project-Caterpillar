using System;
using UnityEngine;

namespace Utility.Util
{
    public class Trigger : MonoBehaviour
    {
        public Action onTriggerEnter2D;
        public Action onTriggerExit2D;

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger)
            {
                return;
            }
            
            onTriggerEnter2D?.Invoke();
        }
        
        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.isTrigger)
            {
                return;
            }
            
            onTriggerExit2D?.Invoke();
        }
    }
}
