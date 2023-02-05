using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility.SaveSystem
{
    public class SavePanelManager : MonoBehaviour
    {
        private static SavePanelManager _instance;
        public static SavePanelManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<SavePanelManager>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Instantiate(Resources.Load<SavePanelManager>("SavePanelManager"));
                    }
                    DontDestroyOnLoad(_instance);
                    _instance.onLoad = new UnityEvent();
                    _instance.onLoadEnd = new UnityEvent();
                    _instance.onSave = new UnityEvent();
                }
                return _instance;
            }
        }

        public GameObject savePanel;
        [SerializeField] private Button savePanelExitButton;

        [SerializeField] private SaveLoadItemProps[] saveItemPropsArray;

        [NonSerialized]
        public UnityEvent onSave;
    
        [NonSerialized]
        public UnityEvent onLoad;
        
        [NonSerialized]
        public UnityEvent onLoadEnd;

        [SerializeField] private int selectedIdx;
        
        private Action<InputAction.CallbackContext> _onInput;
        private Action<InputAction.CallbackContext> _onExecute;

        private void Awake()
        {
            _onInput = _ =>
            {
                if (savePanel.activeSelf)
                {
                    Input(_.ReadValue<Vector2>());
                }
            };
            
            _onExecute = _ =>
            {
                if (savePanel.activeSelf)
                {
                    saveItemPropsArray[selectedIdx].Execute();
                    Debug.Log("로드하고 끄기" + savePanel.activeSelf);
                }
            };
        }

        private void Start()
        {
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.Init();
            }
            savePanelExitButton.onClick.AddListener(() =>
            {
                SetSaveLoadPanelActive(false);
            });
        }
        
        private void OnEnable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Enable();
            uiActions.Select.performed += _onInput;
            uiActions.Execute.performed += _onExecute;
        }

        private void OnDisable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Disable();
            uiActions.Select.performed -= _onInput;
            uiActions.Execute.performed -= _onExecute;
        }

        public void InitLoad()
        {
            onLoadEnd.RemoveAllListeners();
            onLoadEnd.AddListener(() =>
            {
                SetSaveLoadPanelActive(false);
            });
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.RemoveAllListeners();
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    LoadButton(saveLoadItemProps.index);
                });
                saveLoadItemProps.InitEventTrigger(delegate
                {
                    HighlightButton(saveLoadItemProps.index);
                });
                saveLoadItemProps.UpdateUI();
            }
        }

        public void InitSave()
        {
            onSave.RemoveAllListeners();
            onSave.AddListener(() =>
            {
                SetSaveLoadPanelActive(false);
            });
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.RemoveAllListeners();
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    SaveButton(saveLoadItemProps.index);
                });
                saveLoadItemProps.InitEventTrigger(delegate
                {
                    HighlightButton(saveLoadItemProps.index);
                });
                saveLoadItemProps.UpdateUI();
            }
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"> 0 ~ n </param>
        private void SaveButton(int idx)
        {
            Debug.Log($"{idx}번 세이브 파일 저장하기");
            var saveData = SaveManager.GetSaveData();
            saveData.SetHp(idx * 100);
            saveData.SetScenario("테스트입니다.");
            saveData.SetPlayTime(1122);
        
            SaveManager.Save(idx);

            onSave?.Invoke();
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"> 0 ~ n </param>
        private void LoadButton(int idx)
        {
            Debug.Log($"{idx}번 세이브 파일 로드");

            if (SaveManager.Load(idx))
            {
                SceneLoader.SceneLoader.Instance.onLoadSceneEnd += () =>
                {
                    onLoadEnd?.Invoke();
                };
                SceneLoader.SceneLoader.Instance.LoadScene("MainScene");
                onLoad?.Invoke();
            }
            else
            {
                Debug.LogError("Load 실패");
            }
        }

        public void SetSaveLoadPanelActive(bool isActive)
        {
            savePanel.SetActive(isActive);
            if (isActive)
            {
                foreach (var saveItemProps in saveItemPropsArray)
                {
                    saveItemProps.UpdateUI();
                }

                HighlightButton(0);
            }        
        }
        
        private void Input(Vector2 input)
        {
            var idx = selectedIdx;
            if (input == Vector2.up)
            {
                idx = (idx - 1 + saveItemPropsArray.Length) % saveItemPropsArray.Length;
            }
            else if (input == Vector2.down)
            {
                idx = (idx + 1) % saveItemPropsArray.Length;
            }

            HighlightButton(idx);
        }
        
        internal void HighlightButton(int idx)
        {
            Debug.Log($"{idx}입니다");
            saveItemPropsArray[selectedIdx].SetDefault();
            selectedIdx = idx;
            saveItemPropsArray[idx].SetHighlight();
        }
    }
}
