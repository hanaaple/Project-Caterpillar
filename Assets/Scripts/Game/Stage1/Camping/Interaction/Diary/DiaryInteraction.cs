using Game.Camping;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Diary
{
    public class DiaryInteraction : CampingInteraction
    {
        [Header("Interaction")] [SerializeField]
        private Bag bag;

        [SerializeField] private Diary diary;
        [SerializeField] private ShowInteraction tornDiary;

        private void Start()
        {
            diary.onOpen = () =>
            {
                //setInteractable(false);
                //showPanel.SetActive(true);
            };
            diary.onPickUp = () => { bag.isOut = true; };
            diary.onFire = () =>
            {
                diary.gameObject.SetActive(false);
                tornDiary.gameObject.SetActive(true);
            };
        }

        public override void Appear()
        {
        }

        public override void ResetInteraction()
        {
            diary.Reset();
            bag.Reset();
            tornDiary.gameObject.SetActive(false);
        }
    }
}