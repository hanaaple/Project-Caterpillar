using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Audio;
using Utility.Core;
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
        
        if (GUILayout.Button("Set WaitInteraction"))
        {
            generator.SetByWaitInteraction();
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

        [Header("Floating Mark")] [FormerlySerializedAs("floatingMark")] [FormerlySerializedAs("ui")] [SerializeField]
        private GameObject defaultFloatingMark;
        [SerializeField] private FloatingMark[] floatingMarks;
        [FormerlySerializedAs("offset")] [SerializeField]
        private Vector2 defaultOffset;
        
        [SerializeField] private AudioClip interactAudioClip;

        private Camera _camera;

        protected override void Awake()
        {
            _camera = Camera.main;
            base.Awake();
        }

        private void Update()
        {
            if (defaultFloatingMark && defaultFloatingMark.activeSelf && _camera)
            {
                defaultFloatingMark.transform.position =
                    _camera.WorldToScreenPoint(transform.position + (Vector3) defaultOffset);
            }
        }

        public override void StartInteraction(int index = -1)
        {
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
            
            base.StartInteraction(index);
        }
        
        public void StartInputInteraction(int index = -1)
        {
            AudioManager.Instance.PlaySfx(interactAudioClip);
            
            StartInteraction(index);
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

            if (defaultFloatingMark)
            {
                defaultFloatingMark.transform.SetParent(PlayUIManager.Instance.floatingMarkParent.transform);
                defaultFloatingMark.SetActive(true);
            }

            PlayerManager.Instance.PushInteraction(this);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (!col.isTrigger || !col.TryGetComponent(out Player.Player player))
            {
                return;
            }

            if (defaultFloatingMark)
            {
                defaultFloatingMark.SetActive(false);
            }

            PlayerManager.Instance.PopInteraction(this);
        }

        private void OnDestroy()
        {
            Destroy(defaultFloatingMark);
        }
    }
}