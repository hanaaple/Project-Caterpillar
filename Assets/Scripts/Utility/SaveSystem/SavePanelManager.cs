using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.Core;
using Utility.Scene;
using Utility.UI.Check;
using Utility.UI.Highlight;
using Utility.Util;

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
                        _instance = Create();
                    }

                    DontDestroyOnLoad(_instance);
                    _instance.OnSave = new UnityEvent();
                    _instance.OnSavePanelInActive = new UnityEvent();
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

        [Header("Canvas")] [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Button savePanelExitButton;
        [SerializeField] private TMP_Text savePanelText;

        [Header("Save Item")] [SerializeField] private Transform saveItemParent;
        [SerializeField] private GameObject saveItemPrefab;

        [Header("Scroll")] [SerializeField] private Slider slider;
        [SerializeField] private RectTransform scrollView;
        [SerializeField] private RectTransform content;
        [Range(0, 20)][SerializeField] private int divideCount = 20;

        [Header("Check UI")] [SerializeField] private CheckUIManager checkUIManager;
        [TextArea] [SerializeField] private string newLoadText;
        [TextArea] [SerializeField] private string saveCoverText;
        [TextArea] [SerializeField] private string deleteText;

        [NonSerialized] public UnityEvent OnSave;
        [NonSerialized] public UnityEvent OnSavePanelInActive;

        private Highlighter _highlighter;
        private SaveLoadType _saveLoadType;
        private string _targetSceneName;

        private static SavePanelManager Create()
        {
            var savePanelManagerPrefab = Resources.Load<SavePanelManager>("SavePanelManager");
            return Instantiate(savePanelManagerPrefab);
        }
        private void Awake()
        {
            _highlighter = new Highlighter("Save Highlight") {HighlightItems = new List<HighlightItem>()};

            _highlighter.Init(Highlighter.ArrowType.Vertical, () => { savePanelExitButton.onClick?.Invoke(); });
            _highlighter.InputActions.OnMouseWheel = _ =>
            {
                // percentage 말고 고정된 value로 바꿔보자
                var value = _.ReadValue<float>();
                if (value > 0)
                {
                    value = slider.value + 1f / divideCount;
                }
                else if (value < 0)
                {
                    value = slider.value - 1f / divideCount;
                }

                slider.value = Mathf.Clamp01(value);
            };

            SetItemData();
            // Add + (추가하기)
            var emptyItem = saveItemParent.GetChild(saveItemParent.childCount - 1).gameObject;
            AddItem(emptyItem);

            // 유니티 Prefab에서 무조건 마지막 Element로는 추가하기 Item으로 세팅 -> 굳이 그런 예외사항까지 체크할 필욘 없어보임.
            for (var i = 0; i < saveItemParent.childCount - 1; i++)
            {
                var saveLoadItem = saveItemParent.GetChild(i).gameObject;
                AddItem(saveLoadItem);
            }
        }

        private void Start()
        {
            checkUIManager.Initialize();
            checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () => { checkUIManager.Pop(); });

            savePanelExitButton.onClick.AddListener(() =>
            {
                SetActiveSaveLoadPanel(false); 
                PlayUIManager.Instance.PlayAudioClick();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="saveLoadType"> Save or Load, if isActive false, type is None </param>
        /// <param name="targetScene"></param>
        public void SetActiveSaveLoadPanel(bool isActive, SaveLoadType saveLoadType = SaveLoadType.None,
            string targetScene = "")
        {
            _saveLoadType = saveLoadType;

            if (saveLoadType == SaveLoadType.Save)
            {
                savePanelText.text = "Save";
                if (string.IsNullOrEmpty(targetScene))
                {
                    Debug.LogWarning("Save - TargetScene 설정 오류 contents에 저장을 목표로 하는 Scene 이름을 넣으세요.");
                }

                _targetSceneName = targetScene;
            }
            else if (saveLoadType == SaveLoadType.Load)
            {
                savePanelText.text = "Load";
                _targetSceneName = "";
            }

            gameObject.SetActive(isActive);
            if (isActive)
            {
                SetItemData();

                slider.value = 1f;
                // Load All Cover File
                foreach (var saveLoadItemProps in _highlighter.HighlightItems)
                {
                    ((SaveLoadHighlightItem) saveLoadItemProps).UpdateUI();
                }

                HighlightHelper.Instance.Push(_highlighter);
            }
            else
            {
                HighlightHelper.Instance.Pop(_highlighter);
                OnSavePanelInActive?.Invoke();
                OnSavePanelInActive?.RemoveAllListeners();
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
                    RemoveItem(saveItemParent.GetChild(0).GetComponent<SaveLoadItem>());
                }
            }
        }

        private void AddItem(GameObject saveLoadObject, bool isActive = false)
        {
            var saveLoadItem = saveLoadObject.GetComponent<SaveLoadItem>();

            var saveLoadItemProps = new SaveLoadHighlightItem(saveLoadItem)
            {
                onSelect = () =>
                {
                    var itemRectTransform = (RectTransform) saveLoadItem.transform;

                    //현재 화면 상에 보이는 위치 0 ~ 1 + scrollView * offset
                    var up = itemRectTransform.anchoredPosition.y + content.rect.height / 2 +
                             content.anchoredPosition.y + itemRectTransform.rect.height / 2; // (위 위치)
                    var upT = Mathf.InverseLerp(-scrollView.rect.height / 2, scrollView.rect.height / 2, up);
                    var upOffset = .1f;

                    var down = itemRectTransform.anchoredPosition.y + content.rect.height / 2 +
                        content.anchoredPosition.y - itemRectTransform.rect.height / 2; // (아래 위치)
                    var downT = Mathf.InverseLerp(-scrollView.rect.height / 2, scrollView.rect.height / 2, down);
                    var downOffset = .1f;

                    // Debug.Log($" {saveLoadItemProps.button.transform.parent.gameObject}  Up: {up}, {upT} Down: {down}, {downT}");

                    if (upT > 1f - upOffset)
                    {
                        var upRatio =
                            -(itemRectTransform.anchoredPosition.y + itemRectTransform.rect.height / 2 +
                              scrollView.rect.height * upOffset) / (content.rect.height - scrollView.rect.height);
                        var slideValue = Mathf.Lerp(1f, 0f, upRatio);
                        // Debug.Log(
                        //     $"({itemRectTransform.anchoredPosition.y} + {itemRectTransform.rect.height / 2} + {scrollView.rect.height * upOffset}) / ({content.rect.height} - {scrollView.rect.height})" +
                        //     $"{slideValue}");
                        slider.value = slideValue;
                    }
                    else if (downT < 0f + downOffset)
                    {
                        var downRatio =
                            -(itemRectTransform.anchoredPosition.y - itemRectTransform.rect.height / 2 +
                                scrollView.rect.height - scrollView.rect.height * downOffset) /
                            (content.rect.height - scrollView.rect.height);
                        var slideValue = Mathf.Lerp(1f, 0f, downRatio);
                        // Debug.Log(
                        //     $"({itemRectTransform.anchoredPosition.y} - {itemRectTransform.rect.height / 2} + {scrollView.rect.height} - {scrollView.rect.height * downOffset}) / ({content.rect.height} - {scrollView.rect.height})" +
                        //     $"{slideValue}");
                        slider.value = slideValue;
                    }
                }
            };

            if (!saveLoadItem.isEmpty)
            {
                var index = saveLoadObject.transform.GetSiblingIndex();
                var saveDataIndex = SaveManager.GetSaveIndex(index);
                Debug.Log($"Save Item Index: {index}, Save Data Index: {saveDataIndex}");
                saveLoadItemProps.SaveDataIndex = saveDataIndex;
                _highlighter.AddItem(saveLoadItemProps, _highlighter.HighlightItems.Count - 1, isActive);
            }
            else
            {
                _highlighter.AddItem(saveLoadItemProps);
            }

            if (!saveLoadItem.isEmpty)
            {
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    Debug.Log($"클릭 {_saveLoadType}");
                    if (_saveLoadType == SaveLoadType.Save)
                    {
                        checkUIManager.SetText(saveCoverText);
                        checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
                        {
                            SaveHelper.SaveSceneData();
                            var saveData = SaveHelper.GetSaveData(_targetSceneName);
                        
                            var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                            SaveManager.Save(saveDataIndex, saveData);
                        
                            StartCoroutine(WaitSave(saveDataIndex, () =>
                            {
                                OnSave?.Invoke();
                        
                                saveLoadItemProps.UpdateUI();
                            }));
                        
                            checkUIManager.Pop();
                        });
                        checkUIManager.Push();
                    }
                    else if (_saveLoadType == SaveLoadType.Load)
                    {
                        var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                        var saveCoverData = SaveManager.GetSaveCoverData(saveDataIndex);
                        if (saveCoverData != null)
                        {
                            SetActiveSaveLoadPanel(false);
                            SceneLoader.Instance.onLoadSceneEnd += () =>
                            {
                                PlayTimer.SetTime(saveCoverData.playTime);
                            };
                                
                            SceneLoader.Instance.LoadScene(saveCoverData.sceneName, saveDataIndex);
                        }
                        else
                        {
                            Debug.LogWarning($"Load 실패 {saveDataIndex}");
                        }
                    }
                });
                saveLoadItem.deleteButton.onClick.AddListener(() =>
                {
                    checkUIManager.SetText(deleteText);
                    checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
                    {
                        var saveDataIndex = saveLoadItemProps.SaveDataIndex;
                        RemoveItem(saveLoadItem);
                        SaveManager.Delete(saveDataIndex);
                        checkUIManager.Pop();
                    });
                    checkUIManager.Push();
                });
            }
            else
            {
                saveLoadItemProps.button.onClick.AddListener(() =>
                {
                    Debug.Log($"클릭 {_saveLoadType}");
                    if (_saveLoadType == SaveLoadType.Save)
                    {
                        SaveHelper.SaveSceneData();
                        var saveData = SaveHelper.GetSaveData(_targetSceneName);
                        var newSaveDataIndex = SaveManager.GetNewSaveIndex();
                        
                        Debug.Log($"New Save Data Index: {newSaveDataIndex}");
                        
                        SaveManager.Save(newSaveDataIndex, saveData);
                        
                        var addItem = saveItemParent.GetChild(saveItemParent.childCount - 1);
                        var item = Instantiate(saveItemPrefab, saveItemParent);
                        addItem.SetAsLastSibling();
                        
                        AddItem(item, true);
                        
                        StartCoroutine(WaitSave(newSaveDataIndex, () =>
                        {
                            OnSave?.Invoke();
                        
                            saveLoadItemProps.UpdateUI();
                        }));
                    }
                    else if (_saveLoadType == SaveLoadType.Load)
                    {
                        checkUIManager.SetText(newLoadText);
                        checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
                        {
                            SetActiveSaveLoadPanel(false);
                            PlayTimer.ReStart();
                            checkUIManager.Pop();
                            SceneLoader.Instance.LoadScene("MainScene");
                        });
                        checkUIManager.Push();
                    }
                });
            }
        }

        // Highlighter에 추가된 상태라고 가정
        // Remove Add Item은 없으니까 무시
        private void RemoveItem(SaveLoadItem saveLoadItem)
        {
            var saveLoadItemProps = _highlighter.HighlightItems.Find(item =>
                ((SaveLoadHighlightItem) item).SaveLoadItem == saveLoadItem);
            _highlighter.RemoveItem(saveLoadItemProps, true);
            
            DestroyImmediate(saveLoadItem.gameObject);
            
            foreach (var item in _highlighter.HighlightItems)
            {
                ((SaveLoadHighlightItem) item).UpdateUI();
            }
            // index 변경
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