using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.Player;
using Utility.SaveSystem;
using Utility.Util;

namespace Utility.Scene
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;
        public static SceneLoader Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<SceneLoader>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }
                    _instance.gameObject.SetActive(false);
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
    
        [SerializeField] private CanvasGroup sceneLoaderCanvasGroup;
        [SerializeField] private Image progressBar;
        [SerializeField] private float fadeSec;

        [NonSerialized] public bool IsLoading;
        
        private string _loadSceneName;

        public Action onLoadScene;
        public Action onLoadSceneEnd;

        private static SceneLoader Create()
        {
            var sceneLoaderPrefab = Resources.Load<SceneLoader>("SceneLoader");
            return Instantiate(sceneLoaderPrefab);
        }

        public void LoadScene(string sceneName, int index = -1)
        {
            IsLoading = true;
            // 모든 입력 금지
            onLoadScene?.Invoke();
            onLoadScene = () => { };
            Debug.Log("Load");
            
            if (index != -1)
            {
                if (SaveManager.IsLoaded(index))
                {
                    SaveManager.GetSaveData(index);
                }
                else if (SaveManager.Exists(index))
                {
                    SaveManager.Load(index);
                }
                else
                {
                    Debug.LogError("오류");
                }
            }
            
            TimeScaleHelper.Push(0f);
            gameObject.SetActive(true);
            SceneManager.sceneLoaded += LoadSceneEnd;
            _loadSceneName = sceneName;
            StartCoroutine(Load(sceneName, index));
        }

        private IEnumerator Load(string sceneName, int index)
        {
            InputManager.ResetInputAction();
            progressBar.fillAmount = 0f;
            yield return StartCoroutine(Fade(true));

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            var timer = 0f;
            var waitForSecondsRt = new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            while (!op.isDone)
            {
                yield return waitForSecondsRt;
                timer += Time.unscaledDeltaTime;

                if (op.progress < 0.9f)
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                    if (progressBar.fillAmount >= op.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);

                    if (!Mathf.Approximately(progressBar.fillAmount, 1.0f))
                    {
                        continue;
                    }
                    
                    if (index == -1)
                    {
                        GameManager.Instance.InteractionObjects.Clear();
                        op.allowSceneActivation = true;
                        yield break;
                    } 
                    
                    if (SaveManager.IsLoaded(index))
                    {
                        GameManager.Instance.InteractionObjects.Clear();
                        op.allowSceneActivation = true;
                        yield break;
                    }
                }
            }
        }

        private void LoadSceneEnd(UnityEngine.SceneManagement.Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != _loadSceneName)
            {
                return;
            }
            
            IsLoading = false;
            
            Debug.Log("OnLoadSceneEnd");
            StartCoroutine(Fade(false));
            onLoadSceneEnd?.Invoke();
            onLoadSceneEnd = () => { };

            PlayUIManager.Instance.dialogueController.cutSceneImage.SetActive(false);
            PlayerManager.Instance.UpdateCamera();
            
            var sceneHelper = FindObjectOfType<SceneHelper>();
            if (sceneHelper)
            {
                sceneHelper.Play();
            }
            else
            {
                Debug.LogWarning($"In Scene - {SceneManager.GetActiveScene().name}, There is no SceneHelper");
            }
            TimeScaleHelper.Pop();
            SceneManager.sceneLoaded -= LoadSceneEnd;
        }

        private IEnumerator Fade(bool isFadeIn)
        {
            var timer = 0f;

            var waitForSecondsRt = new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            while (timer <= 1)
            {
                yield return waitForSecondsRt;
                timer += Time.unscaledDeltaTime / fadeSec;
                sceneLoaderCanvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
            }

            if (!isFadeIn)
            {
                gameObject.SetActive(false);
            }
        }
    }
}