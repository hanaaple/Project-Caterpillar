using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;
using Utility.Scene;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class ShowInteraction : CampingInteraction, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private ShowPanel showPanel;
        
        [SerializeField] private AudioData openAudioData;
        [SerializeField] private AudioData closeAudioData;
        
        [TextArea] public string[] toastContents;

        private bool _isToasted;

        private void Start()
        {
            showPanel.Initialize();
            showPanel.exitButton.onClick.AddListener(() =>
            {
                closeAudioData.Play();
                showPanel.Hide();
                setInteractable(true);
            });
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// Use in Animation
        /// </summary>
        public void Show()
        {
            Debug.Log($"{enabled}");
            openAudioData.Play();
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

        public void OnPointerDown(PointerEventData eventData)
        {
        }
    }
}