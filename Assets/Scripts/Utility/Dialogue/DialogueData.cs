using System;
using UnityEngine.Events;
using UnityEngine.Playables;
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

        [NonSerialized] public UnityAction OnDialogueStart;
        [NonSerialized] public UnityAction OnDialogueWaitClear;
        [NonSerialized] public UnityAction OnDialogueEnd;

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
        public Interaction.Interaction[] interactions;
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

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public PlayableAsset playableAsset;

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public DirectorWrapMode extrapolationMode;

        [ConditionalHideInInspector("dialogueType", DialogueType.Interact)]
        public Interaction.Interaction interaction;

        [ConditionalHideInInspector("dialogueType", DialogueType.Interact)]
        public int interactIndex;
    }
}