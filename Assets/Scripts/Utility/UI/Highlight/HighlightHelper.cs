using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.InputSystem;

namespace Utility.UI.Highlight
{
    [Serializable]
    public class Highlighter
    {
        /// <summary>
        /// for debuging
        /// </summary>
        public string name;
        public enum ArrowType
        {
            None,
            Horizontal,
            Vertical
        }
        
        public enum HighlightType
        {
            None,
            /// <summary>
            /// hover mouse == select(arrow)
            /// </summary>
            HighlightIsSelect
        }

        public HighlightType highlightType;

        public HighlightItem[] highlightItems;

        private Action<InputAction.CallbackContext> _onArrow;
        private Action<InputAction.CallbackContext> _onExecute;
        private Action<InputAction.CallbackContext> _onCancle;
        
        public Action<InputAction.CallbackContext> onSelect;

        public int selectedIndex = -1;
        public int highlightedIndex = -1;

        public bool enabled;
        
        public void Init(ArrowType arrowType, Action onCancle = null)
        {
            foreach (var highlightItem in highlightItems)
            {
                highlightItem.Init();
            }

            _onArrow = _ =>
            {
                var input = _.ReadValue<Vector2>();
                OnArrow(input, arrowType);
            };

            _onExecute = _ =>
            {
                if (selectedIndex != -1)
                {
                    highlightItems[selectedIndex].Execute();
                }
            };

            _onCancle += _ => { onCancle?.Invoke(); };
        }

        public void SetEnable(bool isEnable, bool isDuplicatePossible = false, bool isRemove = false)
        {
            var uiActions = InputManager.InputControl.Ui;
            if (enabled == isEnable)
            {
                return;
            }
            enabled = isEnable;
            if (isEnable)
            {
                uiActions.Select.performed += _onArrow;
                if (onSelect != null)
                {
                    uiActions.Select.performed += onSelect;
                }

                uiActions.Execute.performed += _onExecute;
                uiActions.Cancle.performed += _onCancle;

                foreach (var highlightItem in highlightItems)
                {
                    highlightItem.AddEventTrigger(EventTriggerType.PointerExit,
                        delegate { UnHighlight(); });   
                    if (highlightType == HighlightType.None)
                    {
                        highlightItem.AddEventTrigger(EventTriggerType.PointerEnter,
                            delegate { Highlight(highlightItem); });
                        highlightItem.AddEventTrigger(EventTriggerType.PointerExit,
                            delegate { UnHighlight(); });
                    }
                    else if (highlightType == HighlightType.HighlightIsSelect)
                    {
                        highlightItem.AddEventTrigger(EventTriggerType.PointerEnter,
                            delegate { Select(highlightItem); });
                    }
                }
            }
            else
            {
                uiActions.Select.performed -= _onArrow;
                if (onSelect != null)
                {
                    uiActions.Select.performed -= onSelect;
                }
                uiActions.Execute.performed -= _onExecute;
                uiActions.Cancle.performed -= _onCancle;
                
                if (!isDuplicatePossible && !isRemove)
                {
                    highlightedIndex = -1;
                    selectedIndex = -1;
                    foreach (var highlightItem in highlightItems)
                    {
                        highlightItem.Reset();
                    }
                }
            }
        }
        
        public void Select(HighlightItem highlightItem)
        {
            var idx = Array.FindIndex(highlightItems, item => item == highlightItem);
            if (selectedIndex != -1)
            {
                highlightItems[selectedIndex].Remove(HighlightItem.TransitionType.Select);
            }

            if (idx == -1)
            {
                Debug.LogError("에러");
            }

            selectedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Select);
        }

