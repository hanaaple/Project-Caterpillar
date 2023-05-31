using System;
using Game.Stage1.Camping.Interaction.Map;
using UnityEngine;
using Utility.Property;

namespace Game.Stage1.Camping.Interaction
{
    public abstract class CampingInteraction : MonoBehaviour
    {
        public bool isHint;
        [ConditionalHideInInspector("isHint")]
        public CampingHint hint;
        
        public Action<bool> setInteractable;

        public virtual void ResetInteraction()
        {
            if (isHint)
            {
                hint.SetHint(false);
            }
        }

        protected virtual void Appear()
        {
            if (isHint)
            {
                hint.SetHint(true);
            }
        }
    }
}