using System;
using UnityEngine;
using Utility.Audio;

namespace Utility.Util
{
    public class FadeBgmPlayer : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            // PlayBgmFade
            // AudioManager.Instance.PlayBgmWithFade();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            throw new NotImplementedException();
        }
    }
}
