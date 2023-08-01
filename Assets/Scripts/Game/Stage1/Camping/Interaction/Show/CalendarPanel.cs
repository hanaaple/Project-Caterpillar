using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class CalendarPanel : ShowPanel
    {
        [SerializeField] private Button calendarButton;

        public override void Initialize()
        {
            calendarButton.onClick.AddListener(() => { calendarButton.image.color = new Color(1, 1, 1, 1); });
        }

        public override void ResetPanel()
        {
            gameObject.SetActive(false);
            calendarButton.image.color = new Color(1, 1, 1, 0);
        }
    }
}