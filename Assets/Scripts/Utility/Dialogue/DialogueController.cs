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

        [NonSerialized] public bool IsDialogue;

        private Stack<DialogueData> _baseDialogueData;
        private bool _isCutSceneSkipEnable;
        private bool _isUnfolding;
        private int _selectedIdx;
        private bool _isSkipEnable;
        private Coroutine _printCoroutine;
        private Coroutine _waitCutsceneEnableCoroutine;
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
            }

            dialogueInputArea.onClick.AddListener(() => { OnInputDialogue(); });
            _baseDialogueData = new Stack<DialogueData>();
            baseDialogueData = new List<DialogueData>();

            _choiceHighlighter = new Highlighter
            {
                HighlightItems = choiceSelectors,
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            foreach (var choiceSelector in choiceSelectors)
            {
                choiceSelector.Init(choiceSelector.button.GetComponent<Animator>());
            }

            _choiceHighlighter.Init(Highlighter.ArrowType.Vertical);
        }

        private void Initialize(DialogueData dialogueData)
        {
            IsDialogue = true;
            if (!_baseDialogueData.Contains(dialogueData))
            {
                dialogueData.index = 0;
                _baseDialogueData.Push(dialogueData);
                baseDialogueData.Add(dialogueData);
            }

            _isUnfolding = false;

            StartCoroutine(InvokeInputEnable(Time.deltaTime, () => { dialogueData?.OnDialogueStart?.Invoke(); }));
        }

        public void StartDialogue(string jsonAsset, UnityAction dialogueEndAction = default)
        {
            if (IsDialogue)
            {
                return;
            }

            var dialogueData = new DialogueData
            {
                OnDialogueEnd = dialogueEndAction
            };
            dialogueData.Init(jsonAsset);
            StartDialogue(dialogueData);
        }

        public void StartDialogue(DialogueData dialogueData)
        {
            if (IsDialogue)
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
                // if (playableDirector.playableAsset)
                // {
                //     Debug.Log(
                //         $"Time: {Math.Abs(playableDirector.time - playableDirector.duration) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps}\n" +
                //         $"Pause: {playableDirector.state == PlayState.Paused}\n" +
                //         $"IsValid: {playableDirector.playableGraph.IsValid()}\n" +
                //         $"IsCutSceneWorking {playableDirector.playableAsset && !(Math.Abs(playableDirector.time - playableDirector.duration) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps || (playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid()))}\n" +
                //         $"CutSceneSkipEnable : {_isCutSceneSkipEnable}\n");
                // }

                if (playableDirector.playableAsset && !(Math.Abs(playableDirector.time - playableDirector.duration) <=
                                                        1 / ((TimelineAsset) playableDirector.playableAsset)
                                                        .editorSettings.fps ||
                                                        (playableDirector.state == PlayState.Paused &&
                                                         !playableDirector.playableGraph.IsValid())))
                {
                    if (_isCutSceneSkipEnable)
                    {
                        Debug.Log("스킵되어라!");
                        playableDirector.time = playableDirector.duration -
                                                2 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps;
                        playableDirector.RebuildGraph();
                        playableDirector.Play();
                    }
                }
                else
                {
                    _baseDialogueData.Peek().index++;
                    ProgressDialogue();
                }
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
            var dialogueElement = dialogueData.dialogueElements[dialogueData.index];
            ItemManager.Instance.SetItem(dialogueElement.option);
            // Debug.Log($"{dialogueData.index} 인덱스 실행");
            switch (dialogueElement.dialogueType)
            {
                case DialogueType.Script:
                {
                    StartDialoguePrint();

                    Option(dialogueElement);
                    break;
                }
                case DialogueType.MoveMap:
                    OnInputDialogue();
                    Debug.Log(dialogueElement.contents + "로 맵 이동");
                    SceneLoader.SceneLoader.Instance.LoadScene(dialogueElement.contents);
                    break;
                case DialogueType.Save:
                    _onComplete = () =>
                    {
                        InputManager.SetUiAction(false);
                        var uiActions = InputManager.InputControl.Ui;
                        uiActions.Dialogue.performed -= OnInputDialogue;

                        SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.ButtonType.Save);
                        _onComplete = () => { };
                        SavePanelManager.Instance.OnSave.AddListener(() =>
                        {
                            OnInputDialogue();
                            SavePanelManager.Instance.OnSavePanelActiveFalse?.RemoveAllListeners();
                            SavePanelManager.Instance.SetSaveLoadPanelActive(false,
                                SavePanelManager.ButtonType.None);

                            InputManager.SetUiAction(true);
                            uiActions.Dialogue.performed += OnInputDialogue;
                        });

                        SavePanelManager.Instance.OnSavePanelActiveFalse.AddListener(() =>
                        {
                            SavePanelManager.Instance.OnSave?.RemoveAllListeners();
                            OnInputDialogue();
                        });
                    };
                    StartDialoguePrint();
                    break;
                case DialogueType.ChoiceEnd:
                    break;
                case DialogueType.CutScene:
                    playableDirector.playableAsset = dialogueElement.playableAsset;
                    playableDirector.extrapolationMode = dialogueElement.extrapolationMode;
                    playableDirector.time = 0;

                    // if (dialogue.option.Contains("UI", StringComparer.OrdinalIgnoreCase))
                    // {
                    //
                    // }
                    // else if (dialogue.option.Contains("Field", StringComparer.OrdinalIgnoreCase))
                    // {
                    //     
                    // }

                    // Binding - Animator 한개만? 아마도? 카메라 흔들리는 트랙, 클립 만들고
                    var timelineAsset = (TimelineAsset) playableDirector.playableAsset;
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
                        Debug.LogWarning($"{dialogueData.index} Task 타임라인 오류");
                    }

                    playableDirector.Play();

                    _isCutSceneSkipEnable = false;

                    var options = Array.FindAll(dialogueElement.option, item => item.Any(char.IsDigit));
                    var floatOptions = options.Select(float.Parse);
                    var enumerable = floatOptions as float[] ?? floatOptions.ToArray();

                    _waitCutsceneEnableCoroutine = StartCoroutine(enumerable.Length == 1
                        ? WaitInteractable(enumerable[0])
                        : WaitInteractable(0));

                    // Auto Next Index
                    // true, false 다음 Index로 Auto 여부 (Default - false)

                    StartCoroutine(WaitCutSceneEnd(() =>
                    {
                        playableDirector.playableAsset = null;
                        if (dialogueElement.option.Contains("True", StringComparer.OrdinalIgnoreCase))
                        {
                            if (_waitCutsceneEnableCoroutine != null)
                            {
                                // 작동하려나? 이미 끝난 코루틴을 멈추면
                                StopCoroutine(_waitCutsceneEnableCoroutine);
                                _waitCutsceneEnableCoroutine = null;
                            }

                            OnInputDialogue();
                        }
                    }));

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
                    var waitInteractions = dialogueElement.waitInteraction.interactions;
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
                        interaction.InitializeWait(() =>
                        {
                            Debug.Log($"클리어 남은 개수: {waitInteractions.Count(item => !item.IsClear)}");
                            if (waitInteractions.Any(item => !item.IsClear))
                            {
                                return;
                            }

                            Debug.Log("클리어, 다음 꺼 플레이 가능");
                            Debug.Log(dialogueData.dialogueElements.Length);

                            dialogueData.OnDialogueWaitClear?.Invoke();

                            if (dialogueElement.interactionWaitType == InteractionWaitType.Immediately)
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
                    if (!dialogueElement.interaction)
                    {
                        Debug.LogError("세팅 오류, Script -> Interaction 세팅");
                    }

                    dialogueElement.interaction.StartInteraction(dialogueElement.interactIndex);
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
                    Debug.Log("즉시(옵션) 실행");
                    Option(dialogueElement);

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

            tendencyData.increase += 4;
            tendencyData.descent += 3;
            tendencyData.activation += 2;
            tendencyData.inactive += 1;

            tendencyText.text = $"activation: {tendencyData.activation}\n" +
                                $"inactive: {tendencyData.inactive}\n" +
                                $"increase: {tendencyData.increase}\n" +
                                $"descent: {tendencyData.descent}\n";
        }

        private void Option(DialogueElement dialogue)
        {
            if (dialogue.option is {Length: < 1})
            {
                return;
            }

            for (var index = 0; index < dialogue.option.Length; index++)
            {
                dialogue.option[index] = dialogue.option[index].Replace(" ", "");
            }

            // if (option.has tendencyData)
            // Debug.Log("더하기");

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
                    animator.SetInteger(CharacterHash, (int) dialogue.name);
                }

                if (dialogue.expression != Expression.Keep)
                {
                    animator.SetInteger(ExpressionHash, (int) dialogue.expression - 1);
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
                var intOptions = options.Select(item => (int) float.Parse(item));
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

            for (var index = 0; index < dialogueItem.contents.Length; index++)
            {
                var t = dialogueItem.contents[index];
                if (t.Equals('<'))
                {
                    while (!t.Equals('>'))
                    {
                        Debug.Log(t);
                        dialogueText.text += t;

                        index++;
                        t = dialogueItem.contents[index];
                    }

                    dialogueText.text += t;

                    index++;
                }

                dialogueText.text += dialogueItem.contents[index];

                if (!t.Equals(' '))
                {
                    yield return waitForSec;
                }
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
            Debug.Log($"대화 끝, 종료 여부: {isEnd}");

            dialoguePanel.SetActive(false);

            IsDialogue = false;

            rightAnimator.SetTrigger(DisappearHash);
            leftAnimator.SetTrigger(DisappearHash);

            InputManager.SetUiAction(false);
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed -= OnInputDialogue;

            // Debug.Log(_baseDialogueData.Count);
            // foreach (var dialogueData in _baseDialogueData)
            // {
            //     Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
            //     foreach (var dialogueDataDialogueElement in dialogueData.dialogueElements)
            //     {
            //         Debug.Log($"{dialogueDataDialogueElement.subject}  {dialogueDataDialogueElement.contents}  {dialogueDataDialogueElement.dialogueType}");
            //     }
            //
            //     Debug.Log("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
            // }
            if (isEnd)
            {
                var dialogueData = _baseDialogueData.Pop();
                baseDialogueData.Remove(dialogueData);
                dialogueData.OnDialogueEnd?.Invoke();
            }
            // Debug.Log(_baseDialogueData.Count);
        }

        private void OnClickChoice(int curIdx, int choiceLen, int choiceContextLen)
        {
            Debug.Log(curIdx);
            Debug.Log("선택 개수: " + choiceLen);
            Debug.Log("선택 대화 길이: " + choiceContextLen);

            InitChoiceDialogue(curIdx + choiceLen, choiceContextLen);

            StartCoroutine(InvokeInputEnable(Time.deltaTime));

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
            while (currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                   currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType !=
                   DialogueType.ChoiceEnd)
            {
                Debug.Log(currentDialogueData.index);

                var choiceCount = 0;
                var choiceContextLen = 0;

                while (currentDialogueData.index + choiceCount < currentDialogueData.dialogueElements.Length &&
                       currentDialogueData.dialogueElements[currentDialogueData.index + choiceCount].dialogueType ==
                       DialogueType.Choice)
                {
                    choiceCount++;
                }

                while (currentDialogueData.index + choiceCount + choiceContextLen <
                       currentDialogueData.dialogueElements.Length)
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

                for (var i = 0; i < choiceCount; i++)
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
            while (currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                   currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType !=
                   DialogueType.RandomEnd)
            {
                Debug.Log(currentDialogueData.index);

                var randomCount = 0;
                var randomContextLen = 0;

                while (currentDialogueData.index + randomCount < currentDialogueData.dialogueElements.Length &&
                       currentDialogueData.dialogueElements[currentDialogueData.index + randomCount].dialogueType ==
                       DialogueType.Random)
                {
                    randomCount++;
                }

                while (currentDialogueData.index + randomCount + randomContextLen <
                       currentDialogueData.dialogueElements.Length)
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

        private IEnumerator InvokeInputEnable(float sec, Action endAction = default)
        {
            yield return new WaitForSeconds(sec);
            var uiActions = InputManager.InputControl.Ui;
            uiActions.Dialogue.performed += OnInputDialogue;
            InputManager.SetUiAction(true);
            endAction?.Invoke();
        }

        private IEnumerator WaitInteractable(float sec)
        {
            yield return new WaitForSeconds(sec);
            _isCutSceneSkipEnable = true;
            _waitCutsceneEnableCoroutine = null;

            Debug.Log("CutScene Skip 가능해짐");
        }

        private IEnumerator WaitCutSceneEnd(Action action)
        {
            Debug.Log($"{playableDirector.time}\n" +
                      $"name: {playableDirector.playableAsset.name}\n" +
                      $"duration: {playableDirector.duration}");

            Debug.Log(
                $"Time: {Math.Abs(playableDirector.duration - playableDirector.time) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps}\n" +
                $"Pause: {playableDirector.state == PlayState.Paused}\n" +
                $"IsValid: {playableDirector.playableGraph.IsValid()}\n" +
                $"IsCutSceneWorking {Math.Abs(playableDirector.duration - playableDirector.time) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps || playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid()}");


            var waitUntil = new WaitUntil(() =>
                Math.Abs(playableDirector.duration - playableDirector.time) <=
                1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.fps ||
                playableDirector.state == PlayState.Paused &&
                !playableDirector.playableGraph.IsValid());

            yield return waitUntil;


            Debug.Log("컷씬 끝났다고 판정내림");

            action?.Invoke();
        }

        private bool IsDialogueEnd()
        {
            var currentDialogueData = _baseDialogueData.Peek();
            return currentDialogueData.index >= currentDialogueData.dialogueElements.Length;
        }
    }
}