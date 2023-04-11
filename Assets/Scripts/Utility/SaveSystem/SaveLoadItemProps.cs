using UnityEngine;
using Utility.UI.Highlight;

namespace Utility.SaveSystem
{
    public class SaveLoadItemProps : HighlightItem
    {
        public readonly SaveLoadItem SaveLoadItem;
        public int SaveDataIndex;

        public SaveLoadItemProps(GameObject saveLoadObject)
        {
            if (saveLoadObject.TryGetComponent(out SaveLoadItem saveLoadItem))
            {
                SaveLoadItem = saveLoadItem;
            }
        }

        public async void UpdateUI()
        {
            if (!SaveLoadItem)
            {
                return;
            }

            SaveLoadItem.text.text = "";
            
            await SaveManager.LoadCoverAsync(SaveDataIndex);
            
            var saveCoverData = SaveManager.GetSaveCoverData(SaveDataIndex);
            if (saveCoverData != null)
            {
                SaveLoadItem.text.text = saveCoverData.describe;
            }
            else
            {
                SaveLoadItem.text.text = "불러오기 오류";
            }
        }

        public override void SetDefault()
        {
            //_saveLoadItem.animator.SetBool("Selected", false);
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            //_saveLoadItem.animator.SetBool("Selected", true);
        }
    }
}
