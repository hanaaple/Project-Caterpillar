using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility.InputSystem;

namespace Utility.UI.Highlight
{
    /// <summary>
    /// Execute - Button.onClick
    /// OnArrow - Select
    /// </summary>
    [Serializable]
    public class Highlighter
    {
        /// <summary>
        /// for debugging
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
            /// hover mouse == select(arrow), don't have to Implement HighlightItem -> EnterHighlight()
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
                if (!highlightItems.Any(item => item.isEnable))
                {
                    return;
                }
                var input = _.ReadValue<Vector2>();
                OnArrow(input, arrowType);
            };

            _onExecute = _ =>
            {
                if (selectedIndex != -1)
                {
                    highlightItems[selectedIndex].Execute();
                }
                else
                {
                    Select(0);
                }
            };

            _onCancle += _ => { onCancle?.Invoke(); };
        }

        public void SetEnable(bool isEnable, bool isDuplicatePossible = false, bool isRemove = false, bool isReset = true)
        {
            var uiActions = InputManager.InputControl.Ui;
            Debug.Log($"SetEnable {name} {isEnable}  {isDuplicatePossible}  {isRemove}");
            if (enabled == isEnable)
            {
                if (!isEnable && !isDuplicatePossible && !isRemove)
                {
                    if (isReset)
                    {
                        highlightedIndex = -1;
                        selectedIndex = -1;
                        
                        foreach (var highlightItem in highlightItems)
                        {
                            highlightItem.Reset();
                            highlightItem.ClearEventTrigger();
                        }
                    }

                    foreach (var highlightItem in highlightItems)
                    {
                        highlightItem.ClearEventTrigger();
                    }
                }
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
                    if (!highlightItem.isEnable)
                    {
                        return;
                    }

                    if (highlightType == HighlightType.None)
                    {
                        highlightItem.AddEventTrigger(EventTriggerType.PointerEnter, delegate { OnHighlight(highlightItem); });
                        highlightItem.AddEventTrigger(EventTriggerType.PointerExit, delegate { OnUnHighlight(); });
                    }
                    else if (highlightType == HighlightType.HighlightIsSelect)
                    {
                        highlightItem.AddEventTrigger(EventTriggerType.PointerEnter, delegate { Select(highlightItem); });
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
                    if (isReset)
                    {
                        highlightedIndex = -1;
                        selectedIndex = -1;
                        
                        foreach (var highlightItem in highlightItems)
                        {
                            highlightItem.Reset();
                        }
                    }

                    foreach (var highlightItem in highlightItems)
                    {
                        highlightItem.ClearEventTrigger();
                    }
                }
            }
        }

        public void Select(HighlightItem highlightItem)
        {
            RemoveItem(selectedIndex, HighlightItem.TransitionType.Select);

            var idx = Array.FindIndex(highlightItems, item => item == highlightItem);
            if (idx == -1)
            {
                Debug.LogError("에러");
            }

            selectedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Select);
        }

        public void Select(int idx)
        {
            RemoveItem(selectedIndex, HighlightItem.TransitionType.Select);

            selectedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Select);
        }

        public void DeSelect()
        {
            RemoveItem(selectedIndex, HighlightItem.TransitionType.Select);
            selectedIndex = -1;
        }

        public void OnHighlight(HighlightItem highlightItem)
        {
            Debug.Log("On 하이라이트");
            RemoveItem(highlightedIndex, HighlightItem.TransitionType.Highlight);
            
            var idx = Array.FindIndex(highlightItems, item => item == highlightItem);
            highlightedIndex = idx;
            highlightItems[idx].Add(HighlightItem.TransitionType.Highlight);
        }

        public void OnUnHighlight()
        {
            RemoveItem(highlightedIndex, HighlightItem.TransitionType.Highlight);
            highlightedIndex = -1;
        }

        private void RemoveItem(int index, HighlightItem.TransitionType transitionType)
        {
            if (index == -1)
            {
                return;
            }
            highlightItems[index].Remove(transitionType);
            foreach (var highlightItem in highlightItems)
            {
                highlightItem.Highlight();
            }
        }

        // Private 유지하세요
        private void OnArrow(Vector2 input, ArrowType arrowType)
        {
            var idx = selectedIndex;
            while(true)
            {
                if (arrowType == ArrowType.Vertical)
                {
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

                    if (highlightItems[idx].isEnable)
                    {
                        Select(idx);
                        return;
                    }
                }
                else if (arrowType == ArrowType.Horizontal)
                {
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

                    if (highlightItems[idx].isEnable)
                    {
                        Select(idx);
                        return;
                    }
                }
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
                if (_instance == null)
                {
                    var obj = FindObjectOfType<HighlightHelper>();
                    if (obj != null)
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

        public void Push(Highlighter highlighter, bool isDuplicatePossible = false, bool isReset = true)
        {
            if (_highlighters.Contains(highlighter))
            {
                return;
            }

            Debug.Log("Push");

            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(false, isDuplicatePossible, default, isReset);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="highlighter"></param>
        /// <param name="isRemove"> When destroy highlighter (ex - LoadScene)  </param>
        public void Pop(Highlighter highlighter, bool isRemove = false)
        {
            if (!_highlighters.Contains(highlighter))
            {
                return;
            }
            Debug.Log($"Pop, 삭제여부: {isRemove}");

            StartCoroutine(PopCoroutine(highlighter, isRemove));
        }

        private IEnumerator PopCoroutine(Highlighter highlighter, bool isRemove)
        {
            yield return null;
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

        public void Enable()
        {
            _highlighters.Last().SetEnable(true);
        }

        public void Disable(bool isReset)
        {
            _highlighters.Last().SetEnable(false, default, default, isReset);
        }

        public void SetLast(Highlighter highlighter, bool isDuplicatePossible = false)
        {
            if (IsLast(highlighter))
            {
                return;
            }

            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(false, isDuplicatePossible);
                _highlighters.Remove(highlighter);
            }
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