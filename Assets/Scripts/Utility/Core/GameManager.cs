using UnityEngine;
using Utility.UI.Dialogue;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                var obj = FindObjectOfType<GameManager>();
                if(obj != null)
                {
                    _instance = obj;
                }
                else
                {
                    _instance = Create();
                }
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }
    
    private static GameManager Create()
    {
        var sceneLoaderPrefab = Resources.Load<GameManager>("GameManager");
        return Instantiate(sceneLoaderPrefab);
    }
    
    public bool IsCharacterControlEnable()
    {
        return !DialogueController.instance.IsDialogue;
    }
}
