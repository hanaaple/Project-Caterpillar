using System;
using Game.Default;
using UnityEngine;
using Utility.Core;
using Utility.Player;
using Utility.Property;
using Utility.Tutorial;

namespace Utility.Scene
{
    public enum PlayType
    {
        None = 1,
        MainField = 2,
        StageField = 4,
        MiniGame = 16,
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
        [Serializable]
        public class FieldProperty
        {
            public BoxCollider2D boundBox;
            public bool isCameraMove;
            public PlayerMoveType playerMoveType;
        }
        
        [Serializable]
        public class BindProperty
        {
            public Animator[] bindAnimators;
            public GameObject[] bindGameObjects;
        }

        public static SceneHelper Instance;
        
        public PlayType playType;

        [ConditionalHideInInspector("playType", PlayType.MiniGame, default, true)]
        public ToastManager toastManager;

        [ConditionalHideInInspector("playType", PlayType.MainField | PlayType.StageField, default, true)]
        public FieldProperty fieldProperty;

        [ConditionalHideInInspector("playType", PlayType.StageField, default, true)]
        public BindProperty bindProperty;
        
        public bool isTutorial;
        [ConditionalHideInInspector("isTutorial")] public TutorialHelper tutorialHelper;

        private void Awake()
        {
            Instance = this;
            Play();
        }

        private void OnValidate()
        {
            if (playType is not (PlayType.StageField or PlayType.MainField))
            {
                fieldProperty.boundBox = null;
                fieldProperty.isCameraMove = false;
                fieldProperty.playerMoveType = default;
            }
            else if (playType != PlayType.MiniGame)
            {
                toastManager = null;
            }
        }

        private void Play()
        {
            PlayUIManager.Instance.Init(playType);
            PlayerManager.Instance.Init(playType);
        }

        public T GetBindObject<T>(string bindObjectName) where T : UnityEngine.Object
        {
            T returnValue = null;

            if (typeof(T) == typeof(Animator))
            {
                returnValue = Array.Find(bindProperty.bindAnimators, item => item.name == bindObjectName) as T;
            }
            else if(typeof(T) == typeof(GameObject))
            {
                returnValue = Array.Find(bindProperty.bindGameObjects, item => item.name == bindObjectName) as T;
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