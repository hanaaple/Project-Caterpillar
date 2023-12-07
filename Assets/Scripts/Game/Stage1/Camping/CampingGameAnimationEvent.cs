using UnityEngine;

namespace Game.Stage1.Camping
{
    public class CampingGameAnimationEvent : MonoBehaviour
    {
        [SerializeField] private CampingManager campingManager;

        /// <summary>
        /// Animator Event
        /// </summary>
        public void GameOverPush()
        {
            campingManager.GameOverPush();
        }
    }
}