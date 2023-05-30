using Game.Default;
using UnityEngine;
using Utility.Core;
using Utility.Property;

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
        public static SceneHelper Instance;
        
        [SerializeField] private PlayType playType;

        [ConditionalHideInInspector("playType", PlayType.MiniGame)]
        public ToastManager toastManager;

        public void Play()
        {
            PlayUIManager.Instance.SetPlayType(playType);
        }
    }
}