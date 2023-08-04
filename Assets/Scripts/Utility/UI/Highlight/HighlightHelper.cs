using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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

        public List<HighlightItem> HighlightItems;

        public InputActions InputActions;

        public int selectedIndex = -1;
        public int highlightedIndex = -1;

        public bool enabled;

        public Action onPush;

        public Highlighter(string name)
        {
            this.name = name;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="highlightItem"></param>
        /// <param name="index"> if index == -1, Add to Last </param>
        /// <param name="isActive"></param>
        public void AddItem(HighlightItem highlightItem, int index = -1, bool isActive = false)
        {
            if (index == -1)
            {
                index = HighlightItems.Count;
            }

            HighlightItems.Insert(index, highlightItem);

            highlightItem.Init();

            if (isActive)
            {
                if (!highlightItem.isEnable)
                {
                    return;
                }

                if (highlightType == HighlightType.None)
                {
                    highlightItem.AddEventTrigger(EventTriggerType.PointerEnter,
                        delegate { OnHighlight(highlightItem); });
                    highlightItem.AddEventTrigger(EventTriggerType.PointerExit, delegate { OnUnHighlight(); });
                }
                else if (highlightType == HighlightType.HighlightIsSelect)
                {
                    highlightItem.AddEventTrigger(EventTriggerType.PointerEnter, delegate { Select(highlightItem); });
                }
            }
        }

        public void RemoveItem(HighlightItem highlightItem, bool isDestroy = false)
        {
            HighlightItems.Remove(highlightItem);

            if (!isDestroy)
            {
                if (highlightType == HighlightType.None)
                {
                    OnUnHighlight();
                }
            }
        }

        public void Init(ArrowType arrowType, Action onCancel = null)
        {
            foreach (var highlightItem in HighlightItems)
            {
                highlightItem.Init();
            }

            InputActions = new InputActions(name)
            {
                OnArrow = _ =>
                {
                    if (!HighlightItems.Any(item => item.isEnable))
                    {
                        return;
                    }

                    var input = _.ReadValue<Vector2>();
                    OnArrow(input, arrowType);
                },
                OnExecute = () =>
                {
                    if (selectedIndex != -1)
                    {
                        HighlightItems[selectedIndex].Execute();
                    }
                    else
                    {
                        Select(0);
                    }
                },
                OnEsc = () => { onCancel?.Invoke(); }
            };
        }

        public void SetEnable(bool isEnable, bool isDuplicatePossible = false, bool isDestroy = false, bool isReset = true)
        {
            Debug.Log($"SetEnable {name}\n" +
                      $" 이전 상태: {enabled}, 현 상태: {isEnable}\n" +
                      $"{isDuplicatePossible}  {isDestroy}");
            if (enabled == isEnable)
            {
                if (!isEnable && !isDuplicatePossible && !isDestroy)
                {
                    if (isReset)
                    {
                        highlightedIndex = -1;
                        selectedIndex = -1;

                        foreach (var highlightItem in HighlightItems)
                        {
                            highlightItem.Reset();
                            highlightItem.ClearEventTrigger();
                        }
                    }

                    foreach (var highlightItem in HighlightItems)
                    {
                        highlightItem.ClearEventTrigger();
                    }
                }

                return;
            }

            enabled = isEnable;
            if (isEnable)
            {
                InputManager.PushInputAction(InputActions);

                foreach (var highlightItem in HighlightItems)
                {
                    // Debug.Log(highlightItem.isEnable);
                    if (!highlightItem.isEnable)
                    {
                        continue;
                    }
                    
                    // Debug.Log($"Set {name}, {highlightItem.button.gameObject}, {highlightType}");

                    if (highlightType == HighlightType.None)
                    {
                        highlightItem.AddEventTrigger(EventTriggerType.PointerEnter,
                            delegate { OnHighlight(highlightItem); });
                        highlightItem.AddEventTrigger(EventTriggerType.PointerExit, delegate { OnUnHighlight(); });
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
                InputManager.PopInputAction(InputActions);

                if (isDuplicatePossible || isDestroy)
                {
                    return;
                }

                if (isReset)
                {
                    highlightedIndex = -1;
                    selectedIndex = -1;

                    foreach (var highlightItem in HighlightItems)
                    {
                        highlightItem.Reset();
                    }
                }

                foreach (var highlightItem in HighlightItems)
                {
                    highlightItem.ClearEventTrigger();
                }
            }
        }

        public void Select(HighlightItem highlightItem)
        {
            var idx = HighlightItems.FindIndex(item => item == highlightItem);
            if (idx == -1)
            {
                Debug.LogError("에러");
            }
            
            if (selectedIndex == idx)
            {
                return;
            }
            
            PopItem(selectedIndex, HighlightItem.TransitionType.Select);

            selectedIndex = idx;
            HighlightItems[idx].Push(HighlightItem.TransitionType.Select);
        }

        public void Select(int idx)
        {
            if (!HighlightItems[idx].isEnable)
            {
                idx = HighlightItems.IndexOf(HighlightItems.First(item => item.isEnable));
            }
            
            if (selectedIndex == idx)
            {
                return;
            }
            
            PopItem(selectedIndex, HighlightItem.TransitionType.Select);

            selectedIndex = idx;
            HighlightItems[idx].Push(HighlightItem.TransitionType.Select);
        }

        public void DeSelect()
        {
            PopItem(selectedIndex, HighlightItem.TransitionType.Select);
            selectedIndex = -1;
        }

        public void OnHighlight(HighlightItem highlightItem)
        {
            var idx = HighlightItems.FindIndex(item => item == highlightItem);
            if (highlightedIndex == idx)
            {
                return;
            }
            
            PopItem(highlightedIndex, HighlightItem.TransitionType.Highlight);
            
            highlightedIndex = idx;
            HighlightItems[idx].Push(HighlightItem.TransitionType.Highlight);
        }

        public void OnUnHighlight()
        {
            PopItem(highlightedIndex, HighlightItem.TransitionType.Highlight);
            highlightedIndex = -1;
        }

        private void PopItem(int index, HighlightItem.TransitionType transitionType)
        {
            if (index == -1)
            {
                return;
            }

            HighlightItems[index].Pop(transitionType);
            foreach (var highlightItem in HighlightItems)
            {
                highlightItem.UpdateDisplay();
            }
        }

        // Private 유지하세요
        private void OnArrow(Vector2 input, ArrowType arrowType)
        {
            var idx = selectedIndex;
            while (true)
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
                            idx = (idx - 1 + HighlightItems.Count) % HighlightItems.Count;
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
                            idx = (idx + 1) % HighlightItems.Count;
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (HighlightItems[idx].isEnable)
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
                            idx = (idx - 1 + HighlightItems.Count) % HighlightItems.Count;
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
                            idx = (idx + 1) % HighlightItems.Count;
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (HighlightItems[idx].isEnable)
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

        /// <summary>
        /// Have to Set HighlightItem.isEnable Before Push
        /// </summary>
        /// <param name="highlighter"></param>
        /// <param name="isDuplicatePossible"></param>
        /// <param name="isReset"></param>
        public void Push(Highlighter highlighter, bool isDuplicatePossible = false, bool isReset = true)
        {
            if (_highlighters.Contains(highlighter))
            {
                return;
            }

            Debug.Log($"Push {highlighter.name} Highlight");

            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(false, isDuplicatePossible, default, isReset);
            }

            _highlighters.Add(highlighter);
            highlighter.selectedIndex = -1;
            highlighter.highlightedIndex = -1;
            highlighter.SetEnable(true);
            
            highlighter.onPush?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="highlighter"></param>
        /// <param name="isDestroy"> When destroy highlighter (ex - LoadScene)  </param>
        public void Pop(Highlighter highlighter, bool isDestroy = false)
        {
            if (!_highlighters.Contains(highlighter))
            {
                return;
            }

            Debug.Log($"Pop, 삭제여부: {isDestroy}");

            Remove(highlighter, isDestroy);
        }

        private void Remove(Highlighter highlighter, bool isDestroy)
        {
            highlighter.SetEnable(false, default, isDestroy);
            _highlighters.Remove(highlighter);

            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(true);
            }
        }

        public void ResetHighlight()
        {
            while (_highlighters.Count > 0)
            {
                Remove(_highlighters.Last(), true);
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

        public void SetLast(Highlighter highlighter, bool isDuplicatePossible = false, bool isPreviousLastReset = false)
        {
            if (IsLast(highlighter))
            {
                return;
            }

            if (_highlighters.Count > 0)
            {
                if (isPreviousLastReset)
                {
                    _highlighters.Last().selectedIndex = -1;
                    _highlighters.Last().highlightedIndex = -1;
                }

                _highlighters.Last().SetEnable(false, isDuplicatePossible);
                if (_highlighters.Contains(highlighter))
                {
                    _highlighters.Remove(highlighter);
                }
            }

            _highlighters.Add(highlighter);
            Debug.Log($"Add Highlight by SetLast {highlighter.name}");

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
            Debug.Log($"Is Contain? - {highlighter.name}, {_highlighters.Contains(highlighter)}");
            return _highlighters.Contains(highlighter);
        }
    }
}