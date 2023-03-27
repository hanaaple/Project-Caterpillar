using System;
using UnityEngine;
using Utility.Dialogue;
using Utility.Property;

namespace Utility.Interaction
{
    public enum InteractType
    {
        Default,
        Dialogue,
        Animator,
    }

    [Serializable]
    public class InteractionData
    {
        public InteractType interactType;

        [ConditionalHideInInspector("interactType", InteractType.Animator)]
        public Animator animator;
        
        [ConditionalHideInInspector("interactType", InteractType.Animator)]
        public int state;

        [ConditionalHideInInspector("interactType", InteractType.Dialogue)]
        public TextAsset jsonAsset;

        public bool isInteractable;

        public bool isContinuable;
        public bool interactNextIndex;
        public bool isLoop;

        [ConditionalHideInInspector("interactType", InteractType.Dialogue)]
        public DialogueData dialogueData;

        [Header("For Debugging")] public bool isInteracted;

        // public Action onInteractionStart;
        // public Action onInteractionEnd;
    }
}