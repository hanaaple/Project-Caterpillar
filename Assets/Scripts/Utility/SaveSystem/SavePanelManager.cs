using System;
using UnityEngine;
using UnityEngine.Events;
using Utility.SaveSystem;

public class SavePanelManager : MonoBehaviour
{
    private static SavePanelManager _instance;

    public static SavePanelManager instance => _instance;


    
    [SerializeField] private GameObject savePanel;

    [SerializeField]
    private SaveLoadItemProps[] saveItemPropsArray;

    [NonSerialized]
    public UnityEvent onSave;
    
    [NonSerialized]
    public UnityEvent onLoad;
    
    private void Awake()
    {
        _instance = this;
    }


    public void InitLoad()
    {
        onLoad = new UnityEvent();
        foreach (var saveLoadItemProps in saveItemPropsArray)
        {
            saveLoadItemProps.button.onClick.RemoveAllListeners();
            saveLoadItemProps.button.onClick.AddListener(() =>
            {
                LoadButton(saveLoadItemProps._index);
            });
            saveLoadItemProps.UpdateUI();
        }
    }

    public void InitSave()
    {
        onSave = new UnityEvent();
        foreach (var saveLoadItemProps in saveItemPropsArray)
        {
            saveLoadItemProps.button.onClick.RemoveAllListeners();
            saveLoadItemProps.button.onClick.AddListener(() =>
            {
                SaveButton(saveLoadItemProps._index);
            });
            saveLoadItemProps.UpdateUI();
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    private void SaveButton(int idx)
    {
        Debug.Log($"{idx}번 세이브 파일 저장하기");
        var saveData = SaveManager.GetSaveData();
        saveData.SetHp(idx * 100);
        saveData.SetScenario("테스트입니다.");
        saveData.SetPlayTime(1122);
        
        SaveManager.Save(idx);

        onSave?.Invoke();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"> 0 ~ n </param>
    private void LoadButton(int idx)
    {
        Debug.Log($"{idx}번 세이브 파일 로드");

        if (SaveManager.Load(idx))
        {
            onLoad?.Invoke();
        }
        else
        {
            Debug.LogError("Load 실패");
        }
    }

    public void SetSaveLoadPanelActive(bool isActive)
    {
        savePanel.SetActive(isActive);
        if (isActive)
        {
            foreach (var saveItemProps in saveItemPropsArray)
            {
                saveItemProps.UpdateUI();
            }
        }        
    }
}
