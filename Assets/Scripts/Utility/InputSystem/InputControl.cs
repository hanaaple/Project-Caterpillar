//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Scripts/Utility/InputSystem/InputControl.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @InputControl : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputControl()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputControl"",
    ""maps"": [
        {
            ""name"": ""Input"",
            ""id"": ""d1a45884-2480-4674-923a-d355311d18e1"",
            ""actions"": [
                {
                    ""name"": ""Arrow"",
                    ""type"": ""Value"",
                    ""id"": ""60d053b6-8023-4d44-8066-01e5a24bc7ff"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Execute"",
                    ""type"": ""Button"",
                    ""id"": ""c4017a23-0745-4845-b13d-9e69ddff1e94"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""99d753ef-87c0-4e26-b3ad-37daffb388e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Tab"",
                    ""type"": ""Button"",
                    ""id"": ""7a6bca68-1571-4c2d-be50-94cd0430b238"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""07429472-8ba7-4536-b3b8-a0168f098e0f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""553d4523-ae73-4104-99ac-694b4d606a47"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Inventory"",
                    ""type"": ""Button"",
                    ""id"": ""f1458855-33a7-434f-9e93-b3af2449b947"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AnyKey"",
                    ""type"": ""Button"",
                    ""id"": ""c11f3372-3b31-4353-9b35-05b5183a42cc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""type"": ""Button"",
                    ""id"": ""61f0287a-ba4a-4210-b465-0a8e58e0c837"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""f2453e94-092d-45d5-a1da-d08e712a6173"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Arrow"",
                    ""id"": ""e0bb25d9-a2e2-4cde-b9b5-84a427f34212"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Arrow"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e022fa43-971c-4301-aed8-01a93d705092"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Arrow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1ff8873e-7afd-4409-b705-0cc190405d93"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Arrow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""71c014b3-af0d-43e7-a084-60f693038ae5"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Arrow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""bec122fa-a1e8-4023-b8a3-cf5698295f30"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Arrow"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""e8a85ff5-1c92-4006-82be-45d2be9b214e"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Execute"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bbb0d5a3-22ff-4d98-84e8-a30116931ed8"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Execute"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1c6506cc-26b0-488b-875a-f8351d0a5ee3"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Arrow"",
                    ""id"": ""f72735c7-d78c-418b-9349-9751ccaac331"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8368b8ab-f37c-4557-a131-aa4ac7fc70f9"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""03e9a76f-0391-4999-80c4-08c600cfd3d4"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""ec7a1e92-7c04-42fe-98d8-5963465e3da9"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""be1bc682-9378-4efb-be83-9f11dbd118b2"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c1b23c73-480a-4f7b-a413-ddc94ef48804"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""38d8598b-1358-443e-8ca6-34bbbf9e11c9"",
                    ""path"": ""<Keyboard>/anyKey"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""AnyKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""96b8916d-f191-4c1f-9360-1e7cdd4a52bc"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse"",
                    ""action"": ""LeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5a09f9f7-dc1b-4974-ac10-9dd4976811cc"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""74b97f2d-6236-4873-bb5f-b651329f2bfe"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Tab"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2209213d-6491-4ea8-a706-b65c592a2615"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4948b37a-53e0-42be-bad8-3151da046e66"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1c9839b9-b5a4-4601-96ad-f4cfc95c6cfc"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""609a2d50-ab33-4834-afbc-10156a13bfac"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ee88bfa3-c48e-44f8-af72-4087ce1f9a8f"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2421631e-29c2-4ec6-bab8-cc6820c9798f"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c822f8b1-f991-4ad0-8c5b-ea4a338113cf"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b5f416b4-0f37-4ff7-9d07-57bd696ef54b"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""145f0cf8-e0c8-4bfb-b813-b8c14eb56047"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""23852113-0387-4843-b64f-551ea8c60780"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ca5828e-8c72-4ffb-a8bb-e3894bafe7a2"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""023ee184-1f01-44b1-becc-9aa26ddc5dd2"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e57eaf0f-1a78-405c-9ebc-989d4de8ac5b"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2dc13a10-8666-4109-bf35-88d1456cf4d2"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a259e5d2-8250-479e-8351-a3b223991f46"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cc4f1f9f-df07-4979-9b7e-828c63cc0ea0"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""226649e8-2bf3-4d29-be83-3ba4eee57875"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""46ebd8f8-00bc-4510-80a7-fad9fc27e3e3"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bd0932ed-5bbd-4208-abb3-ef874a8cf3d1"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""83152019-b04b-4c05-a6a6-c53bec370fc3"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4f25fa5f-41c9-4b58-b57b-c063f8014436"",
                    ""path"": ""<Keyboard>/u"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""755c968b-53f6-4949-9653-d9851a99204b"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4e502d39-5d26-462a-a898-b4a9164c7a05"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""48291168-c7c9-4496-a703-c48268182c2e"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d84f39d6-f3c3-427d-b116-24b43f4ecba0"",
                    ""path"": ""<Keyboard>/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27a9ffa0-14e3-441a-bf54-7e91e614c492"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""22b06b30-389e-489c-b5bb-af478e40d072"",
                    ""path"": ""<Keyboard>/backspace"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyBoard"",
                    ""action"": ""Keyboard"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""KeyBoard"",
            ""bindingGroup"": ""KeyBoard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Mobile"",
            ""bindingGroup"": ""Mobile"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Mouse"",
            ""bindingGroup"": ""Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""GamePad"",
            ""bindingGroup"": ""GamePad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Input
        m_Input = asset.FindActionMap("Input", throwIfNotFound: true);
        m_Input_Arrow = m_Input.FindAction("Arrow", throwIfNotFound: true);
        m_Input_Execute = m_Input.FindAction("Execute", throwIfNotFound: true);
        m_Input_Pause = m_Input.FindAction("Pause", throwIfNotFound: true);
        m_Input_Tab = m_Input.FindAction("Tab", throwIfNotFound: true);
        m_Input_Move = m_Input.FindAction("Move", throwIfNotFound: true);
        m_Input_Interact = m_Input.FindAction("Interact", throwIfNotFound: true);
        m_Input_Inventory = m_Input.FindAction("Inventory", throwIfNotFound: true);
        m_Input_AnyKey = m_Input.FindAction("AnyKey", throwIfNotFound: true);
        m_Input_Keyboard = m_Input.FindAction("Keyboard", throwIfNotFound: true);
        m_Input_LeftClick = m_Input.FindAction("LeftClick", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Input
    private readonly InputActionMap m_Input;
    private IInputActions m_InputActionsCallbackInterface;
    private readonly InputAction m_Input_Arrow;
    private readonly InputAction m_Input_Execute;
    private readonly InputAction m_Input_Pause;
    private readonly InputAction m_Input_Tab;
    private readonly InputAction m_Input_Move;
    private readonly InputAction m_Input_Interact;
    private readonly InputAction m_Input_Inventory;
    private readonly InputAction m_Input_AnyKey;
    private readonly InputAction m_Input_Keyboard;
    private readonly InputAction m_Input_LeftClick;
    public struct InputActions
    {
        private @InputControl m_Wrapper;
        public InputActions(@InputControl wrapper) { m_Wrapper = wrapper; }
        public InputAction @Arrow => m_Wrapper.m_Input_Arrow;
        public InputAction @Execute => m_Wrapper.m_Input_Execute;
        public InputAction @Pause => m_Wrapper.m_Input_Pause;
        public InputAction @Tab => m_Wrapper.m_Input_Tab;
        public InputAction @Move => m_Wrapper.m_Input_Move;
        public InputAction @Interact => m_Wrapper.m_Input_Interact;
        public InputAction @Inventory => m_Wrapper.m_Input_Inventory;
        public InputAction @AnyKey => m_Wrapper.m_Input_AnyKey;
        public InputAction @Keyboard => m_Wrapper.m_Input_Keyboard;
        public InputAction @LeftClick => m_Wrapper.m_Input_LeftClick;
        public InputActionMap Get() { return m_Wrapper.m_Input; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InputActions set) { return set.Get(); }
        public void SetCallbacks(IInputActions instance)
        {
            if (m_Wrapper.m_InputActionsCallbackInterface != null)
            {
                @Arrow.started -= m_Wrapper.m_InputActionsCallbackInterface.OnArrow;
                @Arrow.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnArrow;
                @Arrow.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnArrow;
                @Execute.started -= m_Wrapper.m_InputActionsCallbackInterface.OnExecute;
                @Execute.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnExecute;
                @Execute.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnExecute;
                @Pause.started -= m_Wrapper.m_InputActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnPause;
                @Tab.started -= m_Wrapper.m_InputActionsCallbackInterface.OnTab;
                @Tab.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnTab;
                @Tab.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnTab;
                @Move.started -= m_Wrapper.m_InputActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnMove;
                @Interact.started -= m_Wrapper.m_InputActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnInteract;
                @Inventory.started -= m_Wrapper.m_InputActionsCallbackInterface.OnInventory;
                @Inventory.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnInventory;
                @Inventory.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnInventory;
                @AnyKey.started -= m_Wrapper.m_InputActionsCallbackInterface.OnAnyKey;
                @AnyKey.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnAnyKey;
                @AnyKey.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnAnyKey;
                @Keyboard.started -= m_Wrapper.m_InputActionsCallbackInterface.OnKeyboard;
                @Keyboard.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnKeyboard;
                @Keyboard.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnKeyboard;
                @LeftClick.started -= m_Wrapper.m_InputActionsCallbackInterface.OnLeftClick;
                @LeftClick.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnLeftClick;
                @LeftClick.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnLeftClick;
            }
            m_Wrapper.m_InputActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Arrow.started += instance.OnArrow;
                @Arrow.performed += instance.OnArrow;
                @Arrow.canceled += instance.OnArrow;
                @Execute.started += instance.OnExecute;
                @Execute.performed += instance.OnExecute;
                @Execute.canceled += instance.OnExecute;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Tab.started += instance.OnTab;
                @Tab.performed += instance.OnTab;
                @Tab.canceled += instance.OnTab;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
                @Inventory.started += instance.OnInventory;
                @Inventory.performed += instance.OnInventory;
                @Inventory.canceled += instance.OnInventory;
                @AnyKey.started += instance.OnAnyKey;
                @AnyKey.performed += instance.OnAnyKey;
                @AnyKey.canceled += instance.OnAnyKey;
                @Keyboard.started += instance.OnKeyboard;
                @Keyboard.performed += instance.OnKeyboard;
                @Keyboard.canceled += instance.OnKeyboard;
                @LeftClick.started += instance.OnLeftClick;
                @LeftClick.performed += instance.OnLeftClick;
                @LeftClick.canceled += instance.OnLeftClick;
            }
        }
    }
    public InputActions @Input => new InputActions(this);
    private int m_KeyBoardSchemeIndex = -1;
    public InputControlScheme KeyBoardScheme
    {
        get
        {
            if (m_KeyBoardSchemeIndex == -1) m_KeyBoardSchemeIndex = asset.FindControlSchemeIndex("KeyBoard");
            return asset.controlSchemes[m_KeyBoardSchemeIndex];
        }
    }
    private int m_MobileSchemeIndex = -1;
    public InputControlScheme MobileScheme
    {
        get
        {
            if (m_MobileSchemeIndex == -1) m_MobileSchemeIndex = asset.FindControlSchemeIndex("Mobile");
            return asset.controlSchemes[m_MobileSchemeIndex];
        }
    }
    private int m_MouseSchemeIndex = -1;
    public InputControlScheme MouseScheme
    {
        get
        {
            if (m_MouseSchemeIndex == -1) m_MouseSchemeIndex = asset.FindControlSchemeIndex("Mouse");
            return asset.controlSchemes[m_MouseSchemeIndex];
        }
    }
    private int m_GamePadSchemeIndex = -1;
    public InputControlScheme GamePadScheme
    {
        get
        {
            if (m_GamePadSchemeIndex == -1) m_GamePadSchemeIndex = asset.FindControlSchemeIndex("GamePad");
            return asset.controlSchemes[m_GamePadSchemeIndex];
        }
    }
    public interface IInputActions
    {
        void OnArrow(InputAction.CallbackContext context);
        void OnExecute(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnTab(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
        void OnAnyKey(InputAction.CallbackContext context);
        void OnKeyboard(InputAction.CallbackContext context);
        void OnLeftClick(InputAction.CallbackContext context);
    }
}
