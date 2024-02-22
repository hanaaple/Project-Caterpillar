using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Dialogue;
using Utility.Scene;
using Utility.Tutorial;
using Utility.UI.Inventory;
using Utility.UI.Pause;
using Utility.UI.Preference;
using Utility.UI.QuickSlot;

namespace Utility.Core
{
    public class PlayUIManager : MonoBehaviour
    {
        private static PlayUIManager _instance;

        public static PlayUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<PlayUIManager>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                        _instance.Init();
                    }
                    
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        public PauseManager pauseManager;
        public PreferenceManager preferenceManager;
        public DialogueController dialogueController;
        public QuickSlotManager quickSlotManager;
        public TutorialManager tutorialManager;

        public IInventory Inventory
        {
            get => _inventory;
            private set
            {
                if (_inventory == value) return;
                value?.SetEnable(true);
                _inventory?.SetEnable(false);
                _inventory = value;
            }
        }

        public Transform floatingMarkParent;
        [SerializeField] private CanvasGroup fadeImage;

        public AudioData defaultHighlightAudioData;
        public AudioData defaultClickAudioData;

        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Stage2InventoryManager stage2InventoryManager;

        private bool _isFade;
        private IInventory _inventory;

        private static PlayUIManager Create()
        {
            var playUIManagerPrefab = Resources.Load<PlayUIManager>("Play UI Manager");
            return Instantiate(playUIManagerPrefab);
        }

        private void Init()
        {
            inventoryManager.SetEnable(false);
            stage2InventoryManager.SetEnable(false);
        }

        public void OnLoadSceneInit(PlayType playType, StageType stageType)
        {
            SetPlayType(playType, stageType);
            dialogueController.cutSceneImage.SetActive(false);
            fadeImage.gameObject.SetActive(false);
        }

        private void SetPlayType(PlayType playType, StageType stageType)
        {
            switch (playType)
            {
                case PlayType.None or PlayType.MiniGame:
                    quickSlotManager.SetActive(false);
                    Inventory = null;
                    break;
                case PlayType.StageField:
                    quickSlotManager.SetActive(false);
                    Inventory = stageType == StageType.Stage2 ? stage2InventoryManager : null;
                    break;
                case PlayType.MainField:
                    quickSlotManager.SetActive(true);
                    Inventory = inventoryManager;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playType), playType, null);
            }
        }

        public void ResetFade()
        {
            StopAllCoroutines();
            fadeImage.gameObject.SetActive(false);
            _isFade = false;
        }

        public void FadeIn(Action onEndAction = null)
        {
            _isFade = true;
            StartCoroutine(Fade(true, onEndAction));
        }

        public void FadeOut(Action onEndAction = null)
        {
            _isFade = true;
            StartCoroutine(Fade(false, onEndAction));
        }

        private IEnumerator Fade(bool isFadeIn, Action onEndAction)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.GetComponent<Image>().color = Color.black;
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;

                fadeImage.alpha = isFadeIn ? 1 - t : t;

                yield return null;
            }

            _isFade = false;
            if (isFadeIn)
            {
                fadeImage.gameObject.SetActive(false);
            }

            onEndAction?.Invoke();
        }

        public bool IsFade()
        {
            return _isFade;
        }

        public void PlayAudioClick()
        {
            defaultClickAudioData.Play();
        }

        public void PlayAudioHighlight()
        {
            defaultHighlightAudioData.Play();
        }

        public void Destroy()
        {
            _instance = null;
            Destroy(gameObject);
        }
    }
}