using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Audio;

namespace Game.Stage1.ShadowGame.Default
{
    public class ShadowGameItem : MonoBehaviour
    {
        public GameObject uiPanel;
        [TextArea] public string[] toastContents;

        [SerializeField] private float radius;
        [SerializeField] private int appearStageIndex;
        
        [SerializeField] private AudioData acquireAudioData;
        
        [NonSerialized] public Action OnClick;

        public bool IsEnable(int stageIndex)
        {
            return appearStageIndex == stageIndex;
        }

        public void TryClick(Camera cam)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            
            var cameraWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            if (Vector2.Distance(transform.position, cameraWorldPos) <= radius)
            {
                OnClick?.Invoke();
                acquireAudioData.Play();
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
