using System.Collections;
using Game.Stage1.ShadowGame.Default;
using UnityEngine;

namespace Game.Stage1.ShadowGame.SnowMountain
{
    public class SnowMountainShadowGameManager : ShadowGameManager
    {
        [SerializeField] protected int comeCloseIndex;
        private static readonly int ComeClose = Animator.StringToHash("Come Close");

        protected override void Start()
        {
            base.Start();
            NextScene = "MainScene";   
        }
        
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
    }
}
