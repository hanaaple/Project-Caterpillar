using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Core;
using Utility.Dialogue;
using Utility.SaveSystem;
using Utility.Scene;
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
            Preference,
            Exit
        }

        [SerializeField] private Animator animator;

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;

        public ButtonType buttonType;
        private static readonly int SelectedHash = Animator.StringToHash("Selected");

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
            animator.SetBool(SelectedHash, false);
        }

        public override void EnterHighlightDisplay()
        {
        }

        public override void SelectDisplay()
        {
            button.image.sprite = selectSprite;
            animator.SetBool(SelectedHash, true);
        }
    }

    public class TitleUIManager : MonoBehaviour
    {
        [Space(5)] [SerializeField] private HighlightTitleItem[] highlightItems;

        [SerializeField] private DialogueData dialogueData;

        private Highlighter _highlighter;

        private void Awake()
        {
            _highlighter = new Highlighter("Title Highlight")
            {
                HighlightItems = new List<HighlightItem>(highlightItems),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _highlighter.onPush = () => { _highlighter.Select(0); };

            _highlighter.Init(Highlighter.ArrowType.Vertical);
        }

        private void Start()
        {
            SceneLoader.Instance.onLoadScene += () => { HighlightHelper.Instance.Pop(_highlighter, true); };

            // 게임 첫 시작시에만
            if (!GameManager.Instance.IsTitleCutSceneWorked)
            {
                GameManager.Instance.IsTitleCutSceneWorked = true;
                dialogueData.OnDialogueEnd = Init;

                PlayUIManager.Instance.dialogueController.StartDialogue(dialogueData);
            }
            else
            {
                Init();
            }
        }

        private void Init()
        {
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
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            PlayUIManager.Instance.preferenceManager.SetPreferencePanel(true);
                        });
                        break;
                    case HighlightTitleItem.ButtonType.Exit:
                        highlightItem.button.onClick.AddListener(Application.Quit);
                        break;
                }
            }

            HighlightHelper.Instance.Push(_highlighter);
        }
    }
}