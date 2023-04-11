using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.Audio;
using Utility.InputSystem;
using Utility.UI.Highlight;

namespace Utility.UI.Preference
{
    [Serializable]
    public class PageProps
    {
        public GameObject panel;
        public Button button;
    }
    
    public class PreferenceManager : MonoBehaviour
    {
        [SerializeField] private GameObject preferencePanel;

        [SerializeField] private Button preferenceExitButton;

        [SerializeField] private GameObject checkRebindPanel;

        [SerializeField] private GameObject rebindButtonPanel;
    
        [SerializeField] private Button resetButton;

        [SerializeField] private Button saveButton;

        [SerializeField] private Button cancleSaveButton;

        [SerializeField] private PageProps[] pageProps;

        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private InputController[] inputController;

        [SerializeField] private CheckHighlightItem[] checkHighlightItems;
        
        private Highlighter _checkHighlighter;
        private int _pageIndex;
        private bool _isPerformed;
    
        private Action<InputAction.CallbackContext> _onInput;
        private Action<InputAction.CallbackContext> _onCancle;


        private void Awake()
        {
            _onInput = _ =>
            {
                if (preferencePanel.activeSelf && !_isPerformed && !checkRebindPanel.activeSelf)
                {
                    //_isPerformed = true;
                    //StartCoroutine(WaitPerform());
                }
            };

            _onCancle = _ =>
            {
                if (preferencePanel.activeSelf && !checkRebindPanel.activeSelf)
                {
                    ExitPreferencePanel();
                }
            };
            
            AudioManager.LoadAudio();
        }

        private IEnumerator WaitPerform()
        {
            yield return null;
            _isPerformed = false;
        }

        private void OnEnable()
        {
            InputManager.RebindComplete += SetSaveButton;
            InputManager.RebindEnd += SetSaveButton;
            // InputManager.RebindLoad += SetSaveButton;
            InputManager.RebindReset += SetSaveButton;
        
            InputManager.SetUiAction(true);
            
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Select.performed += _onInput;
            uiActions.Cancle.performed += _onCancle;
        }

        private void OnDisable()
        {
            InputManager.RebindComplete -= SetSaveButton;
            InputManager.RebindEnd -= SetSaveButton;
            // InputManager.RebindLoad -= SetSaveButton;
            InputManager.RebindReset -= SetSaveButton;
        
            var uiActions = InputManager.InputControl.Ui;
            InputManager.SetUiAction(false);
            uiActions.Select.performed -= _onInput;
            uiActions.Cancle.performed -= _onCancle;
        }

        private void SetSaveButton()
        {
            if (InputManager.IsChanged())
            {
                rebindButtonPanel.SetActive(true);
            }
            else
            {
                rebindButtonPanel.SetActive(false);
            }
        }

        private void ExitPreferencePanel()
        {
            if (InputManager.IsChanged())
            {
                checkRebindPanel.SetActive(true);
                HighlightHelper.Instance.Push(_checkHighlighter, default, false);
            }
            else
            {
                HighlightHelper.Instance.Enable();
                preferencePanel.SetActive(false);
            }
        }

        private void Start()
        {
            _checkHighlighter = new Highlighter
            {
                HighlightItems = new List<HighlightItem>(checkHighlightItems),
                name = "check 하이라이트"
            };
            
            var yesHighlightItem = Array.Find(checkHighlightItems,
                item => item.buttonType == CheckHighlightItem.ButtonType.Yes);
            
            var noHighlightItem = Array.Find(checkHighlightItems,
                item => item.buttonType == CheckHighlightItem.ButtonType.No);
            
            yesHighlightItem.button.onClick.AddListener(() =>
            {
                InputManager.EndChange(true);
                checkRebindPanel.SetActive(false);
                Time.timeScale = 1;
                // preferenceButton.gameObject.SetActive(true);
                preferencePanel.SetActive(false);
                HighlightHelper.Instance.Pop(_checkHighlighter);
            });

            noHighlightItem.button.onClick.AddListener(() =>
            {
                InputManager.EndChange(false);
                checkRebindPanel.SetActive(false);
                HighlightHelper.Instance.Pop(_checkHighlighter);
            });
            
            _checkHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                noHighlightItem.button.onClick?.Invoke();
            });
            
            resetButton.onClick.AddListener(() =>
            {
                foreach (var t in inputController)
                {
                    t.TempResetBinding();
                }
            });

            cancleSaveButton.onClick.AddListener(() =>
            {
                InputManager.EndChange(false);
            });
        
            saveButton.onClick.AddListener(() => { InputManager.EndChange(true); });

            preferenceExitButton.onClick.AddListener(ExitPreferencePanel);

            
            foreach (var pageProp in pageProps)
            {
                pageProp.button.onClick.AddListener(() =>
                {
                    foreach (var t in pageProps)
                    {
                        t.panel.SetActive(false);
                    }

                    pageProp.panel.SetActive(true);
                });
            }

            resolutionDropdown.onValueChanged.AddListener(idx =>
            {
                Debug.Log(resolutionDropdown.options[idx].text);
                var resolution = resolutionDropdown.options[idx].text;
                var x = int.Parse(resolution.Split("x")[0]);
                var y = int.Parse(resolution.Split("x")[1]);
                Screen.SetResolution(x, y, false);
                Debug.Log(resolutionDropdown.options[idx].image);
            });
        }
    }
}