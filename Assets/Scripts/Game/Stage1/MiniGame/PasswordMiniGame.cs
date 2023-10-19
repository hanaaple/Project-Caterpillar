using System;
using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.MiniGame
{
    public class PasswordMiniGame : Default.MiniGame
    {
        [SerializeField] private PasswordItem[] passwordItems;
        [SerializeField] private string correct;

        [SerializeField] private AudioClip dialAudioClip;
        [SerializeField] private AudioClip failAudioClip;
        [SerializeField] private AudioClip clearAudioClip;
        
        [Header("For Debug")] [SerializeField] private string password;
        [SerializeField] private int selectedIndex;

        protected override void Init()
        {
            base.Init();
            InputActions.OnExecute = () =>
            {
                if (selectedIndex == passwordItems.Length)
                {
                    Debug.Log($"정답: {password}, {correct}, password == correct {password == correct}"); 
                    if (password == correct)
                    {
                        End(true);
                    }
                    else
                    {
                        End(false);
                    }
                }
            };

            InputActions.OnKeyBoard = _ =>
            {
                var key = _.control.displayName;

                if (key == "Backspace")
                {
                    Pop();
                }
                else if (passwordItems.Length > selectedIndex)
                {
                    Push(key);
                }
            };

            selectedIndex = 0;
            foreach (var passwordItem in passwordItems)
            {
                passwordItem.Remove();
            }
        }

        public override void Play(Action<bool> onEndAction = null)
        {
            base.Play(onEndAction);
            passwordItems[selectedIndex].Select();
        }

        private void Push(string key)
        {
            AudioManager.Instance.PlaySfx(dialAudioClip);
            passwordItems[selectedIndex].DeBlink();
            passwordItems[selectedIndex].SetText(key);
            password += key;
            Debug.Log(password);

            selectedIndex++;
            if (passwordItems.Length > selectedIndex)
            {
                passwordItems[selectedIndex].Select();
            }
        }

        private void Pop()
        {
            if (selectedIndex == 0)
            {
                return;
            }
            
            if (passwordItems.Length > selectedIndex)
            {
                passwordItems[selectedIndex].DeSelect();
            }

            selectedIndex--;
            password = password.Remove(selectedIndex);
            Debug.Log(password);

            passwordItems[selectedIndex].SetText("");
            passwordItems[selectedIndex].Select();
        }

        protected override void End(bool isClear = true)
        {
            if (isClear)
            {
                AudioManager.Instance.PlaySfx(clearAudioClip);
            }
            else
            {
                AudioManager.Instance.PlaySfx(failAudioClip);
            }

            base.End(isClear);
        }
    }
}