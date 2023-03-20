using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Dialogue;
using Utility.InputSystem;
#if UNITY_EDITOR
using Utility.JsonLoader;
using UnityEditor;

[CustomEditor(typeof(NpcInteractor))]
public class NpcInteractorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NpcInteractor generator = (NpcInteractor)target;
        if (GUILayout.Button("ShowDialogue"))
        {
            generator.ShowDialogue();
        }
    }
}

#endif

public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;

    [FormerlySerializedAs("ui")] [SerializeField] private GameObject floatingMark;
    [SerializeField] private Vector2 offset;

    private Action<InputAction.CallbackContext> _onInteract;

    [SerializeField] private DialogueData dialogueData;

    private bool _isEnable;

#if UNITY_EDITOR
    public void ShowDialogue()
    {
        dialogueData.dialogueElements = JsonHelper.GetJsonArray<DialogueElement>(dialogue.text);
        for (var index = 0; index < dialogueData.dialogueElements.Length; index++)
        {
            var dialogueDataDialogueElement = dialogueData.dialogueElements[index];
            if (dialogueDataDialogueElement.dialogueType == DialogueType.Wait)
            {
                Debug.Log($"{index}번째 Element Interactor 세팅 하세요.");
            }
        }
    }
#endif

    private void Awake()
    {
        //이거도 저기 부분에 합쳐라
        dialogueData.onDialogueStart = () => { _isEnable = false; };
        dialogueData.onDialogueWaitClear = () => { _isEnable = true; };
        dialogueData.onDialogueEnd = () =>
        {
            _isEnable = true;
            floatingMark.SetActive(true);
        };

        _onInteract = _ =>
        {
            if (!floatingMark.activeSelf || !GameManager.Instance.IsCharacterControlEnable())
            {
                return;
            }
            
            floatingMark.SetActive(false);
            if (dialogueData.dialogueElements.Length != 0)
            {
                DialogueController.Instance.StartDialogue(dialogueData);
            }
            else
            {
                DialogueController.Instance.StartDialogue(dialogue.text);
            }
        };
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        _isEnable = true;
        
        if (!floatingMark)
        {
            floatingMark.transform.position = transform.position + (Vector3)offset;
        }
    }

    private void OnEnable()
    {
        InputManager.SetPlayerAction(true);
        var playerActions = InputManager.InputControl.PlayerActions;
        playerActions.Interact.performed += _onInteract;
    }

    private void OnDisable()
    {
        InputManager.SetPlayerAction(false);
        var playerActions = InputManager.InputControl.PlayerActions;
        playerActions.Interact.performed -= _onInteract;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_isEnable)
        {
            floatingMark.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        floatingMark.SetActive(false);
    }
}