using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Object = UnityEngine.Object;

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
            HighlightIsSelect,
            /// <summary>
            /// just click is select, select this type
            /// </summary>
            ClickIsSelect
        }

        public int selectedIndex = -1;
        public int highlightedIndex = -1;
        public bool enabled;
        public bool isKeepHighlightState;

        public Object highlightAudioClip;
        public Object clickAudioClip;

        [NonSerialized] public AudioData HighlightAudioData;
        [NonSerialized] public AudioData ClickAudioData;

        public InputActions InputActions;
        public HighlightType highlightType;
        public List<HighlightItem> HighlightItems;

        public Action onPush;
        public Action onSelect;

        private bool _isCleared = true;

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

            InitHighlightItem(highlightItem);

            if (isActive)
            {
                if (!highlightItem.isEnable)
                {
                    return;
                }

                SetHighlightItemEvent(highlightItem);
            }
        }

        public void RemoveItem(HighlightItem highlightItem, bool isDestroy = false)
        {
            HighlightItems.Remove(highlightItem);

            if (!isDestroy)
            {
                if (highlightType == HighlightType.None)
                {
                    DeHighlight();
                }
            }
        }

        public void Init(ArrowType arrowType, Action onCancel = null)
        {
            foreach (var highlightItem in HighlightItems)
            {
                InitHighlightItem(highlightItem);
            }

            InputActions = new InputActions(name)
            {
                OnArrow = context =>
                {
                    if (!HighlightItems.Any(item => item.isEnable))
                    {
                        return;
                    }

                    var input = context.ReadValue<Vector2>();
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

        public void SetEnable(bool isEnable, bool isDuplicatePossible = false, bool isDestroy = false,
            bool isReset = true)
        {
            Debug.Log($"SetEnable {name}\n" +
                      $" 이전 상태: {enabled}, 현 상태: {isEnable}\n" +
                      $"{isDuplicatePossible}  {isDestroy}");
            if (enabled == isEnable)
            {
                if (!isEnable && !isDuplicatePossible && !isDestroy)
                {
                    if (isReset && !isKeepHighlightState)
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

                return;
            }

            enabled = isEnable;
            if (isEnable)
            {
                if (!_isCleared)
                {
                    return;
                }

                _isCleared = false;

                foreach (var highlightItem in HighlightItems)
                {
                    if (!highlightItem.isEnable)
                    {
                        continue;
                    }

                    SetHighlightItemEvent(highlightItem);
                }
            }
            else
            {
                if (isDuplicatePossible || isDestroy)
                {
                    return;
                }

                if (isReset && !isKeepHighlightState)
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

                _isCleared = true;
            }
        }

        public void Select(HighlightItem highlightItem)
        {
            var idx = HighlightItems.FindIndex(item => item == highlightItem);
            if (idx == -1)
            {
                Debug.LogError("에러");
            }

            Select(idx);
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
            onSelect?.Invoke();
            HighlightItems[idx].Push(HighlightItem.TransitionType.Select);
            if (highlightType == HighlightType.HighlightIsSelect)
            {
                Highlight(idx);
            }
        }

        public void DeSelect()
        {
            PopItem(selectedIndex, HighlightItem.TransitionType.Select);
        }

        public void Highlight(HighlightItem highlightItem)
        {
            var idx = HighlightItems.FindIndex(item => item == highlightItem);
            Highlight(idx);
        }

        public void Highlight(int idx)
        {
            if (highlightedIndex == idx)
            {
                return;
            }
            
            PopItem(highlightedIndex, HighlightItem.TransitionType.Highlight);

            highlightedIndex = idx;
            HighlightItems[idx].Push(HighlightItem.TransitionType.Highlight);
            
            if (highlightType == HighlightType.HighlightIsSelect)
            {
                Select(idx);
            }
        }

        public void DeHighlight()
        {
            PopItem(highlightedIndex, HighlightItem.TransitionType.Highlight);
        }

        private void PopItem(int index, HighlightItem.TransitionType transitionType)
        {
            if (index == -1)
            {
                return;
            }

            if (transitionType == HighlightItem.TransitionType.Highlight)
            {
                highlightedIndex = -1;
            }
            else if (transitionType == HighlightItem.TransitionType.Select)
            {
                selectedIndex = -1;
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

        private void SetHighlightItemEvent(HighlightItem highlightItem)
        {
            // 이게 실행되기 전에 Button.OnClick이 먼저 실행되서 이 Action이 사라짐
            
            // 클릭이 아니라 Select에 소리가 나도록
            if (highlightType == HighlightType.ClickIsSelect)
            {
                onSelect += () =>
                {
                    if (ClickAudioData == null)
                    {
                        AudioManager.Instance.PlaySfx(clickAudioClip, 1f, true, true);
                    }
                    else
                    {
                        ClickAudioData.Play();
                    }
                };
            }
            else
            {
                highlightItem.AddEventTrigger(EventTriggerType.PointerDown,
                    delegate
                    {
                        if (ClickAudioData == null)
                        {
                            AudioManager.Instance.PlaySfx(clickAudioClip, 1f, true, true);
                        }
                        else
                        {
                            ClickAudioData.Play();
                        }
                    });
            }

            highlightItem.AddEventTrigger(EventTriggerType.PointerEnter, delegate { Highlight(highlightItem); });
            highlightItem.AddEventTrigger(EventTriggerType.PointerExit, delegate { DeHighlight(); });

            // if (highlightType == HighlightType.HighlightIsSelect)
            // {
            //     highlightItem.AddEventTrigger(EventTriggerType.PointerEnter, delegate { Select(highlightItem); });
            // }
            // else
            if (highlightType == HighlightType.ClickIsSelect)
            {
                highlightItem.AddEventTrigger(EventTriggerType.PointerDown, delegate { Select(highlightItem); });
            }
        }

        private void InitHighlightItem(HighlightItem highlightItem)
        {
            highlightItem.Init();

            if (HighlightAudioData == null)
            {
                HighlightAudioData = PlayUIManager.Instance.defaultHighlightAudioData;
            }
            else if (highlightAudioClip == null)
            {
                highlightAudioClip = PlayUIManager.Instance.defaultHighlightAudioData.AudioObject;
            }

            if (ClickAudioData == null)
            {
                ClickAudioData = PlayUIManager.Instance.defaultClickAudioData;
            }
            else if (clickAudioClip == null)
            {
                clickAudioClip = PlayUIManager.Instance.defaultClickAudioData.AudioObject;
            }

            highlightItem.onHighlight += () =>
            {
                if (HighlightAudioData == null)
                {
                    AudioManager.Instance.PlaySfx(highlightAudioClip, 1f, true, true);
                }
                else
                {
                    HighlightAudioData.Play();
                }
            };

            highlightItem.onExecute += () =>
            {
                if (ClickAudioData == null)
                {
                    AudioManager.Instance.PlaySfx(clickAudioClip, 1f, true, true);
                }
                else
                {
                    ClickAudioData.Play();
                }
            };
        }
    }
}