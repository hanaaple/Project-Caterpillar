using UnityEngine;
using UnityEngine.Events;
namespace Game.Camping
{
    public abstract class CampingInteraction : MonoBehaviour
    {
        public UnityAction onAppear;
        
        public UnityAction<bool> setInteractable;
        
        public abstract void Appear();
        
        public abstract void Reset();
    }
}
