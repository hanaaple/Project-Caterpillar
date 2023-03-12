using System;
using System.Linq;
using UnityEngine;
using Utility.Dialogue;
using Utility.JsonLoader;

namespace Utility.Interaction
{
    public abstract class Interaction : MonoBehaviour
    {
        // protected enum InteractType
        // {
        // Dialogue,
        // CutScene,
        // Asd
        // }

        [SerializeField] private InteractionData[] interactionData;

        [SerializeField] protected int interactionIndex;

        [NonSerialized] public bool isClear;

        protected Action OnClear;

        protected virtual void Start()
        {
                gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        public void Initialize(Action onClear)
        {
            foreach (var interaction in interactionData)
            {
                interaction.isInteracted = false;
            }
            
            isClear = false;
            OnClear = onClear;
            GetComponent<Collider2D>().enabled = true;
        }

        protected virtual void StartInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            var interaction = GetInteractionData(index);
            interaction.onInteractionStart?.Invoke();
        }

        protected virtual void EndInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            var interaction = GetInteractionData(index);
            interaction.isInteracted = true;
            interaction.onInteractionEnd?.Invoke();

            var nextIndex = (index + 1) % interactionData.Length;
            var nextInteraction = GetInteractionData(nextIndex);

            if (nextInteraction.isContinuable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
            }

            if (nextInteraction.useNextInteract)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
                StartInteraction(nextIndex);
            }
        }

        protected virtual bool IsInteractionClear()
        {
            return interactionData.All(item => item.isInteracted);
        }

        protected virtual bool IsInteractable(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            var interaction = interactionData[index];
            return !interaction.isInteracted && interaction.isInteractable;
        }

        protected InteractionData GetInteractionData(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            return interactionData[index];
        }

#if UNITY_EDITOR
        [SerializeField] private DialogueData dialogueData;

        public void ShowDialogue()
        {
            var interaction = GetInteractionData();
            dialogueData.dialogueElements = JsonHelper.GetJsonArray<DialogueElement>(interaction.jsonAsset.text);
        }
#endif
    }
}