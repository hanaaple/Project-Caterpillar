using UnityEngine;
using UnityEngine.UI;
using Utility.SaveSystem;

public class LevelManager : MonoBehaviour
{
    public Button continueButton;
    
    public Button newStartButton;

    void Start()
    {
        continueButton.onClick.AddListener(() =>
        {
            // SavePanelManager.instance.SetSaveLoadPanelActive(true);
        });
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("MainScene");
        });
        
        SavePanelManager.instance.InitLoad();
        SavePanelManager.instance.OnLoad.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("MainScene");
        });
    }
}