        public void Select(int idx)
        {
            if (selectedIndex != -1)
            {
                highlightItems[selectedIndex].Remove(HighlightItem.TransitionType.Select);
            }

            selectedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Select);
        }

        public void DeSelect()
        {
            if (selectedIndex != -1)
            {
                highlightItems[selectedIndex].Remove(HighlightItem.TransitionType.Select);
                selectedIndex = -1;
            }
        }

        public void Highlight(HighlightItem highlightItem)
        {
            var idx = Array.FindIndex(highlightItems, item => item == highlightItem);
            if (highlightedIndex != -1)
            {
                highlightItems[highlightedIndex].Remove(HighlightItem.TransitionType.Highlight);
            }

            highlightedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Highlight);
        }
        
        public void UnHighlight()
        {
            if (highlightedIndex != -1)
            {
                highlightItems[highlightedIndex].Remove(HighlightItem.TransitionType.Highlight);
                highlightedIndex = -1;
            }
        }

        private void OnArrow(Vector2 input, ArrowType arrowType)
        {
            if (arrowType == ArrowType.Vertical)
            {
                var idx = selectedIndex;
                if (input == Vector2.up)
                {
                    if (idx == -1)
                    {
                        idx = 0;
                    }
                    else
                    {
                        idx = (idx - 1 + highlightItems.Length) % highlightItems.Length;
                    }
                }
                else if (input == Vector2.down)
                {
                    if (idx == -1)
                    {
                        idx = 0;
                    }
                    else
                    {
                        idx = (idx + 1) % highlightItems.Length;
                    }
                }
                else
                {
                    return;
                }
                
                Select(idx);
            }
            else if (arrowType == ArrowType.Horizontal)
            {
                var idx = selectedIndex;
                if (input == Vector2.left)
                {
                    if (idx == -1)
                    {
                        idx = 0;
                    }
                    else
                    {
                        idx = (idx - 1 + highlightItems.Length) % highlightItems.Length;
                    }
                }
                else if (input == Vector2.right)
                {
                    if (idx == -1)
                    {
                        idx = 0;
                    }
                    else
                    {
                        idx = (idx + 1) % highlightItems.Length;
                    }
                }
                else
                {
                    return;
                }
                
                Select(idx);
            }
        }
    }

    [Serializable]
    public class HighlightHelper : MonoBehaviour
    {
        private static HighlightHelper _instance;
        public static HighlightHelper Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<HighlightHelper>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }
                    DontDestroyOnLoad(_instance);
                    _instance._highlighters = new List<Highlighter>();
                }
                return _instance;
            }
        }

        private List<Highlighter> _highlighters;

        private static HighlightHelper Create()
        {
            var sceneLoaderPrefab = Resources.Load<HighlightHelper>("HighlightHelper");
            return Instantiate(sceneLoaderPrefab);
        }

        public void Push(Highlighter highlighter, bool isDuplicatePossible = false)
        {
            if (_highlighters.Contains(highlighter))
            {
                return;
            }
            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(false, isDuplicatePossible);
            }
            else
            {
                InputManager.SetUiAction(true);
            }

            _highlighters.Add(highlighter);
            highlighter.selectedIndex = -1;
            highlighter.highlightedIndex = -1;
            highlighter.SetEnable(true);
        }

        public void Pop(Highlighter highlighter, bool isRemove = false)
        {
            if (!_highlighters.Contains(highlighter))
            {
                return;
            }
            highlighter.SetEnable(false, default, isRemove);
            _highlighters.Remove(highlighter);
            
            if (_highlighters.Count == 0)
            {
                InputManager.SetUiAction(false);
            }
            else
            {
                _highlighters.Last().SetEnable(true);
            }
        }
        
        public void SetLast(Highlighter highlighter, bool isDuplicatePossible = false)
        {
            if (IsLast(highlighter))
            {
                return;
            }

            _highlighters.Last().SetEnable(false, isDuplicatePossible);
            _highlighters.Remove(highlighter);
            _highlighters.Add(highlighter);

            _highlighters.Last().SetEnable(true, isDuplicatePossible);
        }
        
        public bool IsLast(Highlighter highlighter)
        {
            if (!_highlighters.Contains(highlighter))
            {
                return false;
            }

            if (_highlighters.Last() == highlighter)
            {
                return true;
            }

            return false;
        }
        public bool Contains(Highlighter highlighter)
        {
            return _highlighters.Contains(highlighter);
        }
    }
}