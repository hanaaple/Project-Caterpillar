using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utility.InputSystem
{
    public static class InputManager
    {
        private static InputControl _inputControl;

        public static InputControl InputControl
        {
            get
            {
                return _inputControl ??= new InputControl();
            }
        }
        
        private static int _inputActionCount;
        
        private static readonly object LockObject = new();

        private static readonly List<InputActions> InputActionsList = new();
        
        private static readonly Queue<InputActions> PushInputActions = new();
        
        private static readonly Queue<InputActions> PopInputActions = new();

        //action (Move, dialogue, interact ...), ID (1 - up, 2 - down ...)
        private static readonly List<BindInputAction> BindInputActions = new();
        
        public static event Action RebindComplete;
        public static event Action RebindCanceled;
        public static event Action RebindEnd;
        public static event Action RebindLoad;
        public static event Action RebindReset;
        public static event Action<InputAction, int> RebindStarted;

        private class BindInputAction
        {
            public InputAction InputAction;
            public int BindingIndex;
            public string OriginalDisplayName;
        }
        
        public static void PushInputAction(InputActions inputActions)
        {
            Debug.Log($"Push InputAction {inputActions.Name}");
            PushInputActions.Enqueue(inputActions);
            AsyncPushInputAction();
        }

        private static async void AsyncPushInputAction()
        {
            // Time.fixedDeltaTime
            await Task.Delay(20);

            lock (LockObject)
            {
                if (InputActionsList.Count > 0)
                {
                    InputActionsList.Last().SetAction(false);
                }

                var inputActions = PushInputActions.Dequeue(); // 1 -> 0
                InputActionsList.Add(inputActions);
                inputActions.SetAction(true);
                
                Debug.Log($"{InputActionsList.Count}\n" +
                          $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");
            }
        }

        public static void PopInputAction(InputActions inputActions)
        {
            Debug.Log($"Pop InputAction {inputActions.Name}");
            PopInputActions.Enqueue(inputActions);
            AsyncPopInputAction();
        }
        
        private static async void AsyncPopInputAction()
        {
            // Time.fixedDeltaTime
            await Task.Delay(20);

            lock (LockObject)
            {
                var inputActions = PopInputActions.Dequeue();
                inputActions.SetAction(false);
                InputActionsList.Remove(inputActions);
            
                Debug.Log($"{InputActionsList.Count}\n" +
                          $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");
                // 몇초후에 가능하도록 Add
                if (InputActionsList.Count > 0)
                {
                    InputActionsList.Last().SetAction(true);
                }
            }
        }

        public static void SetInputActions(bool isAdd)
        {
            if (isAdd)
            {
                if (_inputActionCount == 0)
                {
                    var inputActions = InputControl.Input;
                    inputActions.Enable();
                }

                _inputActionCount++;
            }
            else
            {
                _inputActionCount--;
                if (_inputActionCount == 0)
                {
                    var inputActions = InputControl.Input;
                    inputActions.Disable();
                }
                else if (_inputActionCount < 0)
                {
                    Debug.LogError("오류");
                }
            }
        }

        public static bool IsChanged()
        {
            return BindInputActions.Count > 0;
        }

        public static void SetChange(bool isSave)
        {
            if (isSave)
            {
                foreach (var inputAction in BindInputActions)
                {
                    SaveBindingOverride(inputAction.InputAction);
                }
            }
            else
            {
                foreach (var inputAction in BindInputActions)
                {
                    LoadBindingOverride(inputAction.InputAction.name);
                }
            }

            Debug.Log(isSave + ", " + "변경사항 - " + BindInputActions.Count);
            BindInputActions.Clear();
            RebindEnd?.Invoke();
        }

        public static void StartRebind(string actionName, int bindingIndex, GameObject bindingPanel,
            TMP_Text statusText, bool excludeMouse)
        {
            var inputAction = InputControl.asset.FindAction(actionName);
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
            GameObject bindingPanel, bool allCompositeParts, bool excludeMouse)
        {
            if (actionToRebind == null || bindingIndex < 0)
            {
                return;
            }

            bindingPanel.SetActive(true);
            //statusText.text = $"Press a {actionToRebind.expectedControlType}";
            statusText.text = $"";

            actionToRebind.Disable();

            var originBindingName = GetBindingName(actionToRebind.name, bindingIndex);

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
                    var bind = BindInputActions.Find(item => item.InputAction.name == actionToRebind.name &&
                                                             item.BindingIndex == bindingIndex);


                    if (bind == null)
                    {
                        Debug.Log(originBindingName + "  " + bindingName);
                    }
                    else
                    {
                        // Debug.Log(bind.originalDisplayName + "  " + bindingName);   
                    }

                    if (bind == null && originBindingName != bindingName ||
                        bind != null && bind.OriginalDisplayName != bindingName)
                    {
                        BindInputActions.Remove(bind);
                        if (bind != null)
                        {
                            originBindingName = bind.OriginalDisplayName;
                        }

                        BindInputActions.Add(new BindInputAction
                        {
                            InputAction = actionToRebind,
                            BindingIndex = bindingIndex,
                            OriginalDisplayName = originBindingName
                        });
                    }
                    else
                    {
                        BindInputActions.Remove(bind);
                        Debug.Log("하이  " + BindInputActions.Count);
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
            var action = InputControl.asset.FindAction(actionName);
            return action.GetBindingDisplayString(bindingIndex);
        }

        public static void SaveBindingOverride(InputAction action)
        {
            for (var i = 0; i < action.bindings.Count; i++)
            {
                PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
            }
        }

        public static void LoadBindingOverride(string actionName)
        {
            var action = InputControl.asset.FindAction(actionName);

            for (var i = 0; i < action.bindings.Count; i++)
            {
                var loadActionMap = PlayerPrefs.GetString(action.actionMap + action.name + i);
                if (!string.IsNullOrEmpty(loadActionMap))
                {
                    action.ApplyBindingOverride(i, loadActionMap);
                    Debug.Log("로드");
                }
                else
                {
                    if (action.bindings[i].isComposite)
                    {
                        for (var j = i; j < action.bindings.Count && action.bindings[j].isComposite; j++)
                        {
                            action.RemoveBindingOverride(j);
                        }
                    }
                    else
                    {
                        action.RemoveBindingOverride(i);
                    }
                }
            }

            RebindLoad?.Invoke();
        }

        public static void TempResetBinding(string actionName, int bindingIndex)
        {
            var action = InputControl.asset.FindAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Could not find action or binding");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                {
                    var originBindingName = GetBindingName(actionName, i);
                    action.RemoveBindingOverride(i);

                    if (originBindingName != GetBindingName(actionName, i))
                    {
                        BindInputActions.Add(new BindInputAction
                        {
                            InputAction = action,
                            BindingIndex = bindingIndex,
                            OriginalDisplayName = originBindingName
                        });
                    }
                }
            }
            else
            {
                var originBindingName = GetBindingName(actionName, bindingIndex);
                action.RemoveBindingOverride(bindingIndex);
                if (originBindingName != GetBindingName(actionName, bindingIndex))
                {
                    BindInputActions.Add(new BindInputAction
                    {
                        InputAction = action,
                        BindingIndex = bindingIndex,
                        OriginalDisplayName = originBindingName
                    });
                }
            }

            //isChanged인 경우 == 기존의 것과 리셋 후 바뀐 것이 하나라도 있으면  -> 이걸 체크해서 Add 시켜줘야한다.

            // 초기화 or 변경 이후 저장 및 취소하기 활성화

            // 변경시 inputActions에 Add 후, 다시한번 체크하여 추가(Save) 혹은 취소(Load)

            // 초기화시 변경사항이 있을 경우 Add, 다시한번 체크하여 Save or Load

            RebindReset?.Invoke();
        }

        public static void ResetBinding(string actionName, int bindingIndex)
        {
            var action = InputControl.asset.FindAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Could not find action or binding");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                for (var i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                {
                    action.RemoveBindingOverride(i);
                }
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }

            SaveBindingOverride(action);
            BindInputActions.Clear();

            RebindReset?.Invoke();
        }
    }
}