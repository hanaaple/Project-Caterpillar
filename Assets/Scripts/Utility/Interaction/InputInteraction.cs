using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
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
        [Serializable]
        public class FloatingMark
        {
            public GameObject floatingMark;
            public int index;
            public Vector2 offset;
        }
        
        [Header("Floating Mark")]
        [FormerlySerializedAs("floatingMark")] [FormerlySerializedAs("ui")] [SerializeField]
        private GameObject defaultFloatingMark;

        [SerializeField] private FloatingMark[] floatingMarks;

        [FormerlySerializedAs("offset")] [SerializeField] private Vector2 defaultOffset;

        private Action _onInteract;

        protected override void Awake()
        {
            base.Awake();

            _onInteract = () =>
            {
                Debug.Log($"인터랙트 - {gameObject}");
                if (defaultFloatingMark)
                {
                    defaultFloatingMark.SetActive(false);
                }

                OnEndInteraction += () =>
                {
                    if (IsInteractable())
                    {
                        if (defaultFloatingMark)
                        {
                            defaultFloatingMark.SetActive(true);
                        }
                    }
                };
                StartInteraction();
            };
        }

        private void Update()
        {
            if (defaultFloatingMark && defaultFloatingMark.activeSelf && Camera.main)
            {
                defaultFloatingMark.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3) defaultOffset);
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsInteractable() || !col.isTrigger || !col.TryGetComponent(out Player.Player player))
            {
                return;
            }

            if (floatingMarks?.Length > 0)
            {
                var floatingMark = Array.Find(floatingMarks, item => item.index == interactionIndex);
                if (floatingMark != null)
                {
                    defaultFloatingMark = floatingMark.floatingMark;
                    defaultOffset = floatingMark.offset;
                }
            }

            // Debug.Log($"들어옴! {col} {col.gameObject}");
        if (defaultFloatingMark)
            {
                defaultFloatingMark.transform.SetParent(PlayUIManager.Instance.floatingMarkParent.transform);
                defaultFloatingMark.SetActive(true);
            }

            player.OnInteractAction = _onInteract;
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (!col.isTrigger || !col.TryGetComponent(out Player.Player player))
            {
                return;
            }
            
            // Debug.Log($"나감! {col} {col.gameObject}");
            if (defaultFloatingMark)
            {
                defaultFloatingMark.SetActive(false);
            }

            player.OnInteractAction = null;
        }

        private void OnDestroy()
        {
            Destroy(defaultFloatingMark);
        }
    }
}