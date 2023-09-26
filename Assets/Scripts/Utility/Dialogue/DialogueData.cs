using System;
using System.Linq;
using Game.Default;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
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
        public WaitInteraction[] waitInteractions;

        public void Initialize(Action onClearAction = null)
        {
            foreach (var waitInteraction in waitInteractions)
            {
                if (waitInteraction.interaction)
                {
                    waitInteraction.interaction.InitializeWait(waitInteraction, onClearAction);
                }
            }
        }
        
        public bool IsWaitClear()
        {
            return waitInteractions.All(item => item.isWaitClear);
        }
        
        public int GetWaitCount()
        {
            return waitInteractions.Count(item => !item.isWaitClear);
        }
    }
    
    [Serializable]
    public class WaitInteraction
    {
        public Interaction.Interaction interaction;

        [ConditionalHideInInspector("isPortal", true)]
        public bool isInteraction;

        [ConditionalHideInInspector("isInteraction")]
        public int startIndex;

        [ConditionalHideInInspector("isInteraction")] [FormerlySerializedAs("index")]
        public int targetIndex;

        [ConditionalHideInInspector("isInteraction")]
        public bool isCustom;
        
        [ConditionalHideInInspector("isInteraction", true)]
        public bool isPortal;

        [ConditionalHideInInspector("isPortal")]
        public int targetMapIndex;
        
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

        [FormerlySerializedAs("waitWaitInteraction")] [FormerlySerializedAs("waitInteraction")] [ConditionalHideInInspector("dialogueType", DialogueType.WaitInteract)]
        public WaitInteractions waitInteractions;

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
        public bool isAudioClip;
        
        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public bool isTimelineAudio;
        
        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public bool isBgm;
        
        [ConditionalHideInInspector("dialogueType", DialogueType.Audio)]
        public bool isSfx;

        [ConditionalHideInInspector("isAudioClip")]
        public AudioClip audioClip;
        
        [ConditionalHideInInspector("isTimelineAudio")]
        public TimelineAsset audioTimeline;
        
        public bool isSkipEnable;

        [ConditionalHideInInspector("isSkipEnable")]
        public int skipLength;

        [ConditionalHideInInspector("isSkipEnable")]
        public float skipWaitSec;

        [NonSerialized] public Action OnStartAction;
    }
}