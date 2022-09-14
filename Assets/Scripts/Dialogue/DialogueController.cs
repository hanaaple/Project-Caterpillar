using UnityEngine;
using UnityEngine.UI;
using Utility.JsonLoader;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        private DialogueController _instance;

        [SerializeField] private GameObject dialoguePanel;
        
        [SerializeField] private Text dialogueText;
        
        public DialogueController instance => _instance;

        [SerializeField] private DialogueProps dialogueProps;
        
        private void Awake()
        {
            _instance = this;
        }

        private void InitDialogue(string jsonAsset)
        {
            dialogueProps.datas = JsonHelper.GetJsonArray<DialogueItemProps>(jsonAsset);
            dialogueProps.index = 0;
            
            dialoguePanel.SetActive(true);
        }

        public void Converse(string jsonAsset)
        {
            InitDialogue(jsonAsset);
            
            ProgressConversation();
        }

        private void ProgressConversation()
        {
            if (IsDialogueEnd())
            {
                EndConversation();
            }
            else
            {
                var dialogueItem = dialogueProps.datas[dialogueProps.index];
                dialogueText.text = dialogueItem.contents;
                
                dialogueProps.index++;
            }
        }
        
        private bool IsDialogueEnd()
        {
            return dialogueProps.index >= dialogueProps.datas.Length;
        }

        private void EndConversation()
        {
            Debug.Log("대화 끝");
            dialogueProps = default;
            
            dialoguePanel.SetActive(true);
        }
        
        
    }
}