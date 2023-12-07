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
        private class BindInputAction
        {
            public InputAction InputAction;
            public int BindingIndex;
            public string OriginalDisplayName;
        }

        private static InputControl _inputControl;

        public static InputControl InputControl
        {
            get { return _inputControl ??= new InputControl(); }
        }

        internal static readonly List<InputActions> InputActionsList = new();
        private static bool _isPlaying;

        private static readonly Queue<KeyValuePair<InputActions, bool>> QueueInputActions = new();

        //action (Move, dialogue, interact ...), ID (1 - up, 2 - down ...)
        private static readonly List<BindInputAction> BindInputActions = new();

        public static event Action RebindComplete;
        public static event Action RebindCanceled;
        public static event Action RebindEnd;
        public static event Action RebindLoad;
        public static event Action RebindReset;
        public static event Action<InputAction, int> RebindStarted;

        public static void ResetInputAction()
        {
            if (InputActionsList.Count == 0)
            {
                return;
            }

            foreach (var inputActions in InputActionsList)
            {
                QueueInputActions.Enqueue(new KeyValuePair<InputActions, bool>(inputActions, false));
            }

            EnqueueInputAction();
        }

        public static void PushInputAction(InputActions inputActions)
        {
            Debug.Log($"Push InputAction {inputActions.Name}\n" +
                      $"대기 개수: {QueueInputActions.Count}");
            QueueInputActions.Enqueue(new KeyValuePair<InputActions, bool>(inputActions, true));
            EnqueueInputAction();
        }

        public static void PopInputAction(InputActions inputActions)
        {
            Debug.Log($"Pop InputAction {inputActions.Name}\n" +
                      $"대기 개수: {QueueInputActions.Count}");
            QueueInputActions.Enqueue(new KeyValuePair<InputActions, bool>(inputActions, false));
            EnqueueInputAction();
        }

        /// <summary>
        /// Error - Assertion failed z 쭉 누르면
        /// </summary>
        private static async void AsyncEnqueueInputAction()
        {
            if (_isPlaying)
            {
                return;
            }
            // InputControl.Input.Disable();
            _isPlaying = true;
            while (QueueInputActions.Count > 0)
            {
                var dequeue = QueueInputActions.Dequeue();
                if (dequeue.Value)
                {
                    if (InputActionsList.Count > 0)
                    {
                        InputActionsList.Last().SetAction(false);
                        Debug.LogWarning($"Set InputAction {InputActionsList.Last().Name}");
                        // Debug.Log($"Set Disable Last {InputActionsList.Last().Name}");
                    }

                    await Task.Delay(200);
                    
                    InputActionsList.Add(dequeue.Key);
                    dequeue.Key.SetAction(true);
                    Debug.Log(
                        $"Push InputAction {InputActionsList.Count}   {dequeue.Key.Name}\n" +
                        $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");
                }
                else
                {
                    if (!InputActionsList.Contains(dequeue.Key))
                    {
                        Debug.LogWarning($"{dequeue.Key.Name}이 존재하지 않음");
                        break;
                    }

                    dequeue.Key.SetAction(false);
                    InputActionsList.Remove(dequeue.Key);
                    
                    Debug.Log(
                        $"Pop InputAction {InputActionsList.Count}   {dequeue.Key.Name}\n" +
                        $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");

                    if (InputActionsList.Count > 0)
                    {
                        // Debug.Log($"Set Enable Last {InputActionsList.Last().Name}");
                        await Task.Delay(200);
                        InputActionsList.Last().SetAction(true);
                    }
                }
            }
            _isPlaying = false;
            if (InputActionsList.Count > 0)
            {
                InputControl.Input.Enable();
            }
            else
            {
                InputControl.Input.Disable();
            }
        }

        /// <summary>
        /// Error - Event Works by SetEnable(Last)
        /// </summary>
        private static void EnqueueInputAction()
        {
            var inputActions = QueueInputActions.Dequeue();

            Debug.Log($"SetInputAction {inputActions.Key.Name} {inputActions.Value}");
            if (inputActions.Value)
            {
                if (InputActionsList.Count > 0)
                {
                    InputActionsList.Last().SetAction(false);
                }

                InputActionsList.Add(inputActions.Key);
                inputActions.Key.SetAction(true);

                Debug.Log($"Push {InputActionsList.Count}   {inputActions.Key.Name}\n" +
                          $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");
            }
            else if (InputActionsList.Contains(inputActions.Key))
            {
                InputActionsList.Remove(inputActions.Key);
                inputActions.Key.SetAction(false);

                if (InputActionsList.Count > 0)
                {
                    InputActionsList.Last().SetAction(true);
                }

                Debug.Log($"Pop {InputActionsList.Count}   {inputActions.Key.Name}\n" +
                          $"{string.Concat(InputActionsList.Select(item => " > " + item.Name))}");
            }

            // Last InputAction Works Immediately How Can I Solve this problem?
            if (InputActionsList.Count > 0)
            {
                InputControl.Input.Enable();
            }
            else
            {
                InputControl.Input.Disable();
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
            TMP_Text statusText, bool excludeMouse, string[] excludeInputActionPaths)
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
                    DoRebind(inputAction, bindingIndex, statusText, bindingPanel, true, excludeMouse,
                        excludeInputActionPaths);
                }
            }
            else
            {
                DoRebind(inputAction, bindingIndex, statusText, bindingPanel, false, excludeMouse,
                    excludeInputActionPaths);
            }
        }

        private static void DoRebind(InputAction actionToRebind, int bindingIndex, TMP_Text statusText,
            GameObject bindingPanel, bool allCompositeParts, bool excludeMouse, string[] excludeInputActionPaths)
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
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithControlsExcluding("<Keyboard>/anyKey")
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
                            DoRebind(actionToRebind, nextBindingIndex, statusText, bindingPanel, true, excludeMouse,
                                excludeInputActionPaths);
                        }
                    }

                    // var action = InputControl.asset.FindAction(actionName);
                    string bindingName = GetBindingName(actionToRebind.name, bindingIndex);
                    var bind = BindInputActions.Find(item => item.InputAction.name == actionToRebind.name &&
                                                             item.BindingIndex == bindingIndex);


                    if (bind == null)
                    {
                        Debug.Log(
                            $"{originBindingName},  {bindingName}, {actionToRebind.bindings[bindingIndex].effectivePath}");
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

                    foreach (var inputControlBinding in _inputControl.bindings)
                    {
                        // 이미 바인딩 되어있는 키(Rebind 가능한 것만)로 바꿀 경우
                        // 서로 키 바꾸기
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

            if (excludeInputActionPaths != null)
            {
                foreach (var path in excludeInputActionPaths)
                {
                    rebind.WithControlsExcluding(path);
                    Debug.Log($"불가능 - {path}");
                }
            }

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
                    // Debug.Log("로드");
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

        public static bool IsAnyKeyDuplicated(InputAction a, InputAction b)
        {
            var aArray = a.bindings.Select(inputBinding => inputBinding.ToDisplayString()).ToArray();
            var bArray = b.bindings.Select(inputBinding => inputBinding.ToDisplayString()).ToArray();
            var array = aArray.Concat(bArray).Distinct().ToArray();

//            Debug.Log(string.Join(", ", array));
//            Debug.Log($"Is Duplicated - {array.Length != aArray.Length + bArray.Length}");

//            Debug.Log($"A: {string.Join(", ", aArray)}");
//            Debug.Log($"B: {string.Join(", ", bArray)}");

            return array.Length != aArray.Length + bArray.Length;
        }
    }
}