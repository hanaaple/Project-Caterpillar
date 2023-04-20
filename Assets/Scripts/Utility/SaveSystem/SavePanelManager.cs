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
        [SerializeField] private TMP_Text savePanelText;

        [SerializeField] private Transform saveItemParent;
        [SerializeField] private GameObject saveItemPrefab;

        [SerializeField] private Slider slider;
        [SerializeField] private RectTransform scrollView;
        [SerializeField] private RectTransform content;

        [Header("Check")] [SerializeField] private GameObject checkPanel;
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
                HighlightItems = new List<HighlightItem>()
            };

            _highlighter.Init(
                Highlighter.ArrowType.Vertical,
                () => { savePanelExitButton.onClick?.Invoke(); });

            SetItemData();
            // Add + (추가하기)
            var emptyItem = saveItemParent.GetChild(saveItemParent.childCount - 1).gameObject;
            Add(emptyItem);

            // 유니티 Prefab에서 무조건 마지막 Element로는 추가하기 Item으로 세팅 -> 굳이 그런 예외사항까지 체크할 필욘 없어보임.
            for (var i = 0; i < saveItemParent.childCount - 1; i++)
            {
                var saveLoadItem = saveItemParent.GetChild(i).gameObject;
                Add(saveLoadItem);
            }
        }

        private void Start()
        {
            noButton.onClick.AddListener(() => { checkPanel.SetActive(false); });

            savePanelExitButton.onClick.AddListener(() => { SetSaveLoadPanelActive(false, SaveLoadType.None); });
        }

        public void SetSaveLoadPanelActive(bool isActive, SaveLoadType saveLoadType)
        {
            _saveLoadType = saveLoadType;

            if (saveLoadType == SaveLoadType.Save)
            {
                savePanelText.text = "Save";
            }
            else if (saveLoadType == SaveLoadType.Load)
            {
                savePanelText.text = "Load";
            }

            gameObject.SetActive(isActive);
            if (isActive)
            {
                SetItemData();

                slider.value = 1f;
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
            Debug.Log($"세이브 개수: {saveDataLength}, 있는 SaveItem 개수: {saveItemParent.childCount - 1}");
            if (saveDataLength > saveItemParent.childCount - 1)
            {
                while (saveDataLength > saveItemParent.childCount - 1)
                {
                    // Index는 고정이었는데, 더이상 고정이 아니다.
                    var addItem = saveItemParent.GetChild(saveItemParent.childCount - 1);
                    Instantiate(saveItemPrefab, saveItemParent);
                    addItem.SetAsLastSibling();
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

            var saveLoadItemProps = new SaveLoadItemProps(saveLoadItem);

            saveLoadItemProps.OnSelect = () =>
            {
                var itemRectTransform = saveLoadItemProps.button.image.rectTransform;

                //현재 화면 상에 보이는 위치 0 ~ 1 + scrollView * offset
                var up = itemRectTransform.anchoredPosition.y + content.rect.height / 2 +
                         content.anchoredPosition.y + itemRectTransform.rect.height / 2; // (위 위치)
                var upT = Mathf.InverseLerp(-scrollView.rect.height / 2, scrollView.rect.height / 2, up);
                var upOffset = .1f;

                var down = itemRectTransform.anchoredPosition.y + content.rect.height / 2 +
                    content.anchoredPosition.y - itemRectTransform.rect.height / 2; // (아래 위치)
                var downT = Mathf.InverseLerp(-scrollView.rect.height / 2, scrollView.rect.height / 2, down);
                var downOffset = .1f;


                if (upT > 1f - upOffset)
                {
                    var upRatio =
                        -(itemRectTransform.anchoredPosition.y + itemRectTransform.rect.height / 2 +
                          scrollView.rect.height * upOffset) / (content.rect.height - scrollView.rect.height);
                    var slideValue = Mathf.Lerp(1f, 0f, upRatio);
                    Debug.Log(
                        $"({itemRectTransform.anchoredPosition.y} + {itemRectTransform.rect.height / 2} + {scrollView.rect.height * upOffset}) / ({content.rect.height} - {scrollView.rect.height})" +
                        $"{slideValue}");
                    slider.value = slideValue;
                }
                else if (downT < 0f + downOffset)
                {
                    var downRatio =
                        -(itemRectTransform.anchoredPosition.y - itemRectTransform.rect.height / 2 +
                            scrollView.rect.height - scrollView.rect.height * downOffset) /
                        (content.rect.height - scrollView.rect.height);
                    var slideValue = Mathf.Lerp(1f, 0f, downRatio);
                    Debug.Log(
                        $"({itemRectTransform.anchoredPosition.y} - {itemRectTransform.rect.height / 2} + {scrollView.rect.height} - {scrollView.rect.height * downOffset}) / ({content.rect.height} - {scrollView.rect.height})" +
                        $"{slideValue}");
                    slider.value = slideValue;
                }
            };

            if (!saveLoadItem.isEmpty)
            {
                var index = saveLoadObject.transform.GetSiblingIndex();
                var saveDataIndex = SaveManager.GetSaveIndex(index);
                saveLoadItemProps.SaveDataIndex = saveDataIndex;
                Debug.Log($"{index}번째에 {saveDataIndex} 추가");
                _highlighter.AddItem(saveLoadItemProps, _highlighter.HighlightItems.Count - 1, isActive);
            }
            else
            {
                saveLoadItemProps.SaveDataIndex = -1;
                _highlighter.AddItem(saveLoadItemProps);
            }

            if (!saveLoadItem.isEmpty)
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
            _highlighter.RemoveItem(saveLoadItemProps, true);

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