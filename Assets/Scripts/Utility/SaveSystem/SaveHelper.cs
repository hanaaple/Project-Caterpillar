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
                    sceneName = SceneManager.GetActiveScene().name,
                    playTime = 1122
                }
            };
            
            foreach (var interactionData in GameManager.Instance.InteractionObjects.Select(interaction => interaction.GetInteractionSaveData()))
            {
                saveData.interactionData.Add(interactionData);
            }
            
            return saveData;
        }

        public static void Load(int saveDataIndex)
        {
            ItemManager.Instance.Load(saveDataIndex);
            TendencyManager.Instance.Load(saveDataIndex);
        }
    }
}