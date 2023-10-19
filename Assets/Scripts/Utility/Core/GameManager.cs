using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Interaction;
using Utility.Player;
using Utility.SaveSystem;

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

        [Header("For Debug")] public List<Interaction.Interaction> interactionObjects;
        public List<Npc> npc;
        public List<SceneData> sceneSaveData = new();

        private static GameManager Create()
        {
            var gameManagerPrefab = Resources.Load<GameManager>("GameManager");
            return Instantiate(gameManagerPrefab);
        }

        private void Awake()
        {
            interactionObjects = new List<Interaction.Interaction>();
            npc = new List<Npc>();
        }

        public void AddInteraction(Interaction.Interaction interaction)
        {
            if (!interactionObjects.Contains(interaction))
            {
                interactionObjects.Add(interaction);
            }
        }

        public void AddMainFieldNpc(Npc npc)
        {
            this.npc.Add(npc);
        }

        public void StartOnAwakeInteraction()
        {
            var interactions = interactionObjects.OrderByDescending(item =>
            {

                if (item.interactionIndex >= item.interactionData.Length)
                {
                    return -int.MaxValue;
                }

                var data = item.interactionData[item.interactionIndex];

                if (data.isOnAwake)
                {
                    return data.priority;
                }

                return -int.MaxValue;
            }).ToArray();

            foreach (var interaction in interactions)
            {
                if (interaction.interactionIndex >= interaction.interactionData.Length || !interaction.gameObject.activeSelf)
                {
                    continue;
                }

                var data = interaction.interactionData[interaction.interactionIndex];
                if (data.isOnAwake)
                {
                    Debug.Log($"OnAwake - {interaction.gameObject.name}, Index - {interaction.interactionIndex}, Order - {data.priority}");
                }    
            }

            foreach (var interaction in interactions)
            {
                interaction.OnAwakeInteraction();
            }
        }

        public void Clear()
        {
            interactionObjects.Clear();
            npc.Clear();
            PlayUIManager.Instance.dialogueController.Clear();
            PlayerManager.Instance.PopInputAction();
        }

        private void OnValidate()
        {
            sceneSaveData.Clear();
            foreach (var sceneData in SaveHelper.SceneSaveData)
            {
                sceneSaveData.Add(sceneData);
            }
        }
    }
}