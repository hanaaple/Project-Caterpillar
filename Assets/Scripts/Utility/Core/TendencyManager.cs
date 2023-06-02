using System;
using Utility.SaveSystem;

namespace Utility.Core
{
    [Serializable]
    public class TendencyData
    {
        public int increase;
        public int descent;
        public int activation;
        public int inactive;
    }
    
    public class TendencyManager
    {
        private static TendencyManager _instance;
        
        public static TendencyManager Instance => _instance ??= new TendencyManager
        {
            _tendencyData = new TendencyData()
        };

        private TendencyData _tendencyData;
        
        public TendencyData GetTendencyData()
        {
            return _tendencyData;
        }
        
        public void Load(int saveDataIndex)
        {
            var saveData = SaveManager.GetSaveData(saveDataIndex);
            _tendencyData = saveData.tendencyData;
        }
    }
}