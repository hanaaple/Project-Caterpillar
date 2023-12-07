using UnityEngine;
using UnityEngine.UI;
using Utility.UI.Highlight;

namespace Utility.SaveSystem
{
    public class SaveLoadHighlightItem : HighlightItem
    {
        public int SaveDataIndex;
        
        public readonly SaveLoadItem SaveLoadItem;
        private static readonly int Selected = Animator.StringToHash("Selected");

        public SaveLoadHighlightItem(SaveLoadItem saveLoadItem)
        {
            SaveLoadItem = saveLoadItem;
            button = saveLoadItem.GetComponentInChildren<Button>();
            saveLoadItem.Animator = saveLoadItem.GetComponent<Animator>();
        }

        public async void UpdateUI()
        {
            if (SaveLoadItem.isEmpty)
            {
                return;
            }

            SaveLoadItem.Clear();
            
            await SaveManager.LoadCoverAsync(SaveDataIndex);
            
            var saveItemIndex = SaveLoadItem.transform.GetSiblingIndex();
            
            SaveLoadItem.LoadData(SaveDataIndex, saveItemIndex);
        }

        public override void SetDefault()
        {
            SaveLoadItem.Animator.SetBool(Selected, false);
        }
        
        public override void Select()
        {
            base.Select();
            SaveLoadItem.Animator.SetBool(Selected, true);
        }

        public override void DeSelect()
        {
            base.DeSelect();
            SaveLoadItem.Animator.SetBool(Selected, false);
        }
    }
}
