using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Default;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.Scene;

namespace Game.Stage1.BeachGame
{
    [Serializable]
    public enum BeachInteractType
    {
        ConchMeat,
        Cocktail,
        BeachBall,
        Shovel,
        Flag,
        Fragment,
        Pearl,
        Dolphin
    }

    public class BeachGameManager : MonoBehaviour
    {
        [Serializable]
        private class BeachInteractions
        {
            public BeachInteractType beachInteractType;

            public BeachInteraction[] interactions;
            [NonSerialized] public bool IsClear;
        }
        
        [Header("Field")] [SerializeField] private GameObject[] backgrounds;
        [SerializeField] private BeachInteractions[] beachInteractions;

        [Header("Field UI")] [SerializeField] private WatchDragger watchDragger;

        [Header("Album UI")] [SerializeField] private Button albumButton;
        [SerializeField] private Animator albumAnimator;
        [SerializeField] private AlbumPicture[] albumPictures;

        [SerializeField] private ToastData endToastData;
        
        private InputActions _inputActions;
        
        private static readonly int OpenHash = Animator.StringToHash("Open");

        private void Start()
        {
            Initialize();

            GameStart();
        }

        private void Initialize()
        {
            albumButton.onClick.AddListener(() =>
            {
                if (albumAnimator.GetBool(OpenHash))
                {
                    albumAnimator.SetBool(OpenHash, false);
                    SetInteractable(true);
                }
                else
                {
                    albumAnimator.SetBool(OpenHash, true);
                    SetInteractable(false);
                }
            });

            foreach (var albumPicture in albumPictures)
            {
                albumPicture.Init();
            }

            for (var i = 0; i < watchDragger.actions.Length; i++)
            {
                var idx = i;
                watchDragger.actions[idx] = () =>
                {
                    //Debug.Log("이전 Index: " + watchDragger.index + ", 새 Index: " + idx);

                    watchDragger.Index = idx;

                    //Debug.Log("현재 Stack : " + String.Join("",
                    //new List<int>(watchDragger.pastIdxs).ConvertAll(stackIdx => stackIdx.ToString()).ToArray()));
                    SetInteractable(false);
                    StartCoroutine(ChangeBackground());
                };
            }
            
            _inputActions = new InputActions("BeachGameManager")
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause(); }
            };
            
