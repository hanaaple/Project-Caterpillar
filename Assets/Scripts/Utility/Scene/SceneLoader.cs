using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.Tendency;
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
                if (_instance == null)
                {
                    var obj = FindObjectOfType<SceneLoader>();
                    if (obj != null)
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

        private string _loadSceneName;

        public Action onLoadScene;
        public Action onLoadSceneEnd;

        private static SceneLoader Create()
        {
            var sceneLoaderPrefab = Resources.Load<SceneLoader>("SceneLoader");
            return Instantiate(sceneLoaderPrefab);
        }

        // Title -> Load (Just Load Data)
        // Interaction MoveMap (Do Not Load Data, But have to Save Scene State)
        // Game Over - Go to MainScene (Reset MainScene ~ GameScene)
        // Game Ending - MainScene


        // if stage field moves continuous, have to save them!!!
        // i think it have to like this!!
        // so In SceneHelper, Add Property of Is Save

        // if Load -> Main Scene, GameManager.Instance.Load(saveDataIndex);
        public void LoadScene(string targetSceneName, int index = -1)
        {
            // 모든 입력 금지
            onLoadScene?.Invoke();
            onLoadScene = () => { };
            Debug.Log($"Load targetScene - {targetSceneName}, {index}");

            if (index != -1)
            {
                if (SaveManager.IsLoaded(index))
                {
                    Debug.Log("로드 된 상태임");
                }
                else if (SaveManager.Exists(index))
                {
                    Debug.Log("로드 시작");
                    SaveManager.Load(index);
                }
                else
                {
                    Debug.LogError($"{index}의 저장 데이터가 존재하지 않음");
                }
            }

            AudioManager.Instance.StopAudio();
            TendencyManager.Instance.SaveTendencyData();
            TimeScaleHelper.Push(0f);
            gameObject.SetActive(true);
            SceneManager.sceneLoaded += LoadSceneEnd;
            _loadSceneName = targetSceneName;
            StartCoroutine(Load(targetSceneName, index));
        }

        private IEnumerator Load(string targetSceneName, int index)
        {
            InputManager.ResetInputAction();
            progressBar.fillAmount = 0f;
            yield return StartCoroutine(Fade(true));

            var op = SceneManager.LoadSceneAsync(targetSceneName);
            op.allowSceneActivation = false;

            var timer = 0f;
            const float timeInterval = 0.02f;
            var waitForSecondsRt = new WaitForSecondsRealtime(timeInterval);

            while (!op.isDone)
            {
                yield return waitForSecondsRt;
                timer += timeInterval;

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

                    if (!Mathf.Approximately(progressBar.fillAmount, 1f))
                    {
                        continue;
                    }

                    if (index != -1)
                    {
                        if (SaveManager.IsLoaded(index))
                        {
                            // LoadSaveData가 LoadSceneData보다 먼저 발생하면 안됨.
                            onLoadSceneEnd += () => { SaveHelper.LoadSaveData(index); };
                        }
                        else
                        {
                            Debug.LogWarning("SaveData 불러오는 중");
                            continue;
                        }
                    }

                    // LoadSaveData 순서 주의
                    onLoadSceneEnd += SaveHelper.LoadSceneData;

                    // SaveData Clear
                    if (targetSceneName == "TitleScene")
                    {
                        // Timer.Reset();
                        SaveHelper.Clear();
                    }
                    else
                    {
                        // LoadSceneData 순서 주의 이후에 실행되어야됨.
                        onLoadSceneEnd += GameManager.Instance.StartOnAwakeInteraction;
                        // Timer.Play();
                    }

                    // Game -> Game, Save SceneData
                    if (SceneManager.GetActiveScene().name != "TitleScene" && targetSceneName != "TitleScene")
                    {
                        SaveHelper.SaveSceneData();
                    }

                    GameManager.Instance.Clear();
                    PlayUIManager.Instance.ResetFade();
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }

        private void LoadSceneEnd(UnityEngine.SceneManagement.Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != _loadSceneName)
            {
                return;
            }

            Debug.Log("OnLoadSceneEnd");

            onLoadSceneEnd?.Invoke();
            onLoadSceneEnd = () => { };
            SceneManager.sceneLoaded -= LoadSceneEnd;

            StartCoroutine(Fade(false, TimeScaleHelper.Pop));
        }

        private IEnumerator Fade(bool isFadeIn, Action onEndAction = default)
        {
            var timer = 0f;
            const float timeInterval = 0.02f;
            var waitForSecondsRt = new WaitForSecondsRealtime(timeInterval);

            while (timer <= 1f)
            {
                yield return waitForSecondsRt;
                timer += timeInterval / fadeSec;
                sceneLoaderCanvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
            }

            if (!isFadeIn)
            {
                gameObject.SetActive(false);
            }

            onEndAction?.Invoke();
        }
    }
}