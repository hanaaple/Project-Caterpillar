using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    public Button continueButton;
    
    public Button newStartButton;




    async void Loading()
    {
        var asyncOperation = SceneManager.LoadSceneAsync("SampleScene");
        // asyncOperation.
    }
}
