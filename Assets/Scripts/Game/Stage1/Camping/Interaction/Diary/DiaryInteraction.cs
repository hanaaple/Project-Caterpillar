using Game.Stage1.Camping.Interaction.Show;
using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Diary
{
    public class DiaryInteraction : CampingInteraction
    {
        [Header("Interaction")] [SerializeField]
        private Bag bag;

        [SerializeField] private Diary diary;
        [SerializeField] private ShowInteraction tornDiary;
        [SerializeField] private AudioData fireAudioData;

        public void Awake()
        {
            bag.Init();
        }

        private void Start()
        {
            // diary.onOpen = () =>
            // {
                //setInteractable(false);
                //showPanel.SetActive(true);
            // };
            diary.onPickUp = () => { bag.IsInteractable = false; };
            diary.onFire = () =>
            {
                fireAudioData.Play();
                diary.gameObject.SetActive(false);
                tornDiary.gameObject.SetActive(true);
            };
        }

        public override void ResetInteraction(bool isGameReset = false)
        {
            base.ResetInteraction(isGameReset);
            diary.Reset();
            bag.Reset();
            tornDiary.gameObject.SetActive(false);
        }
    }
}