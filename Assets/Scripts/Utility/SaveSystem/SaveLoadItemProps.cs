using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.SaveSystem;

public class SaveLoadItemProps : MonoBehaviour
{
    public int _index;
    
    private SaveData _saveData;

    [SerializeField]
    private TMP_Text scenarioText;

    public Button button;

    public void UpdateUI()
    {
        _saveData = SaveManager.GetLoadData(_index);
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
}
