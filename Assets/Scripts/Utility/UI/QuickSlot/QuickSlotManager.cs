using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Tendency;
using Utility.UI.Util;

namespace Utility.UI.QuickSlot
{
    public class QuickSlotManager : MonoBehaviour
    {
        [SerializeField] private GameObject quickSlotPanel;
        [SerializeField] private Animator animator;
        [SerializeField] private DynamicGrid dynamicGrid;
        [SerializeField] private GameObject tendencyItemPrefab;

        [Header("For Debug")] [SerializeField] private List<QuickSlotTendencyItem> tendencyItems;

        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

        private void Awake()
        {
            tendencyItems = new List<QuickSlotTendencyItem>();
        }

        public void SetQuickSlot(bool isActive)
        {
            if (isActive)
            {
                UpdateTendencyWord();
                dynamicGrid.UpdateRectSize();
            }

            animator.SetBool(IsOpenHash, isActive);
        }

        public void SetEnable(bool isEnable)
        {
            quickSlotPanel.gameObject.SetActive(isEnable);
        }

        public bool IsActive()
        {
            return animator.GetBool(IsOpenHash);
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

        private void AddItem(TendencyType tendencyType)
        {
            Debug.Log($"Add: {tendencyType}");
            var newObject = Instantiate(tendencyItemPrefab, dynamicGrid.transform);
            var tendencyItem = newObject.GetComponent<QuickSlotTendencyItem>();

            tendencyItem.tendencyType = tendencyType;
            tendencyItem.text.text = tendencyType.ToString();
            tendencyItem.indexText.text = $"{tendencyItem.transform.GetSiblingIndex() + 1:00}";

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