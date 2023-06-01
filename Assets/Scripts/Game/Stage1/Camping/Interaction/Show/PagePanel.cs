using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class PagePanel : ShowPanel
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [SerializeField] private GameObject[] panels;
        
        private int _index;

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

        private void SetPage(int changeValue)
        {
            panels[_index].SetActive(false);
            _index = Mathf.Clamp(_index + changeValue, 0, panels.Length - 1);
            panels[_index].SetActive(true);
        }

        public override void Show()
        {
            base.Show();
            SetPage(-panels.Length);
        }
    }
}