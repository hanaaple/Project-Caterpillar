using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public Vector2 curInput;


    [SerializeField] private InputActionReference inputActionReference;

    [SerializeField] private bool excludeMouse = true;

    [Range(0, 10)] [SerializeField] private int selectedBinding;

    [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

    [Header("Binding Info - DO NOT EDIT")] [SerializeField]
    private InputBinding inputBinding;

    [Header("Ui Fields")] [SerializeField] private TMP_Text actionText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private TMP_Text rebindText;
    [SerializeField] private Button resetButton;
    
    private int _bindingIndex;
    private string _actionName;

    private void OnEnable()
    {
        rebindButton.onClick.AddListener(DoRebind);
        resetButton.onClick.AddListener(ResetBinding);

        if (inputActionReference != null)
        {
            InputManager.LoadBindingOverride(_actionName);
            GetBindingInfo();
            UpdateUi();
        }

        InputManager.RebindComplete += UpdateUi;
        InputManager.RebindCanceled += UpdateUi;
    }

    private void OnDisable()
    {
        InputManager.RebindComplete -= UpdateUi;
        InputManager.RebindCanceled -= UpdateUi;
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
        if (actionText != null)
        {
            actionText.text = _actionName;
        }

        if (rebindText != null)
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
        InputManager.StartRebind(_actionName, _bindingIndex, rebindText, excludeMouse);
    }

    private void ResetBinding()
    {
        InputManager.ResetBinding(_actionName, _bindingIndex);
        UpdateUi();
    }


    void Start()
    {
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Move.performed += delegate(InputAction.CallbackContext context)
        {
            curInput = context.ReadValue<Vector2>();
        };
        
        playerActions.Move.canceled += delegate(InputAction.CallbackContext context)
        {
            curInput = context.ReadValue<Vector2>();
        };
    }
}