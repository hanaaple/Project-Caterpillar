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

        // for debugging
        [SerializeField] private SaveCoverData saveCoverData;
        
        public int saveDataIndex;

        public async void UpdateUI()
        {
            scenarioText.text = "";
            await SaveManager.LoadCoverAsync(saveDataIndex);
            
            if (!SaveManager.Exists(saveDataIndex))
            {
                scenarioText.text = "비어있음";
                return;
            }
            
            saveCoverData = SaveManager.GetSaveCoverData(saveDataIndex);
            if (saveCoverData != null)
            {
                scenarioText.text = saveCoverData.describe;
            }
            else
            {
                scenarioText.text = "불러오기 오류";
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
