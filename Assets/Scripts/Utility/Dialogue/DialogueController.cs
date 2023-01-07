using System;
using System.Collections;
using System.Threading.Tasks;
using Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.JsonLoader;

namespace Utility.Dialogue
{
    public class DialogueController : MonoBehaviour
    {
        private static DialogueController _instance;

        public static DialogueController instance => _instance;

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private GameObject savePanel;

        [SerializeField] private Text dialogueText;

        [SerializeField] private Button dialogueInputArea;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private DialogueProps dialogueProps;

        [SerializeField] private DialogueProps baseDialogueProps;
        [SerializeField] private DialogueProps choicedDialogueProps;

        [SerializeField] private bool isUnfolding;

        [SerializeField] private float textSpeed;

        private Coroutine _printCoroutine;

        private UnityEvent _onComplete;

        private UnityEvent _onLast;

        private bool _isDialogue;


        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            dialogueInputArea.onClick.AddListener(InputConverse);
            baseDialogueProps = new DialogueProps();
            choicedDialogueProps = new DialogueProps();

            _onComplete = new UnityEvent();
            _onLast = new UnityEvent();
        }

        private async void InitDialogue(string jsonAsset)
        {
            _isDialogue = true;
            dialogueProps = baseDialogueProps;
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
            if (_isDialogue)
            {
                return;
            }

            InitDialogue(jsonAsset);

            ProgressConversation();
        }

        public void InputConverse()
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
            if (dialoguePanel.activeSelf && !choicePanel.activeSelf && !savePanel.activeSelf)
            {
                InputConverse();
            }
        }

        private void ProgressConversation()
        {
            if (IsDialogueEnd())
            {
                if (choicedDialogueProps == dialogueProps)
                {
                    choicedDialogueProps.index = 0;
                    choicedDialogueProps.datas = null;
                    dialogueProps = baseDialogueProps;
                    InputConverse();
                }
                else
                {
                    EndConversation();
                }
            }
            else
            {
                var dialogue = dialogueProps.datas[dialogueProps.index];
                if (dialogue.dialogueType == DialogueType.Script)
                {
                    _printCoroutine = StartCoroutine(DialoguePrint());
                }
                else if (dialogue.dialogueType == DialogueType.MoveMap)
                {
                    EndConversation();
                    Debug.Log(dialogue.contents + "로 맵 이동");
                    SceneLoader.SceneLoader.Instance.LoadScene(dialogue.contents);
                }
                else if (dialogue.dialogueType == DialogueType.Save)
                {
                    _onComplete.AddListener(() =>
                    {
                        SavePanelManager.instance.InitSave();
                        SavePanelManager.instance.SetSaveLoadPanelActive(true);
                        _onComplete.RemoveAllListeners();
                        SavePanelManager.instance.onSave.AddListener(() =>
                        {
                            SavePanelManager.instance.SetSaveLoadPanelActive(false);
                            InputConverse();
                            SavePanelManager.instance.onSave.RemoveAllListeners();
                        });
                    });
                    _printCoroutine = StartCoroutine(DialoguePrint());
                }
                else if (dialogue.dialogueType == DialogueType.ChoiceEnd)
                {
                    Debug.LogError("하이");
                }
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
            _printCoroutine = null;
            isUnfolding = false;
            var dialogueItem = dialogueProps.datas[dialogueProps.index];
            dialogueText.text = dialogueItem.contents;


            if ((dialogueItem.dialogueType == DialogueType.Script || dialogueItem.dialogueType == DialogueType.None) &&
                dialogueProps.datas.Length > dialogueProps.index + 1 &&
                dialogueProps.datas[dialogueProps.index + 1].dialogueType == DialogueType.Choice)
            {
                for (var i = 0; i < choicePanel.transform.childCount; i++)
                {
                    choicePanel.transform.GetChild(i).gameObject.SetActive(false);
                    choicePanel.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text =
                        "";
                    choicePanel.transform.GetChild(i).GetComponentInChildren<Button>().onClick
                        .RemoveAllListeners();
                }

                dialogueProps.index++;
                choicePanel.SetActive(true);
                var choicedLen = 0;
                while (dialogueProps.index < dialogueProps.datas.Length &&
                       dialogueProps.datas[dialogueProps.index].dialogueType !=
                       DialogueType.ChoiceEnd)
                {
                    Debug.Log(dialogueProps.index);

                    var choiceLen = 0;
                    // 2111211에서 2까지 읽기
                    while (dialogueProps.index + choiceLen < dialogueProps.datas.Length &&
                           dialogueProps.datas[dialogueProps.index + choiceLen].dialogueType ==
                           DialogueType.Choice)
                    {
                        choiceLen++;
                    }

                    var choiceContextLen = 0;
                    var choiceEnd = 0;
                    // 2111211에서 2111까지
                    while (dialogueProps.index + choiceLen + choiceContextLen < dialogueProps.datas.Length)
                    {
                        if (dialogueProps.datas[dialogueProps.index + choiceLen + choiceContextLen].dialogueType ==
                            DialogueType.Choice)
                        {
                            break;
                        }

                        if (dialogueProps.datas[dialogueProps.index + choiceLen + choiceContextLen]
                                .dialogueType ==
                            DialogueType.ChoiceEnd)
                        {
                            choiceEnd++;
                            break;
                        }

                        choiceContextLen++;
                    }

                    Debug.Log(dialogueProps.index);
                    Debug.Log(choiceLen);
                    Debug.Log(choiceContextLen);

                    for (var i = 0; i < choiceLen; i++)
                    {
                        var child = choicePanel.transform.GetChild(choicedLen + i);
                        Debug.Log(child);
                        child.gameObject.SetActive(true);
                        child.GetComponentInChildren<TMP_Text>().text =
                            dialogueProps.datas[dialogueProps.index + i].contents;

                        var curIdx = dialogueProps.index;
                        var button = child.GetComponentInChildren<Button>();
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() =>
                        {
                            OnClickChoice(curIdx, choiceLen, choiceContextLen + choiceEnd);
                        });
                    }

                    dialogueProps.index += choiceLen + choiceContextLen;
                    choicedLen += choiceLen;
                    Debug.Log(dialogueProps.index);
                }
            }
            else
            {
                blinkingIndicator.SetActive(true);
                _onComplete?.Invoke();
                if (dialogueProps.index == dialogueProps.datas.Length - 1 && (choicedDialogueProps != dialogueProps ||
                                                                              baseDialogueProps.index ==
                                                                              baseDialogueProps.datas.Length))
                {
                    _onLast?.Invoke();
                }
            }
        }

        private void OnClickChoice(int curIdx, int choiceLen, int choiceContextLen)
        {
            Debug.Log(curIdx);
            Debug.Log("선택 개수: " + choiceLen);
            Debug.Log("선택 대화 길이: " + choiceContextLen);


            if (choiceContextLen != 0)
            {
                choicedDialogueProps.index = 0;
                choicedDialogueProps.datas = new DialogueItemProps[choiceContextLen];
                Array.Copy(baseDialogueProps.datas, curIdx + choiceLen, choicedDialogueProps.datas, 0,
                    choiceContextLen);
                dialogueProps = choicedDialogueProps;
            }

            choicePanel.SetActive(false);
            ProgressConversation();
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

            _onLast?.RemoveAllListeners();

            var playerActions = InputManager.inputControl.PlayerActions;
            playerActions.Enable();
            playerActions.Dialogue.performed -= InputConverse;
        }
    }
}