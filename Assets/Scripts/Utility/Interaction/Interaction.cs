using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
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

        [NonSerialized] public bool IsClear;

        protected Action ONClear;

        protected virtual void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        public void Initialize(Action onClearAction)
        {
            foreach (var interaction in interactionData)
            {
                interaction.isInteracted = false;
            }

            IsClear = false;
            ONClear = onClearAction;
            GetComponent<Collider2D>().enabled = true;
        }

        public virtual void StartInteraction(int index = -1)
        {
            if (index == -1)
            {
                index = interactionIndex;
            }

            if (!IsInteractable(index))
            {
                return;
            }

            var interaction = GetInteractionData(index);
            interaction.onInteractionStart?.Invoke();

            if (interaction.dialogueData.dialogueElements.Length == 0)
            {
                Debug.LogWarning("CutScene, Wait 세팅 안되어있을수도 주의");
            }
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

            GetComponent<Collider2D>().enabled = false;
            
            if (nextInteraction.isContinuable)
            {
                nextInteraction.isInteractable = true;
                interactionIndex = nextIndex;
                GetComponent<Collider2D>().enabled = true;
            }

            if (interaction.isLoop)
            {
                GetComponent<Collider2D>().enabled = true;   
            }

            if (interaction.interactNextIndex)
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
            return (interaction.isLoop || !interaction.isInteracted) && interaction.isInteractable;
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
        public void ShowDialogue()
        {
            for(var index = 0; index < interactionData.Length; index++)
            {
                var interaction = interactionData[index];
                interaction.dialogueData.dialogueElements = JsonHelper.GetJsonArray<DialogueElement>(interaction.jsonAsset.text);

                for(var idx = 0; idx < interaction.dialogueData.dialogueElements.Length; idx++)
                {
                    var dialogueElement = interaction.dialogueData.dialogueElements[idx];
                    if (dialogueElement.dialogueType == DialogueType.CutScene && dialogueElement.option?.Length > 0 && dialogueElement.option.Contains("Reset"))
                    {
                        interaction.dialogueData.dialogueElements[idx].playableAsset = Resources.Load<PlayableAsset>("Timeline/Reset");
                        continue;
                    }
                    if (dialogueElement.dialogueType is DialogueType.Interact or DialogueType.WaitInteract or DialogueType.CutScene)
                    {
                        Debug.LogWarning($"interaction: {index}번, {idx}번 대화, {dialogueElement.dialogueType} 세팅해야함.");
                    }
                    
                    if (dialogueElement.option != null && dialogueElement.option.Contains("Hold", StringComparer.OrdinalIgnoreCase))
                    {
                        interaction.dialogueData.dialogueElements[idx].extrapolationMode = DirectorWrapMode.Hold;
                    }
                    else
                    {
                        interaction.dialogueData.dialogueElements[idx].extrapolationMode = DirectorWrapMode.None;
                    }
                }
            }
        }
#endif
    }
}