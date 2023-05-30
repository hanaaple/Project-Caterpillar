using UnityEngine;
using UnityEngine.Events;

namespace Game.Stage1.Camping.Interaction
{
    public abstract class CampingInteraction : MonoBehaviour
    {
        public bool isHint;
        
        public UnityAction onAppear;

        public UnityAction<bool> setInteractable;

        public abstract void Appear();
        
        public abstract void ResetInteraction();
    }
}
