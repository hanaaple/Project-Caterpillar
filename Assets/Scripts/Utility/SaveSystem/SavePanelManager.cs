using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
                    _instance._onLoad = new UnityEvent();
                    _instance.OnSave = new UnityEvent();
                    _instance.OnSavePanelActiveFalse = new UnityEvent();
                }

                return _instance;
            }
        }

        public enum SaveLoadType
        {
            None,
            Save,
            Load
        }

        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Button savePanelExitButton;

        [SerializeField] private Transform saveItemParent;
        [SerializeField] private GameObject saveItemPrefab;

        [Header("Check")]
        [SerializeField] private GameObject checkPanel;
        [SerializeField] private TMP_Text checkText;
        [SerializeField] private Button noButton;
        [SerializeField] private Button yesButton;
        [TextArea] [SerializeField] private string newLoadText;
        [TextArea] [SerializeField] private string saveCoverText;

        [NonSerialized] public UnityEvent OnSave;
        [NonSerialized] public UnityEvent OnSavePanelActiveFalse;

        private UnityEvent _onLoad;

        private Highlighter _highlighter;

        private SaveLoadType _saveLoadType;

        private void Awake()
        {
            _highlighter = new Highlighter
            {
                HighlightItems = new List<HighlightItem>(),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _highlighter.Init(
                Highlighter.ArrowType.Vertical,
                () => { savePanelExitButton.onClick?.Invoke(); });

            SetItemData();
            // Add + (추가하기)
            Add(saveItemParent.GetChild(saveItemParent.childCount - 1).gameObject);

            // 유니티 Prefab에서 무조건 마지막 Element로는 추가하기 Item으로 세팅 -> 굳이 그런 예외사항까지 체크할 필욘 없어보임.
            for (var i = 0; i < saveItemParent.childCount - 1; i++)
            {
                var saveLoadItem = saveItemParent.GetChild(i).gameObject;
                Add(saveLoadItem);
            }

            // Highlighter에서 Index는 List의 Index이다. 추가되거나 삭제될 경우 매우 귀찮아진다. 망했다.
        }

        private void Start()
        {
            noButton.onClick.AddListener(() =>
            {
                checkPanel.SetActive(false);
            });
            
            savePanelExitButton.onClick.AddListener(() => { SetSaveLoadPanelActive(false, SaveLoadType.None); });
        }

        public void SetSaveLoadPanelActive(bool isActive, SaveLoadType saveLoadType)
        {
            _saveLoadType = saveLoadType;
            gameObject.SetActive(isActive);
            if (isActive)
            {
                SetItemData();

                // Load All Cover File
                foreach (var saveLoadItemProps in _highlighter.HighlightItems)
                {
                    ((SaveLoadItemProps) saveLoadItemProps).UpdateUI();
                }

                HighlightHelper.Instance.Push(_highlighter);
            }
            else
            {
                HighlightHelper.Instance.Pop(_highlighter);
                OnSavePanelActiveFalse?.Invoke();
                OnSavePanelActiveFalse?.RemoveAllListeners();
            }
        }

        private void SetItemData()
        {
            var saveDataLength = SaveManager.GetSaveDataLength();

            if (saveDataLength > saveItemParent.childCount - 1)
            {
                while (saveDataLength > saveItemParent.childCount - 1)
                {
                    // Index는 고정이었는데, 더이상 고정이 아니다.
                    var addItem = saveItemParent.GetChild(saveItemParent.childCount - 1);
                    var saveLoadItem = Instantiate(saveItemPrefab, saveItemParent);
                    addItem.SetAsLastSibling();
                    Add(saveLoadItem, true);
                }
            }
            else if (saveDataLength < saveItemParent.childCount - 1)
            {
                while (saveDataLength < saveItemParent.childCount - 1)
                {
                    Remove(saveItemParent.GetChild(0).GetComponent<SaveLoadItem>());
                }
            }
        }

        private void Add(GameObject saveLoadObject, bool isActive = false)
        {
            var saveLoadItem = saveLoadObject.GetComponent<SaveLoadItem>();

            var saveLoadItemProps = new SaveLoadItemProps(saveLoadObject)
            {
                button = saveLoadObject.GetComponent<Button>()
            };

            if (saveLoadItem)
            {
                var index = saveLoadObject.transform.GetSiblingIndex();
                var saveDataIndex = SaveManager.GetSaveIndex(index);
                saveLoadItemProps.SaveDataIndex = saveDataIndex;
                _highlighter.AddItem(saveLoadItemProps, _highlighter.HighlightItems.Count - 1, isActive);
            }
            else
            {
                saveLoadItemProps.SaveDataIndex = -1;
                _highlighter.AddItem(saveLoadItemProps);
            }

            if (saveLoadItem)
            {
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    if (_saveLoadType == SaveLoadType.Save)
                    {
                        checkPanel.SetActive(true);

                        checkText.text = saveCoverText;
                        yesButton.onClick.RemoveAllListeners();
                        yesButton.onClick.AddListener(() =>
                        {
                            var saveData = SaveHelper.GetSaveData();

                            var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                            SaveManager.Save(saveDataIndex, saveData);

                            StartCoroutine(WaitSave(saveDataIndex, () =>
                            {
                                OnSave?.Invoke();

                                saveLoadItemProps.UpdateUI();
                            }));
                            
                            checkPanel.SetActive(false);
                        });
                    }
                    else if (_saveLoadType == SaveLoadType.Load)
                    {
                        var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                        var saveCoverData = SaveManager.GetSaveCoverData(saveDataIndex);
                        if (saveCoverData != null)
                        {
                            SceneLoader.SceneLoader.Instance.OnLoadSceneEnd += () =>
                            {
                                SaveHelper.Load(saveDataIndex);
                                SetSaveLoadPanelActive(false, SaveLoadType.None);
                            };
                            SceneLoader.SceneLoader.Instance.LoadScene(saveCoverData.sceneName, saveDataIndex);
                            _onLoad?.Invoke();
                        }
                        else
                        {
                            Debug.LogWarning($"Load 실패 {saveDataIndex}");
                        }
                    }
                });
                saveLoadItem.deleteButton.onClick.AddListener(() =>
                {
                    var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                    Remove(saveLoadItem);
                    SaveManager.Delete(saveDataIndex);
                });
            }
            else
            {
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    if (_saveLoadType == SaveLoadType.Save)
                    {
                        // 새롭게 저장
                        var saveData = SaveHelper.GetSaveData();
                        var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                        SaveManager.Save(saveDataIndex, saveData);

                        var addItem = saveItemParent.GetChild(saveItemParent.childCount - 1);
                        var item = Instantiate(saveItemPrefab, saveItemParent);
                        addItem.SetAsLastSibling();

                        Add(item, true);

                        StartCoroutine(WaitSave(saveDataIndex, () =>
                        {
                            OnSave?.Invoke();

                            saveLoadItemProps.UpdateUI();
                        }));
                    }
                    else if (_saveLoadType == SaveLoadType.Load)
                    {
                        checkPanel.SetActive(true);
                        
                        checkText.text = newLoadText;
                        yesButton.onClick.RemoveAllListeners();
                        yesButton.onClick.AddListener(() =>
                        {
                            SceneLoader.SceneLoader.Instance.LoadScene("MainScene");

                            SceneLoader.SceneLoader.Instance.OnLoadSceneEnd += () =>
                            {
                                SetSaveLoadPanelActive(false, SaveLoadType.None);
                            };
                            _onLoad?.Invoke();
                            
                            checkPanel.SetActive(false);
                        });
                    }
                });
            }
        }

        // Highlighter에 추가된 상태라고 가정
        // Remove Add Item은 없으니까 무시
        private void Remove(SaveLoadItem saveLoadItem)
        {
            var saveLoadItemProps = _highlighter.HighlightItems.Find(item =>
                ((SaveLoadItemProps) item).SaveLoadItem == saveLoadItem);
            _highlighter.RemoveItem(saveLoadItemProps);

            DestroyImmediate(saveLoadItem.gameObject);
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