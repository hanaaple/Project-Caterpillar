using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
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
        Audio
    }

    [Serializable]
    public class ItemInteractionType
    {
        public ItemManager.ItemType[] itemTypes;
        public int targetIndex;
        public int defaultInteractionIndex;
        public bool isDestroyItem;
    }

    /// <summary>
    /// id, isInteractable, isInteracted를 제외하고 Legacy로 여기 있을 필요 없음
    /// </summary>
    [Serializable]
    public class SerializedInteractionData : ICloneable
    {
        public int id;
        public bool isInteractable;

        [FormerlySerializedAs("isContinuable")]
        public bool isNextInteractable;

        public bool interactNextIndex;
        public bool isLoop;
        public bool isPauseBgm;

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
        public ItemInteractionType itemInteractionType;

        [ConditionalHideInInspector("interactType", InteractType.Tutorial)]
        public TutorialHelper tutorialHelper;

        public bool isMove;

        [ConditionalHideInInspector("isMove")] public Transform targetTransform;

        [ConditionalHideInInspector("isMove")] [Range(0, 5)]
        public float moveSpeed;

        public bool isOnAwake;

        [ConditionalHideInInspector("isOnAwake")]
        public int order;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isAudioClip;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isTimelineAudio;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isBgm;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isSfx;

        [ConditionalHideInInspector("isAudioClip")]
        public AudioClip audioClip;

        [ConditionalHideInInspector("isTimelineAudio")]
        public TimelineAsset audioTimeline;

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