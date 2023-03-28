using UnityEngine;
using Utility.Dialogue;
using Utility.UI.Inventory;
using Utility.UI.Pause;
using Utility.UI.Preference;

namespace Utility.Core
{
    public class PlayUIManager : MonoBehaviour
    {    
        private static PlayUIManager _instance;
        public static PlayUIManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<PlayUIManager>();
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
        
        [SerializeField] private PauseManager pauseManager;
        [SerializeField] private PreferenceManager preferenceManager;
        [SerializeField] private InventoryManager inventoryManager;
        
        [SerializeField] private Canvas canvas;

        private static PlayUIManager Create()
        {
            var playUIManagerPrefab = Resources.Load<PlayUIManager>("Play UI Manager");
            return Instantiate(playUIManagerPrefab);
        }

        private void Awake()
        {
            if (_instance && _instance.gameObject != gameObject)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public bool IsCanvasUse()
        {
            return DialogueController.Instance.IsDialogue || inventoryManager.GetIsOpen() || pauseManager.GetIsOpen();
        }
    }
}
