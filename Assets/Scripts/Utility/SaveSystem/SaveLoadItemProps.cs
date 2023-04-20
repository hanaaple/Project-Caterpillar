using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.UI.Highlight;

namespace Utility.SaveSystem
{
    public class SaveLoadItemProps : HighlightItem
    {
        public int SaveDataIndex;
        public Action OnSelect;
        
        public readonly SaveLoadItem SaveLoadItem;

        public SaveLoadItemProps(SaveLoadItem saveLoadItem)
        {
            SaveLoadItem = saveLoadItem;
            button = saveLoadItem.GetComponent<Button>();
            saveLoadItem.Animator = saveLoadItem.GetComponent<Animator>();
        }

        public async void UpdateUI()
        {
            if (SaveLoadItem.isEmpty)
            {
                return;
            }

            SaveLoadItem.text.text = "";
            
            await SaveManager.LoadCoverAsync(SaveDataIndex);
            
            // for로 동시에 실행시킬 때, 실행 순서를 알 수 없다. index가 다르긴 해도 Dictionary를 동시에 Add가 되어
            // --> ConcurrentDictionary를 사용하라
            
            
            var saveCoverData = SaveManager.GetSaveCoverData(SaveDataIndex);
            if (saveCoverData != null)
            {
                SaveLoadItem.text.text = saveCoverData.describe;
            }
            else
            {
                SaveLoadItem.text.text = $"{SaveDataIndex} 불러오기 오류";
            }
        }

        public override void SetDefault()
        {
            SaveLoadItem.Animator.SetBool("Selected", false);
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            OnSelect?.Invoke();
            SaveLoadItem.Animator.SetBool("Selected", true);
        }
    }
}
