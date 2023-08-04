using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utility.Tendency;
using Utility.UI.Util;

namespace Utility.UI.Inventory
{
    /// <summary>
    /// 현재 Tendency의 상태를 불러와 화면을 갱신하여 독립적을 사용
    /// Init()과 UpdateDisplay만 사용해주면 된다.
    /// </summary>
    public class Necklace : MonoBehaviour
    {
        [Serializable]
        private class NecklaceType
        {
            public Sprite sprite;
            public NecklaceState ascentState;
            public NecklaceState activeState;
        }

        private enum NecklaceState
        {
            Equivalent,
            Positive,
            Negative
        }

        [SerializeField] private GameObject tendencyItemPrefab;
        [SerializeField] private Image necklaceImage;
        [SerializeField] private DynamicGrid dynamicGrid;
        [SerializeField] private NecklaceType[] necklaceTypes;
        [SerializeField] private Sprite[] tendencySprites;
        [SerializeField] private Animator blinkAnimator;
        [SerializeField] private GameObject keywordPanel;

        [Header("For Debug")] [SerializeField] private List<TendencyItem> tendencyItems;
        
        private static readonly int BlinkEmptyHash = Animator.StringToHash("IsEmpty");

        public void Init()
        {
            tendencyItems = new List<TendencyItem>();
        }

        public void UpdateDisplay()
        {
            Debug.Log("Update Necklace Display");
            UpdateNecklaceImage();
            UpdateTendencyWord();
            dynamicGrid.UpdateRectSize();
        }

        public void SetKeywordActive(bool isActive)
        {
            blinkAnimator.SetBool(BlinkEmptyHash, isActive);
            keywordPanel.SetActive(isActive);
        }

        public bool IsKeywordActive()
        {
            return blinkAnimator.GetBool(BlinkEmptyHash);
        }

        private void UpdateTendencyWord()
        {
            var tendencyData = TendencyManager.Instance.GetTendencyData();

            // 새로 생긴거
            var newTendencyItems = tendencyData.tendencyItems.Except(tendencyItems.Select(item => item.tendencyType));

            // 사라진 거
            var removedTendencyItems =
                tendencyItems.Select(item => item.tendencyType).Except(tendencyData.tendencyItems);

            foreach (var tendencyType in newTendencyItems)
            {
                AddItem(tendencyType);
            }

            foreach (var tendencyType in removedTendencyItems)
            {
                RemoveItem(tendencyType);
            }
        }

        private void UpdateNecklaceImage()
        {
            const int equivalentRange = 5;

            var tendencyData = TendencyManager.Instance.GetTendencyData();

            NecklaceState ascentState;
            NecklaceState activeState;

            var active = Mathf.Abs(tendencyData.activation - tendencyData.inactive);
            if (active >= equivalentRange)
            {
                activeState = NecklaceState.Equivalent;
            }
            else if (tendencyData.activation > tendencyData.inactive)
            {
                activeState = NecklaceState.Positive;
            }
            else
            {
                activeState = NecklaceState.Negative;
            }

            var ascent = Mathf.Abs(tendencyData.ascent - tendencyData.descent);
            if (ascent >= equivalentRange)
            {
                ascentState = NecklaceState.Equivalent;
            }
            else if (tendencyData.ascent > tendencyData.descent)
            {
                ascentState = NecklaceState.Positive;
            }
            else
            {
                ascentState = NecklaceState.Negative;
            }

            necklaceImage.sprite = Array.Find(necklaceTypes,
                item => item.activeState == activeState && item.ascentState == ascentState).sprite;
        }

        private void AddItem(TendencyType tendencyType)
        {
            Debug.Log($"Add: {tendencyType}");
            var newObject = Instantiate(tendencyItemPrefab, dynamicGrid.transform);
            var tendencyItem = newObject.GetComponent<TendencyItem>();

            tendencyItem.tendencyType = tendencyType;
            tendencyItem.text.text = tendencyType.ToString();
            var tendencyProps = TendencyManager.Instance.GetTendencyType(tendencyType);


            // 1- 454545 (R: 69 G: 69 B: 69), text - 949494 (R: 148 G: 148 B: 148)
            // 2- 353535 (R: 53 G: 53 B: 53), text - 949494 (R: 148 G: 148 B: 148)
            // 3- bfbfbf (R: 191 G: 191 B: 191), text - 353535 (R: 53 G: 53 B: 53)
            // 4- 949494 (R: 148 G: 148 B: 148), text - 353535 (R: 53 G: 53 B: 53)
            if (tendencyProps.ascent > 0)
            {
                const float tc = 148 / 255f;

                if (tendencyProps.activation > 0)
                {
                    // 1사분면
                    // const float c = 69 / 255f;
                    // tendencyItem.text.color = new Color(c, c, c);
                    tendencyItem.image.sprite = tendencySprites[0];
                }
                else if (tendencyProps.inactive > 0)
                {
                    // 2사분면
                    // const float c = 53 / 255f;
                    // tendencyItem.text.color = new Color(c, c, c);
                    tendencyItem.image.sprite = tendencySprites[1];
                }

                tendencyItem.text.color = new Color(tc, tc, tc);
            }
            else if (tendencyProps.descent > 0)
            {
                const float tc = 53 / 255f;

                if (tendencyProps.activation > 0)
                {
                    // 4사분면
                    // const float c = 148 / 255f;
                    // tendencyItem.text.color = new Color(c, c, c);
                    tendencyItem.image.sprite = tendencySprites[3];
                }
                else if (tendencyProps.inactive > 0)
                {
                    // 3사분면
                    // const float c = 191 / 255f;
                    // tendencyItem.text.color = new Color(c, c, c);
                    tendencyItem.image.sprite = tendencySprites[2];
                }
                tendencyItem.text.color = new Color(tc, tc, tc);
            }

            tendencyItems.Add(tendencyItem);
        }

        private void RemoveItem(TendencyType tendencyType)
        {
            Debug.Log($"Remove: {tendencyType}");
            var tendencyItem = tendencyItems.Find(item => item.tendencyType == tendencyType);
            tendencyItems.Remove(tendencyItem);
            Destroy(tendencyItem.gameObject);
        }
    }
}