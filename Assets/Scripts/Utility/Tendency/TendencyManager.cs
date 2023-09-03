using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.SaveSystem;

namespace Utility.Tendency
{
    public class TendencyManager
    {
        [Serializable]
        public class TendencyData
        {
            public List<TendencyType> tendencyItems;
            public int ascent;
            public int descent;
            public int activation;
            public int inactive;

            public void Copy(TendencyData tendencyData)
            {
                tendencyItems = new List<TendencyType>(tendencyData.tendencyItems);
                ascent = tendencyData.ascent;
                descent = tendencyData.descent;
                activation = tendencyData.activation;
                inactive = tendencyData.inactive;
            }

            public void Clear()
            {
                tendencyItems.Clear();
                ascent = 0;
                descent = 0;
                activation = 0;
                inactive = 0;
            }
        }

        private static TendencyManager _instance;

        public static TendencyManager Instance
        {
            get { return _instance ??= new TendencyManager(); }
        }
        
        public Action OnTendencyUpdate;

        private readonly TendencyData _tendencyData;
        private readonly TendencyData _savedTendencyData;
        private readonly TendencyTable _tendencyTable;

        private TendencyManager()
        {
            _instance = this;

            _tendencyData = new TendencyData
            {
                tendencyItems = new List<TendencyType>()
            };
            _savedTendencyData = new TendencyData
            {
                tendencyItems = new List<TendencyType>()
            };
            _tendencyTable = Resources.Load<TendencyTable>("Tendency Table");

        }

        public TendencyData GetTendencyData()
        {
            return _tendencyData;
        }

        public TendencyData GetSaveTendencyData()
        {
            return _savedTendencyData;
        }

        public TendencyProps GetTendencyType(TendencyType tendencyType)
        {
            return Array.Find(_tendencyTable.tendencyProps, item => item.tendencyType == tendencyType);
        }

        /// <summary>
        /// Update Data
        /// Before LoadScene, Do not Save
        /// </summary>
        public void UpdateTendencyData(string tendencyName)
        {
            var tendencyProps = Array.Find(_tendencyTable.tendencyProps,
                item => item.tendencyType.ToString() == tendencyName);
            _tendencyData.tendencyItems.Add(tendencyProps.tendencyType);
            _tendencyData.ascent += tendencyProps.ascent;
            _tendencyData.descent += tendencyProps.descent;
            _tendencyData.activation += tendencyProps.activation;
            _tendencyData.inactive += tendencyProps.inactive;
            Debug.Log(
                $"성향 (상승, 하강, 활성, 비활성): {tendencyProps.ascent}, {tendencyProps.descent}, {tendencyProps.activation}, {tendencyProps.inactive}");
            
            OnTendencyUpdate?.Invoke();
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

        public void Clear()
        {
            _tendencyData.Clear();
            _savedTendencyData.Clear();
        }
    }
}