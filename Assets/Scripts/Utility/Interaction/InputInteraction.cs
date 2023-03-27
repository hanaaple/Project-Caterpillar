using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.InputSystem;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Utility.Interaction.InputInteraction))]
public class InputInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var generator = (Utility.Interaction.InputInteraction)target;
        if (GUILayout.Button("ShowDialogue"))
        {
            generator.ShowDialogue();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif

namespace Utility.Interaction
{
    public class InputInteraction : Interaction
    {
        [FormerlySerializedAs("ui")] [SerializeField] private GameObject floatingMark;
        [SerializeField] private Vector2 offset;

        private Action<InputAction.CallbackContext> _onInteract;

        protected override void Awake()
        {
            base.Awake();
            
            _onInteract = _ =>
            {
                if (!floatingMark.activeSelf || !GameManager.IsCharacterControlEnable())
                {
                    return;
                }
                floatingMark.SetActive(false);

                StartInteraction();
            };
        }

        protected override void Start()
        {
            base.Start();

            if (floatingMark)
            {
                floatingMark.transform.position = transform.position + (Vector3)offset;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsInteractable())
            {
                return;
            }
            floatingMark.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            floatingMark.SetActive(false);
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
    }
}