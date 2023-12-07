using System;
using UnityEngine;

namespace Utility.UI.Highlight
{
    [Serializable]
    public class SelectHighlightItem : HighlightItem
    {
        private Animator _animator;
        private static readonly int Selected = Animator.StringToHash("Selected");

        public void Init(Animator animator)
        {
            _animator = animator;
        }

        public override void SetDefault()
        {
            if (_animator.gameObject.activeInHierarchy)
            {
                _animator.SetBool(Selected, false);
            }
        }

        public override void Select()
        {
            base.Select();
            _animator.SetBool(Selected, true);
        }

        public override void DeSelect()
        {
            base.DeSelect();
            _animator.SetBool(Selected, false);
        }
    }
}