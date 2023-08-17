using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Core;
using Utility.Tendency;

namespace Utility.SaveSystem
{
    public static class SaveHelper
    {
        private static readonly List<SceneSaveData> SceneSaveData = new();
        
        public static void SetNpcData(NpcType npcType, NpcState npcState)
        {
            var sceneData = SceneSaveData.Find(item => item.sceneName == SceneManager.GetActiveScene().name);
            var npcData = sceneData.npcData.Find(item => item.npcType == npcType);
            npcData.state = npcState;
        }

        public static SaveData GetSaveData(string targetSceneName)
        {
            var saveData = new SaveData
            {
                items = ItemManager.Instance.GetItem<string>(),
                tendencyData = TendencyManager.Instance.GetSaveTendencyData(),
                saveCoverData = new SaveCoverData
                {
                    playTime = 1122
                },
                sceneSaveData = new List<SceneSaveData>()
            };

            if (string.IsNullOrEmpty(targetSceneName))
            {
                saveData.saveCoverData.sceneName = SceneManager.GetActiveScene().name == "TitleScene"
                    ? "MainScene"
                    : SceneManager.GetActiveScene().name;
            }
            else
            {
                saveData.saveCoverData.sceneName = targetSceneName;
            }

            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                saveData.saveCoverData.describe = "이상한 숲";
            }
            else if (SceneManager.GetActiveScene().name == "SmallRoomScene")
            {
                saveData.saveCoverData.describe = "작은 방";
            }
            else if (SceneManager.GetActiveScene().name == "CampingScene")
            {
                saveData.saveCoverData.describe = "한적한 캠핑장";
            }
            else if (SceneManager.GetActiveScene().name == "BeachScene")
            {
                saveData.saveCoverData.describe = "해변가";
            }

            SaveSceneData();

            foreach (var sceneSaveData in SceneSaveData)
            {
                saveData.sceneSaveData.Add(sceneSaveData);
            }

            // if (GameManager.Instance.Player)
            // {
            //     saveData.playerSerializableTransform.position = GameManager.Instance.Player.transform.position;
            //     saveData.playerSerializableTransform.rotation = GameManager.Instance.Player.transform.rotation;
            // }

            return saveData;
        }

        // Title -> Game
        public static void LoadSaveData(int saveDataIndex)
        {
            var saveData = SaveManager.GetSaveData(saveDataIndex);
            
            ItemManager.Instance.Load(saveDataIndex);
            TendencyManager.Instance.Load(saveDataIndex);
            
            SceneSaveData.Clear();
            foreach (var sceneSaveData in saveData.sceneSaveData)
            {
                SceneSaveData.Add(sceneSaveData);
            }

            // if (GameManager.Instance.Player)
            // {
            //     GameManager.Instance.Player.transform.position = saveData.playerSerializableTransform.position;
            //     GameManager.Instance.Player.transform.rotation = saveData.playerSerializableTransform.rotation;
            // }
        }

        // Save SceneData
        public static void SaveSceneData()
        {
            Debug.Log($"Saved Scene Name: {SceneManager.GetActiveScene().name}");
            
            var sceneData = new SceneSaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                interactionData = new List<InteractionSaveData>(),
                npcData = new List<NpcData>()
            };
            
            // Save SceneData -> Npc
            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                foreach (var interactionData in GameManager.Instance.Npc)
                {
                    sceneData.npcData.Add(interactionData.GetSaveData());
                }
            }
            
            // Save SceneData -> Interaction
            foreach (var interaction in GameManager.Instance.InteractionObjects)
            {
                sceneData.interactionData.Add(interaction.GetInteractionSaveData());
            }

            var index = SceneSaveData.FindIndex(item => item.sceneName == sceneData.sceneName);
            if (index != -1)
            {
                SceneSaveData.Remove(SceneSaveData[index]);
            }
            SceneSaveData.Add(sceneData);
        }
        
        // Load SceneData
        public static void LoadSceneData()
        {
            Debug.Log($"Loaded Scene Name: {SceneManager.GetActiveScene().name}");
            
            // Load SceneData -> Npc
            var sceneData = SceneSaveData.Find(item => item.sceneName == SceneManager.GetActiveScene().name);
            if (sceneData == default)
            {
                Debug.Log("Scene 데이터 없심!");
                
                foreach (var npc in GameManager.Instance.Npc)
                {
                    var targetInteraction = Array.Find(npc.npcInteraction,
                        item => item.npcState == NpcState.Default).interaction;

                    foreach (var npcInteraction in npc.npcInteraction)
                    {
                        npcInteraction.interaction.gameObject.SetActive(false);
                    }

                    targetInteraction.gameObject.SetActive(true);
                    
                    Debug.Log($"{npc.npcType} - {NpcState.Default}");
                }
                return;
            }

            foreach (var npcData in sceneData.npcData)
            {
                var npc = GameManager.Instance.Npc.Find(item => item.npcType == npcData.npcType);

                var targetInteraction = Array.Find(npc.npcInteraction,
                    item => item.npcState == npcData.state).interaction;

                foreach (var npcInteraction in npc.npcInteraction)
                {
                    npcInteraction.interaction.gameObject.SetActive(false);
                }

                targetInteraction.gameObject.SetActive(true);
                
                Debug.Log($"{npcData.npcType} - {npcData.state}");
            }

            // Load SceneData -> Interaction
            foreach (var interactionSaveData in sceneData.interactionData)
            {
                var interaction = GameManager.Instance.InteractionObjects.Find(item => item.id == interactionSaveData.id);
                interaction.interactionIndex = interactionSaveData.interactionIndex;

                foreach (var interactionData in interaction.interactionData)
                {
                    var loadedSerializedData = interactionSaveData.serializedInteractionData.Find(item =>
                        item.id == interactionData.serializedInteractionData.id);

                    interactionData.serializedInteractionData = loadedSerializedData;
                }
            }
        }

        // Clear SceneData
        public static void Clear()
        {
            ItemManager.Instance.Clear();
            TendencyManager.Instance.Clear();
            SceneSaveData.Clear();
        }
    }
}