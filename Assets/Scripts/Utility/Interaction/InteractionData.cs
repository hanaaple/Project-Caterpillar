using System;
using UnityEngine;
using Utility.Dialogue;

namespace Utility.Interaction
{
    [Serializable]
    public class InteractionData
    {
        public TextAsset jsonAsset;

        public bool isInteractable;

        public bool isContinuable;
        public bool interactNextIndex;
        public bool isLoop;

        public DialogueData dialogueData;

        [Header("For Debugging")] public bool isInteracted;

        public Action onInteractionStart;
        public Action onInteractionEnd;
    }
}