using UnityEngine;
using UnityEngine.UI;

namespace Game.Camping
{
    public class DiaryInteractor : CampingInteraction
    {
        [SerializeField]
        private GameObject showPanel;

        [SerializeField]
        private Button exitButton;

        [SerializeField] private Bag bag;
        
        [SerializeField]
        private Diary diary;

        [SerializeField] private ShowInteractor tornDiary;

        private void Start()
        {
            exitButton.onClick.AddListener(() =>
            {
                showPanel.SetActive(false);
                setInteractable(true);
            });
            diary.onOpen = () =>
            {
                //setEnable(false);
                //showPanel.SetActive(true);
            };
            diary.onPickUp = () =>
            {
                bag.isOut = true;
            };
            diary.onFire = () =>
            {
                bag.OnFire();
                diary.gameObject.SetActive(false);
                tornDiary.gameObject.SetActive(true);
            };
            tornDiary.onAppear = Appear;
        }

        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void Reset()
        {
            tornDiary.setInteractable = setInteractable;
            diary.Reset();
            bag.Reset();
            tornDiary.gameObject.SetActive(false);
        }
    }
}
