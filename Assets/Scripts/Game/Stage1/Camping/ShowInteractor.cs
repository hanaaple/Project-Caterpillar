using UnityEngine;
using UnityEngine.UI;

namespace Game.Camping
{
    public class ShowInteractor : CampingInteraction
    {
        [SerializeField]
        private GameObject showPanel;

        [SerializeField]
        private Button exitButton;

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
            setInteractable(false);
            showPanel.SetActive(true);
            Appear();
        }

        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void Reset()
        {
        }
    }
}
