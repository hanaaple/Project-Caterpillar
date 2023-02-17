using System;
using UnityEngine;
using Utility.SaveSystem;
using Utility.SceneLoader;
using Utility.UI.Highlight;

namespace Title
{
    [Serializable]
    public class HighlightItemTitleButton : HighlightItem
    {
        public enum ButtonType
        {
            Continue,
            NewStart,
            Preferenece,
            Exit
        }

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;

        public ButtonType buttonType;

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            button.image.sprite = selectSprite;
        }
    }

    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject preferencePanel;

        [Space(5)] [SerializeField] private HighlightItemTitleButton[] highlightButtons;

        private Highlighter _highlighter;

        private void Awake()
        {
            _highlighter = new Highlighter
            {
                highlightItems = highlightButtons, highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _highlighter.Init(Highlighter.ArrowType.Vertical);

            HighlightHelper.Instance.Push(_highlighter);
        }

        private void Start()
        {
            SceneLoader.Instance.onLoadScene += () =>
            {
                HighlightHelper.Instance.Pop(_highlighter, true);
            };
            
            var continueButton = Array.Find(highlightButtons,
                item => item.buttonType == HighlightItemTitleButton.ButtonType.Continue);
            continueButton.button.onClick.AddListener(() =>
            {
                SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.ButtonType.Load);
            });

            var newStartButton = Array.Find(highlightButtons,
                item => item.buttonType == HighlightItemTitleButton.ButtonType.NewStart);
            newStartButton.button.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("MainScene");
            });

            var preferenceButton = Array.Find(highlightButtons,
                item => item.buttonType == HighlightItemTitleButton.ButtonType.Preferenece);
            preferenceButton.button.onClick.AddListener(() =>
            {
                preferencePanel.SetActive(true);

                // PreferenceManager.instance.OpenPreferencePanel();
            });

            var exitButton = Array.Find(highlightButtons,
                item => item.buttonType == HighlightItemTitleButton.ButtonType.Exit);
            exitButton.button.onClick.AddListener(Application.Quit);
        }
    }
}