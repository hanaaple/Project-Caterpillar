using System;
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

        [NonSerialized] public UnityEvent onSave;

        [NonSerialized] public UnityEvent onLoad;

        [NonSerialized] public UnityEvent onLoadEnd;

        private Highlighter _highlighter;

        private void Awake()
        {
            _highlighter = new Highlighter
            {
                highlightItems = saveItemPropsArray,
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };
            _highlighter.Init(
                Highlighter.ArrowType.Vertical,
                () =>
                {
                    savePanelExitButton.onClick?.Invoke();
                });
        }

        private void Start()
        {
            savePanelExitButton.onClick.AddListener(() =>
            {
                SetSaveLoadPanelActive(false);
            });
        }

        public void InitLoad()
        {
            onLoadEnd.RemoveAllListeners();
            onLoadEnd.AddListener(() => { SetSaveLoadPanelActive(false); });
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.RemoveAllListeners();
                saveLoadItemProps.button.onClick.AddListener(() => { LoadButton(saveLoadItemProps.saveDataIndex); });
                saveLoadItemProps.UpdateUI();
            }
        }

        public void InitSave()
        {
            onSave.RemoveAllListeners();
            onSave.AddListener(() => { SetSaveLoadPanelActive(false); });
            foreach (var saveLoadItemProps in saveItemPropsArray)
            {
                saveLoadItemProps.button.onClick.RemoveAllListeners();
                saveLoadItemProps.button.onClick.AddListener(() => { SaveButton(saveLoadItemProps.saveDataIndex); });
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
                SceneLoader.SceneLoader.Instance.onLoadSceneEnd += () => { onLoadEnd?.Invoke(); };
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

                HighlightHelper.Instance.Push(_highlighter);
            }
            else
            {
                HighlightHelper.Instance.Pop(_highlighter);
            }
        }
    }
}
