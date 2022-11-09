using UnityEngine;
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
}
