using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility.JsonLoader;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        private static DialogueController _instance;

        [SerializeField] private GameObject dialoguePanel;
        
        [SerializeField] private Text dialogueText;
        
        public static DialogueController instance => _instance;

        [SerializeField] private DialogueProps dialogueProps;

        [Header("디버깅용")] [SerializeField] private bool isUnfolding;
        
        [SerializeField] private float textSpeed;

        
        [Header("깜빡이는 애니메이션 들어간 ui")]
        [SerializeField] public GameObject blinkingIndicator;
        
        
        
        private void Awake()
        {
            _instance = this;
        }

        private void InitDialogue(string jsonAsset)
        {
            dialogueProps.datas = JsonHelper.GetJsonArray<DialogueItemProps>(jsonAsset);
            dialogueProps.index = 0;
            isUnfolding = false;
            
            dialoguePanel.SetActive(true);
        }

        public void Converse(string jsonAsset)
        {
            InitDialogue(jsonAsset);
            
            ProgressConversation();
        }

        private void InputConverse()
        {
            if (isUnfolding)
            {
                StopCoroutine(DialoguePrint());
                CompleteDialogue();
            }
            else
            {
                dialogueProps.index++;
                blinkingIndicator.SetActive(false);
                ProgressConversation();
            }
        }

        private void ProgressConversation()
        {
            if (IsDialogueEnd())
            {
                EndConversation();
            }
            else
            {
                StartCoroutine(DialoguePrint());
            }
        }

        private IEnumerator DialoguePrint()
        {
            var dialogueItem = dialogueProps.datas[dialogueProps.index];
            
            dialogueText.text = dialogueItem.contents;
            
            var waitForSec = new WaitForSeconds(textSpeed);

            foreach (var t in dialogueItem.contents)
            {
                dialogueText.text += t;
                yield return waitForSec;
            }

            CompleteDialogue();
        }

        private void CompleteDialogue()
        {
            Debug.Log("stop coroutine 확인용 디버그");
            isUnfolding = false;
            var dialogueItem = dialogueProps.datas[dialogueProps.index];
            dialogueText.text = dialogueItem.contents;
            blinkingIndicator.SetActive(true);
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