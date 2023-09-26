using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility.Core;
using Utility.Player;
using Utility.Tendency;

namespace Utility.SaveSystem
{
    public static class SaveHelper
    {
        public static readonly List<SceneData> SceneSaveData = new();

        public static void SetNpcData(NpcType npcType, NpcState npcState)
        {
            // 1. ActiveScene인 경우 해당 npcState 변경
            // 2. 다른 Scene에서 이미 있는 정보를 저장하는 경우

            Debug.Log($"{SceneManager.GetActiveScene().name}");

            var npc = GameManager.Instance.npc.Find(item => item.npcType == npcType);

            // ActiveScene인 경우
            if (npc != null)
            {
                npc.SetNpcState(npcState);
            }
            else
            {
                var sceneData = SceneSaveData.Find(item => item.npcData.Any(npcData => npcData.npcType == npcType));

                // Active인데 NpcList에 아직 추가되지 않거나
                // 저장되지 않은 경우
                if (sceneData == null)
                {
                    Debug.LogError($"저장된 데이터가 없거나 현재 Scene에 Npc가 없습니다. {npcType}");
                }
                else
                {
                    if (sceneData.sceneName == SceneManager.GetActiveScene().name)
                    {
                        Debug.LogError("현재 Scene이나 Npc가 없습니다.");
                    }
                    else
                    {
                        sceneData.UpdateNpcData(npcType, npcState);
                    }
                }
            }
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
                sceneSaveData = new List<SceneData>()
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

            saveData.saveCoverData.describe = saveData.saveCoverData.sceneName switch
            {
                "MainScene" => "이상한 숲",
                "SmallRoomScene" => "작은 방",
                "ShadowGame" => "어두운 작은 방",
                "CampingScene" => "한적한 캠핑장",
                "CampingGame" => "모기가 들끓는 캠핑장",
                "BeachScene" => "해변가",
                "BeachGame" => "모래사장",
                "SnowMountainScene" => "설산",
                "SnowMountainShadowGame" => "어두운 설산",
                _ => saveData.saveCoverData.describe
            };

            foreach (var sceneSaveData in SceneSaveData)
            {
                saveData.sceneSaveData.Add(sceneSaveData);
            }

            return saveData;
        }

        // OnLoadSceneEnd Title -> Load Game
        public static void LoadSaveData(int saveDataIndex)
        {
            Debug.Log("Title -> Load Save Data");
            var saveData = SaveManager.GetSaveData(saveDataIndex);

            ItemManager.Instance.Load(saveDataIndex);
            TendencyManager.Instance.Load(saveDataIndex);

            SceneSaveData.Clear();
            foreach (var sceneSaveData in saveData.sceneSaveData)
            {
                Debug.Log($"Title -> Load Scene - {sceneSaveData.sceneName}");
                SceneSaveData.Add(sceneSaveData);
            }
        }

        /// <summary>
        /// Save SceneData
        /// On LoadScene GameScene to GameScene
        /// </summary>
        public static void SaveSceneData()
        {
            Debug.Log($"Save Target Scene Name: {SceneManager.GetActiveScene().name}");

            var sceneData = SceneSaveData.Find(item => item.sceneName == SceneManager.GetActiveScene().name);
            if (sceneData != null)
            {
                Debug.Log("Save - Override");
                foreach (var npcData in GameManager.Instance.npc.Select(
                             interactionData => interactionData.GetSaveData()))
                {
                    sceneData.UpdateNpcData(npcData.npcType, npcData.state);
                }

                foreach (var interaction in GameManager.Instance.interactionObjects)
                {
                    sceneData.UpdateInteractionData(interaction.GetInteractionSaveData());
                }
            }
            else
            {
                Debug.Log("Save - New");
                sceneData = new SceneData
                {
                    sceneName = SceneManager.GetActiveScene().name,
                    interactionData = new List<InteractionSaveData>(),
                    npcData = new List<NpcData>()
                };

                // Save SceneData -> Npc
                foreach (var interactionData in GameManager.Instance.npc)
                {
                    sceneData.npcData.Add(interactionData.GetSaveData());
                }

                // Save SceneData -> Interaction
                foreach (var interaction in GameManager.Instance.interactionObjects)
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

            if (PlayerManager.Instance.Player)
            {
                sceneData.playerSerializableTransform.position = PlayerManager.Instance.Player.transform.position;
                sceneData.playerSerializableTransform.rotation = PlayerManager.Instance.Player.transform.rotation;
            }
        }

        // OnLoadSceneEnd Load SceneData
        public static void LoadSceneData()
        {
            Debug.Log($"Loaded Scene Name: {SceneManager.GetActiveScene().name}");

            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                return;
            }

            // Load SceneData -> Npc
            var sceneData = SceneSaveData.Find(item => item.sceneName == SceneManager.GetActiveScene().name);
            if (sceneData == default)
            {
                Debug.Log($"Scene - {SceneManager.GetActiveScene().name} 저장된 데이터 없심!");

                foreach (var npc in GameManager.Instance.npc)
                {
                    npc.SetNpcState(NpcState.Default);

                    Debug.Log($"{npc.npcType} - {NpcState.Default}");
                }

                return;
            }

            foreach (var npcData in sceneData.npcData)
            {
                var npc = GameManager.Instance.npc.Find(item => item.npcType == npcData.npcType);
                npc.SetNpcState(npcData.state);

                Debug.Log($"{npcData.npcType} - {npcData.state}");
            }

            // Load SceneData -> Interaction
            foreach (var interactionSaveData in sceneData.interactionData)
            {
                var interaction =
                    GameManager.Instance.interactionObjects.Find(item => item.id == interactionSaveData.id);
                if (!interaction)
                {
                    Debug.LogError($"name: {interactionSaveData.name} - id: {interactionSaveData.id}의 Interaction이 없심 조심행");
                }
                
                interaction.interactionIndex = interactionSaveData.interactionIndex;

                foreach (var interactionData in interaction.interactionData)
                {
                    var loadedSerializedData = interactionSaveData.serializedInteractionData.Find(item =>
                        item.id == interactionData.serializedInteractionData.id);

                    interactionData.serializedInteractionData = loadedSerializedData;
                }
            }
            
            if (PlayerManager.Instance.Player)
            {
                PlayerManager.Instance.Player.transform.position = sceneData.playerSerializableTransform.position;
                PlayerManager.Instance.Player.transform.rotation = sceneData.playerSerializableTransform.rotation;
                PlayerManager.Instance.CameraMove();
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