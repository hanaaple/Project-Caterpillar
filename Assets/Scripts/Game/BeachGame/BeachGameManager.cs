using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Util;

namespace Game.BeachGame
{
    [Serializable]
    public enum BeachInteractType
    {
        ConchMeat, Cocktail, BeachBall, Shovel, Flag, Fragment
    }
    public class BeachGameManager : MonoBehaviour
    {
        [Serializable]
        private class BeachInteraction
        {
            public BeachInteractors[] beachInteractors;
        }
        [Serializable]
        private class BeachInteractors
        {
            public BeachInteractType beachInteractType;
            public string krName;
            
            public BeachInteractor[] beachInteractors;
            [NonSerialized] public bool isClear;
        }
        [SerializeField] private Button mainButton;
        
        [Header("Field")]
        [SerializeField] private GameObject[] backgrounds;

        [SerializeField] private BeachInteraction beachInteraction;
        
        [Header("Field UI")]
        [SerializeField] private WatchDragger watchDragger;
        [SerializeField] private Animator timerAnimator;
        [SerializeField] private Animator toastAnimator;
        [SerializeField] private TMP_Text toastText;
        
        
        [Header("Album UI")]
        [SerializeField] private Button albumButton;

        [SerializeField] private Animator albumAnimator;
        [SerializeField] private AlbumPicture[] albumPictures;


        private void Start()
        {
            mainButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("StartScene");
            });
            albumButton.onClick.AddListener(() =>
            {
                if (albumAnimator.GetBool("Open"))
                {
                    albumAnimator.SetBool("Open", false);
                    SetInteractable(true);
                }
                else
                {
                    albumAnimator.SetBool("Open", true);
                    SetInteractable(false);
                }
            });


            for (var i = 0; i < watchDragger.actions.Length; i++)
            {
                var idx = i;
                watchDragger.actions[idx] = () =>
                {
                    //Debug.Log("이전 Index: " + watchDragger.index + ", 새 Index: " + idx);

                    watchDragger.index = idx;

                    //Debug.Log("현재 Stack : " + String.Join("",
                    //new List<int>(watchDragger.pastIdxs).ConvertAll(stackIdx => stackIdx.ToString()).ToArray()));
                    SetInteractable(false);
                    StartCoroutine(ChangeBackground());
                };
            }

            GameStart();

            foreach (var beachInteractors in beachInteraction.beachInteractors)
            {
                for (var index = 0; index < beachInteractors.beachInteractors.Length; index++)
                {
                    var interactor = beachInteractors.beachInteractors[index];
                    interactor.Init();

                    switch (beachInteractors.beachInteractType)
                    {
                        case BeachInteractType.ConchMeat:
                        case BeachInteractType.Cocktail:
                        case BeachInteractType.BeachBall:
                        case BeachInteractType.Shovel:
                        case BeachInteractType.Flag:
                        {
                            interactor.onInteract += () =>
                            {
                                Debug.Log("유리 외 인터랙션");
                                beachInteractors.isClear = true;
                                interactor.gameObject.SetActive(false);
                                foreach (var t in beachInteractors.beachInteractors)
                                {
                                    t.interactable = false;
                                }

                                var albumPicture = Array.Find(albumPictures,
                                    item => item.beachInteractType == beachInteractors.beachInteractType);
                                albumPicture.SetPanel(AlbumPicture.PictureState.Clear);

                                toastText.text = $"[{beachInteractors.krName}]를 획득했습니다.";
                                toastAnimator.SetTrigger("Active");
                                
                                var isGameClear = beachInteraction.beachInteractors.All(item => item.isClear);
                                var isClearCount = beachInteraction.beachInteractors.Count(item => item.isClear);
                                Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteraction.beachInteractors.Length);
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
                            interactor.onInteract += () =>
                            {
                                Debug.Log("유리 인터랙션");
                                var albumPicture = Array.Find(albumPictures,
                                    item => item.beachInteractType == beachInteractors.beachInteractType);
                                albumPicture.SetPanel(idx);

                                interactor.gameObject.SetActive(false);
                                interactor.interactable = false;

                                var isClear = beachInteractors.beachInteractors.All(item => !item.interactable);
                                if (isClear)
                                {
                                    beachInteractors.isClear = true;
                                    
                                    toastText.text = $"[{beachInteractors.krName}]를 획득했습니다.";
                                    toastAnimator.SetTrigger("Active");
                                    
                                    var isGameClear = beachInteraction.beachInteractors.All(item => item.isClear);
                                    var isClearCount = beachInteraction.beachInteractors.Count(item => item.isClear);
                                    Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteraction.beachInteractors.Length);
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
                albumPicture.SetPanel(AlbumPicture.PictureState.Active);
            }

            timerAnimator.SetTrigger("Active");
        }

        private IEnumerator ChangeBackground()
        {
            var stackList = new List<int>(watchDragger.pastIdxs);
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
            watchDragger.pastIdxs.Clear();
            Debug.Log("끝, 현재 Stack : " + String.Join("",
                stackList.ConvertAll(stackIdx => stackIdx.ToString()).ToArray()));
            
            SetInteractable(true);
        }

        private void SetInteractable(bool isInteractable)
        {
            watchDragger.interactable = isInteractable;
            foreach (var beachInteractionBeachInteractor in beachInteraction.beachInteractors)
            {
                foreach (var beachInteractor in beachInteractionBeachInteractor.beachInteractors)
                {
                    beachInteractor.isStop = !isInteractable;
                }
            }
        }

        private Color GetColorAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}