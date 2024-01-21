using System.Collections.Generic;
using UnityEngine;
using Utility.UI.Highlight;

namespace Utility.UI.Inventory
{
    public class Stage2InventoryManager : MonoBehaviour
    {
        [SerializeField] private GameObject uiPanel;

        [SerializeField] private Animator itemListAnimator;

        [SerializeField] private SelectHighlightItem[] inventoryItemList;

        private Highlighter _itemListHighlighter;

        private static readonly int IsOpenHash = Animator.StringToHash("IsOpened");
        private static readonly int IndexHash = Animator.StringToHash("Index");

        // 임시로 X(Inventory Key)로 Item List 여닫기 기능

        // 소리 (Select, Highlight, 여닫기)
        // 마우스 올렸을때 (열린, 닫힌 상태 전부)
        // 닫힌 상태에서 클릭했을때 - 열기?
        // 열린 상태에서 클릭했을때 - Highlight is Select?, Select 변경?
        
        private void Awake()
        {
            _itemListHighlighter = new Highlighter("Inventory Item Highlight")
            {
                HighlightItems = new List<HighlightItem>(inventoryItemList),
                name = "아이템 리스트",
                
            };

            foreach (var highlightItem in inventoryItemList)
            {
                highlightItem.Init(highlightItem.button.GetComponentInChildren<Animator>(true));
            }

            _itemListHighlighter.isKeepHighlightState = true;
            _itemListHighlighter.Init(Highlighter.ArrowType.Horizontal);

            _itemListHighlighter.InputActions.OnInventory = () => { SetItemList(false); };
        }

        /// <summary>
        /// Open Or Close Item List
        /// Usage in Stage2 
        /// </summary>
        /// <param name="isOpen"></param>
        public void SetItemList(bool isOpen)
        {
            Debug.Log($"Set Item List {isOpen}");
            if (!isOpen)
            {
                itemListAnimator.SetInteger(IndexHash, _itemListHighlighter.selectedIndex);
            }

            itemListAnimator.SetBool(IsOpenHash, isOpen);
            if (isOpen)
            {
                HighlightHelper.Instance.Push(_itemListHighlighter);
            }
            else
            {
                HighlightHelper.Instance.Pop(_itemListHighlighter);
            }
        }

        public void SetEnable(bool isEnable)
        {
            uiPanel.SetActive(isEnable);
            _itemListHighlighter.Select(0);
        }
    }
}