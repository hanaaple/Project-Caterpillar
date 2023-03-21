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
        public override void StartInteraction(int index = -1)
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

            if (interactionData.dialogueData.dialogueElements.Length == 0)
            {
                DialogueController.Instance.StartDialogue(interactionData.jsonAsset.text, () => { EndInteraction(index); });
            }
            else
            {
                interactionData.dialogueData.onDialogueEnd = () => { EndInteraction(index); };
                DialogueController.Instance.StartDialogue(interactionData.dialogueData);
            }
        }

        protected override void EndInteraction(int index = -1)
        {
            base.EndInteraction(index);

            var interactionData = GetInteractionData(index);

            if (IsInteractionClear())
            {
                IsClear = true;
            }

            GetComponent<Collider2D>().enabled = false;
            ONClear?.Invoke();
            interactionData.onInteractionEnd?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            StartInteraction();
        }
    }
}