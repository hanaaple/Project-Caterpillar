using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.SaveSystem;

public class SaveLoadTest : MonoBehaviour
{
    public GameObject savePanel;
    public List<Button> saveButtons;
    
    public GameObject continuePanel;
    public List<Button> loadButtons;
    
    void Start()
    {
        saveButtons = new List<Button>();
        for (var i = 0; i < savePanel.transform.childCount; i++)
        {
            var button = savePanel.transform.GetChild(i).GetComponent<Button>();
            var idx = i;
            button.onClick.AddListener(() =>
            {
                SaveButton(idx);
            });
            saveButtons.Add(button);
        }
        
        loadButtons = new List<Button>();
        for (var i = 0; i < continuePanel.transform.childCount; i++)
        {
            var button = continuePanel.transform.GetChild(i).GetComponent<Button>();
            var idx = i;
            button.onClick.AddListener(() =>
            {
                LoadButton(idx);
            });
            loadButtons.Add(button);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    private void LoadButton(int idx)
    {
        if (SaveManager.Load(idx))
        {
            var saveData = SaveManager.GetSaveData(); 
            Debug.Log(saveData.hp);
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    private void SaveButton(int idx)
    {
        var saveData = SaveManager.GetSaveData();
        saveData.SetHp(idx * 100);
        
        SaveManager.Save(idx);
        Debug.Log(saveData.hp);
    }
}
