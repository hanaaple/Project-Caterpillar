using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utility.UI.Highlight;

namespace Utility.UI.Check
{
    public class CheckUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text checkText;
        [SerializeField] private CheckHighlightItem[] checkHighlightItems;
        
        private Highlighter _checkHighlighter;

        public void Initialize(Highlighter.HighlightType highlightType = Highlighter.HighlightType.HighlightIsSelect)
        {
            _checkHighlighter = new Highlighter("check 하이라이트")
            {
                HighlightItems = new List<HighlightItem>(checkHighlightItems),
                highlightType = highlightType
            };

            _checkHighlighter.Init(Highlighter.ArrowType.Horizontal, () =>
            {
                var noIndex = Array.FindIndex(checkHighlightItems,
                    item => item.buttonType == CheckHighlightItem.ButtonType.No);
                if (_checkHighlighter.selectedIndex == noIndex)
                {
                    checkHighlightItems[noIndex].button.onClick?.Invoke();
                }
                else
                {
                    _checkHighlighter.Select(noIndex);
                }
            });
        }

        public void SetOnClickListener(CheckHighlightItem.ButtonType buttonType, UnityAction onClick)
        {
            var highlightItem = Array.Find(checkHighlightItems,
                item => item.buttonType == buttonType);
            highlightItem.button.onClick.RemoveAllListeners();
            highlightItem.button.onClick.AddListener(onClick);
        }

        public void SetText(string text)
        {
            if (checkText)
            {
                checkText.text = text;
            }
        }

        public void Push(bool isDuplicatePossible = false, bool isReset = false)
        {
            gameObject.SetActive(true);
            HighlightHelper.Instance.Push(_checkHighlighter, isDuplicatePossible, isReset);
        }

        public void Pop(bool isDestroy = false)
        {
            HighlightHelper.Instance.Pop(_checkHighlighter, isDestroy);
            gameObject.SetActive(false);
        }
    }
}
