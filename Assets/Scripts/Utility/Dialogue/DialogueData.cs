using System;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utility.Interaction;
using Utility.JsonLoader;
using Utility.Property;

namespace Utility.Dialogue
{
    [Serializable]
    public class DialogueData
    {
        [NonSerialized] public int index;
        public DialogueElement[] dialogueElements;
        
        [NonSerialized] public UnityAction onDialogueStart;
        [NonSerialized] public UnityAction onDialogueWaitClear;
        [NonSerialized] public UnityAction onDialogueEnd;

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
    public struct DialogueElement
    {
        public CharacterType name;
        public string subject;
        public string contents;
        public DialogueType dialogueType;
        
        public Expression expression;
        public string[] option;

        public InteractionWaitType interactionWaitType;
        [ConditionalHideInInspector("dialogueType", DialogueType.Wait)]
        public Interaction.Interaction[] interactions;
    }
}