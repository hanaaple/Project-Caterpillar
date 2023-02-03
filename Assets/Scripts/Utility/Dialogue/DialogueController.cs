using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;
using Utility.InputSystem;
using Utility.JsonLoader;
using Utility.SaveSystem;

namespace Utility.Dialogue
{
    [Serializable]
    public class DialogueSelector : Highlight
    {
        private Animator _animator;

        public void Init(Animator animator)
        {
            _animator = animator;
        }


        public override void SetDefault()
        {
            _animator.SetBool("Selected", false);
        }
        
        public override void SetHighlight()
        {
            _animator.SetBool("Selected", true);
        }
    }
    public class DialogueController : MonoBehaviour
    {
        private static DialogueController _instance;

        public static DialogueController instance => _instance;

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private GameObject savePanel;

        [SerializeField] private TMP_Text dialogueText;

        [SerializeField] private Button dialogueInputArea;
        
        [Header("Choice")]
        [SerializeField] private DialogueSelector[] dialogueSelectors;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;
        
        [Header("좌 애니메이터")] [SerializeField]
        private Animator leftAnimator;
        
        [Header("우 애니메이터")] [SerializeField]
        private Animator rightAnimator;
        
        [SerializeField] private float textSpeed;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private DialogueProps dialogueProps;

        [SerializeField] private DialogueProps baseDialogueProps;
        [SerializeField] private DialogueProps choicedDialogueProps;

        private bool _isUnfolding;

        private Coroutine _printCoroutine;

        private UnityEvent _onComplete;

        private UnityEvent _onLast;

        [NonSerialized] public bool IsDialogue;

        private int _selectedIdx;
        private bool _isSkipEnable;

        private Action<InputAction.CallbackContext> _onInput;
        private Action<InputAction.CallbackContext> _onExecute;
        private void Awake()
        {
            _instance = this;
            _onInput = _ =>
            {
                if (!choicePanel.activeSelf)
                {
                    return;
                }
                var input = _.ReadValue<Vector2>();
                var idx = _selectedIdx;
                if (input == Vector2.up)
                {
                    idx = (idx - 1 + dialogueSelectors.Length) % dialogueSelectors.Length;
                }
                else if (input == Vector2.down)
                {
                    idx = (idx + 1) % dialogueSelectors.Length;
                }

                HighlightButton(idx);
            };

            _onExecute = _ =>
            {
                if (!choicePanel.activeSelf)
                {
                    return;
                }

                dialogueSelectors[_selectedIdx].button.onClick?.Invoke();
            };
        }

        void Start()
        {
            dialogueInputArea.onClick.AddListener(InputConverse);
            baseDialogueProps = new DialogueProps();
            choicedDialogueProps = new DialogueProps();

            _onComplete = new UnityEvent();
            _onLast = new UnityEvent();
            
            foreach (var dialogueSelector in dialogueSelectors)
            {
                dialogueSelector.Init(dialogueSelector.button.GetComponent<Animator>());
            }
        }

        private async void InitDialogue(string jsonAsset)
        {
            IsDialogue = true;
            dialogueProps = baseDialogueProps;
            dialogueProps.datas = JsonHelper.GetJsonArray<DialogueItemProps>(jsonAsset);
            dialogueProps.index = 0;
            _isUnfolding = false;

            dialoguePanel.SetActive(true);

            await Task.Delay((int) (Time.deltaTime * 1000));
            
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Enable();
            uiActions.Dialogue.performed += InputConverse;
            uiActions.Select.performed += _onInput;
            uiActions.Execute.performed += _onExecute;
        }

        public void Converse(string jsonAsset)
        {
            if (IsDialogue)
            {
                return;
            }

            InitDialogue(jsonAsset);

            ProgressConversation();
        }

