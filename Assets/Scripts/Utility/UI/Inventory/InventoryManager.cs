using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.Core;
using Utility.UI.Highlight;

namespace Utility.UI.Inventory
{
    [Serializable]
    public class InventoryMenuItem : HighlightItem
    {
        public enum InventoryMenuType
        {
            Bag = 0,
            Necklace = 1,
            Exit = 2
        }

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;

        public InventoryMenuType inventoryMenuType;

        public Action onSelect;

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
        }

        public override void EnterHighlightDisplay()
        {
            if (!TransitionTypes.Contains(TransitionType.Select))
            {
                button.image.sprite = defaultSprite;
            }
        }

        public override void SelectDisplay()
        {
            button.image.sprite = selectSprite;
        }

        public override void Select()
        {
            onSelect?.Invoke();
        }
    }

    [Serializable]
    public class InventoryItem : HighlightItem
    {
        public ItemManager.ItemType itemType;
        public GameObject itemPanel;

        public Action onPointerEnter;
        public Action onPointerExit;
        public Action onPointSelect;
        public Action onPointDeSelect;

        public void SetActive(bool isActive)
        {
            isEnable = isActive;
            button.gameObject.SetActive(isActive);
        }

        public override void Pop(TransitionType transitionType)
        {
            if (TransitionTypes.Contains(transitionType))
            {
                TransitionTypes.Remove(transitionType);
                if (transitionType == TransitionType.Highlight)
                {
                    onPointerExit?.Invoke();
                }
                else if (transitionType == TransitionType.Select)
                {
                    onPointDeSelect?.Invoke();
                }
            }
            else
            {
                Debug.Log(transitionType + "없음");
            }

            UpdateDisplay();
        }

        public override void SetDefault()
        {
        }

        public override void EnterHighlightDisplay()
        {
            onPointerEnter?.Invoke();
        }

        public override void SelectDisplay()
        {
            onPointSelect?.Invoke();
        }

        public override void Reset()
        {
            base.Reset();
            onPointDeSelect?.Invoke();
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private Button inventoryButton;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private InventoryMenuItem[] inventoryMenuItems;
        [SerializeField] private Image[] highlights;

        [Header("Bag")] [SerializeField] private GameObject bagPanel;
        [SerializeField] private InventoryItem[] inventoryItems;

        [Header("Necklace")] [SerializeField] private GameObject necklacePanel;
        [SerializeField] private Necklace necklace;

        private Transform _highlightParent;
        private Highlighter _menuHighlighter;
        private Highlighter _itemHighlighter;
        private int _selectedItemIdx;
        private Action<InputAction.CallbackContext> _onAfterMenuArrow;
        private Action<InputAction.CallbackContext> _onAfterItemArrow;

        private static readonly int State = Animator.StringToHash("State");

        // On Execute or Click -> onClick?.Invoke(); -> Select 직접 해줘야됨
        // On Arrow -> Select

        private void Awake()
        {
            _onAfterMenuArrow = _ =>
            {
                var input = _.ReadValue<Vector2>();
                if (input == Vector2.down)
                {
                    var necklaceIndex = Array.FindIndex(inventoryMenuItems,
                        item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Necklace);
                    var bagIndex = Array.FindIndex(inventoryMenuItems,
                        item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Bag);

                    if (_menuHighlighter.selectedIndex == necklaceIndex)
                    {
                        necklace.SetKeywordActive(true);
                    }
                    else if (_menuHighlighter.selectedIndex == bagIndex)
                    {
                        if (inventoryItems.Any(item => item.isEnable))
                        {
                            HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                            _itemHighlighter.Select(0);
                        }
                    }
                }
                else if (input == Vector2.up)
                {
                    var necklaceIndex = Array.FindIndex(inventoryMenuItems,
                        item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Necklace);
                    if (_menuHighlighter.selectedIndex == necklaceIndex)
                    {
                        necklace.SetKeywordActive(false);
                    }
                }
            };
            _onAfterItemArrow = _ =>
            {
                var input = _.ReadValue<Vector2>();
                if (input == Vector2.up)
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true, true);
                    foreach (var highlightItem in _itemHighlighter.HighlightItems)
                    {
                        highlightItem.Reset();
                    }
                }
            };

            _menuHighlighter = new Highlighter("Inventory Menu Highlight")
            {
                HighlightItems = new List<HighlightItem>(inventoryMenuItems),
                name = "메뉴"
            };
            _itemHighlighter = new Highlighter("Inventory Item Highlight")
            {
                HighlightItems = new List<HighlightItem>(inventoryItems),
                name = "아이템"
            };

            _menuHighlighter.Init(Highlighter.ArrowType.Horizontal, () => { SetInventory(false); });
            _itemHighlighter.Init(Highlighter.ArrowType.Horizontal,
                () => { HighlightHelper.Instance.SetLast(_menuHighlighter, default, true); });

            _menuHighlighter.InputActions.OnInventory = _ => { SetInventory(false); };
            _itemHighlighter.InputActions.OnInventory = _ => { SetInventory(false); };

            // Menu UpDown
            _menuHighlighter.InputActions.OnAfterArrow += _onAfterMenuArrow;
            _itemHighlighter.InputActions.OnAfterArrow += _onAfterItemArrow;
        }

        private void Start()
        {
            _highlightParent = highlights[0].transform.parent;
            inventoryButton.onClick.AddListener(() => SetInventory(true));

            necklace.Init();

            // Inventory Menu Set
            // OnClick or Execute -> Button.OnClick
            // OnArrow -> OnSelect
            for (var idx = 0; idx < inventoryMenuItems.Length; idx++)
            {
                var index = idx;
                var inventoryMenuItem = inventoryMenuItems[idx];

                switch (inventoryMenuItem.inventoryMenuType)
                {
                    case InventoryMenuItem.InventoryMenuType.Bag:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"{inventoryMenuItem.inventoryMenuType} 누름");

                            // First or Other Menu -> Bag
                            if (_menuHighlighter.selectedIndex != index)
                            {
                                HighlightHelper.Instance.Push(_itemHighlighter, true);
                                HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                                _menuHighlighter.Select(index);
                            }
                            // Bag -> Bag
                            else if (inventoryItems.Any(item => item.isEnable))
                            {
                                // Bag Item -> Menu
                                if (HighlightHelper.Instance.IsLast(_itemHighlighter))
                                {
                                    HighlightHelper.Instance.SetLast(_menuHighlighter, true, true);
                                    foreach (var highlightItem in _itemHighlighter.HighlightItems)
                                    {
                                        highlightItem.Reset();
                                    }
                                }
                                // Bag Menu -> Item
                                else
                                {
                                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                                    _itemHighlighter.Select(0);
                                }
                            }
                        });
                        break;
                    case InventoryMenuItem.InventoryMenuType.Necklace:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"{inventoryMenuItem.inventoryMenuType} 누름");

                            HighlightHelper.Instance.Pop(_itemHighlighter);
                            necklace.SetKeywordActive(!necklace.IsKeywordActive());

                            if (_menuHighlighter.selectedIndex != index)
                            {
                                _menuHighlighter.Select(index);
                            }
                        });
                        break;
                    case InventoryMenuItem.InventoryMenuType.Exit:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"{inventoryMenuItem.inventoryMenuType} 누름");

                            inventoryPanel.SetActive(false);
                            HighlightHelper.Instance.Pop(_itemHighlighter);
                            HighlightHelper.Instance.Pop(_menuHighlighter);
                        });
                        break;
                }

                inventoryMenuItem.onSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);

                    switch (inventoryMenuItem.inventoryMenuType)
                    {
                        case InventoryMenuItem.InventoryMenuType.Bag:
                            bagPanel.SetActive(true);
                            necklacePanel.SetActive(false);
                            break;
                        case InventoryMenuItem.InventoryMenuType.Necklace:
                            bagPanel.SetActive(false);
                            necklacePanel.SetActive(true);
                            necklace.SetKeywordActive(false);
                            break;
                    }
                };
            }

            // Inventory Item Set
            foreach (var inventoryItem in inventoryItems)
            {
                inventoryItem.onPointSelect = () =>
                {
                    // Debug.Log($"OnSelect {inventoryItem.itemPanel}");
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    for (var index = 0; index < highlights.Length; index++)
                    {
                        var highlight = highlights[index];
                        var animator = highlight.GetComponent<Animator>();
                        highlight.transform.SetParent(inventoryItem.button.transform);
                        highlight.rectTransform.anchoredPosition = Vector2.zero;
                        animator.SetInteger(State, index + 1);
                    }

                    inventoryItem.itemPanel.SetActive(true);
                };

                inventoryItem.onPointDeSelect = () =>
                {
                    // Debug.Log($"OnDeSelect {inventoryItem.itemPanel}");
                    foreach (var highlight in highlights)
                    {
                        highlight.GetComponent<Animator>().SetInteger(State, 0);
                        highlight.transform.SetParent(_highlightParent);
                    }

                    inventoryItem.itemPanel.SetActive(false);
                };

                inventoryItem.onPointerEnter = () =>
                {
                    // Debug.Log($"OnPointerEnter {inventoryItem.itemPanel}");
                    for (var index = 0; index < highlights.Length; index++)
                    {
                        var highlight = highlights[index];
                        var animator = highlight.GetComponent<Animator>();
                        highlight.transform.SetParent(inventoryItem.button.transform);
                        highlight.rectTransform.anchoredPosition = Vector2.zero;
                        animator.SetInteger(State, index + 1);
                    }
                };
                inventoryItem.onPointerExit = () =>
                {
                    // Debug.Log("OnPointerExit");
                    // Debug.Log(
                    //     $"버튼: {inventoryItem.button.gameObject}, 현재: {highlights[0].transform.parent.gameObject}");
                    if (inventoryItem.button.transform != highlights[0].transform.parent)
                    {
                        Debug.Log("하이라이트 빼지마라");
                        return;
                    }

                    foreach (var highlight in highlights)
                    {
                        highlight.GetComponent<Animator>().SetInteger(State, 0);
                        highlight.transform.SetParent(_highlightParent);
                    }
                };

                inventoryItem.button.onClick.AddListener(() =>
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    _itemHighlighter.Select(inventoryItem);
                });
            }
        }

        private void LoadItemData()
        {
            Debug.Log("Inventory Load Item");

            var ownItems = ItemManager.Instance.GetItem<ItemManager.ItemType>();

            foreach (var item in inventoryItems)
            {
                Debug.Log($"{item.itemType} - Active? {ownItems.Contains(item.itemType)}");
                item.SetActive(ownItems.Contains(item.itemType));
            }
        }

        public void SetInventory(bool isActive)
        {
            if (isActive)
            {
                LoadItemData();
                necklace.UpdateDisplay();
                HighlightHelper.Instance.Push(_menuHighlighter);
                inventoryPanel.SetActive(true);

                var bagButton = Array.Find(inventoryMenuItems,
                    item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Bag);
                bagButton.button.onClick?.Invoke();
            }
            else
            {
                var exitButton = Array.Find(inventoryMenuItems,
                    item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Exit);
                exitButton.button.onClick?.Invoke();
            }
        }

        public void SetEnable(bool isEnable)
        {
            inventoryButton.gameObject.SetActive(isEnable);
        }
    }
}