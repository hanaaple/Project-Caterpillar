using System;
using UnityEngine.InputSystem;

namespace Utility.InputSystem
{
    /// <summary>
    /// 직접 Action에 접근하는 경우, Push 이전에 실행해야됨.
    /// Highlighter에서 직접 접근해서 더해주는 경우, highlighter.Init 이후 해줘야됨.
    /// </summary>
    public class InputActions
    {
        public readonly string Name;

        public Action<bool> OnActive;

        public Action<InputAction.CallbackContext> OnArrow; // const arrow
        public Action<InputAction.CallbackContext> OnMovePerformed; // arrow
        public Action<InputAction.CallbackContext> OnMoveCanceled; // arrow

        public Action OnExecute; // enter, space, z key, OnFixedExecute와 OnInteract를 같이 쓰는 경우 사용
        public Action OnFixedExecute; // enter, space key
        public Action OnInteract; // z key

        public Action OnEsc; // escape Key
        public Action<InputAction.CallbackContext> OnInventory; // x Key

        public Action<InputAction.CallbackContext> OnAnyKey; // any key
        public Action<InputAction.CallbackContext> OnLeftClick; // mouse leftClick

        private readonly Action<InputAction.CallbackContext> _onExecute;
        private readonly Action<InputAction.CallbackContext> _onFixedExecute;
        private readonly Action<InputAction.CallbackContext> _onInteract;
        private readonly Action<InputAction.CallbackContext> _onEsc;

        public InputActions(string name)
        {
            Name = name;
            _onEsc = _ => { OnEsc?.Invoke(); };
            _onFixedExecute = _ => { OnFixedExecute?.Invoke(); };
            _onInteract = _ => { OnInteract?.Invoke(); };
            _onExecute = _ => { OnExecute?.Invoke(); };
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

                if (OnFixedExecute != null)
                    inputActions.Execute.performed += _onFixedExecute;

                if (OnEsc != null)
                    inputActions.Pause.performed += _onEsc;

                if (OnMovePerformed != null)
                    inputActions.Move.performed += OnMovePerformed;

                if (OnMoveCanceled != null)
                    inputActions.Move.canceled += OnMoveCanceled;

                if (OnInteract != null)
                    inputActions.Interact.performed += _onInteract;

                if (OnInventory != null)
                    inputActions.Inventory.performed += OnInventory;

                if (OnAnyKey != null)
                    inputActions.AnyKey.performed += OnAnyKey;

                if (OnLeftClick != null)
                    inputActions.LeftClick.performed += OnLeftClick;

                if (OnExecute != null)
                {
                    inputActions.Execute.performed += _onExecute;
                    inputActions.Interact.performed += _onExecute;
                }
            }
            else
            {
                InputManager.SetInputActions(false);

                var inputActions = InputManager.InputControl.Input;

                if (OnArrow != null)
                    inputActions.Arrow.performed -= OnArrow;

                if (OnFixedExecute != null)
                    inputActions.Execute.performed -= _onFixedExecute;

                if (OnEsc != null)
                    inputActions.Pause.performed -= _onEsc;

                if (OnMovePerformed != null)
                    inputActions.Move.performed -= OnMovePerformed;

                if (OnMoveCanceled != null)
                    inputActions.Move.canceled -= OnMoveCanceled;

                if (OnInteract != null)
                    inputActions.Interact.performed -= _onInteract;

                if (OnInventory != null)
                    inputActions.Inventory.performed -= OnInventory;

                if (OnAnyKey != null)
                    inputActions.AnyKey.performed -= OnAnyKey;

                if (OnLeftClick != null)
                    inputActions.LeftClick.performed -= OnLeftClick;
                
                if (OnExecute != null)
                {
                    inputActions.Execute.performed -= _onExecute;
                    inputActions.Interact.performed -= _onExecute;
                }
            }
        }
    }
}