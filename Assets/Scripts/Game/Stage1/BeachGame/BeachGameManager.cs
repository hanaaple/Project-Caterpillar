using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Default;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.Scene;
using Utility.Tutorial;

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

    public class BeachGameManager : MonoBehaviour, IGamePlayable
    {
        [Serializable]
        private class BeachInteractionList
        {
            public BeachInteractType beachInteractType;

            public BeachInteraction[] interactions;
            
            public AudioData audioData;
            [NonSerialized] public bool IsClear;
        }
        
        [Header("Tutorial")] [SerializeField] private TutorialHelper tutorialHelper;

        [Header("Audio")] [SerializeField] private AudioData bgmAudioData;
        [SerializeField] private AudioData glassAudioData;
        [SerializeField] private AudioData collectAudioData;
        [SerializeField] private AudioData albumCloseAudioData;
        
        [Header("Field")] [SerializeField] private GameObject[] backgrounds;
        [FormerlySerializedAs("beachInteractions")] [SerializeField] private BeachInteractionList[] beachInteractionList;
        [SerializeField] private int glassAudioMapIndex;

        [Header("Field UI")] [SerializeField] private WatchDragger watchDragger;

        [Header("Album UI")] [SerializeField] private Button albumButton;
        [SerializeField] private Animator albumAnimator;
        [SerializeField] private AlbumPicture[] albumPictures;

        [SerializeField] private ToastData endToastData;
        
        private InputActions _inputActions;
        private bool _isGlassPlayed;
        
        private static readonly int OpenHash = Animator.StringToHash("Open");
        private static readonly int EndHash = Animator.StringToHash("GameEnd");

        private void Start()
        {
            Initialize();
            Play();
        }

        public void Play()
        {
            PlayUIManager.Instance.tutorialManager.StartTutorial(tutorialHelper, GameStart);
        }

        private void Initialize()
        {
            albumButton.onClick.AddListener(() =>
            {
                if (albumAnimator.GetBool(OpenHash))
                {
                    albumCloseAudioData.Play();
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
            
            foreach (var beachInteraction in beachInteractionList)
            {
                for (var index = 0; index < beachInteraction.interactions.Length; index++)
                {
                    var interaction = beachInteraction.interactions[index];
                    interaction.Init();

                    interaction.onInteract += () =>
                    {
                        if (beachInteraction.audioData.audioObject)
                        {
                            beachInteraction.audioData.Play();    
                        }
                        else
                        {
                            collectAudioData.Play();
                        }
                    };

                    switch (beachInteraction.beachInteractType)
                    {
                        case BeachInteractType.ConchMeat:
                        case BeachInteractType.Cocktail:
                        case BeachInteractType.BeachBall:
                        case BeachInteractType.Shovel:
                        case BeachInteractType.Flag:
                        case BeachInteractType.Pearl:
                        case BeachInteractType.Dolphin:
                        {
                            interaction.onInteract += () =>
                            {
                                Debug.Log("유리 외 인터랙션");
                                beachInteraction.IsClear = true;
                                interaction.gameObject.SetActive(false);
                                foreach (var t in beachInteraction.interactions)
                                {
                                    t.IsInteractable = false;
                                }

                                var albumPicture = Array.Find(albumPictures,
                                    item => item.beachInteractType == beachInteraction.beachInteractType);
                                albumPicture.SetPanel(PictureState.Clear);

                                // SceneHelper.Instance.toastManager.Enqueue($"[{interactions.krName}]를 획득했습니다.");

                                var isGameClear = beachInteractionList.All(item => item.IsClear);
                                var isClearCount = beachInteractionList.Count(item => item.IsClear);
                                Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteractionList.Length);
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
                                    item => item.beachInteractType == beachInteraction.beachInteractType);
                                albumPicture.SetPanel(idx);

                                interaction.gameObject.SetActive(false);
                                interaction.IsInteractable = false;

                                var isClear = beachInteraction.interactions.All(item => !item.IsInteractable);
                                if (isClear)
                                {
                                    beachInteraction.IsClear = true;

                                    var isGameClear = beachInteractionList.All(item => item.IsClear);
                                    var isClearCount = beachInteractionList.Count(item => item.IsClear);
                                    Debug.Log("클리어 현황 : " + isClearCount + " / " + beachInteractionList.Length);
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
            watchDragger.Init();
            foreach (var albumPicture in albumPictures)
            {
                albumPicture.Reeset();
            }
            
            bgmAudioData.Play();

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
                albumAnimator.SetTrigger(EndHash);
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

                var curSpriteRenderers = backgrounds[curIdx].GetComponentsInChildren<SpriteRenderer>();
                var nextSpriteRenderers = backgrounds[nextIdx].GetComponentsInChildren<SpriteRenderer>();
                var nextSpriteRenderer = backgrounds[nextIdx].GetComponent<SpriteRenderer>();
                nextSpriteRenderer.sortingOrder = 3;

                backgrounds[nextIdx].SetActive(true);
                if (nextIdx == glassAudioMapIndex && !_isGlassPlayed)
                {
                    glassAudioData.Play();
                    _isGlassPlayed = true;
                }

                var t = 0f;
                const float timer = .5f;
                while (t <= 1f)
                {
                    t += Time.deltaTime / timer;
                    foreach (var curSpriteRenderer in curSpriteRenderers)
                    {
                        SetColorAlpha(curSpriteRenderer, 1 - t);
                    }

                    foreach (var spriteRenderer in nextSpriteRenderers)
                    {
                        SetColorAlpha(spriteRenderer, t);
                    }

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
            foreach (var interactions in beachInteractionList)
            {
                foreach (var beachInteraction in interactions.interactions)
                {
                    beachInteraction.IsStop = !isInteractable;
                }
            }
        }

        private static void SetColorAlpha(SpriteRenderer spriteRenderer, float alpha)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}