using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.InputSystem;
using Utility.UI.Check;
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

        [SerializeField] private GameObject rebindButtonPanel;

        [SerializeField] private Button resetButton;

        [SerializeField] private Button saveButton;

        [SerializeField] private Button cancelSaveButton;

        [SerializeField] private PageProps[] pageProps;

        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private InputController[] inputController;

        [SerializeField] private CheckUIManager rebindCheckUIManager;

        private InputActions _inputActions;

        private void Awake()
        {
            _inputActions = new InputActions(nameof(PreferenceManager))
            {
                OnCancel = _ =>
                {
                    if (preferencePanel.activeSelf && !rebindCheckUIManager.gameObject.activeSelf)
                    {
                        SetPreferencePanel(false);
                    }
                }
            };

            AudioManager.LoadAudio();
        }

        private void Start()
        {
            rebindCheckUIManager.Initialize();
            rebindCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
            {
                InputManager.SetChange(true);
                rebindCheckUIManager.Pop();
                SetPreferencePanel(false);
            });

            rebindCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () =>
            {
                InputManager.SetChange(false);
                rebindCheckUIManager.Pop();
            });

            resetButton.onClick.AddListener(() =>
            {
                foreach (var t in inputController)
                {
                    t.TempResetBinding();
                }
            });

            cancelSaveButton.onClick.AddListener(() => { InputManager.SetChange(false); });

            saveButton.onClick.AddListener(() => { InputManager.SetChange(true); });

            preferenceExitButton.onClick.AddListener(() =>
            {
                SetPreferencePanel(false);
            });


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

        private void SetRebindButton()
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

        public void SetPreferencePanel(bool isActive)
        {
            if (isActive)
            {
                preferencePanel.SetActive(true);
                InputManager.RebindComplete += SetRebindButton;
                InputManager.RebindEnd += SetRebindButton;
                // InputManager.RebindLoad += SetRebindButton;
                InputManager.RebindReset += SetRebindButton;

                InputManager.PushInputAction(_inputActions);
            }
            else
            {
                if (InputManager.IsChanged())
                {
                    rebindCheckUIManager.Push();
                }
                else
                {
                    // HighlightHelper.Instance.Enable();
                    preferencePanel.SetActive(false);
                    
                    InputManager.RebindComplete -= SetRebindButton;
                    InputManager.RebindEnd -= SetRebindButton;
                    // InputManager.RebindLoad -= SetRebindButton;
                    InputManager.RebindReset -= SetRebindButton;

                    InputManager.PopInputAction(_inputActions);
                }                
            }
        }
    }
}