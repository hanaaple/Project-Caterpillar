using System;
using System.Collections;
using UnityEngine;
using Utility.Dialogue;
using Utility.Scene;
using Utility.UI.Inventory;
using Utility.UI.Pause;
using Utility.UI.Preference;

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
        [SerializeField] private InventoryManager inventoryManager;

        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup fadeImage;

        private bool _isFade;

        private static PlayUIManager Create()
        {
            var playUIManagerPrefab = Resources.Load<PlayUIManager>("Play UI Manager");
            return Instantiate(playUIManagerPrefab);
        }

        private void Awake()
        {
            var sceneHelper = FindObjectOfType<SceneHelper>();
            sceneHelper.Play();
        }

        public void SetPlayType(PlayType playType)
        {
            if (playType == PlayType.None)
            {
                inventoryManager.SetEnable(false);
            }
            else if (playType == PlayType.MainField)
            {
                inventoryManager.SetEnable(true);
            }
            else if (playType == PlayType.StageField)
            {
                inventoryManager.SetEnable(false);
            }
            else if (playType == PlayType.MiniGame)
            {
                inventoryManager.SetEnable(false);
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
    }
}
