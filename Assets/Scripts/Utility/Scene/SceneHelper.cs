using UnityEngine;
using Utility.Core;

namespace Utility.Scene
{
    public enum PlayType
    {
        None,
        Field,
        MiniGame
    }

    public class SceneHelper : MonoBehaviour
    {
        [SerializeField] private PlayType playType;

        public void Play()
        {
            PlayUIManager.Instance.SetPlayType(playType);
        }
    }
}