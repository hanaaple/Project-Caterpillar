using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class PagePanel : ShowPanel
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [SerializeField] private GameObject[] panels;
        
        protected int Index;

        public override void Initialize()
        {
            base.Initialize();
            
            leftButton.onClick.AddListener(() =>
            {
                SetPage(-1);
            });
            
            rightButton.onClick.AddListener(() =>
            {
                SetPage(1);
            });
        }

        protected virtual void SetPage(int changeValue)
        {
            panels[Index].SetActive(false);
            Index = Mathf.Clamp(Index + changeValue, 0, panels.Length - 1);
            panels[Index].SetActive(true);
        }

        public override void Show()
        {
            base.Show();
            SetPage(-int.MaxValue);
        }
    }
}