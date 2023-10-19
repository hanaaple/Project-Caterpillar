using UnityEngine;
using Utility.Audio;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class ShowInteraction : CampingInteraction
    {
        [SerializeField] private ShowPanel showPanel;

        [SerializeField] private AudioClip openAudioClip;
        [SerializeField] private AudioClip closeAudioClip;
        
        [TextArea] public string[] toastContents;

        private bool _isToasted;

        private void Start()
        {
            showPanel.Initialize();
            showPanel.exitButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySfx(closeAudioClip);
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
            AudioManager.Instance.PlaySfx(openAudioClip);
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

        public override void ResetInteraction(bool isGameReset = false)
        {
            base.ResetInteraction(isGameReset);
            showPanel.ResetPanel();
        }
    }
}