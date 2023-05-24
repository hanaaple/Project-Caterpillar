using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using Utility.JsonLoader;
using Utility.Property;

namespace Utility.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        // for Debugging
        public int index;
        public DialogueElement[] dialogueElements;

        [NonSerialized] public UnityAction OnDialogueEnd;

        public DialogueData()
        {
        }

        public DialogueData(DialogueData dialogueData)
        {
            dialogueElements = dialogueData.dialogueElements;
        }

        public void Init(string json)
        {
            index = 0;
            dialogueElements = JsonHelper.GetJsonArray<DialogueElement>(json);
        }

        public void Reset()
        {
            index = 0;
            dialogueElements = null;
        }
    }

    [Serializable]
    public class Interactions
    {
        public WaitInteraction[] waitInteractions;

        public bool IsWaitClear()
        {
            return waitInteractions.All(item =>
                item.interaction.GetInteractionData(item.targetIndex).serializedInteractionData.isWaitClear);
        }
        
        public int GetWaitCount()
        {
            return waitInteractions.Count(item =>
                !item.interaction.GetInteractionData(item.targetIndex).serializedInteractionData.isWaitClear);
        }
    }

    [Serializable]
    public class WaitInteraction
    {
        public Interaction.Interaction interaction;
        public int startIndex;
        [FormerlySerializedAs("index")] public int targetIndex;
    }

    [Serializable]
    public struct DialogueElement
    {
        public CharacterType name;
        public string subject;
        public string contents;
        public DialogueType dialogueType;

        public Expression expression;

        public string[] option;

        [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public InteractionWaitType interactionWaitType;

        [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public Interactions waitInteraction;

        // Enable Set Interactable or Set Interactable In Timeline 

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public PlayableAsset playableAsset;

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public DirectorWrapMode extrapolationMode;

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public float waitSec;

        [ConditionalHideInInspector("dialogueType", DialogueType.Interact)]
        public Interaction.Interaction interaction;

        [ConditionalHideInInspector("dialogueType", DialogueType.Interact)]
        public int interactIndex;

        public bool isSkipEnable;

        [ConditionalHideInInspector("isSkipEnable")]
        public int skipLength;

        [ConditionalHideInInspector("isSkipEnable")]
        public float skipWaitSec;

        [NonSerialized] public Action OnStartAction;
    }
}