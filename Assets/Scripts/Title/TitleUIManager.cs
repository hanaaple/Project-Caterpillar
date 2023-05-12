using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.UI.Highlight;
using Utility.UI.Preference;

namespace Title
{
    [Serializable]
    public class HighlightTitleItem : HighlightItem
    {
        public enum ButtonType
        {
            Continue,
            NewStart,
            Preference,
            Exit
        }

        [SerializeField] private Animator animator;

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;

        public ButtonType buttonType;

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
            animator.SetBool("Selected", false);
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            button.image.sprite = selectSprite;
            animator.SetBool("Selected", true);
        }
    }

    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] private PreferenceManager preferenceManager;

        [Space(5)] [SerializeField] private HighlightTitleItem[] highlightItems;

        private Highlighter _highlighter;

        private void Awake()
        {
            _highlighter = new Highlighter("Title Highlight")
            {
                HighlightItems = new List<HighlightItem>(highlightItems),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _highlighter.Init(Highlighter.ArrowType.Vertical);
            HighlightHelper.Instance.Push(_highlighter);
        }

        private void Start()
        {
            SceneLoader.Instance.onLoadScene += () => { HighlightHelper.Instance.Pop(_highlighter, true); };

            foreach (var highlightItem in highlightItems)
            {
                switch (highlightItem.buttonType)
                {
                    case HighlightTitleItem.ButtonType.Continue:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SavePanelManager.Instance.SetSaveLoadPanelActive(true,
                                SavePanelManager.SaveLoadType.Load);
                        });
                        break;
                    case HighlightTitleItem.ButtonType.NewStart:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SceneLoader.Instance.LoadScene("MainScene");
                        });
                        break;
                    case HighlightTitleItem.ButtonType.Preference:
                        highlightItem.button.onClick.AddListener(() => { preferenceManager.SetPreferencePanel(true); });
                        break;
                    case HighlightTitleItem.ButtonType.Exit:
                        highlightItem.button.onClick.AddListener(Application.Quit);
                        break;
                }
            }
        }
    }
}