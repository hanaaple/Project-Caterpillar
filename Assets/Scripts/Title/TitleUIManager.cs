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

            foreach (var highlightItem in highlightButtons)
            {
                switch(highlightItem.buttonType)
                {
                    case HighlightItemTitleButton.ButtonType.Continue:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.ButtonType.Load);
                        });
                        break;
                    case HighlightItemTitleButton.ButtonType.NewStart:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SceneLoader.Instance.LoadScene("PrologueScene");
                        });
                        break;
                    case HighlightItemTitleButton.ButtonType.Preferenece:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            preferencePanel.SetActive(true);
                        });
                        break;
                    case HighlightItemTitleButton.ButtonType.Exit:
                        highlightItem.button.onClick.AddListener(Application.Quit);
                        break;
                }
            }
        }
    }
}