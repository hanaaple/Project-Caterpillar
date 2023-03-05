using System.Linq;
using Utility.Core;

namespace Utility.SaveSystem
{
    public class SaveHelper
    {
        public SaveData GetSaveData()
        {
            var saveData = new SaveData
            {
                items = ItemManager.Instance.GetItem<string>()
            };
            
            return saveData;
        }
    }
}