using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Utility.SaveSystem
{
    public class SavePanelManager : MonoBehaviour
    {
        private static SavePanelManager _instance;

        public static SavePanelManager instance => _instance;

        [SerializeField] private GameObject savePanel;

        [SerializeField] private SaveLoadItemProps[] saveItemPropsArray;

        [NonSerialized]
        public UnityEvent OnSave;
    
        [NonSerialized]
        public UnityEvent OnLoad;

        [SerializeField] private int selectedIdx;
        
        private Action<InputAction.CallbackContext> _onInput;
        private Action<InputAction.CallbackContext> _onExecute;

        private void Awake()
        {
            _instance = this;
            
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
                }
            };
        }

        private void Start()
        {
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.Init();
            }
        }
        
        private void OnEnable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Enable();
            uiActions.Select.performed += _onInput;
            uiActions.Select.performed += _onExecute;
        }

        private void OnDisable()
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Disable();
            uiActions.Select.performed -= _onInput;
            uiActions.Select.performed -= _onExecute;
        }

        public void InitLoad()
        {
            OnLoad = new UnityEvent();
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
            OnSave = new UnityEvent();
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.RemoveAllListeners();
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    SaveButton(saveLoadItemProps.index);
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

            OnSave?.Invoke();
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
                OnLoad?.Invoke();
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
