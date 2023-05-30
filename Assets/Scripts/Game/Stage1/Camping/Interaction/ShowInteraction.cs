using UnityEngine;
using UnityEngine.UI;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction
{
    public class ShowInteraction : CampingInteraction
    {
        [SerializeField] private GameObject showPanel;
        [SerializeField] private Button exitButton;
        
        [TextArea] public string[] toastContents;

        private bool _isToasted;
        
        private void Start()
        {
            exitButton.onClick.AddListener(() =>
            {
                showPanel.SetActive(false);
                setInteractable(true);
            });
        }

        private void OnMouseUp()
        {
            Debug.Log($"{enabled}");
            showPanel.SetActive(true);
            setInteractable(false);
            Appear();
        }

        public override void Appear()
        {
            if (!_isToasted)
            {
                _isToasted = true;
            
                foreach (var toastContent in toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(toastContent);
                }
            }
            onAppear?.Invoke();
        }

        public override void ResetInteraction()
        {
            showPanel.SetActive(false);
        }
    }
}