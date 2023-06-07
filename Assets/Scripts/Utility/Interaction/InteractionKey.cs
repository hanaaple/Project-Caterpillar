using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.InputSystem;

namespace Utility.Interaction
{
    public class InteractionKey : MonoBehaviour
    {
        [SerializeField] private InputActionReference inputActionReference;

        [Range(0, 10)] [SerializeField] private int selectedBinding;

        [Header("Binding Info - DO NOT EDIT")] [SerializeField]
        private InputBinding inputBinding;

        [Header("Ui Fields")] [SerializeField] private TMP_Text bindText;

        private int _bindingIndex;
        private string _actionName;

        private void OnEnable()
        {
            if (inputActionReference != null)
            {
                InputManager.LoadBindingOverride(_actionName);
                GetBindingInfo();
                UpdateUi();
                var action = InputManager.InputControl.asset.FindAction(_actionName);
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
        }

        private void UpdateUi()
        {
            if (Application.isPlaying)
            {
                bindText.text = InputManager.GetBindingName(_actionName, _bindingIndex);
            }
            else
            {
                bindText.text = inputActionReference.action.GetBindingDisplayString(_bindingIndex);
            }
        }
    }
}