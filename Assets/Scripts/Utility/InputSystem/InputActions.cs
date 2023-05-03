using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Utility.InputSystem
{
    public class InputActions
    {
        public enum ActionType
        {
            UiAction,
            PlayerAction
        }

        public readonly string Name;

        public Action<bool> OnActive;

        // UI
        public Action<InputAction.CallbackContext> OnArrow;
        public Action<InputAction.CallbackContext> OnExecute;
        public Action<InputAction.CallbackContext> OnCancel;

        /// <summary>
        /// Highlighter에서 직접 접근해서 더해주는 경우, highlighter.Init 이후 해줘야됨. 
        /// </summary>
        public Action<InputAction.CallbackContext> OnPause;

        // Player
        public Action<InputAction.CallbackContext> OnInteract;
        public Action<InputAction.CallbackContext> OnMovePerformed;
        public Action<InputAction.CallbackContext> OnMoveCanceled;

        public InputActions(string name)
        {
            Name = name;
        }

        /// <summary>
        ///  InputManager 외에는 사용을 금합니다.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetAction(bool isActive)
        {
            OnActive?.Invoke(isActive);

            if (isActive)
            {
                InputManager.SetInputActions(true);
                var inputActions = InputManager.InputControl.Input;

                if (OnArrow != null)
                    inputActions.Arrow.performed += OnArrow;

                if (OnExecute != null)
                    inputActions.Execute.performed += OnExecute;

                if (OnCancel != null)
                    inputActions.Cancel.performed += OnCancel;

                if (OnPause != null)
                    inputActions.Pause.performed += OnPause;

                if (OnMovePerformed != null)
                    inputActions.Move.performed += OnMovePerformed;

                if (OnMoveCanceled != null)
                    inputActions.Move.canceled += OnMoveCanceled;

                if (OnInteract != null)
                    inputActions.Interact.performed += OnInteract;
            }
            else
            {
                InputManager.SetInputActions(false);

                var inputActions = InputManager.InputControl.Input;

                if (OnArrow != null)
                    inputActions.Arrow.performed -= OnArrow;

                if (OnExecute != null)
                    inputActions.Execute.performed -= OnExecute;

                if (OnCancel != null)
                    inputActions.Cancel.performed -= OnCancel;

                if (OnPause != null)
                    inputActions.Pause.performed -= OnPause;

                if (OnMovePerformed != null)
                    inputActions.Move.performed -= OnMovePerformed;

                if (OnMoveCanceled != null)
                    inputActions.Move.canceled -= OnMoveCanceled;

                if (OnInteract != null)
                    inputActions.Interact.performed -= OnInteract;
            }
        }
    }
}