using UnityEngine;
using Utility.Scene;
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
        
        public PauseManager pauseManager;
        
        public PreferenceManager preferenceManager;
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

        public void SetPlayType(PlayType playType)
        {
            if (playType == PlayType.None)
            {
                inventoryManager.SetEnable(false);
            }
            else if (playType == PlayType.Field)
            {
                inventoryManager.SetEnable(true);
            }
            else if (playType == PlayType.MiniGame)
            {
                inventoryManager.SetEnable(false);
            }
        }
    }
}
