using UnityEngine;
using Utility.JsonLoader;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        private DialogueController _instance;
        
        public DialogueController instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public void StartConversation()
        {
            JsonHelper.
        }
    }
}