using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.InputSystem;

namespace Utility.UI.Preference
{
    public class PreferenceManager : MonoBehaviour
    {
        // [SerializeField] private InputAction preferenceInputAction;

        // [SerializeField] private Button preferenceButton;

        [SerializeField] private GameObject preferencePanel;

        [SerializeField] private Image preferenceFrontPanel;

        [SerializeField] private Button preferenceExitButton;

        [SerializeField] private GameObject checkRebindPanel;

        [SerializeField] private GameObject rebindButtonPanel;
    
        [SerializeField] private Button resetButton;

        [SerializeField] private Button saveButton;
    
        [SerializeField] private Button cancleSaveButton;

        [SerializeField] private Button rebindButton;

        [SerializeField] private Button notRebindButton;

        [SerializeField] private GameObject[] pagePanels;

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [SerializeField] private TMP_Text pageText;

        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private InputController[] inputController;

        private int _pageIndex;
    
        private Action<InputAction.CallbackContext> _onInput;

        private bool _isPerformed;

        private void Awake()
        {
            _onInput = _ =>
            {
                if (preferencePanel.activeSelf && !_isPerformed)
                {
                    _isPerformed = true;
                    StartCoroutine(WaitPerform());
                    Input(_.ReadValue<Vector2>());
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
            // preferenceInputAction.Enable();
            InputManager.RebindComplete += SetSaveButton;
            InputManager.RebindEnd += SetSaveButton;
            // InputManager.RebindLoad += SetSaveButton;
            InputManager.RebindReset += SetSaveButton;
        
            InputManager.SetUiAction(true);
            
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Select.performed += _onInput;
            Debug.Log("+++++++++++");
        }

        private void OnDisable()
        {
            // preferenceInputAction.Disable();
            InputManager.RebindComplete -= SetSaveButton;
            InputManager.RebindEnd -= SetSaveButton;
            // InputManager.RebindLoad -= SetSaveButton;
            InputManager.RebindReset -= SetSaveButton;
        
            var uiActions = InputManager.inputControl.Ui;
            InputManager.SetUiAction(false);
            uiActions.Select.performed -= _onInput;
            Debug.Log("-----------");
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

        internal void OpenPreferencePanel()
        {
            if (InputManager.IsChanged())
            {
                checkRebindPanel.SetActive(true);
            }
            else
            {
                if (preferencePanel.activeSelf)
                {

                    Time.timeScale = 1;
                    // preferenceButton.gameObject.SetActive(true);
                    preferencePanel.SetActive(false);
                }
                else
                {
                    Time.timeScale = 0;
                    // preferenceButton.gameObject.SetActive(false);
                    preferencePanel.SetActive(true);
                }
            }
        }

        private void Start()
        {
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
            // preferenceInputAction.performed += _ => OpenPreferencePanel();

            // preferenceButton.onClick.AddListener(OpenPreferencePanel);

            preferenceExitButton.onClick.AddListener(OpenPreferencePanel);


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

            rebindButton.onClick.AddListener(() =>
            {
                InputManager.EndChange(true);
                checkRebindPanel.SetActive(false);
                Time.timeScale = 1;
                // preferenceButton.gameObject.SetActive(true);
                preferencePanel.SetActive(false);
            });

            notRebindButton.onClick.AddListener(() =>
            {
                InputManager.EndChange(false);
                checkRebindPanel.SetActive(false);
            });
        }

        private void UpdateUI(int nextIdx)
        {
            Debug.Log("현재" + _pageIndex + "목표" + nextIdx);
            foreach (var pagePanel in pagePanels)
            {
                pagePanel.SetActive(false);
            }

            pagePanels[nextIdx].SetActive(true);

            _pageIndex = nextIdx;
            pageText.text = (_pageIndex + 1) + " / " + pagePanels.Length;
        }
    
        private void Input(Vector2 input)
        {
            if (input == Vector2.left)
            {
                var nextIdx = (_pageIndex - 1 + pagePanels.Length) % pagePanels.Length;
            
                pagePanels[_pageIndex].SetActive(false);
                _pageIndex = nextIdx;
                pagePanels[_pageIndex].SetActive(true);
                pageText.text = (_pageIndex + 1) + " / " + pagePanels.Length;
            }
            else if (input == Vector2.right)
            {
                var nextIdx = (_pageIndex + 1) % pagePanels.Length;
            
                pagePanels[_pageIndex].SetActive(false);
                _pageIndex = nextIdx;
                pagePanels[_pageIndex].SetActive(true);
                pageText.text = (_pageIndex + 1) + " / " + pagePanels.Length;
            }
        }
    }
}