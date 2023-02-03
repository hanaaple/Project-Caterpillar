using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.InputSystem;
using Utility.SaveSystem;

namespace Title
{
    [Serializable]
    public class HighlightButton : Highlight
    {
        public enum ButtonType
        {
            Continue, NewStart, Preferenece, Exit
        }
        
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite highlightSprite;
        
        public ButtonType buttonType;
        
        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
        }
        public override void SetHighlight()
        {
            button.image.sprite = highlightSprite;
        }
    }

    public class TitleUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject loadPanel;
        [SerializeField] private Button savePanelExitButton;
        
        [SerializeField] private GameObject preferencePanel;
        
        [Space(5)]
        [SerializeField] private HighlightButton[] highlightButtons;

        [SerializeField] private int selectedIdx;
    
        private Action<InputAction.CallbackContext> _onInput;
        private Action<InputAction.CallbackContext> _onExecute;
        private Action<InputAction.CallbackContext> _onCancle;

        private void Awake()
        {
            _onInput = _ =>
            {
                if (!(loadPanel.activeSelf || preferencePanel.activeSelf))
                {
                    Input(_.ReadValue<Vector2>());
                }
            };

            _onExecute = _ =>
            {
                Execute();
            };
            _onCancle = _ =>
            {
                if (loadPanel.activeSelf)
                {
                    SavePanelManager.instance.SetSaveLoadPanelActive(false);
                }
                else if (preferencePanel.activeSelf)
                {
                    PreferenceManager.instance.OpenPreferencePanel();
                }
            };
        }
    
        private void Start()
        {
            foreach (var highlightButton in highlightButtons)
            {
                highlightButton.InitEventTrigger(delegate
                {
                    HighlightButton(highlightButton.buttonType);
                });
            }
            
            var continueButton = Array.Find(highlightButtons, item => item.buttonType == Title.HighlightButton.ButtonType.Continue);
            continueButton.button.onClick.AddListener(() =>
            {
                SavePanelManager.instance.InitLoad();
                SavePanelManager.instance.SetSaveLoadPanelActive(true);
            });
            
            savePanelExitButton.onClick.AddListener(() =>
            {
                SavePanelManager.instance.SetSaveLoadPanelActive(false);
            });
            
            var newStartButton = Array.Find(highlightButtons, item => item.buttonType == Title.HighlightButton.ButtonType.NewStart);
            newStartButton.button.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("MainScene");
            });
            
            var preferenceButton = Array.Find(highlightButtons, item => item.buttonType == Title.HighlightButton.ButtonType.Preferenece);
            preferenceButton.button.onClick.AddListener(() =>
            {
                preferencePanel.SetActive(true);
            });
        
            SavePanelManager.instance.InitLoad();
            SavePanelManager.instance.OnLoad.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("MainScene");
            });
        
            selectedIdx = 0;
            HighlightButton(0);
        }

        private void OnEnable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Enable();
            uiActions.Select.performed += _onInput;
            uiActions.Execute.performed += _onExecute;
            uiActions.Cancle.performed += _onCancle;
        }

        private void OnDisable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Disable();
            uiActions.Select.performed -= _onInput;
            uiActions.Execute.performed -= _onExecute;
            uiActions.Cancle.performed -= _onCancle;
        }

        private void Execute()
        {
            highlightButtons[selectedIdx].Execute();
        }

        private void Input(Vector2 input)
        {
            var idx = selectedIdx;
            if (input == Vector2.up)
            {
                idx = (idx - 1 + highlightButtons.Length) % highlightButtons.Length;
            }
            else if (input == Vector2.down)
            {
                idx = (idx + 1) % highlightButtons.Length;
            }

            HighlightButton(idx);
        }
        
        private void HighlightButton(HighlightButton.ButtonType buttonType)
        {
            var idx = Array.FindIndex(highlightButtons, item => item.buttonType == buttonType);
            HighlightButton(idx);
        }

        private void HighlightButton(int idx)
        {
            Debug.Log($"{idx}입니다");
            highlightButtons[selectedIdx].SetDefault();
            selectedIdx = idx;
            highlightButtons[idx].SetHighlight();
        }
    }
}