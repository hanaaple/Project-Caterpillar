using System;
using Game.Default;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Property;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction
{
    [Serializable]
    public class ToastClickObjects
    {
        public ToastClickObject[] toastClickObjects;
    }

    public class ToastClickObject : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private bool isShareCount;

        [ConditionalHideInInspector("isShareCount")] [SerializeField]
        private ToastClickObjects toastClickObjects;

        public ToastData toastData;

        private void OnValidate()
        {
            if (!isShareCount)
            {
                toastClickObjects = null;
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Toast();
        }

        private void Toast()
        {
            if (toastData.IsToasted)
            {
                return;
            }

            toastData.IsToasted = true;

            foreach (var toastContent in toastData.toastContents)
            {
                SceneHelper.Instance.toastManager.Enqueue(toastContent);
            }

            if (isShareCount)
            {
                foreach (var toastClickObject in toastClickObjects.toastClickObjects)
                {
                    toastClickObject.toastData.IsToasted = true;
                }
            }
        }
    }
}