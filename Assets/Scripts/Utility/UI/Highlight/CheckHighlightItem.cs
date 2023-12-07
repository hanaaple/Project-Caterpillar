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
}