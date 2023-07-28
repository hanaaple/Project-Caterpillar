using System;
using UnityEngine;
using Utility.Core;
using Utility.InputSystem;

namespace Utility.Game
{
    public abstract class MiniGame : MonoBehaviour
    {
        private Action _onEndAction;
        
        private InputActions _inputActions;

        protected virtual void Init()
        {
            _inputActions = new InputActions(nameof(MiniGame))
            {
                OnEsc = () =>
                {
                    PlayUIManager.Instance.pauseManager.onPause?.Invoke();
                    PlayUIManager.Instance.pauseManager.onExit = () =>
                    {
                        // ??
                        PlayUIManager.Instance.dialogueController.EndDialogue();
                        PlayUIManager.Instance.pauseManager.onExit = () => { };
                    };
                }
            };
        }

        public virtual void Play(Action onEndAction)
        {
            Init();
            _onEndAction = onEndAction;
            InputManager.PushInputAction(_inputActions);
        }

        protected virtual void End()
        {
            InputManager.PopInputAction(_inputActions);
            _onEndAction?.Invoke();
        }
    }
}