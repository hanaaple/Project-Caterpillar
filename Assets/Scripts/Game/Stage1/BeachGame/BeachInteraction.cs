using System;
using Game.Default;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Scene;

namespace Game.Stage1.BeachGame
{
    public class BeachInteraction : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private ToastData toastData;
        
        [NonSerialized] public bool IsInteractable;
        [NonSerialized] public bool IsStop;

        public Action onInteract;

        public void Init()
        {
            IsInteractable = true;
            IsStop = false;
            gameObject.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable || IsStop)
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

        public void OnPointerDown(PointerEventData eventData)
        {
        }
    }
}