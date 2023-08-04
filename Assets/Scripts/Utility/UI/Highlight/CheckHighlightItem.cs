using System;
using UnityEngine;

namespace Utility.UI.Highlight
{
    [Serializable]
    public class CheckHighlightItem : HighlightItem
    {
        public enum ButtonType
        {
            Yes,
            No
        }
        
        public ButtonType buttonType;
        
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectSprite;

        public override void SetDefault()
        {
            button.image.sprite = defaultSprite;
        }

        public override void EnterHighlightDisplay()
        {
        }

        public override void SelectDisplay()
        {
            button.image.sprite = selectSprite;
        }
    }
}