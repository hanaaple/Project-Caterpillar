using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.InputSystem;

namespace Utility.UI.Highlight
{
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
                _highlighters.Last().SetEnable(false, isDuplicatePossible, false, isReset);
            }
            
            highlighter.selectedIndex = -1;
            highlighter.highlightedIndex = -1;
            highlighter.SetEnable(true);
            
            AddHighlighter(highlighter);

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

            RemoveHighlighter(highlighter, isDestroy);
        }

        public void ResetHighlight()
        {
            while (_highlighters.Count > 0)
            {
                RemoveHighlighter(_highlighters.Last(), true);
            }
        }

        public void Enable()
        {
            _highlighters.Last().SetEnable(true);
        }

        public void Disable(bool isReset)
        {
            _highlighters.Last().SetEnable(false, false, false, isReset);
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
                    Debug.Log($"중복 삭제 {highlighter.name}");
                    RemoveHighlighter(highlighter);
                }
            }

            AddHighlighter(highlighter);
            Debug.Log($"Add Highlight by SetLast {highlighter.name}");

            _highlighters.Last().SetEnable(true, isDuplicatePossible);
        }

        public bool IsLast(Highlighter highlighter)
        {
            if (!_highlighters.Contains(highlighter))
            {
                return false;
            }

            if (_highlighters.Count > 0 && _highlighters.Last() == highlighter)
            {
                return true;
            }

            return false;
        }
        
        private void AddHighlighter(Highlighter highlighter)
        {
            _highlighters.Add(highlighter);
            InputManager.PushInputAction(highlighter.InputActions);
        }
        
        private void RemoveHighlighter(Highlighter highlighter)
        {
            InputManager.PopInputAction(highlighter.InputActions);
            _highlighters.Remove(highlighter);
            
            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(true);
            }
        }
        
        private void RemoveHighlighter(Highlighter highlighter, bool isDestroy)
        {
            // highlighter가 top이 아니면 -> 
            // 여기서 Pop을 하는게 맞는가? 삭제하는거면 알겠는데
            highlighter.SetEnable(false, false, isDestroy);
            RemoveHighlighter(highlighter);

            if (_highlighters.Count > 0)
            {
                _highlighters.Last().SetEnable(true);
            }
        }
    }
}