using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Core;
using Utility.InputSystem;
using Utility.UI.Highlight;

namespace Utility.UI.Pause
{
    [Serializable]
    public class PauseHighlightItem : HighlightItem
    {
        public enum ButtonType
        {
            Continue,
            Preferenece,
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
        [SerializeField] private GameObject preferencePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private PauseHighlightItem[] pauseHighlightItems;

        [SerializeField] private GameObject checkPanel;
        [SerializeField] private TMP_Text checkText;
        [SerializeField] private CheckHighlightItem[] checkHighlightItems;
        
        private Highlighter _pauseHighlighter;
        private Highlighter _checkHighlighter;
        private Action<InputAction.CallbackContext> _onPause;

        private void Awake()
        {
            _onPause = _ =>
            {
                if (!pausePanel.activeSelf)
                {
                    SetActive(true);
                }
                else if (!preferencePanel.activeSelf && !checkPanel.activeSelf)
                {
                    SetActive(false);
                }
            };
        }

        private void Start()
        {
            _pauseHighlighter = new Highlighter
            {
                HighlightItems = pauseHighlightItems,
                name = "Pause 하이라이트"
            };

            _pauseHighlighter.Init(Highlighter.ArrowType.Vertical, () =>
            {
                HighlightHelper.Instance.Pop(_checkHighlighter, true);
            });

            _checkHighlighter = new Highlighter
            {
                HighlightItems = checkHighlightItems,
                name = "check 하이라이트"
            };
            
            var yesHighlightItem = Array.Find(checkHighlightItems,
                item => item.buttonType == CheckHighlightItem.ButtonType.Yes);
            
            var noHighlightItem = Array.Find(checkHighlightItems,
                item => item.buttonType == CheckHighlightItem.ButtonType.No);
            
            _checkHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                noHighlightItem.button.onClick?.Invoke();
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
                    case PauseHighlightItem.ButtonType.Preferenece:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            // Set DisAble 
                            HighlightHelper.Instance.Disable(false);
                            preferencePanel.SetActive(true);
                        });
                        break;
                    }
                    case PauseHighlightItem.ButtonType.ExitTitle:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"현재: {_pauseHighlighter.selectedIndex}");
                            checkText.text = "저장되지 않은 데이터가 있을 수 있습니다.\n타이틀 화면으로 돌아가시겠습니까?";
                            yesHighlightItem.button.onClick.RemoveAllListeners();
                            yesHighlightItem.button.onClick.AddListener(() =>
                            {
                                Time.timeScale = 1f;
                                HighlightHelper.Instance.ResetHighlight();
                                PlayUIManager.Instance.Destroy();
                                SceneLoader.SceneLoader.Instance.LoadScene("TitleScene");
                            });
                            checkPanel.SetActive(true);
                            HighlightHelper.Instance.Push(_checkHighlighter, default, false);
                            Debug.Log($"다음: {_pauseHighlighter.selectedIndex}");
                        });
                        break;
                    }
                    case PauseHighlightItem.ButtonType.ExitGame:
                    {
                        highlightItem.button.onClick.AddListener(() =>
                        {
                            checkText.text = "저장되지 않은 데이터가 있을 수 있습니다.\n게임을 종료하시겠습니까?";
                            yesHighlightItem.button.onClick.RemoveAllListeners();
                            yesHighlightItem.button.onClick.AddListener(Application.Quit);
                            checkPanel.SetActive(true);
                            HighlightHelper.Instance.Push(_checkHighlighter, default, false);
                        });
                        break;
                    }
                }
            }
            
            noHighlightItem.button.onClick.AddListener(() =>
            {
                HighlightHelper.Instance.Pop(_checkHighlighter);
                checkPanel.SetActive(false);
                checkText.text = "";
            });
        }

        private void SetActive(bool isTrue)
        {
            if (isTrue)
            {
                Time.timeScale = 0f;
                pausePanel.SetActive(true);
                HighlightHelper.Instance.Push(_pauseHighlighter);
                _pauseHighlighter.Select(0);
            }
            else
            {
                Time.timeScale = 1f;
                // Enable All Input
                pausePanel.SetActive(false);
                HighlightHelper.Instance.Pop(_checkHighlighter, true);
                HighlightHelper.Instance.Pop(_pauseHighlighter, true);
            }
        }

        private void OnEnable()
        {
            var esc = InputManager.InputControl.Esc;
            esc.Pause.performed += _onPause;
            esc.Enable();
        }
        
        private void OnDisable()
        {
            var esc = InputManager.InputControl.Esc;
            esc.Pause.performed -= _onPause;
            esc.Disable();
        }
    }
}