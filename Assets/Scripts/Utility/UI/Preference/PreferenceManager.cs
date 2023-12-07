using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
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
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    public class PreferenceManager : MonoBehaviour
    {
        [Header("Common")] [SerializeField] private GameObject preferencePanel;
        [SerializeField] private Button preferenceExitButton;
        [SerializeField] private PageProps[] pageProps;
        [SerializeField] private CheckUIManager rebindCheckUIManager;

        [Header("Control")] [SerializeField] private GameObject rebindButtonPanel;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelSaveButton;
        [SerializeField] private InputController[] inputController;

        [Header("Audio & Resolution")] [SerializeField]
        private TMP_Dropdown resolutionDropdown;

        [SerializeField] private Button fullScreenButton;
        [SerializeField] private Button windowScreenButton;

        [SerializeField] private Sprite window;
        [SerializeField] private Sprite windowSelect;
        [SerializeField] private Sprite fullscreen;
        [SerializeField] private Sprite fullscreenSelect;


        private InputActions _inputActions;

        private void Awake()
        {
            _inputActions = new InputActions(nameof(PreferenceManager))
            {
                OnEsc = () =>
                {
                    if (preferencePanel.activeSelf && !rebindCheckUIManager.gameObject.activeSelf)
                    {
                        preferenceExitButton.onClick?.Invoke();
                    }
                }
            };

            AudioManager.Instance.LoadAudio();
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
                PlayUIManager.Instance.PlayAudioClick();
            });

            foreach (var pageProp in pageProps)
            {
                pageProp.button.onClick.AddListener(() =>
                {
                    foreach (var t in pageProps)
                    {
                        t.panel.SetActive(false);
                        t.button.image.sprite = t.inactiveSprite;
                    }

                    pageProp.panel.SetActive(true);
                    pageProp.button.image.sprite = pageProp.activeSprite;
                    PlayUIManager.Instance.PlayAudioClick();
                });

                if (pageProp.panel.activeSelf)
                {
                    pageProp.button.image.sprite = pageProp.activeSprite;
                }
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

            fullScreenButton.onClick.AddListener(() =>
            {
                fullScreenButton.image.sprite = fullscreenSelect;
                windowScreenButton.image.sprite = window;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            });

            windowScreenButton.onClick.AddListener(() =>
            {
                fullScreenButton.image.sprite = fullscreen;
                windowScreenButton.image.sprite = windowSelect;
                Screen.fullScreenMode = FullScreenMode.Windowed;
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

                if (Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    fullScreenButton.image.sprite = fullscreen;
                    windowScreenButton.image.sprite = windowSelect;
                }
                else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                {
                    fullScreenButton.image.sprite = fullscreenSelect;
                    windowScreenButton.image.sprite = window;
                }
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