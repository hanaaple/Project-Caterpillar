using System;
using System.Collections;
using System.Linq;
using Game.Default;
using Game.Stage1.Camping.Interaction;
using Game.Stage1.Camping.Interaction.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.Tutorial;
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

        [Header("Audio")] [SerializeField] private AudioClip bgm;
        
        [Header("필드")] [SerializeField] private GameObject filedPanel;
        [SerializeField] private Button openMapButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private CampingInteraction[] interactions;

        [SerializeField] private ToastData wrongToastData;

        [Space(10)] [Header("Timer")] [SerializeField]
        private TMP_Text timerText;

        [SerializeField] private float timerSec;
        [SerializeField] private TimerToastData[] timerToastData;

        [Space(10)] [Header("지도")] [SerializeField]
        private GameObject mapPanel;

        [SerializeField] private Button mapExitButton;
        [SerializeField] private Button campingButton;
        
        [SerializeField] private AudioClip openAudioClip;
        [SerializeField] private AudioClip closeAudioClip;

        [Header("지도 - 클리어 조건")] [SerializeField]
        private CampingDropItem[] clearDropItems;

        [SerializeField] private CampingDropItem[] dropItems;

        [Header("결과")] [SerializeField] private GameObject failPanel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button giveUpButton;

        private InputActions _inputActions;
        
        private void Start()
        {
            mapExitButton.onClick.AddListener(() =>
            {
                SetInteractable(true);
                mapPanel.SetActive(false);
                filedPanel.SetActive(true);
            });

            openMapButton.onClick.AddListener(() =>
            {
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
                    var index = Random.Range(0, wrongToastData.toastContents.Length);
                    SceneHelper.Instance.toastManager.Enqueue(wrongToastData.toastContents[index]);
                }
            });

            retryButton.onClick.AddListener(ResetGame);

            giveUpButton.onClick.AddListener(() =>
            {
                SaveHelper.SetNpcData(NpcType.Photographer, NpcState.Fail);
                SceneLoader.Instance.LoadScene("MainScene");
            });


            foreach (var interaction in interactions)
            {
                interaction.setInteractable = SetInteractable;
                interaction.ResetInteraction(true);
            }
            
            _inputActions = new InputActions("ShadowGameManager")
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
                AudioManager.Instance.PlayBgmWithFade(bgm);
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
            failPanel.SetActive(true);
            mapPanel.SetActive(false);
            filedPanel.SetActive(true);
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

            failPanel.SetActive(false);
            foreach (var t in interactions)
            {
                t.ResetInteraction(true);
            }

            Play();
        }
    }
}