using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.InputSystem;
using Utility.UI.Highlight;

namespace Utility.UI.Preference
{
    public class PreferenceManager : MonoBehaviour
    {
        [SerializeField] private GameObject preferencePanel;

        [SerializeField] private Image preferenceFrontPanel;

        [SerializeField] private Button preferenceExitButton;

        [SerializeField] private GameObject checkRebindPanel;

        [SerializeField] private GameObject rebindButtonPanel;
    
        [SerializeField] private Button resetButton;

        [SerializeField] private Button saveButton;

        [SerializeField] private Button cancleSaveButton;

        [SerializeField] private GameObject[] pagePanels;

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [SerializeField] private TMP_Text pageText;

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
                    _isPerformed = true;
                    StartCoroutine(WaitPerform());
                    Input(_.ReadValue<Vector2>());
                }
            };

            _onCancle = _ =>
            {
                if (preferencePanel.activeSelf && !checkRebindPanel.activeSelf)
                {
                    ExitPreferencePanel();
                }
            };
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
                HighlightItems = checkHighlightItems,
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

            preferenceFrontPanel.alphaHitTestMinimumThreshold = 0.1f;
            UpdateUI(0);

            preferenceExitButton.onClick.AddListener(ExitPreferencePanel);


            leftButton.onClick.AddListener(() =>
            {
                var nextIdx = (_pageIndex - 1 + pagePanels.Length) % pagePanels.Length;

                UpdateUI(nextIdx);
            });

            rightButton.onClick.AddListener(() =>
            {
                var nextIdx = (_pageIndex + 1) % pagePanels.Length;

                UpdateUI(nextIdx);
            });

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

        private void UpdateUI(int nextIdx)
        {
            foreach (var pagePanel in pagePanels)
            {
                pagePanel.SetActive(false);
            }

            pagePanels[nextIdx].SetActive(true);

            _pageIndex = nextIdx;
            pageText.text = _pageIndex + 1 + " / " + pagePanels.Length;
        }
    
        private void Input(Vector2 input)
        {
            if (input == Vector2.left)
            {
                var nextIdx = (_pageIndex - 1 + pagePanels.Length) % pagePanels.Length;
            
                pagePanels[_pageIndex].SetActive(false);
                _pageIndex = nextIdx;
                pagePanels[_pageIndex].SetActive(true);
                pageText.text = _pageIndex + 1 + " / " + pagePanels.Length;
            }
            else if (input == Vector2.right)
            {
                var nextIdx = (_pageIndex + 1) % pagePanels.Length;
            
                pagePanels[_pageIndex].SetActive(false);
                _pageIndex = nextIdx;
                pagePanels[_pageIndex].SetActive(true);
                pageText.text = _pageIndex + 1 + " / " + pagePanels.Length;
            }
        }
    }
}