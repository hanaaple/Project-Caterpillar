using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Util;

namespace Game.Camping
{
    public class CampingManager : MonoBehaviour
    {
        [SerializeField]
        private Button[] beachTempButton;
        
        [Header("필드")]
        [SerializeField]
        private GameObject filedPanel;
        [SerializeField]
        private Button openButton;
        [SerializeField]
        private Button retryButton;

        [SerializeField]
        private CampingInteraction[] interactions;
    
    
        [Space(10)]
        [Header("맵")]
        [SerializeField]
        private GameObject mapPanel;
        [SerializeField]
        private Button exitButton;
    
        [SerializeField]
        private Button campingButton;
        [SerializeField]
        private CampingDropItem[] clearDropItems;
    
        [SerializeField]
        private CampingHint[] hints;
    
        [SerializeField]
        private GameObject clearPanel;
        [SerializeField]
        private GameObject failPanel;
    
        void Start()
        {
            foreach (var button in beachTempButton)
            {
                button.onClick.AddListener(() =>
                {
                    SceneLoader.Instance.LoadScene("BeachGameTest");
                });
            }
            
            exitButton.onClick.AddListener(() =>
            {
                mapPanel.SetActive(false);
                filedPanel.SetActive(true);
            });
        
            openButton.onClick.AddListener(() =>
            {
                mapPanel.SetActive(true);
                filedPanel.SetActive(false);
            });
        
            retryButton.onClick.AddListener(() =>
            {
                foreach (var t in interactions)
                {
                    t.Reset();
                }
            });
        
            campingButton.onClick.AddListener(() =>
            {
                if (IsClear())
                {
                    clearPanel.SetActive(true);
                }
                else
                {
                    failPanel.SetActive(true);
                }
            });
        
            foreach (var campingHint in hints)
            {
                campingHint.SetHint(false);
            }

            for (var i = 0; i < interactions.Length; i++)
            {
                var idx = i;
                interactions[idx].onAppear += () => { hints[idx].SetHint(true); };

                interactions[idx].setInteractable += (isEnable) =>
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
                interactions[idx].Reset();
            }
        }

        private bool IsClear()
        {
            foreach (var campingDropItem in clearDropItems)
            {
                if (!campingDropItem.HasItem())
                {
                    return false;
                }
            }

            return true;
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
