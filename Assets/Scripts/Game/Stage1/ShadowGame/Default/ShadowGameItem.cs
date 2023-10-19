using System;
using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.ShadowGame.Default
{
    public class ShadowGameItem : MonoBehaviour
    {
        public GameObject uiPanel;
        [TextArea] public string[] toastContents;

        [SerializeField] private float radius;
        [SerializeField] private int appearStageIndex;
        
        [SerializeField] private AudioClip acquireAudioClip;
        
        [NonSerialized] public Action OnClick;

        public bool IsEnable(int stageIndex)
        {
            return appearStageIndex == stageIndex;
        }

        public void TryClick()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            
            var cameraWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, cameraWorldPos) <= radius)
            {
                OnClick?.Invoke();
                AudioManager.Instance.PlaySfx(acquireAudioClip);
            }
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
