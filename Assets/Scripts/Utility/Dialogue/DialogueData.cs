using System;
using System.Linq;
using Game.Default;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Utility.Interaction;
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

        [NonSerialized] public UnityAction<int> OnDialogueEnd;

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
    public class WaitInteractions
    {
        [FormerlySerializedAs("waitInteractions")] public WaitInteractionData[] waitInteractionData;

        public void Initialize(Action onClearAction = null)
        {
            foreach (var waitInteraction in waitInteractionData)
            {
                if (waitInteraction.interaction)
                {
                    waitInteraction.interaction.InitializeWait(waitInteraction, onClearAction);
                }
            }
        }

        public bool IsWaitClear()
        {
            return waitInteractionData.All(item => item.isWaitClear);
        }

        public int GetWaitCount()
        {
            return waitInteractionData.Count(item => !item.isWaitClear);
        }
    }

    [Serializable]
    public class WaitInteractionData
    {
        public Interaction.Interaction interaction;

        [ConditionalHideInInspector("isPortal", true)]
        public bool isInteraction;

        [ConditionalHideInInspector("isInteraction")]
        public int startIndex;

        [ConditionalHideInInspector("isInteraction")] [FormerlySerializedAs("index")]
        public int targetIndex;

        /// <summary>
        /// WaitClear 시작 이후 해당 Interaction을 Interactable하게 만들지 않음.
        /// </summary>
        [ConditionalHideInInspector("isInteraction")]
        public bool isCustom;

        [ConditionalHideInInspector("isInteraction", true)]
        public bool isPortal;

        [ConditionalHideInInspector("isPortal")]
        public int targetMapIndex;
        
        [ConditionalHideInInspector("isPortal")]
        public bool isActionAfterFadeOut;

        public bool isWaitClear;

        public void Clear()
        {
            if (isPortal)
            {
                isWaitClear = true;
            }
            else if (isInteraction)
            {
                isWaitClear = true;
            }
        }
    }

    [Serializable]
    public struct DialogueElement
    {
        public CharacterType name;
        public string subject;
        [TextArea] public string contents;
        public DialogueType dialogueType;

        public Expression expression;

        public string[] option;

        [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public InteractionWaitType interactionWaitType;

        [FormerlySerializedAs("waitWaitInteraction")]
        [FormerlySerializedAs("waitInteraction")]
        [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public WaitInteractions waitInteractions;

        [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public WaitInteractionHelper waitInteractionHelper;

        // Enable Set Interactable or Set Interactable In Timeline 

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public PlayableAsset playableAsset;

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        public DirectorWrapMode extrapolationMode;

        [ConditionalHideInInspector("extrapolationMode", DirectorWrapMode.Loop)]
        public string playableDirectorName;

        [ConditionalHideInInspector("dialogueType", DialogueType.CutScene)]
        // [ConditionalHideInInspector("extrapolationMode", DirectorWrapMode.Loop, true)]
        public float waitSec;

        [ConditionalHideInInspector("dialogueType", DialogueType.MiniGame)]
        public MiniGame miniGame;

        [ConditionalHideInInspector("dialogueType", DialogueType.MiniGame)]
        public bool isCustomEnd;

        [ConditionalHideInInspector("isCustomEnd")]
        public int successNextInteractionIndex;

        [ConditionalHideInInspector("isCustomEnd")]
        public int failNextInteractionIndex;

        //[ConditionalHideInInspector("dialogueType", DialogueType.Interact)]
        //public int interactIndex;

        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public bool isBgm;

        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public bool isSfx;

        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public AudioClip audioClip;

        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public TimelineAsset audioTimeline;
        
        // [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        // public AudioData audioData;

        [ConditionalHideInInspector("dialogueType", DialogueType.DialogueEnd)]
        public int endTargetIndex;

        public bool isSkipEnable;

        [ConditionalHideInInspector("isSkipEnable")]
        public int skipLength;

        [ConditionalHideInInspector("isSkipEnable")]
        public float skipWaitSec;

        [NonSerialized] public Action OnStartAction;
    }
}