using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
                    }

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        
        public PauseManager pauseManager;
        public PreferenceManager preferenceManager;
        public DialogueController dialogueController;
        public InventoryManager inventoryManager;
        public QuickSlotManager quickSlotManager;
        public TutorialManager tutorialManager;
        
        public Transform floatingMarkParent;
        [SerializeField] private CanvasGroup fadeImage;

        private bool _isFade;

        private static PlayUIManager Create()
        {
            var playUIManagerPrefab = Resources.Load<PlayUIManager>("Play UI Manager");
            return Instantiate(playUIManagerPrefab);
        }

        public void Init(PlayType playType)
        {
            SetPlayType(playType);
            dialogueController.cutSceneImage.SetActive(false);
            fadeImage.gameObject.SetActive(false);
        }

        private void SetPlayType(PlayType playType)
        {
            switch (playType)
            {
                case PlayType.None:
                case PlayType.StageField:
                case PlayType.MiniGame:
                    quickSlotManager.SetEnable(false);
                    inventoryManager.SetEnable(false);
                    break;
                case PlayType.MainField:
                    quickSlotManager.SetEnable(true);
                    inventoryManager.SetEnable(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playType), playType, null);
            }
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
            fadeImage.gameObject.SetActive(!isFadeIn);
            onEndAction?.Invoke();
        }

        public bool IsFade()
        {
            return _isFade;
        }

        public void Destroy()
        {
            _instance = null;
            Destroy(gameObject);
        }
    }
}
