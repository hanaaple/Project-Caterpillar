using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Core;
using Utility.Scene;
using Utility.UI.Check;
using Utility.UI.Highlight;
using Utility.Util;

namespace Utility.UI.Pause
{
    [Serializable]
    public class PauseHighlightItem : HighlightItem
    {
        public enum ButtonType
        {
            Continue,
            Preference,
            ExitTitle,
            ExitGame
        }
        
        public ButtonType buttonType;

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;


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

    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private PauseHighlightItem[] pauseHighlightItems;

        [SerializeField] private CheckUIManager exitCheckUIManager;
        
        private Highlighter _pauseHighlighter;
        public Action onPause;
        public Action onExit;

        private void Awake()
        {
            onPause = () =>
            {
                if (!SceneManager.GetActiveScene().name.Equals("TitleScene"))
                {
                    SetActive(true);
                }
            };
            
            _pauseHighlighter = new Highlighter("Pause Highlight")
            {
                HighlightItems = new List<HighlightItem>(pauseHighlightItems),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _pauseHighlighter.Init(Highlighter.ArrowType.Vertical, () =>
            {
                SetActive(false);
            });
        }

        private void Start()
        {
            exitCheckUIManager.Initialize();
            
            exitCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () =>
            {
                exitCheckUIManager.Pop();
            });

            foreach (var highlightItem in pauseHighlightItems)
            {
                switch (highlightItem.buttonType)
                {
                    case PauseHighlightItem.ButtonType.Continue:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            SetActive(false);
                        });
                        break;
                    }
                    case PauseHighlightItem.ButtonType.Preference:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            PlayUIManager.Instance.preferenceManager.SetPreferencePanel(true);
                        });
                        break;
                    }
                    case PauseHighlightItem.ButtonType.ExitTitle:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"현재: {_pauseHighlighter.selectedIndex}");
                            exitCheckUIManager.SetText("저장되지 않은 데이터가 있을 수 있습니다.\n타이틀 화면으로 돌아가시겠습니까?");
                            exitCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
                            {
                                exitCheckUIManager.Pop();
                                SetActive(false);
                                onExit?.Invoke();
                                HighlightHelper.Instance.ResetHighlight();
                                PlayUIManager.Instance.Destroy();
                                SceneLoader.Instance.LoadScene("TitleScene");
                            });
                            exitCheckUIManager.Push();
                            Debug.Log($"다음: {_pauseHighlighter.selectedIndex}");
                        });
                        break;
                    }
                    case PauseHighlightItem.ButtonType.ExitGame:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            exitCheckUIManager.SetText("저장되지 않은 데이터가 있을 수 있습니다.\n게임을 종료하시겠습니까?");
                            exitCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, Application.Quit);
                            
                            exitCheckUIManager.Push();
                        });
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set Active Pause UI
        /// </summary>
        /// <param name="isActive">isActive</param>
        private void SetActive(bool isActive)
        {
            if (isActive)
            {
                TimeScaleHelper.Push(0f);
                pausePanel.SetActive(true);
                HighlightHelper.Instance.Push(_pauseHighlighter);
                _pauseHighlighter.Select(0);
            }
            else
            {
                TimeScaleHelper.Pop();
                pausePanel.SetActive(false);
                HighlightHelper.Instance.Pop(_pauseHighlighter);
            }
        }

        public bool GetIsActive()
        {
            return pausePanel.activeSelf;
        }
    }
}