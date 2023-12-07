using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.Audio;

namespace Game.Stage1.MiniGame
{
    public class Diary : Default.MiniGame
    {
        [SerializeField] private Button button;
        [SerializeField] private Sprite[] sprites;
        
        [SerializeField] private AudioData turnAudioData;
        [SerializeField] private AudioData endAudioData;

        private int _index;
        
        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                _index++;
                if (sprites.Length == _index)
                {
                    endAudioData.Play();
                    button.image.raycastTarget = false;
                    End();
                }
                else
                {
                    button.image.sprite = sprites[_index];
                    turnAudioData.Play();
                }
            });
        }

        public override void Play(Action<bool> onEndAction = null)
        {
            _index = 0;
            button.image.raycastTarget = true;
            base.Play(onEndAction);
        }
    }
}