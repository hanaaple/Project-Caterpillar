using System.Collections;
using Game.Stage1.ShadowGame.Default;
using UnityEngine;
using Utility.Core;
using Utility.SaveSystem;
using Utility.Scene;

namespace Game.Stage1.ShadowGame.SnowMountain
{
    public class SnowMountainShadowGameManager : ShadowGameManager
    {
        [SerializeField] protected int comeCloseIndex;
        private static readonly int ComeClose = Animator.StringToHash("Come Close");

        protected override IEnumerator OnStageEnd(bool isClear)
        {
            if (comeCloseIndex == stageIndex)
            {
                gameAnimator.SetTrigger(ComeClose);
                yield return null;
                yield return new WaitUntil(() => gameAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayDefault"));
            }

            StartCoroutine(base.OnStageEnd(isClear));
        }

        protected override void ClearGame()
        {
            NextScene = "MainScene";
            SaveHelper.SetNpcData(NpcType.Photographer, NpcState.End);
            base.ClearGame();
        }
    }
}
