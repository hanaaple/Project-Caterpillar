using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.SaveSystem;

namespace Utility.Tendency
{
    public class TendencyManager : MonoBehaviour
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
        }

        public static TendencyManager _instance;

        public static TendencyManager Instance => _instance;

        public TendencyData _tendencyData;
        private TendencyData _savedTendencyData;
        private TendencyTable _tendencyTable;

        private void Awake()
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