using UnityEngine;
using UnityEngine.Events;
namespace Game.Camping
{
    public abstract class CampingInteraction : MonoBehaviour
    {
        public UnityAction onAppear;
        
        public UnityAction<bool> setEnable;

        // public abstract void Init();
        public abstract void Appear();
        
        public abstract void Reset();
    }
}
