using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.SaveSystem
{
    public class SaveLoadItemProps : MonoBehaviour
    {
        public int index;

        private SaveData _saveData;

        [SerializeField] private TMP_Text scenarioText;

        public Button button;

        private Animator _animator;

        public void Init()
        {
            _animator = GetComponent<Animator>();
        }

        public void InitEventTrigger(UnityAction<BaseEventData> call)
        {
            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryPointerDown.callback.AddListener(call);
            eventTrigger.triggers.Add(entryPointerDown);
        }

        public void UpdateUI()
        {
            _saveData = SaveManager.GetLoadData(index);
            if (_saveData != null)
            {
                scenarioText.text = _saveData.scenario;
            }
            else
            {
                scenarioText.text = "";
                button.onClick.RemoveAllListeners();
            }
        }

        public void Execute()
        {
            button.onClick?.Invoke();
        }

        public void SetDefault()
        {
            _animator.SetBool("Selected", false);
        }

        public void SetHighlight()
        {
            _animator.SetBool("Selected", true);
        }
    }
}
