using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Utility.InputSystem
{
    [Serializable]
    public class InputControlDisplay
    {
        [SerializeField] private InputActionReference inputActionReference;
        [Range(0, 10)] [SerializeField] private int selectedBinding;

        [Header("Binding Info - DO NOT EDIT")] public InputBinding inputBinding;

        public void OnValidate()
        {
            if (inputActionReference == null)
            {
                return;
            }

            GetBindingInfo();
        }

        private void GetBindingInfo()
        {
            if (inputActionReference.action != null)
            {
                if (inputActionReference.action.bindings.Count > selectedBinding)
                {
                    inputBinding = inputActionReference.action.bindings[selectedBinding];
                }
            }
        }
    }

    public class InputController : MonoBehaviour
    {
        [SerializeField] private InputActionReference inputActionReference;

        [SerializeField] private bool excludeMouse = true;
        [SerializeField] private InputControlDisplay[] excludeInputActions;

        [Range(0, 10)] [SerializeField] private int selectedBinding;

        [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

        [Header("Binding Info - DO NOT EDIT")] [SerializeField]
        private InputBinding inputBinding;

        [Header("Ui Fields")] [SerializeField] private TMP_Text keyText;
        [SerializeField] private Button rebindButton;
        [SerializeField] private TMP_Text rebindText;

        private int _bindingIndex;
        private string _actionName;

        public GameObject bindingPanel;

        private void OnEnable()
        {
            rebindButton.onClick.AddListener(DoRebind);

            if (inputActionReference != null)
            {
                GetBindingInfo();
                InputManager.LoadBindingOverride(_actionName);
                GetBindingInfo();
                UpdateUi();
                InputAction action = InputManager.InputControl.asset.FindAction(_actionName);
                InputManager.SaveBindingOverride(action);
            }

            InputManager.RebindComplete += UpdateUi;
            InputManager.RebindCanceled += UpdateUi;
            InputManager.RebindEnd += UpdateUi;
            // InputManager.RebindLoad += LoadUpdateUI;
            InputManager.RebindReset += UpdateUi;
        }

        private void OnDisable()
        {
            InputManager.RebindComplete -= UpdateUi;
            InputManager.RebindCanceled -= UpdateUi;
            InputManager.RebindEnd -= UpdateUi;
            // InputManager.RebindLoad -= LoadUpdateUI;
            InputManager.RebindReset -= UpdateUi;
        }

        private void OnValidate()
        {
            if (inputActionReference == null)
            {
                return;
            }

            GetBindingInfo();
            UpdateUi();
        }

        private void GetBindingInfo()
        {
            if (inputActionReference.action != null)
            {
                _actionName = inputActionReference.action.name;

                if (inputActionReference.action.bindings.Count > selectedBinding)
                {
                    inputBinding = inputActionReference.action.bindings[selectedBinding];
                    _bindingIndex = selectedBinding;
                }
            }

            if (excludeInputActions != null)
            {
                foreach (var inputControlDisplay in excludeInputActions)
                {
                    inputControlDisplay.OnValidate();
                }
            }
        }

        private void UpdateUi()
        {
            if (keyText && inputActionReference.action.bindings.Count > selectedBinding)
            {
                keyText.text = inputActionReference.action.bindings[selectedBinding].name;
            }

            if (rebindText)
            {
                if (Application.isPlaying)
                {
                    rebindText.text = InputManager.GetBindingName(_actionName, _bindingIndex);
                }
                else
                {
                    rebindText.text = inputActionReference.action.GetBindingDisplayString(_bindingIndex);
                }
            }
        }

        private void DoRebind()
        {
            InputManager.StartRebind(_actionName, _bindingIndex, bindingPanel, rebindText, excludeMouse,
                excludeInputActions?.Select(item => item.inputBinding.effectivePath).ToArray());
        }

        public void ResetBinding()
        {
            InputManager.ResetBinding(_actionName, _bindingIndex);
        }

        public void TempResetBinding()
        {
            InputManager.TempResetBinding(_actionName, _bindingIndex);
        }

        // public void LoadBindingOverride()
        // {
        //     InputManager.inputActions.Clear();
        //     InputManager.LoadBindingOverride(_actionName);
        // }


        // void Start()
        // {
        //     var playerActions = InputManager.inputControl.PlayerActions;
        //     playerActions.Move.performed += delegate(InputAction.CallbackContext context)
        //     {
        //         curInput = context.ReadValue<Vector2>();
        //     };
        //     
        //     playerActions.Move.canceled += delegate(InputAction.CallbackContext context)
        //     {
        //         curInput = context.ReadValue<Vector2>();
        //     };
        // }
    }
}