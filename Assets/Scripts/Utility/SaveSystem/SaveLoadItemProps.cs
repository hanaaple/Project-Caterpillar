using System;
using TMPro;
using UnityEngine;
using Utility.UI.Highlight;

namespace Utility.SaveSystem
{
    [Serializable]
    public class SaveLoadItemProps : HighlightItem
    {
        [SerializeField] private TMP_Text scenarioText;

        [SerializeField] private Animator animator;
        
        private SaveData _saveData;
        
        public int saveDataIndex;
        
        public void UpdateUI()
        {
            _saveData = SaveManager.GetLoadData(saveDataIndex);
            if (_saveData != null)
            {
                scenarioText.text = _saveData.scenario;
            }
            else
            {
                scenarioText.text = "";
                button.onClick.RemoveAllListeners();
            }
        }

        public override void SetDefault()
        {
            animator.SetBool("Selected", false);
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            animator.SetBool("Selected", true);
        }
    }
}
