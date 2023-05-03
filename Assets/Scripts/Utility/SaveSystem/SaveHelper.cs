using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Utility.Core;

namespace Utility.SaveSystem
{
    public static class SaveHelper
    {
        public static SaveData GetSaveData()
        {
            var saveData = new SaveData
            {
                items = ItemManager.Instance.GetItem<string>(),
                tendencyData = TendencyManager.Instance.GetTendencyData(),
                saveCoverData = new SaveCoverData
                {
                    describe = "테스트입니다." + SceneManager.GetActiveScene().name,
                    sceneName = SceneManager.GetActiveScene().name == "TitleScene" ? "MainScene" : SceneManager.GetActiveScene().name,
                    playTime = 1122
                }
            };
            
            saveData.interactionData = new List<InteractionSaveData>();
            
            foreach (var interactionData in GameManager.Instance.InteractionObjects.Select(interaction =>
                         interaction.GetInteractionSaveData()))
            {
                saveData.interactionData.Add(interactionData);
            }

            if (GameManager.Instance.Player)
            {
                saveData.playerSerializableTransform.position = GameManager.Instance.Player.transform.position;
                saveData.playerSerializableTransform.rotation = GameManager.Instance.Player.transform.rotation;
            }

            return saveData;
        }

        public static void Load(int saveDataIndex)
        {
            ItemManager.Instance.Load(saveDataIndex);
            TendencyManager.Instance.Load(saveDataIndex);
            GameManager.Instance.Load(saveDataIndex);
        }
    }
}