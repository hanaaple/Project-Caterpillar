using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Interaction;

namespace Utility.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<GameManager>();
                    if (obj != null)
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

        [NonSerialized] public bool IsTitleCutSceneWorked;

        [NonSerialized] public Player.Player Player;

        [NonSerialized] public List<Interaction.Interaction> InteractionObjects;
        
        [NonSerialized] public List<Npc> Npc;

        private static GameManager Create()
        {
            var gameManagerPrefab = Resources.Load<GameManager>("GameManager");
            return Instantiate(gameManagerPrefab);
        }

        private void Awake()
        {
            InteractionObjects = new List<Interaction.Interaction>();
            Npc = new List<Npc>();
        }

        public void AddInteraction(Interaction.Interaction interaction)
        {
            InteractionObjects.Add(interaction);
        }

        public void AddMainFieldNpc(Npc npc)
        {
            Npc.Add(npc);
        }

        public void Clear()
        {
            InteractionObjects.Clear();
            Npc.Clear();
        }
    }
}