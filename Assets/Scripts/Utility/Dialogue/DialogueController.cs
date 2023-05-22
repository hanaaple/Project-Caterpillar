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
using Utility.Scene;
using Utility.UI.Check;
using Utility.UI.Highlight;
using Utility.Util;
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
        [SerializeField] private Animator cutSceneAnimator;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueInputArea;
        [SerializeField] private Button skipButton;
        [SerializeField] private CheckUIManager skipCheckUIManager;

        [Header("CutScene")] [SerializeField] private PlayableDirector playableDirector;

        [Header("Choice")] [SerializeField] private ChoiceSelector[] choiceSelectors;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Header("좌 애니메이터")] [SerializeField] private Animator leftAnimator;

        [Header("우 애니메이터")] [SerializeField] private Animator rightAnimator;

        [SerializeField] private float textSpeed;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private List<DialogueData> baseDialogueData;

        private bool _isDialogue;
        private Stack<DialogueData> _baseDialogueData;
        private bool _isCutSceneSkipEnable;
        private bool _isUnfolding;
        private int _selectedIdx;
        private bool _isSkipEnable;
        private Coroutine _printCoroutine;
        private Coroutine _waitCutsceneEnableCoroutine;
        private Coroutine _waitCutsceneEndCoroutine;
        private Highlighter _choiceHighlighter;
        private UnityAction _onComplete;
        private UnityAction _onSkip;
        private InputActions _dialogueInputActions;

        private static readonly int CharacterHash = Animator.StringToHash("Character");
        private static readonly int ExpressionHash = Animator.StringToHash("Expression");
        private static readonly int DisappearHash = Animator.StringToHash("Disappear");
        private static readonly int ActiveHash = Animator.StringToHash("Active");
        private static readonly int InActiveHash = Animator.StringToHash("Inactive");

        private void Awake()
        {
            dialogueInputArea.onClick.AddListener(() => { OnInputDialogue(); });
            _baseDialogueData = new Stack<DialogueData>();
            baseDialogueData = new List<DialogueData>();

            _choiceHighlighter = new Highlighter("Choice Highlight")
            {
                HighlightItems = new List<HighlightItem>(choiceSelectors),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            foreach (var choiceSelector in choiceSelectors)
            {
                choiceSelector.Init(choiceSelector.button.GetComponent<Animator>());
            }

            _choiceHighlighter.Init(Highlighter.ArrowType.Vertical);

            _choiceHighlighter.InputActions.OnPause = _ => { PlayUIManager.Instance.pauseManager.onPause?.Invoke(); };

            skipButton.onClick.AddListener(() =>
            {
                TimeScaleHelper.Push(0f);
                skipCheckUIManager.Push();
            });

            skipCheckUIManager.Initialize();
            skipCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
            {
                _onSkip?.Invoke();

                TimeScaleHelper.Pop();
                skipCheckUIManager.Pop();
            });

            skipCheckUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () =>
            {
                TimeScaleHelper.Pop();
                skipCheckUIManager.Pop();
            });

            _dialogueInputActions = new InputActions(nameof(DialogueController))
            {
                // It Works When Before Save
                OnExecute = OnInputDialogue,
                OnPause = _ => { PlayUIManager.Instance.pauseManager.onPause?.Invoke(); }
            };
        }

        private void Initialize(DialogueData dialogueData)
        {
            _isDialogue = true;
            if (!_baseDialogueData.Contains(dialogueData))
            {
                dialogueData.index = 0;
                _baseDialogueData.Push(dialogueData);
                baseDialogueData.Add(dialogueData);
            }

            _isUnfolding = false;

            dialogueData?.OnDialogueStart?.Invoke();
        }

        public void StartDialogue(string jsonAsset, UnityAction dialogueEndAction = default)
        {
            if (_isDialogue)
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
            if (_isDialogue)
            {
                Debug.Log("이미 진행 중임 왜?");
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

            InputManager.PushInputAction(_dialogueInputActions);

            Initialize(dialogueData);

            ProgressDialogue();
        }

        private void OnInputDialogue(InputAction.CallbackContext obj = default)
        {
            if (_isUnfolding)
            {
                if (_isSkipEnable)
                {
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
                                                        .editorSettings.frameRate ||
                                                        (playableDirector.state == PlayState.Paused &&
                                                         !playableDirector.playableGraph.IsValid())))
                {
                    if (_isCutSceneSkipEnable)
                    {
                        Debug.Log("스킵되어라!");
                        playableDirector.time = playableDirector.duration -
                                                2 / ((TimelineAsset) playableDirector.playableAsset).editorSettings
                                                .frameRate;
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

        /// <summary>
        /// Do not Add Index here. Just Work by Current DialogueData State (Index, options, etc..)
        /// </summary>
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

            dialogueElement.OnStartAction?.Invoke();

            SkipOption(dialogueData);

            switch (dialogueElement.dialogueType)
            {
                case DialogueType.Script:
                {
                    StartDialoguePrint();

                    ScriptOption(dialogueElement);
                    break;
                }
                case DialogueType.MoveMap:
                    EndDialogue(false);
                    baseDialogueData.Clear();
                    _baseDialogueData.Clear();
                    
                    Debug.Log(dialogueElement.contents + "로 맵 이동");
                    SceneLoader.Instance.LoadScene(dialogueElement.contents);
                    
                    break;
                case DialogueType.Save:
                    SavePanelManager.Instance.SetSaveLoadPanelActive(true, SavePanelManager.SaveLoadType.Save);

                    SavePanelManager.Instance.OnSave.AddListener(() =>
                    {
                        OnInputDialogue();
                        SavePanelManager.Instance.OnSavePanelInActive?.RemoveAllListeners();
                        SavePanelManager.Instance.SetSaveLoadPanelActive(false, SavePanelManager.SaveLoadType.None);
                    });

                    SavePanelManager.Instance.OnSavePanelInActive.AddListener(() =>
                    {
                        SavePanelManager.Instance.OnSave?.RemoveAllListeners();
                        OnInputDialogue();
                    });
                    break;
                case DialogueType.ChoiceEnd:
                    break;
                case DialogueType.CutScene:
                    playableDirector.playableAsset = dialogueElement.playableAsset;
                    playableDirector.extrapolationMode = dialogueElement.extrapolationMode;
                    playableDirector.time = 0;

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

                    if (Mathf.Approximately(dialogueElement.waitSec, 0f))
                    {
                        _isCutSceneSkipEnable = true;
                    }
                    else if (dialogueElement.waitSec > 0f)
                    {
                        _waitCutsceneEnableCoroutine = StartCoroutine(WaitSecAfterAction(dialogueElement.waitSec, () =>
                        {
                            _isCutSceneSkipEnable = true;

                            Debug.Log("CutScene Skip 가능해짐");
                        }));
                    }

                    // Auto Next Index
                    // true, false 다음 Index로 Auto 여부 (Default - false)

                    _waitCutsceneEndCoroutine = StartCoroutine(WaitCutSceneEnd(() =>
                    {
                        playableDirector.playableAsset = null;
                        if (!dialogueElement.option.Contains("False", StringComparer.OrdinalIgnoreCase))
                        {
                            if (_waitCutsceneEnableCoroutine != null)
                            {
                                StopCoroutine(_waitCutsceneEnableCoroutine);
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
                        Debug.LogWarning($"세팅 오류, Interaction 개수: {waitInteractions.Length}개");
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
                    ScriptOption(dialogueElement);

                    if (_isUnfolding)
                    {
                        CompleteDialogue();
                    }

                    _baseDialogueData.Peek().index++;
                    ProgressDialogue();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 컷씬 외에 사용 금지, 컷씬에서 사용할 경우에도 중간에 Hold가 있을 경우 (시각적) 오류가 발생할 수 있음
        /// </summary>
        /// <param name="dialogueData"></param>
        private void SkipOption(DialogueData dialogueData)
        {
            var dialogue = dialogueData.dialogueElements[dialogueData.index];
            if (dialogue.isSkipEnable)
            {
                StartCoroutine(WaitSecAfterAction(dialogue.skipWaitSec, () =>
                {
                    skipButton.gameObject.SetActive(true);
                    var targetIndex = dialogueData.index + dialogue.skipLength;

                    _onSkip = () =>
                    {
                        skipButton.gameObject.SetActive(false);
                        dialogueData.index = targetIndex;
                        dialogueData.dialogueElements[targetIndex].OnStartAction = null;

                        if (_waitCutsceneEnableCoroutine != null)
                        {
                            StopCoroutine(_waitCutsceneEnableCoroutine);
                        }

                        _isCutSceneSkipEnable = true;

                        if (_waitCutsceneEndCoroutine != null)
                        {
                            StopCoroutine(_waitCutsceneEndCoroutine);
                        }

                        ProgressDialogue();
                    };

                    dialogueData.dialogueElements[targetIndex].OnStartAction = () =>
                    {
                        skipButton.gameObject.SetActive(false);
                    };
                }));
            }
        }

        private void ScriptOption(DialogueElement dialogue)
        {
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
                    item => Enum.TryParse(item, out CharacterOption _));
                if (!string.IsNullOrEmpty(state))
                {
                    Debug.Log(state + "  " + dialogue.name + " " + dialogue.expression);
                    if (Enum.TryParse(state, out CharacterOption _))
                    {
                        var isAlreadyTriggered = animator.GetBool(state);
                        if (!isAlreadyTriggered && animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
                            Animator.StringToHash(state))
                        {
                            animator.SetTrigger(state);
                        }
                    }
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
            else
            {
                if (dialogue.name is CharacterType.Keep or CharacterType.None)
                {
                    return;
                }
                
                var leftCharacter = leftAnimator.GetInteger(CharacterHash);
                if (leftCharacter == (int) dialogue.name)
                {
                    var leftActivate = leftAnimator.GetBool(ActiveHash);
                    var rightInActivate = rightAnimator.GetBool(InActiveHash);

                    Debug.Log($"Left Active: {leftActivate}, Right Inactive: {rightInActivate}");

                    // if character is left
                    // set left to Active, Right to InActive
                    if (!leftActivate && leftAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != ActiveHash)
                    {
                        leftAnimator.SetTrigger(ActiveHash);
                    }

                    if (!rightInActivate && rightAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != InActiveHash)
                    {
                        rightAnimator.SetTrigger(InActiveHash);
                    }
                }

                var rightCharacter = rightAnimator.GetInteger(CharacterHash);
                if (rightCharacter == (int) dialogue.name)
                {
                    var leftInActivate = leftAnimator.GetBool(InActiveHash);
                    var rightActivate = rightAnimator.GetBool(ActiveHash);

                    Debug.Log($"Left InActive: {leftInActivate}, Right Active: {rightActivate}");
                    // if character is right
                    // set right to Active, left to InActive
                    if (!leftInActivate)
                    {
                        leftAnimator.SetTrigger(InActiveHash);
                    }

                    if (!rightActivate)
                    {
                        rightAnimator.SetTrigger(ActiveHash);
                    }
                }

                Debug.Log($"Left: {leftCharacter}, Right: {rightCharacter}, Dialogue: {(int) dialogue.name}");
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
            if (_printCoroutine != null)
            {
                StopCoroutine(_printCoroutine);
                _printCoroutine = null;
            }

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

            _isDialogue = false;
            
            // rightAnimator.SetTrigger(DisappearHash);
            // leftAnimator.SetTrigger(DisappearHash);

            InputManager.PopInputAction(_dialogueInputActions);

            skipButton.gameObject.SetActive(false);

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
            Debug.Log($"Dialogue Index: {curIdx}\n" +
                      $"선택 개수: {choiceLen}\n" +
                      $"대화 길이: {choiceContextLen}");

            HighlightHelper.Instance.Pop(_choiceHighlighter);
            choicePanel.SetActive(false);

            InitChoiceDialogue(curIdx + choiceLen, choiceContextLen);

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
                var highlightItem =
                    _choiceHighlighter.HighlightItems.Find(item => item.button == dialogueSelector.button);
                highlightItem.isEnable = false;
            }

            choicePanel.SetActive(true);

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

                    var highlightItem = _choiceHighlighter.HighlightItems.Find(item => item.button == choiceButton);
                    highlightItem.isEnable = true;

                    choiceButton.gameObject.SetActive(true);
                    choiceButton.GetComponentInChildren<TMP_Text>().text =
                        currentDialogueData.dialogueElements[curIdx + i].contents;

                    choiceButton.onClick.RemoveAllListeners();
                    choiceButton.onClick.AddListener(() => { OnClickChoice(curIdx, choiceCount, choiceContextLen); });
                }

                currentDialogueData.index += choiceCount + choiceContextLen;
                choicedCount += choiceCount;

                // Debug.Log($"다음 Index: {currentDialogueData.index}\n" +
                //           $"선택된 개수: {choicedCount}\n");
            }

            Debug.Log($"총 선택 개수: {choicedCount}");

        }

        private void InitChoiceDialogue(int nextIndex, int choiceContextLen)
        {
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

        private IEnumerator WaitCutSceneEnd(Action action)
        {
            Debug.Log($"{playableDirector.time}\n" +
                      $"name: {playableDirector.playableAsset.name}\n" +
                      $"duration: {playableDirector.duration}");

            Debug.Log(
                $"Time: {Math.Abs(playableDirector.duration - playableDirector.time) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate}\n" +
                $"Pause: {playableDirector.state == PlayState.Paused}\n" +
                $"IsValid: {playableDirector.playableGraph.IsValid()}\n" +
                $"IsCutSceneWorking {Math.Abs(playableDirector.duration - playableDirector.time) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate || playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid()}");


            var waitUntil = new WaitUntil(() =>
                Math.Abs(playableDirector.duration - playableDirector.time) <=
                1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate ||
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

        private static IEnumerator WaitSecAfterAction(float sec, Action afterAction)
        {
            yield return new WaitForSeconds(sec);
            afterAction?.Invoke();
        }
    }
}