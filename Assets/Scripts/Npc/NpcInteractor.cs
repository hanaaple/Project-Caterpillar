using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Dialogue;
#if UNITY_EDITOR
using Utility.JsonLoader;
using UnityEditor;

[CustomEditor(typeof(NpcInteractor))]
public class CubeGenerateButton : Editor
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

    [SerializeField] private GameObject ui;
    [SerializeField] private Vector2 offset;

    private Action<InputAction.CallbackContext> _onInteract;

#if UNITY_EDITOR
    [SerializeField] private DialogueProps dialogueProps;

    public void ShowDialogue()
    {
        dialogueProps.datas = JsonHelper.GetJsonArray<DialogueItemProps>(dialogue.text);
    }
#endif

    private void Awake()
    {
        _onInteract = _ =>
        {
            if (ui.activeSelf)
            {
                ui.SetActive(false);
                DialogueController.instance.Converse(dialogue.text);
            }
        };
    }

    private void Start()
    {
        SetUIPos();
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Enable();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Interact.performed += _onInteract;
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Interact.performed -= _onInteract;
    }

    private void SetUIPos()
    {
        if (!ui)
        {
            return;
        }

        ui.transform.position = transform.position + (Vector3) offset;
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ui.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ui.SetActive(false);
        }
    }
}