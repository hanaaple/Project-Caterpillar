using UnityEngine;
using Utility.Dialogue;

namespace Utility.Interaction
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(TriggerInteraction))]
    public class TriggerInteractionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var generator = (TriggerInteraction)target;
            if (GUILayout.Button("ShowDialogue"))
            {
                generator.ShowDialogue();
            }
        }
    }
#endif

    public class TriggerInteraction : Interaction
    {
        protected override void StartInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            if (!IsInteractable(index))
            {
                return;
            }

            base.StartInteraction(index);

            var interactionData = GetInteractionData(index);

            if (interactionData.jsonAsset)
            {
                DialogueController.Instance.StartDialogue(interactionData.jsonAsset.text,
                    () => { EndInteraction(index); });
            }
            else
            {
                EndInteraction(index);
            }
        }

        protected override void EndInteraction(int index = -1)
        {
            base.EndInteraction(index);

            var interactionData = GetInteractionData(index);

            if (IsInteractionClear())
            {
                isClear = true;
            }

            GetComponent<Collider2D>().enabled = false;
            OnClear?.Invoke();
            interactionData.onInteractionEnd?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            StartInteraction();
        }
    }
}