            foreach (var interactions in beachInteractions)
            {
                for (var index = 0; index < interactions.interactions.Length; index++)
                {
                    var interaction = interactions.interactions[index];
                    interaction.Init();

                    switch (interactions.beachInteractType)
                    {
                        case BeachInteractType.ConchMeat:
                        case BeachInteractType.Cocktail:
                        case BeachInteractType.BeachBall:
                        case BeachInteractType.Shovel:
                        case BeachInteractType.Flag:
                        case BeachInteractType.Pearl:
                        case BeachInteractType.Dolphin:
                        {
                            interaction.onInteract = () =>
                            {
                                Debug.Log("유리 외 인터랙션");
                                interactions.IsClear = true;
                                interaction.gameObject.SetActive(false);
                                foreach (var t in interactions.interactions)
                                {
                                    t.Interactable = false;
                                }

                                var albumPicture = Array.Find(albumPictures,
                                    item => item.beachInteractType == interactions.beachInteractType);
                                albumPicture.SetPanel(PictureState.Clear);

                                // SceneHelper.Instance.toastManager.Enqueue($"[{interactions.krName}]를 획득했습니다.");

                                var isGameClear = beachInteractions.All(item => item.IsClear);
                                var isClearCount = beachInteractions.Count(item => item.IsClear);
                                Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteractions.Length);
                                if (isGameClear)
                                {
                                    GameEnd();
                                }
                            };
                            break;
                        }
                        case BeachInteractType.Fragment:
                        {
                            var idx = index;
                            interaction.onInteract = () =>
                            {
                                Debug.Log("유리 인터랙션");
                                var albumPicture = Array.Find(albumPictures,
                                    item => item.beachInteractType == interactions.beachInteractType);
                                albumPicture.SetPanel(idx);

                                interaction.gameObject.SetActive(false);
                                interaction.Interactable = false;

                                var isClear = interactions.interactions.All(item => !item.Interactable);
                                if (isClear)
                                {
                                    interactions.IsClear = true;

                                    var isGameClear = beachInteractions.All(item => item.IsClear);
                                    var isClearCount = beachInteractions.Count(item => item.IsClear);
                                    Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteractions.Length);
                                    if (isGameClear)
                                    {
                                        GameEnd();
                                    }
                                }
                            };
                            break;
                        }
                        default:
                        {
                            Debug.LogError("오류");
                            break;
                        }
                    }
                }
            }
        }

        private void GameStart()
        {
            InputManager.PushInputAction(_inputActions);
            albumButton.gameObject.SetActive(true);
            watchDragger.Reseet();
            foreach (var albumPicture in albumPictures)
            {
                albumPicture.Reeset();
            }

            StartCoroutine(HintTimer());
        }

        private void GameEnd()
        {
            InputManager.PopInputAction(_inputActions);
            SetInteractable(false);

            StopAllCoroutines();

            if (!endToastData.IsToasted)
            {
                foreach (var content in endToastData.toastContents)
                {
                    SceneHelper.Instance.toastManager.Enqueue(content);
                }

                endToastData.IsToasted = true;
            }
            
            albumButton.gameObject.SetActive(false);

            SceneHelper.Instance.toastManager.onToastEnd = () =>
            {
                albumAnimator.SetTrigger("GameEnd");
                StartCoroutine(GameEndCoroutine());
            };
        }

        private IEnumerator GameEndCoroutine()
        {
            yield return new WaitUntil(() => albumAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));
            SceneLoader.Instance.LoadScene("SnowMountainScene");
        }

        private IEnumerator HintTimer()
        {
            yield return new WaitForSeconds(10f);

            foreach (var albumPicture in albumPictures)
            {
                albumPicture.SetPanel(PictureState.Active);
            }

            SceneHelper.Instance.toastManager.Enqueue("···? 앨범이 변화한 거 같아.");
        }

        private IEnumerator ChangeBackground()
        {
            var stackList = new List<int>(watchDragger.PastIndex);
            stackList.Reverse();

            Debug.Log("시작, 현재 Stack : " + string.Join("",
                stackList.ConvertAll(stackIdx => stackIdx.ToString()).ToArray()));

            for (var index = 0; index < stackList.Count - 1; index++)
            {
                var curIdx = stackList[index];
                var nextIdx = stackList[index + 1];

                var curSpriteRenderer = backgrounds[curIdx].GetComponent<SpriteRenderer>();
                var nextSpriteRenderer = backgrounds[nextIdx].GetComponent<SpriteRenderer>();
                nextSpriteRenderer.sortingOrder = 3;

                backgrounds[nextIdx].SetActive(true);

                var t = 0f;
                const float timer = .5f;
                while (t <= 1f)
                {
                    t += Time.deltaTime / timer;
                    curSpriteRenderer.color = GetColorAlpha(curSpriteRenderer.color, 1 - t);
                    nextSpriteRenderer.color = GetColorAlpha(nextSpriteRenderer.color, t);

                    yield return null;
                }

                nextSpriteRenderer.sortingOrder = 2;
                backgrounds[curIdx].SetActive(false);
            }

            watchDragger.PastIndex.Clear();
            Debug.Log("끝, 현재 Stack : " + string.Join("",
                stackList.ConvertAll(stackIdx => stackIdx.ToString()).ToArray()));

            SetInteractable(true);
        }

        private void SetInteractable(bool isInteractable)
        {
            watchDragger.Interactable = isInteractable;
            foreach (var interactions in beachInteractions)
            {
                foreach (var beachInteraction in interactions.interactions)
                {
                    beachInteraction.IsStop = !isInteractable;
                }
            }
        }

        private static Color GetColorAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}