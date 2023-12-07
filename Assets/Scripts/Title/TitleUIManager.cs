using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Audio;
using Utility.Core;
using Utility.Dialogue;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.UI.Highlight;
using Utility.Util;

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

        public override void Select()
        {
            base.Select();
            button.image.sprite = selectSprite;
            animator.SetBool(SelectedHash, true);
        }
        
        public override void DeSelect()
        {
            base.DeSelect();
            button.image.sprite = defaultSprite;
            animator.SetBool(SelectedHash, false);
        }
    }

    public class TitleUIManager : MonoBehaviour
    {
        [Space(5)] [SerializeField] private HighlightTitleItem[] highlightItems;

        [SerializeField] private DialogueData dialogueData;

        [SerializeField] private AudioData bgmAudioData;
        
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
                dialogueData.OnDialogueEnd = _ => { Init(); };
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
                            SavePanelManager.Instance.SetActiveSaveLoadPanel(true,
                                SavePanelManager.SaveLoadType.Load);
                        });
                        break;
                    case HighlightTitleItem.ButtonType.NewStart:
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            PlayTimer.ReStart();
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
            
            bgmAudioData.Play();
        }
    }
}