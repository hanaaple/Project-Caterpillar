using System;
using UnityEngine;

namespace Game.Stage1.BeachGame
{
    public class BeachInteraction : MonoBehaviour
    {
        [NonSerialized] public bool Interactable;
        [NonSerialized] public bool IsStop;

        public Action onInteract;

        public void Init()
        {
            onInteract = () => { };
            Interactable = true;
            IsStop = false;
            gameObject.SetActive(true);
        }

        private void OnMouseUp()
        {
            if (!Interactable || IsStop)
            {
                return;
            }

            Debug.Log("Interact");

            onInteract?.Invoke();
        }
    }
}