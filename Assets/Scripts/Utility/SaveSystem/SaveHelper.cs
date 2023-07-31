using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Utility.Core;
using Utility.Tendency;

namespace Utility.SaveSystem
{
    public static class SaveHelper
    {
        public static SaveData GetSaveData()
        {
            var saveData = new SaveData
            {
                items = ItemManager.Instance.GetItem<string>(),
                tendencyData = TendencyManager.Instance.GetSaveTendencyData(),
                saveCoverData = new SaveCoverData
                {
                    sceneName = SceneManager.GetActiveScene().name == "TitleScene"
                        ? "MainScene"
                        : SceneManager.GetActiveScene().name,
                    playTime = 1122
                },
                interactionData = new List<InteractionSaveData>()
            };

            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                saveData.saveCoverData.describe = "이상한 숲";
            }
            else if (SceneManager.GetActiveScene().name == "MainScene")
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
            
            // it have to load all on Load (continuous)Scene
            GameManager.Instance.Load(saveDataIndex);
        }
    }
}