using System;
using UnityEngine;

namespace Game.Stage1.ShadowGame.Default
{
    public class ShadowGameItem : MonoBehaviour
    {
        public GameObject uiPanel;
        [TextArea] public string[] toastContents;

        [SerializeField] private float radius;
        [SerializeField] private int appearStageIndex;
        
        [NonSerialized] public Action OnClick;

        public bool IsEnable(int stageIndex)
        {
            return appearStageIndex == stageIndex;
        }

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
            OnClick?.Invoke();
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
