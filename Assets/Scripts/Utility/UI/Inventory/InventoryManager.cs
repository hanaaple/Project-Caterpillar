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

        public Action onPointSelect;

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
            button.image.color = Color.white;
        }

        public override void EnterHighlight()
        {
            if (!TransitionTypes.Contains(TransitionType.Select))
            {
                button.image.sprite = defaultSprite;
            }

            button.image.color = Color.blue;
        }

        public override void SetSelect()
        {
            if (!TransitionTypes.Contains(TransitionType.Highlight))
            {
                button.image.color = Color.white;
            }

            button.image.sprite = selectSprite;
            onPointSelect?.Invoke();
        }
    }

    [Serializable]
    public class InventoryItem : HighlightItem
    {
        public ItemManager.ItemType itemType;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectedSprite;
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
            // Debug.Log($"Set Default {itemType}");
            button.image.sprite = defaultSprite;
            button.image.color = Color.white;
        }

        public override void EnterHighlight()
        {
            // Debug.Log($"Enter Highlight {itemType}");
            if (!TransitionTypes.Contains(TransitionType.Select))
            {
                button.image.sprite = defaultSprite;
            }

            button.image.color = Color.blue;
            onPointerEnter?.Invoke();
        }

        public override void SetSelect()
        {
            // Debug.Log($"Set Select {itemType}");
            if (!TransitionTypes.Contains(TransitionType.Highlight))
            {
                button.image.color = Color.white;
            }

            button.image.sprite = selectedSprite;
            onPointSelect?.Invoke();
        }

        public override void Reset()
        {
            // Debug.Log("리셋?");
            base.Reset();
            onPointDeSelect?.Invoke();
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private Button inventoryButton;
        [SerializeField] private InventoryMenuItem[] inventoryMenuItems;
        [SerializeField] private InventoryItem[] inventoryItems;
        [SerializeField] private Image[] highlights;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject bagPanel;
        [SerializeField] private GameObject necklacePanel;

        private Transform _highlightParent;
        private Highlighter _menuHighlighter;
        private Highlighter _itemHighlighter;
        private int _selectedMenuIdx;
        private int _selectedItemIdx;
        private Action<InputAction.CallbackContext> _onMenuArrow;
        private Action<InputAction.CallbackContext> _onItemArrow;

        private static readonly int State = Animator.StringToHash("State");

        private void Awake()
        {
            _onMenuArrow = _ =>
            {
                var input = _.ReadValue<Vector2>();
                if (input == Vector2.down && HighlightHelper.Instance.Contains(_itemHighlighter))
                {
                    _menuHighlighter.Select(0);
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    _itemHighlighter.Select(0);
                }
            };
            _onItemArrow = _ =>
            {
                var input = _.ReadValue<Vector2>();
                if (input == Vector2.up)
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);
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

            _menuHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                var exitIndex = Array.FindIndex(inventoryMenuItems,
                    item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Exit);
                if (_menuHighlighter.selectedIndex == exitIndex)
                {
                    inventoryMenuItems[exitIndex].button.onClick?.Invoke();
                }
                else
                {
                    _itemHighlighter.DeSelect();
                    _menuHighlighter.Select(exitIndex);
                }
            });

            _itemHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                HighlightHelper.Instance.SetLast(_menuHighlighter);
            });
            
            
            // Menu UpDown
            _menuHighlighter.InputActions.OnArrow += _onMenuArrow;
            _itemHighlighter.InputActions.OnArrow += _onItemArrow;
        }

        private void Start()
        {
            _highlightParent = highlights[0].transform.parent;
            inventoryButton.onClick.AddListener(() => SetActive(true));

            // Menu Arrow Select
            for(var idx = 0; idx < inventoryMenuItems.Length; idx++)
            {
                var index = idx;
                var inventoryMenuItem = inventoryMenuItems[idx];
                switch (inventoryMenuItem.inventoryMenuType)
                {
                    case InventoryMenuItem.InventoryMenuType.Bag:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"{inventoryMenuItem.inventoryMenuType} 누름");
                            var items = ItemManager.Instance.GetItem<ItemManager.ItemType>();
                            var duplicated = items.Intersect(inventoryItems.Select(item => item.itemType));

                            if (!duplicated.Any())
                            {
                                bagPanel.SetActive(true);
                                necklacePanel.SetActive(false);
                                return;
                            }

                            if (!HighlightHelper.Instance.Contains(_itemHighlighter))
                            {
                                bagPanel.SetActive(true);
                                necklacePanel.SetActive(false);
                                HighlightHelper.Instance.Push(_itemHighlighter, true);
                                HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                                _menuHighlighter.Select(index);
                            }
                            else
                            {
                                HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                                _itemHighlighter.Select(0);
                            }
                        });
                        break;
                    case InventoryMenuItem.InventoryMenuType.Necklace:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            Debug.Log($"{inventoryMenuItem.inventoryMenuType} 누름");
                            HighlightHelper.Instance.Pop(_itemHighlighter);
                            necklacePanel.SetActive(true);
                            bagPanel.SetActive(false);

                            HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                            _menuHighlighter.Select(index);
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

                inventoryMenuItem.onPointSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);

                    switch(inventoryMenuItem.inventoryMenuType)
                    {
                        case InventoryMenuItem.InventoryMenuType.Bag:
                            bagPanel.SetActive(true);
                            necklacePanel.SetActive(false);
                            break;
                        case InventoryMenuItem.InventoryMenuType.Necklace:
                            bagPanel.SetActive(false);
                            necklacePanel.SetActive(true);
                            break;
                    }
                };
            }

            // Inventory Item Set
            foreach (var inventoryItem in inventoryItems)
            {
                inventoryItem.onPointSelect = () =>
                {
                    Debug.Log("OnSelect");
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    for(var index = 0; index < highlights.Length; index++)
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
                    Debug.Log("OnDeSelect");
                    foreach (var highlight in highlights)
                    {
                        highlight.GetComponent<Animator>().SetInteger(State, 0);
                        highlight.transform.SetParent(_highlightParent);
                    }
                    inventoryItem.itemPanel.SetActive(false);
                };

                inventoryItem.onPointerEnter = () =>
                {
                    Debug.Log("OnPointerEnter");
                    for(var index = 0; index < highlights.Length; index++)
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
                    Debug.Log("OnPointerExit");
                    Debug.Log($"버튼: {inventoryItem.button.gameObject}, 현재: {highlights[0].transform.parent.gameObject}");
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
            // Get From ItemManager
            // ItemManager Have to load At Start Game
            Debug.Log("Inventory Init");


            var ownItems = ItemManager.Instance.GetItem<ItemManager.ItemType>();
            foreach (var inventoryItemHighLight in inventoryItems)
            {
                if (ownItems.Contains(inventoryItemHighLight.itemType))
                {
                    inventoryItemHighLight.SetActive(true);
                }
                else
                {
                    inventoryItemHighLight.SetActive(false);
                }
            }
        }

        private void SetActive(bool isActive)
        {
            if (isActive)
            {
                LoadItemData();
                HighlightHelper.Instance.Push(_menuHighlighter);

                inventoryPanel.SetActive(true);
                bagPanel.SetActive(false);
                necklacePanel.SetActive(false);
                _menuHighlighter.Select(0);

                var bagButton = Array.Find(inventoryMenuItems, item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Bag);
                bagButton.button.onClick?.Invoke();
            }
            else
            {
                var exitButton = Array.Find(inventoryMenuItems, item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Exit);
                exitButton.button.onClick?.Invoke();
            }
        }

        public void SetEnable(bool isEnable)
        {
            inventoryButton.gameObject.SetActive(isEnable);
        }
    }
}