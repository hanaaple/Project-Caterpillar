using System;
using UnityEngine;
using Utility.Dialogue;

namespace Utility.Portal
{
    public class Portal : Interaction.Interaction
    {
        [Space(10)] [SerializeField] private WaitInteractions waitInteractions;
        public int portalIndex;

        [NonSerialized] public int CurMapIndex;
        [NonSerialized] public bool TeleportEndIsFadeOut;

        public Action onTryTeleport;
        public Action onEndTeleport;

        protected override void Start()
        {
            base.Start();
            waitInteractions.Initialize(() =>
            {
                Debug.Log($"포탈 클리어 남은 개수: {waitInteractions.GetWaitCount()}");
            });
        }

        protected void OnTriggerEnter2D(Collider2D col)
        {
            onTryTeleport?.Invoke();
        }
        
        public bool IsWaitClear()
        {
            return waitInteractions.IsWaitClear();
        }
    }
}