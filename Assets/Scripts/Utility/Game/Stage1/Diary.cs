using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Game.Stage1
{
    public class Diary : MiniGame
    {
        [SerializeField] private Button button;
        [SerializeField] private Sprite[] sprites;

        private int _index;
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                _index++;
                if (sprites.Length == _index)
                {
                    button.image.raycastTarget = false;
                    End();
                }
                else
                {
                    button.image.sprite = sprites[_index];
                }
            });
        }

        public override void Play(Action onEndAction)
        {
            _index = 0;
            button.image.raycastTarget = true;
            base.Play(onEndAction);
        }
    }
}