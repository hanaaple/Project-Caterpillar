using System;
using Game.Default;
using UnityEngine;
using Utility.Scene;

namespace Game.Stage1.BeachGame
{
    public class BeachInteraction : MonoBehaviour
    {
        [SerializeField] private ToastData toastData;
        
        [NonSerialized] public bool Interactable;
        [NonSerialized] public bool IsStop;

        public Action onInteract;

        public void Init()
        {
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

            if (!toastData.IsToasted)
            {
                foreach (var content in toastData.toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(content);
                }

                toastData.IsToasted = true;
            }
            
            onInteract?.Invoke();
        }
    }
}