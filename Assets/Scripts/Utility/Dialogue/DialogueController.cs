using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.JsonLoader;

namespace Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        private static DialogueController _instance;

        public static DialogueController instance => _instance;

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [SerializeField] private Text dialogueText;

        [SerializeField] private Button dialogueInputArea;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private DialogueProps dialogueProps;

        [SerializeField] private bool isUnfolding;

        [SerializeField] private float textSpeed;

        private Coroutine _printCoroutine;


        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            dialogueInputArea.onClick.AddListener(InputConverse);
        }

        private async void InitDialogue(string jsonAsset)
        {
            dialogueProps.datas = JsonHelper.GetJsonArray<DialogueItemProps>(jsonAsset);
            dialogueProps.index = 0;
            isUnfolding = false;

            dialoguePanel.SetActive(true);

            await Task.Delay((int) (Time.deltaTime * 1000));
            var playerActions = InputManager.inputControl.PlayerActions;
            playerActions.Enable();
            playerActions.Dialogue.performed += InputConverse;
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

        private void InputConverse(InputAction.CallbackContext obj)
        {
            if (dialoguePanel.activeSelf && !choicePanel.activeSelf)
            {
                InputConverse();
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
            
            
            for (var i = 0; i < choicePanel.transform.childCount; i++)
            {
                choicePanel.transform.GetChild(i).gameObject.SetActive(false);
                choicePanel.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text =
                    "";
                choicePanel.transform.GetChild(i).GetComponentInChildren<Button>().onClick
                    .RemoveAllListeners();
            }

            
            
            if (dialogueItem.dialogueType == DialogueType.Choice)
            {
                dialogueProps.index++;
                choicePanel.SetActive(true);
                var choicedLen = 0; 
                while (dialogueProps.index < dialogueProps.datas.Length &&
                       (dialogueProps.datas[dialogueProps.index].dialogueType ==
                           DialogueType.ChoiceContext || dialogueProps.datas[dialogueProps.index].dialogueType ==
                           DialogueType.ChoiceDialogue))
                {
                    var choiceLen = 0;
                    // 2333 4444에서 2333까지 읽기
                    while (dialogueProps.index + choiceLen < dialogueProps.datas.Length &&
                           dialogueProps.datas[dialogueProps.index + choiceLen].dialogueType ==
                           DialogueType.ChoiceContext)
                    {
                        choiceLen++;
                    }

                    var choiceDialogueLen = 0;
                    // 23 4444에서  23 4444까지 읽기
                    while (dialogueProps.index + choiceLen + choiceDialogueLen < dialogueProps.datas.Length &&
                           dialogueProps.datas[dialogueProps.index + choiceLen + choiceDialogueLen].dialogueType ==
                           DialogueType.ChoiceDialogue)
                    {
                        choiceDialogueLen++;
                    }
                    
                    Debug.Log(dialogueProps.index);
                    Debug.Log(choiceLen);
                    Debug.Log(choiceDialogueLen);

                    for (var i = 0; i < choiceLen; i++)
                    {
                        var child = choicePanel.transform.GetChild(choicedLen + i);
                        Debug.Log(child);
                        child.gameObject.SetActive(true);
                        child.GetComponentInChildren<TMP_Text>().text =
                            dialogueProps.datas[dialogueProps.index + i].contents;

                        var curIdx = dialogueProps.index;
                        child.GetComponentInChildren<Button>().onClick
                            .AddListener(() =>
                            {
                                Debug.Log(curIdx);
                                Debug.Log("선택 개수: " + choiceLen);
                                Debug.Log("선택 대화 길이: " + choiceDialogueLen);
                            });
                    }

                    dialogueProps.index += choiceLen + choiceDialogueLen;
                    choicedLen += choiceLen;
                    Debug.Log(dialogueProps.index);
                }
            }
            else
            {
                blinkingIndicator.SetActive(true);
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

            dialoguePanel.SetActive(false);

            var playerActions = InputManager.inputControl.PlayerActions;
            playerActions.Enable();
            playerActions.Dialogue.performed -= InputConverse;
        }
    }
}