        public void InputConverse()
        {
            if (_isUnfolding)
            {
                if (_isSkipEnable)
                {
                    StopCoroutine(_printCoroutine);
                    CompleteDialogue();
                }
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

                    
                    if (dialogue.option != null && dialogue.option.Length >= 1)
                    {
                        var side = dialogue.option[0];
                        // Debug.Log(side);
                        Animator animator = null;
                        if (side == "Left")
                        {
                            animator = leftAnimator;
                        }
                        else if (side == "Right")
                        {
                            animator = rightAnimator;
                        }
                        
                        if (dialogue.option.Length == 2)
                        {
                            var state = dialogue.option[1].Replace(" ", "");
                            Debug.Log(state + "  " + dialogue.name + " " + dialogue.expression);
                            if (animator != null)
                            {
                                animator.SetTrigger(state);
                            }
                        }
                    
                        if (animator != null)
                        {
                            animator.SetInteger("Character", (int)dialogue.name);
                            animator.SetInteger("Expression", (int)dialogue.expression);
                        }
                    }
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
                        SavePanelManager.instance.OnSave.AddListener(() =>
                        {
                            SavePanelManager.instance.SetSaveLoadPanelActive(false);
                            InputConverse();
                            SavePanelManager.instance.OnSave.RemoveAllListeners();
                        });
                    });
                    _printCoroutine = StartCoroutine(DialoguePrint());
                }
                else if (dialogue.dialogueType == DialogueType.ChoiceEnd)
                {
                    // Debug.LogError("하이");
                }
                else if (dialogue.dialogueType == DialogueType.Character)
                {
                    var side = dialogue.option[0];
                    // Debug.Log(side);
                    Animator animator = null;
                    if (side == "Left")
                    {
                        animator = leftAnimator;
                    }
                    else if (side == "Right")
                    {
                        animator = rightAnimator;
                    }

                    var state = dialogue.option[1].Replace(" ", "");
                    Debug.Log(state + "  " + dialogue.name + " " + dialogue.expression);
                    
                    if (animator != null)
                    {
                        animator.SetTrigger(state);
                        animator.SetInteger("Character", (int)dialogue.name);
                        animator.SetInteger("Expression", (int)dialogue.expression);
                    }
                }
            }
        }

        private IEnumerator DialoguePrint()
        {
            Debug.Log("프린트 시작");
            _isUnfolding = true;
            blinkingIndicator.SetActive(false);
            dialogueText.text = "";
            var dialogueItem = dialogueProps.datas[dialogueProps.index];

            float speed = 1;

            if (dialogueItem.option != null)
            {
                _isSkipEnable = true;

                var options = Array.FindAll(dialogueItem.option, item => !item.Contains("(") && item.Any(char.IsDigit));
                var intOptions = options.Select(item => (int)float.Parse(item));
                var enumerable = intOptions as int[] ?? intOptions.ToArray();
                if (enumerable.Length > 0)
                {
                    if (enumerable.Contains(0))
                    {
                        Debug.Log("스킵 불가능!");
                        _isSkipEnable = false;
                    }

                }

                if (dialogueItem.option.Any(item => item.Contains("1(")))
                {
                    var speedString = Array.Find(dialogueItem.option, item => item.Contains("1("));
                    speed = float.Parse(speedString.Split("(")[1].Split(")")[0]);
                    Debug.Log("속도: " + speed);
                }
            }
            
            var waitForSec = new WaitForSeconds(textSpeed / speed);

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
            _isUnfolding = false;
            var dialogueItem = dialogueProps.datas[dialogueProps.index];
            dialogueText.text = dialogueItem.contents;


            if (dialogueProps.datas.Length > dialogueProps.index + 1 &&
                dialogueProps.datas[dialogueProps.index + 1].dialogueType == DialogueType.Choice)
            {
                choicePanel.SetActive(true);
                InitChoice();

                dialogueProps.index++;
                
                var choicedIdx = 0;
                while (dialogueProps.index < dialogueProps.datas.Length &&
                       dialogueProps.datas[dialogueProps.index].dialogueType !=
                       DialogueType.ChoiceEnd)
                {
                    Debug.Log(dialogueProps.index);

                    var choiceCount = 0;
                    // 2111211에서 2까지 읽기
                    while (dialogueProps.index + choiceCount < dialogueProps.datas.Length &&
                           dialogueProps.datas[dialogueProps.index + choiceCount].dialogueType ==
                           DialogueType.Choice)
                    {
                        choiceCount++;
                    }

                    var choiceContextLen = 0;
                    var choiceEnd = 0;
                    // 2111211에서 2111까지
                    while (dialogueProps.index + choiceCount + choiceContextLen < dialogueProps.datas.Length)
                    {
                        if (dialogueProps.datas[dialogueProps.index + choiceCount + choiceContextLen].dialogueType ==
                            DialogueType.Choice)
                        {
                            break;
                        }

                        if (dialogueProps.datas[dialogueProps.index + choiceCount + choiceContextLen]
                                .dialogueType ==
                            DialogueType.ChoiceEnd)
                        {
                            choiceEnd++;
                            break;
                        }

                        choiceContextLen++;
                    }

                    Debug.Log(dialogueProps.index);
                    Debug.Log(choiceCount);
                    Debug.Log(choiceContextLen);

                    for (var i = 0; i < choiceCount; i++)
                    {
                        var idx = choicedIdx + i;
                        var child = choicePanel.transform.GetChild(idx);
                        Debug.Log(child);
                        child.gameObject.SetActive(true);
                        child.GetComponentInChildren<TMP_Text>().text =
                            dialogueProps.datas[dialogueProps.index + i].contents;

                        var curIdx = dialogueProps.index;
                        dialogueSelectors[idx].button.onClick.RemoveAllListeners();
                        dialogueSelectors[idx].button.onClick.AddListener(() =>
                        {
                            OnClickChoice(curIdx, choiceCount, choiceContextLen + choiceEnd);
                        });

                        dialogueSelectors[idx].InitEventTrigger((_) => HighlightButton(idx));
                    }

                    dialogueProps.index += choiceCount + choiceContextLen;
                    choicedIdx += choiceCount;
                    Debug.Log(dialogueProps.index);
                }
                HighlightButton(0);
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

        private void InitChoice()
        {
            foreach (var dialogueSelector in dialogueSelectors)
            {
                dialogueSelector.button.gameObject.SetActive(false);
                dialogueSelector.button.GetComponentInChildren<TMP_Text>().text =
                    "";
                dialogueSelector.button.GetComponentInChildren<Button>().onClick
                    .RemoveAllListeners();
            }
        }
        private void HighlightButton(int idx)
        {
            Debug.Log($"{idx}입니다");
            dialogueSelectors[_selectedIdx].SetDefault();
            _selectedIdx = idx;
            dialogueSelectors[idx].SetHighlight();
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

            IsDialogue = false;
            
            _onLast?.RemoveAllListeners();
            
            
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Disable();
            uiActions.Dialogue.performed -= InputConverse;
            uiActions.Select.performed -= _onInput;
            uiActions.Execute.performed -= _onExecute;
        }
    }
}