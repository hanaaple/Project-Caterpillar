using System;
using Utility.SaveSystem;

namespace Utility.Core
{
    [Serializable]
    public class TendencyData
    {
        public int ascent;
        public int descent;
        public int activation;
        public int inactive;

        public void Copy(TendencyData tendencyData)
        {
            ascent = tendencyData.ascent;
            descent = tendencyData.descent;
            activation = tendencyData.activation;
            inactive = tendencyData.inactive;
        }
    }
    
    public class TendencyManager
    {
        private static TendencyManager _instance;
        
        public static TendencyManager Instance => _instance ??= new TendencyManager
        {
            _tendencyData = new TendencyData(),
            _savedTendencyData = new TendencyData()
        };

        private TendencyData _tendencyData;
        private TendencyData _savedTendencyData;
        
        public TendencyData GetTendencyData()
        {
            return _tendencyData;
        }
        
        public TendencyData GetSaveTendencyData()
        {
            return _savedTendencyData;
        }

        /// <summary>
        /// Update Data
        /// Before LoadScene, Do not Save
        /// </summary>
        public void UpdateTendencyData(int[] updateValue)
        {
            _tendencyData.ascent += updateValue[0];
            _tendencyData.descent += updateValue[1];
            _tendencyData.activation += updateValue[2];
            _tendencyData.inactive += updateValue[3];
        }

        /// <summary>
        /// Save Updated Data
        /// </summary>
        public void SaveTendencyData()
        {
            _savedTendencyData.Copy(_tendencyData);
        }
        
        public void Load(int saveDataIndex)
        {
            var saveData = SaveManager.GetSaveData(saveDataIndex);
            _tendencyData.Copy(saveData.tendencyData);
            _savedTendencyData.Copy(saveData.tendencyData);
        }
    }
}