using System;
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


        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        public GameObject blinkingIndicator;


        [SerializeField] private Button dialogueInputArea;


        private Coroutine _printCoroutine;


        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            dialogueInputArea.onClick.AddListener(() => { InputConverse(); });
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
                StopCoroutine(_printCoroutine);
                CompleteDialogue();
            }
            else
            {
                dialogueProps.index++;
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
                _printCoroutine = StartCoroutine(DialoguePrint());
            }
        }

        private IEnumerator DialoguePrint()
        {
            Debug.Log("프린트 시작");
            isUnfolding = true;
            blinkingIndicator.SetActive(false);
            dialogueText.text = "";
            var dialogueItem = dialogueProps.datas[dialogueProps.index];

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
            _printCoroutine = null;
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

            dialoguePanel.SetActive(false);
        }
    }
}