using System.Linq;
using Game.Stage1.Camping.Interaction;
using Game.Stage1.Camping.Interaction.Map;
using UnityEngine;
using UnityEngine.UI;
using Utility.Scene;

namespace Game.Stage1.Camping
{
    public class CampingManager : MonoBehaviour
    {
        [Header("필드")] [SerializeField] private GameObject filedPanel;
        [SerializeField] private Button openMapButton;
        [SerializeField] private Button resetButton;

        [SerializeField] private CampingInteraction[] interactions;


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
                    SceneLoader.Instance.LoadScene("BeachScene");
                }
                else
                {
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
        }

        private bool IsClear()
        {
            return clearDropItems.All(campingDropItem => campingDropItem.HasItem());
        }

        // private void OnDrawGizmos()
        // {
        //     if (Application.isEditor)
        //     {
        //         foreach (var campingDropItem in clearDropItems)
        //         {
        //             Gizmos.color = Color.red;
        //             RectTransform rectTr = canvas.transform as RectTransform;
        //             Matrix4x4 canvasMatrix = rectTr.localToWorldMatrix;
        //             // canvasMatrix *= Matrix4x4.Translate(-rectTr.sizeDelta / 2);
        //             Gizmos.matrix = canvasMatrix;
        //             Gizmos.DrawSphere(campingDropItem.GetComponent<RectTransform>().anchoredPosition, 50);
        //         }
        //     }
        // }
    }
}