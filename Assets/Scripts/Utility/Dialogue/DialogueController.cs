using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.UI.Highlight;
using Random = UnityEngine.Random;

namespace Utility.Dialogue
{
    [Serializable]
    public class ChoiceSelector : HighlightItem
    {
        private Animator _animator;
        private static readonly int Selected = Animator.StringToHash("Selected");

        public void Init(Animator animator)
        {
            _animator = animator;
        }

        public override void SetDefault()
        {
            _animator.SetBool(Selected, false);
        }

        public override void EnterHighlight()
        {
        }

        public override void SetSelect()
        {
            _animator.SetBool(Selected, true);
        }
    }

    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        [SerializeField] private GameObject dialogueCanvas;
        [SerializeField] private Animator cutSceneAnimator;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueInputArea;

        [Header("CutScene")] [SerializeField] private PlayableDirector playableDirector;

        [Header("Choice")] [SerializeField] private ChoiceSelector[] choiceSelectors;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Header("좌 애니메이터")] [SerializeField] private Animator leftAnimator;

        [Header("우 애니메이터")] [SerializeField] private Animator rightAnimator;

        [SerializeField] private float textSpeed;
        
        [SerializeField] private TMP_Text tendencyText;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private List<DialogueData> baseDialogueData;

        [NonSerialized] public bool isDialogue;

        private Stack<DialogueData> _baseDialogueData;
        private bool _isCutSceneSkipEnable;
        private bool _isUnfolding;
        private int _selectedIdx;
        private bool _isSkipEnable;
        private Coroutine _printCoroutine;
        private Coroutine _waitInputCoroutine;
        private Highlighter _choiceHighlighter;
        private UnityAction _onComplete;

        private static readonly int CharacterHash = Animator.StringToHash("Character");
        private static readonly int ExpressionHash = Animator.StringToHash("Expression");
        private static readonly int DisappearHash = Animator.StringToHash("Disappear");

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(dialogueCanvas);
            }

            _choiceHighlighter = new Highlighter
            {
                highlightItems = choiceSelectors,
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            foreach (var choiceSelector in choiceSelectors)
            {
                choiceSelector.Init(choiceSelector.button.GetComponent<Animator>());
            }

            _choiceHighlighter.Init(Highlighter.ArrowType.Vertical);
        }

        private void Start()
        {
            dialogueInputArea.onClick.AddListener(() => { OnInputDialogue(); });
            _baseDialogueData = new Stack<DialogueData>();
            baseDialogueData = new List<DialogueData>();
        }

        private async void Initialize(DialogueData dialogueData)
        {
            isDialogue = true;
            if (!_baseDialogueData.Contains(dialogueData))
            {
                dialogueData.index = 0;
                _baseDialogueData.Push(dialogueData);
                baseDialogueData.Add(dialogueData);
            }

            _isUnfolding = false;
            
            // await Task.Delay((int)(Time.deltaTime * 1000f));

            InputManager.SetUiAction(true);
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed += OnInputDialogue;

            dialogueData?.onDialogueStart?.Invoke();
        }

        public void StartDialogue(string jsonAsset, UnityAction dialogueEndAction = default)
        {
            if (isDialogue)
            {
                return;
            }

            var dialogueData = new DialogueData
            {
                onDialogueEnd = dialogueEndAction
            };
            dialogueData.Init(jsonAsset);
            StartDialogue(dialogueData);
        }

        public void StartDialogue(DialogueData dialogueData)
        {
            if (isDialogue)
            {
                return;
            }

            leftAnimator.ResetTrigger(DisappearHash);
            rightAnimator.SetInteger(CharacterHash, 0);
            rightAnimator.SetInteger(ExpressionHash, 0);

            rightAnimator.ResetTrigger(DisappearHash);
            leftAnimator.SetInteger(CharacterHash, 0);
            leftAnimator.SetInteger(ExpressionHash, 0);
            
            dialoguePanel.SetActive(true);
            subjectText.text = "";
            dialogueText.text = "";
            
            Initialize(dialogueData);

            ProgressDialogue();
        }

        private void OnInputDialogue(InputAction.CallbackContext obj = default)
        {
            if (_waitInputCoroutine != null)
            {
                if (!_isCutSceneSkipEnable)
                {
                    return;
                }

                StopCoroutine(_waitInputCoroutine);
                _waitInputCoroutine = null;
            }
            

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
                _baseDialogueData.Peek().index++;
                ProgressDialogue();
            }
        }

        private void ProgressDialogue()
        {
            if (IsDialogueEnd())
            {
                if (_baseDialogueData.Count >= 2)
                {
                    _baseDialogueData.Pop();
                    OnInputDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                DialogueAction();
            }
        }

        private void DialogueAction()
        {
            var dialogueData = _baseDialogueData.Peek();
            var dialogue = dialogueData.dialogueElements[dialogueData.index];
            ItemManager.Instance.SetItem(dialogue.option);
            // Debug.Log($"{dialogueData.index} 인덱스 실행");
            switch(dialogue.dialogueType)
            {
                case DialogueType.Script:
                {
                    StartDialoguePrint();

                    Option(dialogue);
                    break;
                }
                case DialogueType.MoveMap:
                    EndDialogue();
                    Debug.Log(dialogue.contents + "로 맵 이동");
                    SceneLoader.SceneLoader.Instance.LoadScene(dialogue.contents);
                    break;
                case DialogueType.Save:
                    _onComplete = () =>
                    {
                        InputManager.SetUiAction(false);
                        var uiActions = InputManager.InputControl.Ui;
                        uiActions.Dialogue.performed -= OnInputDialogue;

                        SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.ButtonType.Save);
                        _onComplete = () => {};
                        SavePanelManager.Instance.onSave.AddListener(() =>
                        {
                            OnInputDialogue();
                            SavePanelManager.Instance.onSavePanelActiveFalse?.RemoveAllListeners();
                            SavePanelManager.Instance.SetSaveLoadPanelActive(false,
                                SavePanelManager.ButtonType.None);

                            InputManager.SetUiAction(true);
                            uiActions.Dialogue.performed += OnInputDialogue;
                        });

                        SavePanelManager.Instance.onSavePanelActiveFalse.AddListener(() =>
                        {
                            SavePanelManager.Instance.onSave?.RemoveAllListeners();
                            OnInputDialogue();
                        });
                    };
                    StartDialoguePrint();
                    break;
                case DialogueType.ChoiceEnd:
                    break;
                case DialogueType.CutScene:
                    playableDirector.playableAsset = dialogue.playableAsset;
                    playableDirector.extrapolationMode = dialogue.extrapolationMode;

                    // if (dialogue.option.Contains("UI", StringComparer.OrdinalIgnoreCase))
                    // {
                    //
                    // }
                    // else if (dialogue.option.Contains("Field", StringComparer.OrdinalIgnoreCase))
                    // {
                    //     
                    // }

                    // Binding - Animator 한개만? 아마도? 카메라 흔들리는 트랙, 클립 만들고
                    var timelineAsset = (TimelineAsset)playableDirector.playableAsset;
                    if (timelineAsset != null)
                    {
                        var tracks = timelineAsset.GetOutputTracks();
                        foreach (var temp in tracks)
                        {
                            if (temp is AnimationTrack && cutSceneAnimator)
                            {
                                playableDirector.SetGenericBinding(temp, cutSceneAnimator);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{dialogue.interactIndex}-{dialogueData.index} Task 타임라인 오류");
                    }

                    playableDirector.Play();

                    var options = Array.FindAll(dialogue.option, item => item.Any(char.IsDigit));
                    var intOptions = options.Select(item => (int)float.Parse(item));
                    var enumerable = intOptions as int[] ?? intOptions.ToArray();
                    if (enumerable.Length == 1)
                    {
                        _isCutSceneSkipEnable = true;
                        if (dialogue.option.Contains("False", StringComparer.OrdinalIgnoreCase))
                        {
                            _isCutSceneSkipEnable = false;
                        }

                        _waitInputCoroutine = StartCoroutine(WaitInput(enumerable[0]));
                    }

                    break;
                case DialogueType.None:
                {
                    Debug.LogWarning("무슨 이유로 이걸 쓰신거죠 세상에 맙소사");
                    break;
                }
                case DialogueType.Choice:
                {
                    InitChoice();
                    break;
                }
                case DialogueType.WaitInteract:
                {
                    var waitInteractions = dialogue.waitInteraction.interactions;
                    if (waitInteractions.Length == 0)
                    {
                        Debug.LogWarning($"세팅 오류, Interactor 개수: {waitInteractions.Length}개");
                        OnInputDialogue();
                        break;
                    }

                    dialogueData.index++;
                    EndDialogue(false);
                    foreach (var interaction in waitInteractions)
                    {
                        interaction.Initialize(() =>
                        {
                            Debug.Log($"클리어 남은 개수: {waitInteractions.Count(item => !item.IsClear)}");
                            if (waitInteractions.Any(item => !item.IsClear))
                            {
                                return;
                            }
                            Debug.Log("클리어, 다음 꺼 플레이 가능");
                            Debug.Log(dialogueData.dialogueElements.Length);

                            dialogueData.onDialogueWaitClear?.Invoke();

                            if (dialogue.interactionWaitType == InteractionWaitType.Immediately)
                            {
                                StartDialogue(dialogueData);
                            }
                        });
                    }

                    Debug.Log($"클리어 대기, {waitInteractions.Length}개");

                    break;
                }
                case DialogueType.Interact:
                {
                    if (!dialogue.interaction)
                    {
                        Debug.LogError("세팅 오류, Script -> Interaction 세팅");
                    }
                    dialogue.interaction.StartInteraction(dialogue.interactIndex);
                    break;
                }
                case DialogueType.Random:
                {
                    InitRandom();
                    break;
                }
                case DialogueType.RandomEnd:
                {
                    Debug.LogError("멈춰라 오류다");
                    break;
                }
                case DialogueType.ImmediatelyExecute:
                {
                    //Execute
                    // 옵션 실행
                    Debug.Log("실행했임");
                    Option(dialogue);

                    if (_isUnfolding)
                    {
                        StopCoroutine(_printCoroutine);
                        CompleteDialogue();
                    }
                    _baseDialogueData.Peek().index++;
                    ProgressDialogue();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var tendencyData = TendencyManager.Instance.GetTendencyData();
            tendencyText.text = $"activation: {tendencyData.activation}\n" +
                                $"inactive: {tendencyData.inactive}\n" +
                                $"increase: {tendencyData.increase}\n" +
                                $"descent: {tendencyData.descent}\n";
        }

        private void Option(DialogueElement dialogue)
        {
            if (dialogue.option is { Length: < 1 })
            {
                return;
            }
            for(var index = 0; index < dialogue.option.Length; index++)
            {
                dialogue.option[index] = dialogue.option[index].Replace(" ", "");
            }
            
            // if (option.has tendencyData)
            Debug.Log("더하기");
            var tendencyData = TendencyManager.Instance.GetTendencyData();
            
            tendencyData.increase += 4;
            tendencyData.descent += 3;
            tendencyData.activation += 2;
            tendencyData.inactive += 1;

            var side = Array.Find(dialogue.option, item => item is "Left" or "Right");

            Animator animator = null;
            if (!string.IsNullOrEmpty(side))
            {
                if (side == "Left")
                {
                    animator = leftAnimator;
                }
                else if (side == "Right")
                {
                    animator = rightAnimator;
                }
            }

            if (animator != null)
            {
                var state = Array.Find(dialogue.option,
                    item => Enum.TryParse(item, out CharacterOption characterOption));
                if (!string.IsNullOrEmpty(state))
                {
                    Debug.Log(state + "  " + dialogue.name + " " + dialogue.expression);
                    animator.SetTrigger(state);
                }

                if (dialogue.name != CharacterType.Keep)
                {
                    Debug.Log("ㅎㅇ");
                    animator.SetInteger(CharacterHash, (int)dialogue.name);
                }

                if (dialogue.expression != Expression.Keep)
                {
                    animator.SetInteger(ExpressionHash, (int)dialogue.expression - 1);
                }
            }
        }

        private void StartDialoguePrint()
        {
            _isUnfolding = true;
            blinkingIndicator.SetActive(false);
            subjectText.text = "";
            dialogueText.text = "";
            _printCoroutine = StartCoroutine(DialoguePrint());
        }

        private IEnumerator DialoguePrint()
        {
            Debug.Log("프린트 시작");
            var dialogueItem = _baseDialogueData.Peek().dialogueElements[_baseDialogueData.Peek().index];

            subjectText.text = dialogueItem.subject;
            _isSkipEnable = true;

            var wordSpeed = 1f;
            if (dialogueItem.option != null)
            {
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
                    wordSpeed = float.Parse(speedString.Split("(")[1].Split(")")[0]);
                    Debug.Log("속도: " + wordSpeed);
                }
            }

            var waitForSec = new WaitForSeconds(textSpeed / wordSpeed);

            foreach (var t in dialogueItem.contents)
            {
                dialogueText.text += t;
                yield return waitForSec;
            }
            Debug.Log("프린트 끝");
            CompleteDialogue();
        }

        private void CompleteDialogue()
        {
            _printCoroutine = null;
            _isUnfolding = false;
            var currentDialogueData = _baseDialogueData.Peek();
            var dialogueItem = currentDialogueData.dialogueElements[currentDialogueData.index];
            subjectText.text = dialogueItem.subject;
            dialogueText.text = dialogueItem.contents;


            if (currentDialogueData.dialogueElements.Length > currentDialogueData.index + 1 &&
                currentDialogueData.dialogueElements[currentDialogueData.index + 1].dialogueType == DialogueType.Choice)
            {
                currentDialogueData.index++;
                InitChoice();
            }
            else
            {
                blinkingIndicator.SetActive(true);
                _onComplete?.Invoke();
            }
        }

        private void EndDialogue(bool isEnd = true)
        {
            Debug.Log("대화 끝");

            dialoguePanel.SetActive(false);

            isDialogue = false;

            rightAnimator.SetTrigger(DisappearHash);
            leftAnimator.SetTrigger(DisappearHash);

            InputManager.SetUiAction(false);
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed -= OnInputDialogue;

            if (isEnd)
            {
                var dialogueData = _baseDialogueData.Pop();
                baseDialogueData.Remove(dialogueData);
                dialogueData.onDialogueEnd?.Invoke();
            }
        }

        private void OnClickChoice(int curIdx, int choiceLen, int choiceContextLen)
        {
            Debug.Log(curIdx);
            Debug.Log("선택 개수: " + choiceLen);
            Debug.Log("선택 대화 길이: " + choiceContextLen);

            InitChoiceDialogue(curIdx + choiceLen, choiceContextLen);

            Debug.Log("선택");
            // 클릭할때는 되는데 입력으로 할때는 한번 더 하는 오류 있음
            // 입력시 Dialogue.performed가 실행됨 -> 이걸 막아야됨.
            if (choiceContextLen == 0)
            {
                OnInputDialogue();
            }
            else
            {
                ProgressDialogue();
            }
        }

        private void InitChoice()
        {
            foreach (var dialogueSelector in choiceSelectors)
            {
                dialogueSelector.button.gameObject.SetActive(false);
                dialogueSelector.button.GetComponentInChildren<TMP_Text>().text = "";
                dialogueSelector.button.onClick.RemoveAllListeners();
            }

            choicePanel.SetActive(true);

            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed -= OnInputDialogue;

            HighlightHelper.Instance.Push(_choiceHighlighter);

            var choicedCount = 0;
            var currentDialogueData = _baseDialogueData.Peek();
            while(currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                  currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType !=
                  DialogueType.ChoiceEnd)
            {
                Debug.Log(currentDialogueData.index);

                var choiceCount = 0;
                var choiceContextLen = 0;

                while(currentDialogueData.index + choiceCount < currentDialogueData.dialogueElements.Length &&
                      currentDialogueData.dialogueElements[currentDialogueData.index + choiceCount].dialogueType ==
                      DialogueType.Choice)
                {
                    choiceCount++;
                }

                while(currentDialogueData.index + choiceCount + choiceContextLen < currentDialogueData.dialogueElements.Length)
                {
                    var dialogueElement =
                        currentDialogueData.dialogueElements[
                            currentDialogueData.index + choiceCount + choiceContextLen];
                    if (dialogueElement.dialogueType == DialogueType.Choice)
                    {
                        break;
                    }

                    if (dialogueElement.dialogueType == DialogueType.ChoiceEnd)
                    {
                        break;
                    }

                    choiceContextLen++;
                }

                Debug.Log($"현재 Index: {currentDialogueData.index}\n" +
                          $"선택 개수: {choiceCount}\n" +
                          $"선택 Context 길이: {choiceContextLen}");

                for(var i = 0; i < choiceCount; i++)
                {
                    var curIdx = currentDialogueData.index;
                    var choiceButton = choiceSelectors[choicedCount + i].button;

                    choiceButton.gameObject.SetActive(true);
                    choiceButton.GetComponentInChildren<TMP_Text>().text =
                        currentDialogueData.dialogueElements[curIdx + i].contents;

                    choiceButton.onClick.RemoveAllListeners();
                    choiceButton.onClick.AddListener(() => { OnClickChoice(curIdx, choiceCount, choiceContextLen); });
                }

                currentDialogueData.index += choiceCount + choiceContextLen;
                choicedCount += choiceCount;

                Debug.Log($"다음 Index: {currentDialogueData.index}\n" +
                          $"선택된 개수: {choicedCount}\n");
            }
        }

        private void InitChoiceDialogue(int nextIndex, int choiceContextLen)
        {
            StartCoroutine(InvokeInputEnable(Time.deltaTime));

            HighlightHelper.Instance.Pop(_choiceHighlighter);

            choicePanel.SetActive(false);

            if (choiceContextLen == 0)
            {
                return;
            }

            var choiceDialogueData = new DialogueData
            {
                index = 0,
                dialogueElements = new DialogueElement[choiceContextLen]
            };

            Array.Copy(_baseDialogueData.Peek().dialogueElements, nextIndex,
                choiceDialogueData.dialogueElements, 0, choiceContextLen);

            Initialize(choiceDialogueData);
        }

        private void InitRandom()
        {
            Debug.Log("랜덤 시작");
            // 덩어리 개수
            var countedRandomIndex = 0;
            var dictionary = new Dictionary<int, int>();
            var currentDialogueData = _baseDialogueData.Peek();
            while(currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                  currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType != DialogueType.RandomEnd)
            {
                Debug.Log(currentDialogueData.index);

                var randomCount = 0;
                var randomContextLen = 0;

                while(currentDialogueData.index + randomCount < currentDialogueData.dialogueElements.Length &&
                      currentDialogueData.dialogueElements[currentDialogueData.index + randomCount].dialogueType == DialogueType.Random)
                {
                    randomCount++;
                }

                while(currentDialogueData.index + randomCount + randomContextLen < currentDialogueData.dialogueElements.Length)
                {
                    var dialogueElement =
                        currentDialogueData.dialogueElements[
                            currentDialogueData.index + randomCount + randomContextLen];
                    if (dialogueElement.dialogueType == DialogueType.Random)
                    {
                        break;
                    }

                    if (dialogueElement.dialogueType == DialogueType.RandomEnd)
                    {
                        break;
                    }

                    randomContextLen++;
                }

                Debug.Log($"현재 Index: {currentDialogueData.index}\n" +
                          $"선택 개수: {randomCount}\n" +
                          $"선택 Context 길이: {randomContextLen}");

                dictionary.Add(currentDialogueData.index + randomCount, randomContextLen);

                currentDialogueData.index += randomCount + randomContextLen;
                countedRandomIndex += randomCount;

                // currentDialogueData.index
                // curIdx + choiceLen, choiceContextLen

                Debug.Log($"다음 Index: {currentDialogueData.index}\n" +
                          $"선택된 개수: {countedRandomIndex}\n");
            }

            var randomIndex = Random.Range(0, countedRandomIndex);

            var nextIndex = dictionary.ElementAt(randomIndex).Key;
            var contextLen = dictionary.ElementAt(randomIndex).Value;

            if (contextLen == 0)
            {
                return;
            }

            var randomDialogueData = new DialogueData
            {
                index = 0,
                dialogueElements = new DialogueElement[contextLen]
            };

            Array.Copy(_baseDialogueData.Peek().dialogueElements, nextIndex,
                randomDialogueData.dialogueElements, 0, contextLen);

            Initialize(randomDialogueData);

            // 클릭할때는 되는데 입력으로 할때는 한번 더 하는 오류 있음
            // 입력시 Dialogue.performed가 실행됨 -> 이걸 막아야됨.
            ProgressDialogue();
        }

        private IEnumerator InvokeInputEnable(float sec)
        {
            yield return new WaitForSeconds(sec);
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed += OnInputDialogue;
        }

        private IEnumerator WaitInput(float sec)
        {
            yield return new WaitForSeconds(sec);
            _isCutSceneSkipEnable = true;
            OnInputDialogue();
        }

        private bool IsDialogueEnd()
        {
            var currentDialogueData = _baseDialogueData.Peek();
            return currentDialogueData.index >= currentDialogueData.dialogueElements.Length;
        }
    }
}