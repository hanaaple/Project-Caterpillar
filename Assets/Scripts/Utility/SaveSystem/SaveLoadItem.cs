using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.SaveSystem
{
    public class SaveLoadItem : MonoBehaviour
    {
        public Button deleteButton;

        public TMP_Text contextText;
        public TMP_Text indexText;
        public TMP_Text stageText;
        public TMP_Text playTime;
        public TMP_Text date;
        public TMP_Text lastPlayTime;

        public bool isEmpty;

        [NonSerialized] public Animator Animator;

        public void LoadData(int saveDataIndex, int saveItemIndex)
        {
            var saveCoverData = SaveManager.GetSaveCoverData(saveDataIndex);
            if (saveCoverData != null)
            {
                contextText.text = saveCoverData.describe;
                indexText.text = $"{saveItemIndex + 1:D2}";
                stageText.text = saveCoverData.stageText;
                date.text = saveCoverData.date;
                lastPlayTime.text = saveCoverData.lastPlayTime;


                var day = $"{(int) saveCoverData.playTime.TotalDays:D2}일";
                var hour = $"{saveCoverData.playTime.Hours:D2}시간";
                var minute = $"{saveCoverData.playTime.Minutes:D2}분";
                if ((int) saveCoverData.playTime.TotalDays == 0)
                    day = "";
                if (saveCoverData.playTime.Hours == 0)
                    hour = "";

                playTime.text = $"{day} {hour} {minute}";
            }
            else
            {
                contextText.text = $"{saveDataIndex} 불러오기 오류";
            }
        }

        public void Clear()
        {
            contextText.text = "";
            indexText.text = "";
            stageText.text = "";
            playTime.text = "";
            date.text = "";
            lastPlayTime.text = "";
        }
    }
}