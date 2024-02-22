using System;
using UnityEngine;
using Utility.Core;
using Utility.InputSystem;

namespace Game.Default
{
    public abstract class MiniGame : MonoBehaviour
    {
        private Action<bool> _onEndAction;
        
        protected InputActions InputActions;

        protected virtual void Init()
        {
            InputActions = new InputActions(nameof(MiniGame))
            {
                OnEsc = () =>
                {
                    PlayUIManager.Instance.pauseManager.onPause?.Invoke();
                    PlayUIManager.Instance.pauseManager.onExit = () =>
                    {
                        // ??
                        PlayUIManager.Instance.dialogueController.EndDialogueImmediately();
                        PlayUIManager.Instance.pauseManager.onExit = () => { };
                    };
                }
            };
        }

        public virtual void Play(Action<bool> onEndAction = null)
        {
            Init();
            _onEndAction = onEndAction;
            InputManager.PushInputAction(InputActions);
        }

        protected virtual void End(bool isClear = true)
        {
            InputManager.PopInputAction(InputActions);
            _onEndAction?.Invoke(isClear);
        }
    }
}