using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Utility.Audio;
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
        Audio = 8,
        StopAudio = 16,
    }

    public enum ItemUseType
    {
        Inventory,
        HoldHand,
    }

    [Serializable]
    public class ItemData
    {
        public ItemManager.ItemType itemType;
        public ItemUseType itemUseType;
        public bool isDestroyItem;
    }

    [Serializable]
    public class ItemInteractionData
    {
        public ItemData[] itemData;
        public int targetIndex;
        public int defaultInteractionIndex;

        // 조건에 실패하여 Default인 경우 -> Default index 실행 후 -> Item Index로 돌아올 것인지, 그대로 실행 쭉 할 것인지 (기본) 선택 가능하게
        public bool isLoopDefault;
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

        [FormerlySerializedAs("interactNextIndex")]
        public bool isInteractNextIndex;

        public bool isCustomNextIndex;

        [ConditionalHideInInspector("isCustomNextIndex")]
        public int targetIndex;

        public bool isLoop;
        [FormerlySerializedAs("isPauseBgm")] public bool isReduceBgm;

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

        [FormerlySerializedAs("itemInteractionType")] [ConditionalHideInInspector("interactType", InteractType.Item)]
        public ItemInteractionData itemInteractionData;

        [ConditionalHideInInspector("interactType", InteractType.Tutorial)]
        public TutorialHelper tutorialHelper;

        public bool isMove;

        [ConditionalHideInInspector("isMove")] public Transform targetTransform;

        [ConditionalHideInInspector("isMove")] [Range(0, 5)]
        public float moveSpeed;

        public bool isOnAwake;

        [FormerlySerializedAs("order")] [ConditionalHideInInspector("isOnAwake")]
        public int priority;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isAudioClip;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isTimelineAudio;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isBgm;

        [ConditionalHideInInspector("interactType", InteractType.Audio)]
        public bool isSfx;

        [ConditionalHideInInspector("interactType", InteractType.Audio)] [Range(0, 1)]
        public float volume = 1;

        [ConditionalHideInInspector("isAudioClip")]
        public AudioClip audioClip;

        [ConditionalHideInInspector("isTimelineAudio")]
        public TimelineAsset audioTimeline;

        [ConditionalHideInInspector("interactType", InteractType.Audio | InteractType.StopAudio, false, true)]
        public AudioData audioData;

        [ConditionalHideInInspector("interactType", InteractType.StopAudio)]
        public bool isStopBgm;

        public SerializedInteractionData serializedInteractionData;

        public Action OnEndAction;
        public Action OnCompletelyEndAction;

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