using UnityEngine;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class ShowInteraction : CampingInteraction
    {
        [SerializeField] private ShowPanel showPanel;

        [TextArea] public string[] toastContents;

        private bool _isToasted;

        private void Start()
        {
            showPanel.Initialize();
            showPanel.exitButton.onClick.AddListener(() =>
            {
                showPanel.Hide();
                setInteractable(true);
            });
        }

        private void OnMouseUp()
        {
            Show();
        }

        /// <summary>
        /// Use in Animation
        /// </summary>
        public void Show()
        {
            Debug.Log($"{enabled}");
            showPanel.Show();
            setInteractable(false);
            Appear();
        }

        protected override void Appear()
        {
            base.Appear();

            if (!_isToasted)
            {
                _isToasted = true;

                foreach (var toastContent in toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(toastContent);
                }
            }
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();
            showPanel.gameObject.SetActive(false);
        }
    }
}