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
            public bool isDefaultPortal;
            public int mapIndex;
            public WaitInteractions waitInteractions;
        }

        [Space(10)] [SerializeField] private PortalWaitInteraction[] portalWaitInteractions;
        
        
        public int portalIndex;

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

        protected void OnTriggerEnter2D(Collider2D col)
        {
            onPortal?.Invoke();
        }

        public bool IsWaitClear()
        {
            var portal = Array.Find(portalWaitInteractions, item => item.isDefaultPortal);
            if (portal != null)
            {
                return portal.waitInteractions.IsWaitClear();
            }
            
            var portalWaitInteraction = Array.Find(portalWaitInteractions, item => item.mapIndex == MapIndex);
            return portalWaitInteraction?.waitInteractions?.IsWaitClear() ?? true;
        }
    }
}