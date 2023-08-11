using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Dialogue;
using Utility.Property;
using Utility.Tutorial;

namespace Utility.Interaction
{
    public enum InteractType
    {
        Default,
        Dialogue,
        Animator,
        OneOff,
        Item,
        Tutorial,
    }
    
    [Serializable]
    public class ItemInteractionType
    {
        public ItemManager.ItemType itemType;
        public int targetIndex;
    }

    [Serializable]
    public class SerializedInteractionData : ICloneable
    {
        public int id;
        public bool isInteractable;

        [FormerlySerializedAs("isContinuable")]
        public bool isNextInteractable;

        public bool interactNextIndex;
        public bool isLoop;

        [Header("For Debugging")] public bool isInteracted;
        // public bool isWaitClear;

        public object Clone()
        {
            return MemberwiseClone();
        }
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

        [ConditionalHideInInspector("interactType", InteractType.Dialogue)]
        public DialogueData dialogueData;
        
        [ConditionalHideInInspector("interactType", InteractType.Item)]
        public ItemInteractionType[] itemInteractionTypes;
        
        [ConditionalHideInInspector("interactType", InteractType.Tutorial)]
        public TutorialHelper tutorialHelper;
        
        public bool isMove;

        [ConditionalHideInInspector("isMove")] public Transform targetTransform;

        [ConditionalHideInInspector("isMove")] [Range(0, 5)]
        public float moveSpeed;

        public SerializedInteractionData serializedInteractionData;

        public Action onEndAction;
        
        //public InteractionEvent onInteractionEndEvent;

        public InteractionData DeepCopy()
        {
            var interactionData = (InteractionData) MemberwiseClone();
            interactionData.dialogueData = new DialogueData(interactionData.dialogueData);
            interactionData.serializedInteractionData =
                (SerializedInteractionData) interactionData.serializedInteractionData.Clone();

            return interactionData;
        }
    }
}