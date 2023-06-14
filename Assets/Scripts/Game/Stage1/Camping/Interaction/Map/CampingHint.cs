using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Map
{
    public class CampingHint : MonoBehaviour
    {
        [SerializeField] private GameObject on;
        [SerializeField] private GameObject off;
        [SerializeField] private GameObject hintPanel;

        private void Start()
        {
            on.GetComponentInChildren<Button>(true).onClick.AddListener(() => { hintPanel.SetActive(true); });
        }

        public void SetHint(bool isOpen)
        {
            on.SetActive(isOpen);
            off.SetActive(!isOpen);
        }
    }
}