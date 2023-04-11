using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.SaveSystem;
using Utility.SceneLoader;
using Utility.UI.Highlight;

namespace Title
{
    [Serializable]
    public class HighlightTitleItem : HighlightItem
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

        [FormerlySerializedAs("highlightButtons")] [Space(5)] [SerializeField] private HighlightTitleItem[] highlightItems;

        private Highlighter _highlighter;

        private void Awake()
        {
            _highlighter = new Highlighter
            {
                HighlightItems = new List<HighlightItem>(highlightItems), highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _highlighter.Init(Highlighter.ArrowType.Vertical);

            HighlightHelper.Instance.Push(_highlighter);
        }

        private void Start()
        {
            SceneLoader.Instance.OnLoadScene += () =>
            {
                HighlightHelper.Instance.Pop(_highlighter, true);
            };

            foreach (var highlightItem in highlightItems)
            {
                switch(highlightItem.buttonType)
                {
                    case HighlightTitleItem.ButtonType.Continue:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.SaveLoadType.Load);
                        });
                        break;
                    case HighlightTitleItem.ButtonType.NewStart:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SceneLoader.Instance.LoadScene("PrologueScene");
                        });
                        break;
                    case HighlightTitleItem.ButtonType.Preferenece:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            HighlightHelper.Instance.Disable(false);
                            preferencePanel.SetActive(true);
                        });
                        break;
                    case HighlightTitleItem.ButtonType.Exit:
                        highlightItem.button.onClick.AddListener(Application.Quit);
                        break;
                }
            }
        }
    }
}