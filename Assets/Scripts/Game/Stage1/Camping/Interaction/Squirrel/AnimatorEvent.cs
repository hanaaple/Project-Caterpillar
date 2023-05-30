using UnityEngine;

namespace Game.Camping
{
    public class AnimatorEvent : MonoBehaviour
    {
        public void DisableAnimator()
        {
            GetComponent<Animator>().enabled = false;
        }
    }
}
