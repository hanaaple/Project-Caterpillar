using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Default;
using UnityEngine;
using UnityEngine.UI;

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
        Fragment
    }

    public class BeachGameManager : MonoBehaviour
    {
        [Serializable]
        private class BeachInteractions
        {
            public BeachInteractType beachInteractType;
            public string krName;

            public BeachInteraction[] interactions;
            [NonSerialized] public bool IsClear;
        }

        [SerializeField] private ToastManager toastManager;
        
        [Header("Field")] [SerializeField] private GameObject[] backgrounds;
        [SerializeField] private BeachInteractions[] beachInteractions;

        [Header("Field UI")] [SerializeField] private WatchDragger watchDragger;

        [Header("Album UI")] [SerializeField] private Button albumButton;
        [SerializeField] private Animator albumAnimator;
        [SerializeField] private AlbumPicture[] albumPictures;
        
        private static readonly int OpenHash = Animator.StringToHash("Open");

        private void Start()
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

            GameStart();

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
                        {
                            interaction.onInteract += () =>
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

                                toastManager.Enqueue($"[{interactions.krName}]를 획득했습니다.");

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
                            interaction.onInteract += () =>
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

                                    toastManager.Enqueue($"[{interactions.krName}]를 획득했습니다.");

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
            watchDragger.Init();
            foreach (var albumPicture in albumPictures)
            {
                albumPicture.Init();
            }

            StartCoroutine(HintTimer());
        }

        private void GameEnd()
        {
            SetInteractable(false);

            StopAllCoroutines();

            Invoke(nameof(GameEndTrigger), 2f);
        }

        private void GameEndTrigger()
        {
            albumAnimator.SetTrigger("GameEnd");
        }

        private IEnumerator HintTimer()
        {
            var t = 0f;

            while (t <= 10f)
            {
                t += Time.deltaTime;
                yield return null;
            }

            foreach (var albumPicture in albumPictures)
            {
                albumPicture.SetPanel(PictureState.Active);
            }

            toastManager.Enqueue("앨범 활성화");
        }

        private IEnumerator ChangeBackground()
        {
            var stackList = new List<int>(watchDragger.PastIndex);
            stackList.Reverse();

            Debug.Log("시작, 현재 Stack : " + String.Join("",
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
                var timer = .5f;
                while (t <= timer)
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
            Debug.Log("끝, 현재 Stack : " + String.Join("",
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