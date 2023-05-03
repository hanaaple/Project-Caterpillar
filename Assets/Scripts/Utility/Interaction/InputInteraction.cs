using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Player;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Utility.Interaction.InputInteraction))]
public class InputInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var generator = (Utility.Interaction.InputInteraction)target;
        if (GUILayout.Button("SetDialogue"))
        {
            generator.SetDialogue();
            EditorUtility.SetDirty(generator);
        }
        
        if (GUILayout.Button("Debug"))
        {
            generator.Debugg();
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif

namespace Utility.Interaction
{
    public class InputInteraction : Interaction
    {
        [FormerlySerializedAs("ui")] [SerializeField]
        private GameObject floatingMark;

        [SerializeField] private Vector2 offset;

        private Action _onInteract;

        protected override void Awake()
        {
            base.Awake();

            _onInteract = () =>
            {
                Debug.Log("인터랙트");
                floatingMark.SetActive(false);

                StartInteraction();
            };
        }

        protected override void Start()
        {
            base.Start();

            if (floatingMark)
            {
                floatingMark.transform.position = transform.position + (Vector3) offset;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsInteractable())
            {
                return;
            }

            floatingMark.SetActive(true);
            var player = col.GetComponent<TestPlayer>();
            player.onInteractAction = _onInteract;
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            floatingMark.SetActive(false);
        }
    }
}