using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.UI.Highlight;

namespace Utility.SaveSystem
{
    public class SavePanelManager : MonoBehaviour
    {
        private static SavePanelManager _instance;

        public static SavePanelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<SavePanelManager>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Instantiate(Resources.Load<SavePanelManager>("SavePanelManager"));
                    }

                    DontDestroyOnLoad(_instance);
                    _instance.onLoad = new UnityEvent();
                    _instance.onSave = new UnityEvent();
                    _instance.onSavePanelActiveFalse = new UnityEvent();
                }

                return _instance;
            }
        }

        public enum ButtonType
        {
            None,
            Save,
            Load,
            Delete
        }

        public GameObject savePanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Button savePanelExitButton;

        [SerializeField] private SaveLoadItemProps[] saveItemPropsArray;

        [NonSerialized] public UnityEvent onSave;

        [NonSerialized] public UnityEvent onLoad;
        [NonSerialized] public UnityEvent onSavePanelActiveFalse;

        private Highlighter _highlighter;

        private ButtonType _buttonType;

        private void Awake()
        {
            _highlighter = new Highlighter
            {
                highlightItems = saveItemPropsArray,
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };
            _highlighter.Init(
                Highlighter.ArrowType.Vertical,
                () => { savePanelExitButton.onClick?.Invoke(); });
        }

        private void Start()
        {
            savePanelExitButton.onClick.AddListener(() => { SetSaveLoadPanelActive(false, ButtonType.None); });

            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    if (_buttonType == ButtonType.Save)
                    {
                        var saveData = new SaveData
                        {
                            saveCoverData = new SaveCoverData
                            {
                                describe = "테스트입니다." + SceneManager.GetActiveScene().name,
                                sceneName = SceneManager.GetActiveScene().name,
                                playTime = 1122
                            }
                        };
                        
                        SaveManager.Save(saveLoadItemProps.saveDataIndex, saveData);

                        StartCoroutine(WaitSave(saveLoadItemProps.saveDataIndex, () =>
                        {
                            onSave?.Invoke();

                            saveLoadItemProps.UpdateUI();
                        }));
                    }
                    else if (_buttonType == ButtonType.Load)
                    {
                        var saveCoverData = SaveManager.GetSaveCoverData(saveLoadItemProps.saveDataIndex);
                        if (saveCoverData != null)
                        {
                            SceneLoader.SceneLoader.Instance.onLoadSceneEnd += () => { SetSaveLoadPanelActive(false, ButtonType.None); };
                            SceneLoader.SceneLoader.Instance.LoadScene(saveCoverData.sceneName,
                                saveLoadItemProps.saveDataIndex);
                            onLoad?.Invoke();
                        }
                        else
                        {
                            Debug.LogError("Load 실패");
                        }
                    }
                    else if (_buttonType == ButtonType.Delete)
                    {
                        saveLoadItemProps.UpdateUI();
                    }
                });
            }
        }

        public void SetSaveLoadPanelActive(bool isActive, ButtonType buttonType)
        {
            _buttonType = buttonType;
            savePanel.SetActive(isActive);
            if (isActive)
            {
                foreach (var saveItemProps in saveItemPropsArray)
                {
                    saveItemProps.UpdateUI();
                }

                HighlightHelper.Instance.Push(_highlighter);
            }
            else
            {
                HighlightHelper.Instance.Pop(_highlighter);
                onSavePanelActiveFalse?.Invoke();
                onSavePanelActiveFalse?.RemoveAllListeners();
            }
        }

        private IEnumerator WaitSave(int index, Action onSaveAction)
        {
            loadingPanel.SetActive(true);
            yield return new WaitUntil(() => SaveManager.IsLoaded(index));
            loadingPanel.SetActive(false);
            onSaveAction?.Invoke();
        }
    }
}