using System;
using UnityEngine;

namespace Utility.Interaction
{
    [Serializable]
    public class InteractionData
    {
        public TextAsset jsonAsset;

        public bool isInteractable;

        public bool isContinuable;
        public bool useNextInteract;

        [Header("For Debugging")] public bool isInteracted;

        public Action onInteractionStart;
        public Action onInteractionEnd;
    }
}