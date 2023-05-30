using System;
using UnityEngine;
using Utility.Dialogue;

namespace Utility.Portal
{
    public class Portal : Interaction.Interaction
    {
        [Serializable]
        public class PortalWaitInteraction
        {
            public int mapIndex;
            public WaitInteractions waitInteractions;
        }

        [Space(10)] [SerializeField] private PortalWaitInteraction[] portalWaitInteractions;

        [NonSerialized] public int MapIndex;

        public Action onPortal;
        public Action onEndTeleport;

        protected override void Start()
        {
            base.Start();
            foreach (var portalWaitInteraction in portalWaitInteractions)
            {
                portalWaitInteraction.waitInteractions?.Initialize(() =>
                {
                    Debug.Log($"포탈 클리어 남은 개수: {portalWaitInteraction.waitInteractions.GetWaitCount()}");
                });
            }
        }

        public override void StartInteraction(int index = -1)
        {
            var portalWaitInteraction = Array.Find(portalWaitInteractions, item => item.mapIndex == MapIndex);
            if (portalWaitInteraction?.waitInteractions?.IsWaitClear() ?? true)
            {
                onPortal?.Invoke();
            }
            else
            {
                base.StartInteraction(index);
            }
        }

        protected void OnTriggerEnter2D(Collider2D col)
        {
            StartInteraction();
        }
    }
}