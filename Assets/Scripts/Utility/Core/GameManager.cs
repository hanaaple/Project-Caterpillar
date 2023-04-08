using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<GameManager>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
        
        [NonSerialized] public List<Interaction.Interaction> InteractionObjects;
    
        private static GameManager Create()
        {
            var gameManagerPrefab = Resources.Load<GameManager>("GameManager");
            return Instantiate(gameManagerPrefab);
        }

        public static bool IsCharacterControlEnable()
        {
            return !PlayUIManager.Instance.IsCanvasUse();
        }

        private void Awake()
        {
            InteractionObjects = new List<Interaction.Interaction>();
        }

        public void AddInteraction(Interaction.Interaction interaction)
        {
            InteractionObjects.Add(interaction);
        }
    }
}
