using System;
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

    public enum PlayerMoveType
    {
        None,
        Vertical,
        Horizontal,
        Both
    }

    public class SceneHelper : MonoBehaviour
    {
        public static SceneHelper Instance;
        
        [SerializeField] private PlayType playType;

        [ConditionalHideInInspector("playType", PlayType.Field)]
        public BoxCollider2D boundBox; 

        [ConditionalHideInInspector("playType", PlayType.Field)]
        public PlayerMoveType playerMoveType;
        
        [ConditionalHideInInspector("playType", PlayType.Field)]
        public bool isCameraMove;

        [SerializeField] private Animator[] bindAnimators;
        
        [SerializeField] private GameObject[] bindGameObjects;

        private void Awake()
        {
            Instance = this;
        }

        private void OnValidate()
        {
            if (playType != PlayType.Field)
            {
                boundBox = null;
                playerMoveType = default;
                isCameraMove = false;
            }
        }

        public void Play()
        {
            PlayUIManager.Instance.SetPlayType(playType);
        }

        public T GetBindObject<T>(string bindObjectName) where T : UnityEngine.Object
        {
            T returnValue = null;

            if (typeof(T) == typeof(Animator))
            {
                returnValue = Array.Find(bindAnimators, item => item.name == bindObjectName) as T;
            }
            else if(typeof(T) == typeof(GameObject))
            {
                returnValue = Array.Find(bindGameObjects, item => item.name == bindObjectName) as T;
            }

            return returnValue;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}