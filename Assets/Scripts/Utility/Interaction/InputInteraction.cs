using System;
using UnityEngine;
using UnityEngine.Serialization;
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
            generator.DebugInteractionData();
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
                Debug.Log($"인터랙트 - {gameObject}");
                floatingMark.SetActive(false);

                OnEndInteraction += () =>
                {
                    if (IsInteractable())
                    {
                        floatingMark.SetActive(true);
                    }
                };
                StartInteraction();
            };
        }

        protected override void Start()
        {
            base.Start();

            floatingMark.transform.position = transform.position + (Vector3) offset;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsInteractable() || !col.isTrigger || !col.TryGetComponent(out Player.Player player))
            {
                return;
            }

            // Debug.Log($"들어옴! {col} {col.gameObject}");
            floatingMark.SetActive(true);
            player.OnInteractAction = _onInteract;
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (!col.isTrigger || !col.TryGetComponent(out Player.Player player))
            {
                return;
            }
            
            // Debug.Log($"나감! {col} {col.gameObject}");
            floatingMark.SetActive(false);
            player.OnInteractAction = null;
        }
    }
}