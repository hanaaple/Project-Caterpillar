using System;
using UnityEngine;
using Utility.Core;
using Utility.Property;

namespace Utility.Scene
{
    public enum PlayType
    {
        None,
        MainField,
        StageField,
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

        public BoxCollider2D boundBox; 
        
        public bool isCameraMove;
        
        public PlayerMoveType playerMoveType;

        [SerializeField] private Animator[] bindAnimators;
        
        [SerializeField] private GameObject[] bindGameObjects;

        private void Awake()
        {
            Instance = this;
        }

        private void OnValidate()
        {
            if (playType is PlayType.StageField or PlayType.MainField)
            {
                return;
            }

            boundBox = null;
            isCameraMove = false;
            playerMoveType = default;
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