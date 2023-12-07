using System;
using UnityEngine;
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
        public bool Enable;

        public Action<bool> OnActive;

        public Action<InputAction.CallbackContext> OnArrow; // const arrow
        public Action<InputAction.CallbackContext> OnEndArrow; // const arrow
        public Action<InputAction.CallbackContext> OnMovePerformed; // arrow
        public Action<InputAction.CallbackContext> OnMoveCanceled; // arrow
        public Action<InputAction.CallbackContext> OnKeyBoard; // OnAlphabet & Backspace
        public Action<InputAction.CallbackContext> OnMouseWheel; // On Mouse Wheel

        public Action
            OnExecute; // enter, space, z key, OnFixedExecute와 OnInteract를 같이 쓰는 경우 사용, 단, 같이 사용하는 경우 같은 프레임에 작동할 위험이 있다.

        public Action OnInteractPerformed; // z key (Dynamic)
        public Action OnInteractCanceled;
        public Action OnEsc; // escape Key
        public Action OnInventory; // x Key
        public Action OnLeftClick; // mouse leftClick
        public Action OnTab;

        private int _frameCount = -1;

        // 위 Action들을 Reference하게 사용하기 위해 Wrapper로 이용
        private readonly Action<InputAction.CallbackContext> _onArrow; // arrow
        private readonly Action<InputAction.CallbackContext> _onMovePerformed; // arrow
        private readonly Action<InputAction.CallbackContext> _onMoveCanceled; // arrow
        private readonly Action<InputAction.CallbackContext> _onFixedExecute; // Enter, Space  
        private readonly Action<InputAction.CallbackContext> _onInteract; // z (Dynamic)
        private readonly Action<InputAction.CallbackContext> _onInteractCanceled;
        private readonly Action<InputAction.CallbackContext> _onEsc; // escape Key

        private readonly Action<InputAction.CallbackContext> _onInventory; // x Key
        private readonly Action<InputAction.CallbackContext> _onTab; // Tab
        private readonly Action<InputAction.CallbackContext> _onMouseWheel; // Mouse Wheel
        private readonly Action<InputAction.CallbackContext> _onLeftClick; // mouse leftClick
        private readonly Action<InputAction.CallbackContext> _onKeyBoard; // Keyboard Input

        // public Action OnFixedExecute; // enter, space key

        public InputActions(string name)
        {
            Name = name;

            _onFixedExecute = _ =>
            {
                if (!Enable) return;

                OnExecuteAction();
            };

            _onInteract = _ =>
            {
                if (!Enable) return;
                var input = InputManager.InputControl.Input;
                if (!InputManager.IsAnyKeyDuplicated(input.Interact, input.Execute))
                {
                    // 키가 겹치지 않은 경우 실행, z, z인 경우 한쪽에서만 실행되도록
                    OnExecuteAction();
                }

                // 이전에 실행했던 inputActions.Interact.performed가 이어서 바로 실행됨
                // -> Dialogue Controller에서 대화 종료시 잠시 멈춘 후 실행 
                OnInteractPerformed?.Invoke();
            };

            _onInteractCanceled = _ =>
            {
                if (!Enable) return;
                OnInteractCanceled?.Invoke();
            };

            _onArrow = _ =>
            {
                if (!Enable) return;
                OnArrow?.Invoke(_);
                OnEndArrow?.Invoke(_);
            };

            _onMovePerformed = _ =>
            {
                if (!Enable) return;
                OnMovePerformed?.Invoke(_);
            };

            _onMoveCanceled = _ =>
            {
                if (!Enable) return;
                OnMoveCanceled?.Invoke(_);
            };

            _onEsc = _ =>
            {
                if (!Enable) return;
                OnEsc?.Invoke();
            };

            _onInventory = _ =>
            {
                if (!Enable) return;
                OnInventory?.Invoke();
            };
            _onTab = _ =>
            {
                if (!Enable) return;
                OnTab?.Invoke();
            };

            _onMouseWheel = _ =>
            {
                if (!Enable) return;
                OnMouseWheel?.Invoke(_);
            };
            _onLeftClick = _ =>
            {
                if (!Enable) return;
                OnLeftClick?.Invoke();
            };
            _onKeyBoard = _ =>
            {
                if (!Enable) return;
                OnKeyBoard?.Invoke(_);
            };
        }

        /// <summary>
        ///  InputManager 외에는 사용을 금합니다.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetAction(bool isActive)
        {
            Debug.LogWarning($"SetAction {Name} {isActive}");
            OnActive?.Invoke(isActive);

            var inputActions = InputManager.InputControl.Input;
            if (isActive)
            {
                Enable = true;
                inputActions.Arrow.performed += _onArrow;
                inputActions.Move.performed += _onMovePerformed;
                inputActions.Move.canceled += _onMoveCanceled;

                inputActions.Execute.performed += _onFixedExecute;
                inputActions.Interact.performed += _onInteract;
                inputActions.Interact.canceled += _onInteractCanceled;

                inputActions.Pause.performed += _onEsc;
                inputActions.Inventory.performed += _onInventory;
                inputActions.Tab.performed += _onTab;

                inputActions.MouseWheel.performed += _onMouseWheel;
                inputActions.LeftClick.performed += _onLeftClick;
                inputActions.Keyboard.performed += _onKeyBoard;
            }
            else
            {
                Enable = false;
                inputActions.Arrow.performed -= _onArrow;
                inputActions.Move.performed -= _onMovePerformed;
                inputActions.Move.canceled -= _onMoveCanceled;

                inputActions.Execute.performed -= _onFixedExecute;
                inputActions.Interact.performed -= _onInteract;
                inputActions.Interact.canceled -= _onInteractCanceled;

                inputActions.Pause.performed -= _onEsc;
                inputActions.Inventory.performed -= _onInventory;
                inputActions.Tab.performed -= _onTab;

                inputActions.MouseWheel.performed -= _onMouseWheel;
                inputActions.LeftClick.performed -= _onLeftClick;
                inputActions.Keyboard.performed -= _onKeyBoard;
            }
        }

        public void OnExecuteAction()
        {
            if (_frameCount == Time.frameCount)
            {
                Debug.Log($"Reject Execute, frame - {Time.frameCount}");
                return;
            }

            _frameCount = Time.frameCount;
            OnExecute?.Invoke();
        }
    }
}