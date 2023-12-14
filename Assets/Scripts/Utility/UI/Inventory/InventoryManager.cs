using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.UI.Highlight;
using Utility.Util;

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

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
        }

        public override void Select()
        {
            base.Select();
            button.image.sprite = selectSprite;
        }

        public override void DeSelect()
        {
            base.DeSelect();
            button.image.sprite = defaultSprite;
        }
    }

    [Serializable]
    public class InventoryItem : HighlightItem
    {
        public ItemManager.ItemType itemType;
        public GameObject itemPanel;

        public void SetActive(bool isActive)
        {
            isEnable = isActive;
            button.gameObject.SetActive(isActive);
        }

        public override void Reset()
        {
            base.Reset();
            onDeSelect?.Invoke();
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

        [SerializeField] private AudioData bagHighlightAudioData;
        [SerializeField] private AudioData bagClickAudioData;
        [SerializeField] private AudioData bagExitAudioData;
        [SerializeField] private AudioData bagMenuHighlightAudioData;

        [FormerlySerializedAs("bagMenuClickAudioData")] [SerializeField]
        private AudioData bagMenuSelectAudioData;

        [SerializeField] private AudioData bagItemSelectAudioData;

        [Header("Necklace")] [SerializeField] private GameObject necklacePanel;
        [SerializeField] private Necklace necklace;

        private Transform _highlightParent;
        private Highlighter _menuHighlighter;
        private Highlighter _itemHighlighter;
        private Action<InputAction.CallbackContext> _onEndMenuArrow;
        private Action<InputAction.CallbackContext> _onEndItemArrow;

        private static readonly int State = Animator.StringToHash("State");

        // On Execute or Click -> onClick?.Invoke(); -> Select 직접 해줘야됨
        // On Arrow -> Select

        private void Awake()
        {
            _onEndMenuArrow = _ =>
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
                        if (!necklace.IsKeywordActive())
                        {
                            PlayUIManager.Instance.PlayAudioClick();
                        }

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
            _onEndItemArrow = _ =>
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
                name = "메뉴",
                HighlightAudioData = bagMenuHighlightAudioData,
                // ClickAudioData = bagMenuClickAudioData // 이미 Select인 상태에선 소리가 안났으면 좋겠음
            };

            _itemHighlighter = new Highlighter("Inventory Item Highlight")
            {
                HighlightItems = new List<HighlightItem>(inventoryItems),
                name = "아이템",
                HighlightAudioData = bagHighlightAudioData
            };

            _menuHighlighter.Init(Highlighter.ArrowType.Horizontal, () => { SetInventory(false); });
            _itemHighlighter.Init(Highlighter.ArrowType.Horizontal,
                () => { HighlightHelper.Instance.SetLast(_menuHighlighter, false, true); });

            _menuHighlighter.InputActions.OnInventory = () => { SetInventory(false); };
            _itemHighlighter.InputActions.OnInventory = () => { SetInventory(false); };

            // Menu UpDown
            _menuHighlighter.InputActions.OnEndArrow += _onEndMenuArrow;
            _itemHighlighter.InputActions.OnEndArrow += _onEndItemArrow;
        }

        private void Start()
        {
            _highlightParent = highlights[0].transform.parent;


            var eventTrigger = inventoryButton.GetComponent<EventTrigger>();
            
            EventTriggerHelper.AddEntry(eventTrigger, EventTriggerType.PointerEnter, bagHighlightAudioData.Play);
            
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
                            HighlightHelper.Instance.Pop(_itemHighlighter);

                            if (_menuHighlighter.selectedIndex != index)
                            {
                                _menuHighlighter.Select(index);
                            }
                            else
                            {
                                necklace.SetKeywordActive(!necklace.IsKeywordActive());
                            }
                        });
                        break;
                    case InventoryMenuItem.InventoryMenuType.Exit:
                        inventoryMenuItem.button.onClick.AddListener(() =>
                        {
                            inventoryPanel.SetActive(false);
                            HighlightHelper.Instance.Pop(_itemHighlighter);
                            HighlightHelper.Instance.Pop(_menuHighlighter);
                        });
                        break;
                }

                inventoryMenuItem.onSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                    bagMenuSelectAudioData.Play();
                    switch (inventoryMenuItem.inventoryMenuType)
                    {
                        case InventoryMenuItem.InventoryMenuType.Bag:
                            bagPanel.SetActive(true);
                            necklacePanel.SetActive(false);
                            necklace.SetKeywordActive(false);
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
                // 이렇게 하면 select, highlight가 각각 있으면
                // 안된다 오류남


                // Select -> Highlight (Other)      UpdateDisplay를 하면 안됨 큰일남

                // Highlight -> Other Select -> Select

                // Select -> Other Highlight -> 

                // Highlight -> Highlight


                inventoryItem.onSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);

                    Highlight(inventoryItem);

                    bagItemSelectAudioData.Play();
                    inventoryItem.itemPanel.SetActive(true);
                };

                inventoryItem.onDeSelect = () =>
                {
                    DeHighlight();

                    inventoryItem.itemPanel.SetActive(false);
                };

                inventoryItem.onHighlight += () => { Highlight(inventoryItem); };

                inventoryItem.onDeHighlight = () =>
                {
                    if (inventoryItem.button.transform != highlights[0].transform.parent)
                    {
                        Debug.Log("하이라이트 빼지마라");
                        return;
                    }

                    DeHighlight();

                    // 기존에 Select가 있다면

                    // selected
                    if (_itemHighlighter.selectedIndex != -1)
                    {
                        var selectedItem = Array.Find(inventoryItems,
                            item => _itemHighlighter.selectedIndex == Array.IndexOf(inventoryItems, item));
                        _itemHighlighter.Select(selectedItem);
                    }
                };

                inventoryItem.button.onClick.AddListener(() =>
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    _itemHighlighter.Select(inventoryItem);
                });
            }
        }

        private void Highlight(HighlightItem inventoryItem)
        {
            for (var index = 0; index < highlights.Length; index++)
            {
                var highlight = highlights[index];
                var animator = highlight.GetComponent<Animator>();
                highlight.transform.SetParent(inventoryItem.button.transform);
                highlight.rectTransform.anchoredPosition = Vector2.zero;
                animator.SetInteger(State, index + 1);
            }
        }

        private void DeHighlight()
        {
            foreach (var highlight in highlights)
            {
                highlight.GetComponent<Animator>().SetInteger(State, 0);
                highlight.transform.SetParent(_highlightParent);
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
                bagClickAudioData.Play();

                var bagButton = Array.Find(inventoryMenuItems,
                    item => item.inventoryMenuType == InventoryMenuItem.InventoryMenuType.Bag);
                bagButton.button.onClick?.Invoke();
            }
            else
            {
                bagExitAudioData.Play();

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