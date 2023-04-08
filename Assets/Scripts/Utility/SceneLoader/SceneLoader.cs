using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Core;
using Utility.SaveSystem;

namespace Utility.SceneLoader
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

        private string _loadSceneName;

        public Action OnLoadScene;
        public Action OnLoadSceneEnd;

        private static SceneLoader Create()
        {
            var sceneLoaderPrefab = Resources.Load<SceneLoader>("SceneLoader");
            return Instantiate(sceneLoaderPrefab);
        }

        public void LoadScene(string sceneName, int index = -1)
        {
            OnLoadScene?.Invoke();
            OnLoadScene = () => { };
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

            gameObject.SetActive(true);
            SceneManager.sceneLoaded += LoadSceneEnd;
            _loadSceneName = sceneName;
            StartCoroutine(Load(sceneName, index));
        }

        private IEnumerator Load(string sceneName, int index)
        {
            progressBar.fillAmount = 0f;
            yield return StartCoroutine(Fade(true));

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            var timer = 0.0f;
            while (!op.isDone)
            {
                yield return null;
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

        private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != _loadSceneName)
            {
                return;
            }
            
            Debug.Log("OnLoadSceneEnd");
            StartCoroutine(Fade(false));
            OnLoadSceneEnd?.Invoke();
            OnLoadSceneEnd = () => { };
            SceneManager.sceneLoaded -= LoadSceneEnd;
        }

        private IEnumerator Fade(bool isFadeIn)
        {
            var timer = 0f;

            while (timer <= 1f)
            {
                yield return null;
                timer += Time.unscaledDeltaTime * 2f;
                sceneLoaderCanvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
            }

            if (!isFadeIn)
            {
                gameObject.SetActive(false);
            }
        }
    }
}