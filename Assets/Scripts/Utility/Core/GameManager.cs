using UnityEngine;
using Utility.Dialogue;

namespace Utility.Core
{
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
            var gameManagerPrefab = Resources.Load<GameManager>("GameManager");
            return Instantiate(gameManagerPrefab);
        }
    
        public bool IsCharacterControlEnable()
        {
            return !DialogueController.Instance.isDialogue && !Mathf.Approximately(Time.timeScale, 0f);
        }
    }
}
