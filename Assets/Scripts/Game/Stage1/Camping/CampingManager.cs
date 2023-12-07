using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Default;
using Game.Stage1.Camping.Interaction;
using Game.Stage1.Camping.Interaction.Map;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.Tutorial;
using Utility.UI.Check;
using Utility.UI.Highlight;
using Random = UnityEngine.Random;

namespace Game.Stage1.Camping
{
    public class CampingManager : MonoBehaviour, IGamePlayable
    {
        [Serializable]
        public class TimerToastData : ToastData
        {
            public int time;
            public Color color;
        }

        [Header("Tutorial")] [SerializeField] private TutorialHelper tutorialHelper;

        [Header("Audio")] [SerializeField] private AudioData bgmAudioData;
        [SerializeField] private AudioData mapOpenAudioData;
        [SerializeField] private AudioData mapCloseAudioData;

        [Header("필드")] [SerializeField] private GameObject filedPanel;
        [SerializeField] private Button openMapButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private CampingInteraction[] interactions;

        [SerializeField] private ToastData wrongToastData;

        [Space(10)] [Header("Timer")] [SerializeField]
        private TMP_Text timerText;

        [SerializeField] private float timerSec;
        [SerializeField] private TimerToastData[] timerToastData;

        [Space(10)] [Header("지도 UI")] [SerializeField]
        private GameObject mapPanel;

        [SerializeField] private Button mapExitButton;
        [SerializeField] private Button campingButton;
        [SerializeField] private Animator failAnimator;

        [Header("지도 - 클리어 조건")] [SerializeField]
        private CampingDropItem[] clearDropItems;

        [SerializeField] private CampingDropItem[] dropItems;

        [Header("GameOver")]
        [FormerlySerializedAs("failPanel")] [SerializeField]
        private GameObject gameOverPanel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button giveUpButton;
        [SerializeField] private SelectHighlightItem[] gameOverHighlightItems;
        [SerializeField] private CheckUIManager checkUIManager;

        private InputActions _inputActions;
        private Highlighter _gameOverHighlighter;

        private static readonly int FailHash = Animator.StringToHash("Fail");

        private void Start()
        {
            mapExitButton.onClick.AddListener(() =>
            {
                SetInteractable(true);
                mapPanel.SetActive(false);
                filedPanel.SetActive(true);
                mapCloseAudioData.Play();
            });

            openMapButton.onClick.AddListener(() =>
            {
                mapOpenAudioData.Play();
                SetInteractable(false);
                mapPanel.SetActive(true);
                filedPanel.SetActive(false);
            });

            resetButton.onClick.AddListener(() =>
            {
                foreach (var t in interactions)
                {
                    t.ResetInteraction();
                }
            });

            campingButton.onClick.AddListener(() =>
            {
                if (IsClear())
                {
                    InputManager.PopInputAction(_inputActions);
                    StopAllCoroutines();
                    SceneLoader.Instance.LoadScene("BeachScene");
                }
                else
                {
                    failAnimator.SetTrigger(FailHash);
                    var index = Random.Range(0, wrongToastData.toastContents.Length);
                    SceneHelper.Instance.toastManager.Enqueue(wrongToastData.toastContents[index]);
                }
            });

            checkUIManager.Initialize();
            checkUIManager.SetText("이야기를 포기할 경우, 재 진행이 어렵습니다.\n이 기억의 이야기를 포기 하시겠습니까?");
            
            checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
            {
                checkUIManager.Pop();
                HighlightHelper.Instance.Pop(_gameOverHighlighter);
                SaveHelper.SetNpcData(NpcType.Photographer, NpcState.Fail);
                SceneLoader.Instance.LoadScene("MainScene");
            });
            
            checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () =>
            {
                checkUIManager.Pop();
            });
            
            retryButton.onClick.AddListener(ResetGame);

            giveUpButton.onClick.AddListener(() =>
            {
                checkUIManager.Push();
            });

            _gameOverHighlighter = new Highlighter("GameOver Highlight")
            {
                HighlightItems = new List<HighlightItem>(gameOverHighlightItems),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _gameOverHighlighter.onPush = () => { _gameOverHighlighter.Select(0); };

            foreach (var highlightItem in gameOverHighlightItems)
            {
                highlightItem.Init(highlightItem.button.GetComponentInChildren<Animator>(true));
            }
            
            _gameOverHighlighter.Init(Highlighter.ArrowType.Horizontal);

            foreach (var interaction in interactions)
            {
                interaction.setInteractable = SetInteractable;
                interaction.ResetInteraction(true);
            }

            _inputActions = new InputActions("CampingGameManager")
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause(); }
            };

            Play();
        }

        public void Play()
        {
            PlayUIManager.Instance.tutorialManager.StartTutorial(tutorialHelper, () =>
            {
                InputManager.PushInputAction(_inputActions);
                StartCoroutine(StartTimer());
                bgmAudioData.Play();
            });
        }

        private IEnumerator StartTimer()
        {
            timerText.text = $"{timerSec / 60: #0}:{timerSec % 60:00}";
            var t = timerSec;
            while (t > 0)
            {
                timerText.text = $"{Mathf.Floor(t / 60): #0}:{Mathf.Floor(t % 60):00}";
                yield return null;
                t -= Time.deltaTime;

                var t1 = t;
                var toastData = timerToastData.Where(item => !item.IsToasted && item.time > t1).ToArray();
                foreach (var data in toastData)
                {
                    data.IsToasted = true;
                    timerText.color = data.color;
                    foreach (var content in data.toastContents)
                    {
                        SceneHelper.Instance.toastManager.Enqueue(content);
                    }
                }
            }

            GameOver();
        }

        private void SetInteractable(bool isInteractable)
        {
            foreach (var t in interactions)
            {
                var collider2ds = t.GetComponentsInChildren<Collider2D>(true);
                foreach (var collider2d in collider2ds)
                {
                    collider2d.enabled = isInteractable;
                }
            }
        }

        private void GameOver()
        {
            InputManager.PopInputAction(_inputActions);
            StopAllCoroutines();
            gameOverPanel.SetActive(true);
            mapPanel.SetActive(false);
            filedPanel.SetActive(true);
        }
        
        /// <summary>
        /// Animator Event
        /// </summary>
        public void GameOverPush()
        {
            HighlightHelper.Instance.Push(_gameOverHighlighter);
        }

        private bool IsClear()
        {
            return clearDropItems.All(campingDropItem => campingDropItem.HasItem());
        }

        private void ResetGame()
        {
            foreach (var dragItem in dropItems)
            {
                dragItem.ResetItem();
            }

            gameOverPanel.SetActive(false);
            HighlightHelper.Instance.Pop(_gameOverHighlighter);
            foreach (var t in interactions)
            {
                t.ResetInteraction(true);
            }

            Play();
        }
    }
}