using System;
using System.Collections;
using System.Linq;
using Game.Default;
using Game.Stage1.Camping.Interaction;
using Game.Stage1.Camping.Interaction.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Scene;

namespace Game.Stage1.Camping
{
    public class CampingManager : MonoBehaviour, IGamePlayable
    {
        [Serializable]
        public class TimerToastData : ToastData
        {
            public int time;
        }

        [Header("필드")] [SerializeField] private GameObject filedPanel;
        [SerializeField] private Button openMapButton;
        [SerializeField] private Button resetButton;

        [SerializeField] private CampingInteraction[] interactions;

        [Space(10)] [Header("Timer")] [SerializeField]
        private TMP_Text timerText;

        [SerializeField] private float timerSec;
        [SerializeField] private TimerToastData[] timerToastData;

        [Space(10)] [Header("지도")] [SerializeField]
        private GameObject mapPanel;

        [SerializeField] private Button mapExitButton;
        [SerializeField] private Button campingButton;

        [Header("지도 - 클리어 조건")] [SerializeField]
        private CampingDropItem[] clearDropItems;

        [Header("결과")] [SerializeField] private GameObject failPanel;

        private void Start()
        {
            mapExitButton.onClick.AddListener(() =>
            {
                mapPanel.SetActive(false);
                filedPanel.SetActive(true);
            });

            openMapButton.onClick.AddListener(() =>
            {
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
                    StopAllCoroutines();
                    SceneLoader.Instance.LoadScene("BeachScene");
                }
                else
                {
                    StopAllCoroutines();
                    failPanel.SetActive(true);
                }
            });

            foreach (var interaction in interactions)
            {
                interaction.setInteractable = isEnable =>
                {
                    foreach (var t in interactions)
                    {
                        var collider2ds = t.GetComponentsInChildren<Collider2D>(true);
                        foreach (var collider2d in collider2ds)
                        {
                            collider2d.enabled = isEnable;
                        }
                    }
                };
                interaction.ResetInteraction();
            }

            Play();
        }

        public void Play()
        {
            StartCoroutine(StartTimer());
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
                    foreach (var content in data.toastContents)
                    {
                        SceneHelper.Instance.toastManager.Enqueue(content);
                    }
                }
            }

            failPanel.SetActive(true);
        }


        private bool IsClear()
        {
            return clearDropItems.All(campingDropItem => campingDropItem.HasItem());
        }

        private void ResetGame()
        {
            foreach (var t in interactions)
            {
                t.ResetInteraction();
            }
        }
    }
}