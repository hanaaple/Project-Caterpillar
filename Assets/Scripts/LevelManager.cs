using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    public Button continueButton;
    
    public Button newStartButton;

    void Start()
    {
        continueButton.onClick.AddListener(() =>
        {
            SavePanelManager.instance.SetSaveLoadPanelActive(true);
        });
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("TestScene");
        });
        
        SavePanelManager.instance.InitLoad();
        SavePanelManager.instance.onLoad.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("TestScene");
        });
    }
}
