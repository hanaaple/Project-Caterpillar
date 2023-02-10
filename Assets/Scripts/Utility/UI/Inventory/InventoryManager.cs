using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.UI.Highlight;

namespace Utility.UI.Inventory
{
    [Serializable]
    public class InventoryMenuHighLight : HighlightItem
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
            if (!transitionTypes.Contains(TransitionType.Select))
            {
                button.image.sprite = defaultSprite;
            }

            button.image.color = Color.blue;
        }

        public override void SetSelect()
        {
            if (!transitionTypes.Contains(TransitionType.Highlight))
            {
                button.image.color = Color.white;
            }

            button.image.sprite = selectSprite;
            onPointSelect?.Invoke();
        }
    }

    [Serializable]
    public class InventoryItemHighLight : HighlightItem
    {
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectedSprite;

        public Action onPointerEnter;
        public Action onPointerExit;
        public Action onPointSelect;
        
        public override void Remove(TransitionType transitionType)
        {
            if (transitionTypes.Contains(transitionType))
            {
                transitionTypes.Remove(transitionType);
                if (transitionType == TransitionType.Highlight)
                {
                    onPointerExit?.Invoke();
                }
            }
            else
            {
                Debug.Log(transitionType + "없음");
            }

            Highlight();
        }

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
            button.image.color = Color.white;
        }

        public override void EnterHighlight()
        {
            if (!transitionTypes.Contains(TransitionType.Select))
            {
                button.image.sprite = defaultSprite;
            }

            button.image.color = Color.blue;
            onPointerEnter?.Invoke();
        }

        public override void SetSelect()
        {
            if (!transitionTypes.Contains(TransitionType.Highlight))
            {
                button.image.color = Color.white;
            }

            button.image.sprite = selectedSprite;
            onPointSelect?.Invoke();
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private Button inventoryButton;
        
        [SerializeField] private InventoryMenuHighLight[] inventoryMenuHighLights;

        [SerializeField] private InventoryItemHighLight[] inventoryItemHighLights;

        [SerializeField] private Image[] highlights;

        private Highlighter _menuHighlighter;
        
        private Highlighter _itemHighlighter;
        
        private int _selectedMenuIdx;
        
        private int _selectedItemIdx;

        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject bagPanel;
        [SerializeField] private GameObject necklacePanel;

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
                    foreach (var highlightItem in _itemHighlighter.highlightItems)
                    {
                        highlightItem.Reset();
                    }
                }
            };
        }

        private void Start()
        {
            inventoryButton.onClick.AddListener(() => SetActive(true));
            
            _menuHighlighter = new Highlighter
            {
                highlightItems = inventoryMenuHighLights,
                name = "메뉴"
            };
            
            _itemHighlighter = new Highlighter
            {
                highlightItems = inventoryItemHighLights,
                name = "아이템"
            };
            
            // Menu UpDown
            _menuHighlighter.onSelect = _onMenuArrow;
            _itemHighlighter.onSelect = _onItemArrow;
            
            // Menu Arrow Select
            foreach (var inventoryMenuHighLight in inventoryMenuHighLights)
            {
                inventoryMenuHighLight.onPointSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                };
            }
            
            foreach (var inventoryItemHighLight in inventoryItemHighLights)
            {
                inventoryItemHighLight.onPointSelect = () =>
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                };
                inventoryItemHighLight.onPointerEnter = () =>
                {
                    for (var index = 0; index < highlights.Length; index++)
                    {
                        var highlight = highlights[index];
                        var animator = highlight.GetComponent<Animator>();
                        highlight.transform.SetParent(inventoryItemHighLight.button.transform);
                        highlight.rectTransform.anchoredPosition = Vector2.zero;
                        animator.SetInteger(State, index + 1);
                    }
                };
                inventoryItemHighLight.onPointerExit = () =>
                {
                    foreach (var highlight in highlights)
                    {
                        highlight.GetComponent<Animator>().SetInteger(State, 0);
                    }
                };
                
                inventoryItemHighLight.button.onClick.AddListener(() =>
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    _itemHighlighter.Select(inventoryItemHighLight);
                });
            }
            
            _menuHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                var exitIndex = Array.FindIndex(inventoryMenuHighLights,
                    item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Exit);
                if (_menuHighlighter.selectedIndex == exitIndex)
                {
                    inventoryMenuHighLights[exitIndex].button.onClick?.Invoke();
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

            var bagIndex = Array.FindIndex(inventoryMenuHighLights, item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Bag);
            inventoryMenuHighLights[bagIndex].button.onClick.AddListener(() =>
            {
                if (!HighlightHelper.Instance.Contains(_itemHighlighter))
                {
                    bagPanel.SetActive(true);
                    necklacePanel.SetActive(false);
                    HighlightHelper.Instance.Push(_itemHighlighter, true);
                    HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                    _menuHighlighter.Select(bagIndex);
                }
                else
                {
                    HighlightHelper.Instance.SetLast(_itemHighlighter, true);
                    _itemHighlighter.Select(0);
                }
            });
            
            var necklaceIndex = Array.FindIndex(inventoryMenuHighLights, item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Necklace);
            inventoryMenuHighLights[necklaceIndex].button.onClick.AddListener(() =>
            {
                HighlightHelper.Instance.Pop(_itemHighlighter);
                necklacePanel.SetActive(true);
                bagPanel.SetActive(false);
                
                HighlightHelper.Instance.SetLast(_menuHighlighter, true);
                _menuHighlighter.Select(necklaceIndex);
            });
            
            var exitIndex = Array.FindIndex(inventoryMenuHighLights, item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Exit);
            inventoryMenuHighLights[exitIndex].button.onClick.AddListener(() =>
            {
                inventoryPanel.SetActive(false);
                HighlightHelper.Instance.Pop(_itemHighlighter);
                HighlightHelper.Instance.Pop(_menuHighlighter);
            });
        }

        private void SetActive(bool isActive)
        {
            if (isActive)
            {
                HighlightHelper.Instance.Push(_menuHighlighter);
                inventoryPanel.SetActive(true);
                bagPanel.SetActive(false);
                necklacePanel.SetActive(false);
                _menuHighlighter.Select(0);
                
                var bagButton = Array.Find(inventoryMenuHighLights, item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Bag);
                bagButton.button.onClick?.Invoke();
            }
            else
            {
                var exitButton = Array.Find(inventoryMenuHighLights, item => item.inventoryMenuType == InventoryMenuHighLight.InventoryMenuType.Exit);
                exitButton.button.onClick?.Invoke();
            }
        }
    }
}