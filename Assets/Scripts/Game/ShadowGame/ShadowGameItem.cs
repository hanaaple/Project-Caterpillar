using System;
using UnityEngine;

namespace Game.ShadowGame
{
    public class ShadowGameItem : MonoBehaviour
    {
        [SerializeField] private float radius;
        [NonSerialized] public Action onClick;
        
        public GameObject uiPanel;

        public void CheckClick()
        {
            var cameraWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, cameraWorldPos) <= radius)
            {
                Click();
            }
        }

        private void Click()
        {
            onClick?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(transform.position, radius);
            }
        }
    }
}
