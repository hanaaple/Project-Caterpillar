using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.BeachGame
{
    public enum PictureState
    {
        Default,
        Active,
        Clear
    }
    
    public class AlbumPicture : MonoBehaviour
    {
        public BeachInteractType beachInteractType;

        [SerializeField] private GameObject textImage;
        [SerializeField] private Sprite inActiveSprite;
        [SerializeField] private Sprite activeSprite;

        [Space(5)] [Header("사진 패널")] [SerializeField]
        private GameObject picturePanel;

        [SerializeField] private Button panelExitButton;
        [SerializeField] private GameObject defaultPanel;
        [SerializeField] private GameObject activePanel;
        [SerializeField] private GameObject clearPanel;

        protected Button PanelButton;

        public void Init()
        {
            Reeset();
            
            PanelButton = GetComponentInChildren<Button>(true);
            PanelButton.onClick.AddListener(() => { picturePanel.SetActive(true); });
            panelExitButton.onClick.AddListener(() => { picturePanel.SetActive(false); });
        }

        public void SetPanel(PictureState state)
        {
            Debug.Log("상태: " + state);
            if (state == PictureState.Default)
            {
                PanelButton.image.sprite = inActiveSprite;
                defaultPanel.SetActive(true);
                activePanel.SetActive(false);
                clearPanel.SetActive(false);
            }
            else if (state == PictureState.Active)
            {
                if (!clearPanel.activeSelf)
                {
                    defaultPanel.SetActive(false);
                    activePanel.SetActive(true);
                    clearPanel.SetActive(false);
                }

                textImage.SetActive(true);
            }
            else if (state == PictureState.Clear)
            {
                PanelButton.image.sprite = activeSprite;
                defaultPanel.SetActive(false);
                activePanel.SetActive(false);
                clearPanel.SetActive(true);
            }
        }

        public virtual void SetPanel(int idx)
        {

        }

        public void Reeset()
        {
            SetPanel(PictureState.Default);

            textImage.SetActive(false);
        }
    }
}