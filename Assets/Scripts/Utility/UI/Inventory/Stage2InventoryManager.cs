using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.UI.Highlight;

namespace Utility.UI.Inventory
{
    public class Stage2InventoryManager : MonoBehaviour, IInventory
    {
        [SerializeField] private GameObject uiPanel;

        [SerializeField] private Animator itemListAnimator;

        [SerializeField] private SelectHighlightItem[] inventoryItemList;
        [SerializeField] private Transform[] inventoryItemBoxList;

        private Highlighter _itemListHighlighter;

        private int _selectedIndex;

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
                highlightType = Highlighter.HighlightType.ClickIsSelect
            };

            foreach (var highlightItem in inventoryItemList)
            {
                highlightItem.Init(highlightItem.button.GetComponentInChildren<Animator>(true));
            }

            _itemListHighlighter.isKeepHighlightState = true;
            _itemListHighlighter.Init(Highlighter.ArrowType.Horizontal);
            
            
            // 클릭시 소리가 나는게 아니라
            // Select시 소리가 나도록
            
            
            
            // 1. 마우스를 올리거나 클릭하면 원본과 동일한 기능을 해야한다.
            // 2. Arrow를 하는 경우에도 원본과 동일한 기능을 해야한다.
            
            // Temp와 원본이 바뀌는 조건 - 원본이 완전히 화면에서 나간다.
            
            // Temp를 눌렀을때는 원본을 누른 것과 동일하면서 원본을 향해 움직이면 안된다.
            
            // 아 그냥 텔레포트 시켜
            
            // Temp이면서 동시에 클릭하면 "가능한"
            
            // this Awake() work faster than SceneHelper Awake()?
            _itemListHighlighter.onSelect = () =>
            {
                // 안움직이다가 해당 Index로 이동
                // 움직이다가 멈추고 해당 Index로 이동
                
                // Init
                // _selectedIndex = _itemListHighlighter.selectedIndex;
                
                // onSelect 도중에 들어온다면
                // 움직이는 도중에도 다른 Index로 누르면 (입력하면) 가능해야한다.
                // StartCoroutine(SelectItem());
                SelectItemImmediately();
            };
            

            _itemListHighlighter.InputActions.OnInventory = () => { SetInventory(false); };
        }

        /// <summary>
        /// Open Or Close Item List
        /// Usage in Stage2 
        /// </summary>
        /// <param name="isActive"></param>
        public void SetInventory(bool isActive)
        {
            Debug.Log($"Set Item List {isActive}");
            if (!isActive)
            {
                itemListAnimator.SetInteger(IndexHash, _itemListHighlighter.selectedIndex);
            }

            itemListAnimator.SetBool(IsOpenHash, isActive);
            if (isActive)
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

            if (isEnable)
            {
                Debug.Log($"Set Enable Inventory,  {_itemListHighlighter != null}");
                _itemListHighlighter?.Select(0);
            }
        }

        private IEnumerator SelectItem()
        {
            var deltaIndex = _itemListHighlighter.selectedIndex - _selectedIndex;
            // deltaIndex * 이동 거리만큼
                
            foreach (var highlightItem in _itemListHighlighter.HighlightItems)
            {
                // distance = space + button_width  
                // var speed = deltaIndex * distance * Time.deltaTime;

                var t = 1f;
                while (t > 0)
                {
                    // highlightItem.button.transform.position += speed;
                    t -= Time.deltaTime;
                    yield return null;
                }
                
                // 
                
                // Mask 왼쪽 좌표, 오른쪽 좌표
                    
                // 전부 이동 후, 이동 방향에 있던 것은 이동
                // 각 Temp는 각 위치로 이동, (이미지는 해당 Index의 이미지로 변경)
            }
                
            _selectedIndex = _itemListHighlighter.selectedIndex;
        }

        private void SelectItemImmediately()
        {
            var half = _itemListHighlighter.HighlightItems.Count / 2;

            int dif;
                
            // 한번 돌았다고 판단 (ex - 6 -> 0, 1)
            if (Mathf.Abs(_selectedIndex - _itemListHighlighter.selectedIndex) > half)
            {
                // 오른쪽
                if (_selectedIndex > _itemListHighlighter.selectedIndex)
                {
                    dif = _itemListHighlighter.selectedIndex + _itemListHighlighter.HighlightItems.Count -
                          _selectedIndex;
                }
                // 왼쪽
                else
                {
                    dif = _itemListHighlighter.selectedIndex - _itemListHighlighter.HighlightItems.Count -
                          _selectedIndex;
                }
            }
            else
            {
                dif = _itemListHighlighter.selectedIndex - _selectedIndex;
            }
            
            Debug.Log($"{_selectedIndex} -> {_itemListHighlighter.selectedIndex}, diff: {dif}");
                
            for (var index = 0; index < _itemListHighlighter.HighlightItems.Count; index++)
            {
                var highlightItem = _itemListHighlighter.HighlightItems[index].button;
                var nextIndex = (index - dif -_selectedIndex + 2 * _itemListHighlighter.HighlightItems.Count) % _itemListHighlighter.HighlightItems.Count;
                highlightItem.transform.SetParent(inventoryItemBoxList[nextIndex]);
                ((RectTransform)highlightItem.transform).anchoredPosition = Vector2.zero;
            }

            _selectedIndex = _itemListHighlighter.selectedIndex;
        }
    }
}