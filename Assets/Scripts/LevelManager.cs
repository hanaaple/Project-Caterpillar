using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    public Button continueButton;
    
    public Button newStartButton;

    public GameObject continuePanel;

    void Start()
    {
        continueButton.onClick.AddListener(() =>
        {
            continuePanel.SetActive(true);
        });
        newStartButton.onClick.AddListener(() =>
        {
            SceneLoader.Instance.LoadScene("TestScene");
        });
        for (var i = 0; i < continuePanel.transform.childCount; i++)
        {
            continuePanel.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("TestScene");
            });
        }
    }
    
    void LoadScene()
    {
        
    }

    void LoadScene(int idx)
    {
        
    }

    async void Loading()
    {
        var asyncOperation = SceneManager.LoadSceneAsync("SampleScene");
        // asyncOperation.
    }
}
