using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputControl inputControl;

    public static event Action RebindComplete;
    public static event Action RebindCanceled;
    public static event Action RebindEnd;
    public static event Action RebindLoad;
    
    public static event Action RebindReset;
    
    public static event Action<InputAction, int> RebindStarted;

    //action (Move, dialogue, interact ...), ID (1 - up, 2 - down ...)
    public static List<InputActions> inputActions = new();

    public class InputActions
    {
        public InputAction inputAction;
        public int bindingIndex;
        public string originalDisplayName;
    }

    public static bool IsChanged()
    {
        return inputActions.Count > 0;
    }

    public static void EndChange(bool isSave)
    {
        if (isSave)
        {
            foreach (var inputAction in inputActions)
            {
                SaveBindingOverride(inputAction.inputAction);
            }
        }
        else
        {
            foreach (var inputAction in inputActions)
            {
                LoadBindingOverride(inputAction.inputAction.name);
            }
        }
        inputActions.Clear();
        RebindEnd?.Invoke();
    }

    private void Awake()
    {
        if (inputControl == null)
        {
            Debug.Log("Awake");
            inputControl = new InputControl();
        }
    }

    public static void StartRebind(string actionName, int bindingIndex, GameObject bindingPanel, TMP_Text statusText,
        bool excludeMouse)
    {
        InputAction inputAction = inputControl.asset.FindAction(actionName);
        if (inputAction == null || inputAction.bindings.Count <= bindingIndex)
        {
            Debug.Log("Couldn't find action or binding");
            return;
        }

        if (inputAction.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < inputAction.bindings.Count && inputAction.bindings[firstPartIndex].isComposite)
            {
                DoRebind(inputAction, bindingIndex, statusText, bindingPanel, true, excludeMouse);
            }
        }
        else
        {
            DoRebind(inputAction, bindingIndex, statusText, bindingPanel, false, excludeMouse);
        }
    }

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, TMP_Text statusText,
        GameObject bindingPanel,
        bool allCompositeParts, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0)
        {
            return;
        }

        bindingPanel.SetActive(true);
        statusText.text = $"Press a {actionToRebind.expectedControlType}";

        actionToRebind.Disable();

        string originBindingName = GetBindingName(actionToRebind.name, bindingIndex);

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(_ =>
            {
                actionToRebind.Enable();
                _.Dispose();
                if (allCompositeParts)
                {
                    var nextBindingIndex = bindingIndex + 1;
                    if (nextBindingIndex < actionToRebind.bindings.Count &&
                        actionToRebind.bindings[nextBindingIndex].isComposite)
                    {
                        DoRebind(actionToRebind, nextBindingIndex, statusText, bindingPanel, true, excludeMouse);
                    }
                }

                string bindingName = GetBindingName(actionToRebind.name, bindingIndex);
                var bind = inputActions.Find(item => item.inputAction.name == actionToRebind.name &&
                                                     item.bindingIndex == bindingIndex);


                if (bind == null)
                {
                    Debug.Log(originBindingName + "  " + bindingName);
                }
                else
                {
                    Debug.Log(bind.originalDisplayName + "  " + bindingName);   
                }
                if (bind == null && originBindingName != bindingName || bind != null && bind.originalDisplayName != bindingName)
                {
                    inputActions.Remove(bind);
                    if (bind != null)
                    {
                        originBindingName = bind.originalDisplayName;
                    }
                    inputActions.Add(new InputActions
                    {
                        inputAction = actionToRebind,
                        bindingIndex = bindingIndex,
                        originalDisplayName = originBindingName
                    });
                }
                else
                {
                    inputActions.Remove(bind);
                    Debug.Log("하이  " + inputActions.Count);
                }

                RebindComplete?.Invoke();
                bindingPanel.SetActive(false);
            })
            .OnCancel(_ =>
            {
                actionToRebind.Enable();
                _.Dispose();
                RebindCanceled?.Invoke();
                bindingPanel.SetActive(false);
            });

        if (excludeMouse)
        {
            rebind.WithControlsExcluding("Mouse");
        }

        RebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start();
    }

    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (inputControl == null)
        {
            inputControl = new InputControl();
        }

        InputAction action = inputControl.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    public static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        if (inputControl == null)
        {
            inputControl = new InputControl();
        }

        InputAction action = inputControl.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            string loadActionMap = PlayerPrefs.GetString(action.actionMap + action.name + i);
            if (!string.IsNullOrEmpty(loadActionMap))
            {
                action.ApplyBindingOverride(i, loadActionMap);
                Debug.Log("로드");
            }
        }
        RebindLoad?.Invoke();
    }

    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = inputControl.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Could not find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }

        SaveBindingOverride(action);
        inputActions.Clear();
        
        RebindReset?.Invoke();
    }
}