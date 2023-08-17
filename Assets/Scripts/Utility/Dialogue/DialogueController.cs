using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Pool;
using UnityEngine.Timeline;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.Tendency;
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

        public override void EnterHighlightDisplay()
        {
        }

        public override void SelectDisplay()
        {
            _animator.SetBool(Selected, true);
        }
    }

    public class DialogueController : MonoBehaviour
    {
        [Header("Panel")] [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [Header("Dialogue")] [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueInputArea;
        [SerializeField] private Button skipButton;
        [SerializeField] private CheckUIManager skipCheckUIManager;
        [Header("텍스트 속도")] [SerializeField] private float textSpeed;

        [Space(10)] [Header("CutScene")] public GameObject cutSceneImage;

        [FormerlySerializedAs("cutSceneAnimator")] [SerializeField]
        private Animator defaultCutSceneAnimator;

        [Header("Timeline")] [SerializeField] private PlayableDirector playableDirectorPrefab;

        [Header("Choice")] [SerializeField] private ChoiceSelector[] choiceSelectors;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Header("좌 애니메이터")] [SerializeField] private Animator leftAnimator;
        [Header("우 애니메이터")] [SerializeField] private Animator rightAnimator;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private List<DialogueData> baseDialogueData;

        private CharacterOption _leftAnimatorState;
        private CharacterOption _rightAnimatorState;

        /// <summary>
        /// Default PlayableDirector - can skip, affected by code
        /// </summary>
        private PlayableDirector _playableDirector;

        /// <summary>
        /// Named PlayableDirector - just work by myself
        /// </summary>
        private IObjectPool<PlayableDirector> _playableDirectors;

        private List<PlayableDirector> _activePlayableDirectors;
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
        private static readonly int InActiveHash = Animator.StringToHash("Inactive");

        private void Awake()
        {
            _playableDirectors = new ObjectPool<PlayableDirector>(
                () =>
                {
                    var playableDirector = Instantiate(playableDirectorPrefab);
                    playableDirector.playOnAwake = false;
                    return playableDirector;
                },
                item => { item.gameObject.SetActive(true); },
                item =>
                {
                    item.gameObject.SetActive(false);
                    item.playableAsset = null;
                    item.extrapolationMode = DirectorWrapMode.None;
                    item.time = 0;
                    item.Stop();
                },
                item => { Destroy(item.gameObject); });

            _activePlayableDirectors = new List<PlayableDirector>();
            _playableDirector = _playableDirectors.Get();
            _playableDirector.transform.SetParent(transform);

            dialogueInputArea.onClick.AddListener(OnInputDialogue);
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

            _choiceHighlighter.InputActions.OnEsc = () =>
            {
                PlayUIManager.Instance.pauseManager.onPause?.Invoke();
                PlayUIManager.Instance.pauseManager.onExit = () =>
                {
                    EndDialogue();
                    PlayUIManager.Instance.pauseManager.onExit = () => { };
                };
            };

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
                // if both work, error
                OnExecute = OnInputDialogue,
                OnEsc = () =>
                {
                    PlayUIManager.Instance.pauseManager.onPause?.Invoke();
                    PlayUIManager.Instance.pauseManager.onExit = () =>
                    {
                        EndDialogue();
                        PlayUIManager.Instance.pauseManager.onExit = () => { };
                    };
                },
            };
        }

        private void Initialize(DialogueData dialogueData)
        {
            subjectText.text = "";
            dialogueText.text = "";

            _isDialogue = true;
            if (!_baseDialogueData.Contains(dialogueData))
            {
                Debug.LogWarning("Add DialogueData");
                dialogueData.index = 0;
                _baseDialogueData.Push(dialogueData);
                baseDialogueData.Add(dialogueData);
            }

            _isUnfolding = false;
        }

        public void StartDialogue(string jsonAsset, UnityAction<int> dialogueEndAction = default)
        {
            if (_isDialogue)
            {
                Debug.Log("이미 진행 중임 왜?");
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

            InputManager.PushInputAction(_dialogueInputActions);

            Initialize(dialogueData);

            ProgressDialogue();
        }

        private void OnInputDialogue()
        {
            if (InputManager.InputActionsList.Last() != _dialogueInputActions)
            {
                return;
            }

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

                if (_playableDirector.playableAsset &&
                    !(Math.Abs(_playableDirector.time - _playableDirector.duration) <=
                      1 / ((TimelineAsset) _playableDirector.playableAsset)
                      .editorSettings.frameRate ||
                      (_playableDirector.state == PlayState.Paused &&
                       !_playableDirector.playableGraph.IsValid())))
                {
                    if (_isCutSceneSkipEnable)
                    {
                        _isCutSceneSkipEnable = false;
                        Debug.Log("스킵되어라!");
                        _playableDirector.time = _playableDirector.duration
                                                 - 2 / ((TimelineAsset) _playableDirector.playableAsset).editorSettings
                                                 .frameRate;
                        _playableDirector.RebuildGraph();
                        _playableDirector.Play();
                    }
                }
                else
                {
                    if (PlayUIManager.Instance.IsFade())
                    {
                        Debug.Log("OnInputDialogue, Fade 중");
                        return;
                    }

                    InteractContinue();
                }
            }
        }

        private void InteractContinue()
        {
            _baseDialogueData.Peek().index++;
            ProgressDialogue();
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
                    var dialogueData = _baseDialogueData.Pop();
                    baseDialogueData.RemoveAt(baseDialogueData.Count - 1);
                    var waitDialogueData = _baseDialogueData.Peek();
                    var waitDialogueElement = waitDialogueData.dialogueElements[waitDialogueData.index];
                    if (waitDialogueElement.dialogueType == DialogueType.WaitInteract &&
                        !waitDialogueElement.waitInteractions.IsWaitClear())
                    {
                        EndDialogue(true, default, dialogueData);
                        return;
                    }

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
                    var targetScene = dialogueElement.contents;
                    SavePanelManager.Instance.SetActiveSaveLoadPanel(true, SavePanelManager.SaveLoadType.Save, targetScene);

                    SavePanelManager.Instance.OnSave.AddListener(() =>
                    {
                        SavePanelManager.Instance.OnSavePanelInActive?.RemoveAllListeners();
                        SavePanelManager.Instance.SetActiveSaveLoadPanel(false);
                        OnInputDialogue();
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
                    PlayableDirector playableDirector;
                    if (!string.IsNullOrEmpty(dialogueElement.playableDirectorName))
                    {
                        if (_activePlayableDirectors.Exists(item =>
                                item.gameObject.name.Equals(dialogueElement.playableDirectorName)))
                        {
                            playableDirector = _activePlayableDirectors.Find(item =>
                                item.gameObject.name.Equals(dialogueElement.playableDirectorName));
                        }
                        else
                        {
                            playableDirector = _playableDirectors.Get();
                            playableDirector.name = dialogueElement.playableDirectorName;
                            _activePlayableDirectors.Add(playableDirector);
                        }
                    }
                    else
                    {
                        playableDirector = _playableDirector;
                    }

                    playableDirector.playableAsset = dialogueElement.playableAsset;
                    playableDirector.extrapolationMode = dialogueElement.extrapolationMode;
                    playableDirector.time = 0;

                    var timelineAsset = (TimelineAsset) playableDirector.playableAsset;
                    if (timelineAsset != null)
                    {
                        var tracks = timelineAsset.GetOutputTracks()
                            .Where(item => item is AnimationTrack or ActivationTrack);
                        foreach (var temp in tracks)
                        {
                            UnityEngine.Object bindObject = temp switch
                            {
                                AnimationTrack => SceneHelper.Instance.GetBindObject<Animator>(temp.name),
                                ActivationTrack => SceneHelper.Instance.GetBindObject<GameObject>(temp.name),
                                _ => null
                            };

                            if (bindObject)
                            {
                                playableDirector.SetGenericBinding(temp, bindObject);
                            }
                            else if (defaultCutSceneAnimator)
                            {
                                playableDirector.SetGenericBinding(temp, defaultCutSceneAnimator);
                            }
                            //Debug.Log($"Track 명: {temp.name}, Track Type: {temp.GetType()}, 바인드 오브젝트 {bindObject?.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{dialogueData.index} Task 타임라인 오류");
                    }

                    playableDirector.RebuildGraph();
                    playableDirector.Play();

                    if (!string.IsNullOrEmpty(dialogueElement.playableDirectorName))
                    {
                        InteractContinue();
                    }
                    else
                    {
                        _isCutSceneSkipEnable = false;

                        if (Mathf.Approximately(dialogueElement.waitSec, 0f))
                        {
                            _isCutSceneSkipEnable = true;
                        }
                        else if (dialogueElement.waitSec > 0f)
                        {
                            _waitCutsceneEnableCoroutine = StartCoroutine(WaitSecAfterAction(dialogueElement.waitSec,
                                () =>
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

                                InteractContinue();
                            }
                        }));
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
                    var waitInteractions = dialogueElement.waitInteractions;
                    if (waitInteractions.waitInteractions.Length == 0)
                    {
                        Debug.LogWarning($"세팅 오류, Interaction 개수: {waitInteractions.waitInteractions.Length}개");
                        OnInputDialogue();
                        break;
                    }

                    EndDialogue(false);

                    Debug.Log($"클리어 대기, {waitInteractions.waitInteractions.Length}개");

                    waitInteractions.Initialize(() =>
                    {
                        Debug.Log($"클리어 남은 개수: {waitInteractions.GetWaitCount()}");
                        if (!waitInteractions.IsWaitClear())
                        {
                            return;
                        }

                        dialogueData.index++;
                        Debug.Log("클리어, 다음 꺼 플레이 가능");

                        if (dialogueElement.interactionWaitType == InteractionWaitType.ImmediatelyInteract)
                        {
                            StartDialogue(dialogueData);
                        }
                        else
                        {
                            Debug.LogWarning("멈춘 거 어떻게 실행시키려고 ㅁㅁㅁ");
                        }
                    });

                    break;
                }
                case DialogueType.MiniGame:
                {
                    if (!dialogueElement.miniGame)
                    {
                        Debug.LogError("세팅 오류, Script -> Interaction 세팅");
                    }

                    dialogueElement.miniGame.Play(isSuccess =>
                    {
                        if (dialogueElement.isCustomEnd)
                        {
                            if (isSuccess)
                            {
                                // 성공 여부, NextIndex를 넘겨줘야됨
                                EndDialogue(true, dialogueElement.successNextInteractionIndex);
                            }
                            else
                            {
                                EndDialogue(true, dialogueElement.failNextInteractionIndex);
                            }
                        }
                        else
                        {
                            InteractContinue();
                        }
                    });
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

                    // if (option.has tendencyData)
                    // // Debug.Log("더하기");


                    for (var index = 0; index < dialogueElement.option.Length; index++)
                    {
                        dialogueElement.option[index] = dialogueElement.option[index].Replace(" ", "");
                    }

                    var fadeOut = Array.Find(dialogueElement.option,
                        item => item.Equals("FadeOut", StringComparison.OrdinalIgnoreCase));
                    var fadeIn = Array.Find(dialogueElement.option,
                        item => item.Equals("FadeIn", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(fadeOut))
                    {
                        dialoguePanel.SetActive(false);
                        PlayUIManager.Instance.FadeOut(InteractContinue);
                    }
                    else if (!string.IsNullOrEmpty(fadeIn))
                    {
                        PlayUIManager.Instance.FadeIn(InteractContinue);
                    }
                    else
                    {
                        ScriptOption(dialogueElement);

                        InteractContinue();
                    }

                    // if (_isUnfolding)
                    // {
                    //     CompleteDialogue();
                    // }
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
                    var action = _dialogueInputActions.OnEsc;
                    _dialogueInputActions.OnEsc = () => { skipButton.onClick.Invoke(); };

                    skipButton.gameObject.SetActive(true);
                    var targetIndex = dialogueData.index + dialogue.skipLength;

                    _onSkip = () =>
                    {
                        _dialogueInputActions.OnEsc = action;

                        skipButton.gameObject.SetActive(false);
                        dialogueData.index = targetIndex;
                        dialogueData.dialogueElements[targetIndex].OnStartAction = null;

                        if (_waitCutsceneEnableCoroutine != null)
                        {
                            StopCoroutine(_waitCutsceneEnableCoroutine);
                        }


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
            // Debug.Log($"Option!   {string.Join(", ", dialogue.option)}");
            for (var index = 0; index < dialogue.option.Length; index++)
            {
                dialogue.option[index] = dialogue.option[index].Replace(" ", "");
            }

            var reset = Array.Find(dialogue.option, item => item.Equals("Reset", StringComparison.OrdinalIgnoreCase));
            // Debug.LogWarning(reset);
            if (!string.IsNullOrEmpty(reset))
            {
                SetDialogueCharacter(leftAnimator, CharacterOption.Disappear.ToString());
                SetDialogueCharacter(rightAnimator, CharacterOption.Disappear.ToString());
            }
            else
            {
                var side = Array.Find(dialogue.option,
                    item => item.Equals("Left", StringComparison.OrdinalIgnoreCase) ||
                            item.Equals("Right", StringComparison.OrdinalIgnoreCase));

                Animator animator = null;
                if (!string.IsNullOrEmpty(side))
                {
                    if (side.Equals("Left", StringComparison.OrdinalIgnoreCase))
                    {
                        animator = leftAnimator;
                    }
                    else if (side.Equals("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        animator = rightAnimator;
                    }
                }
                else if (dialogue.name != CharacterType.None && dialogue.name != CharacterType.Keep)
                {
                    var rightCharacter = rightAnimator.GetInteger(CharacterHash);
                    var leftCharacter = leftAnimator.GetInteger(CharacterHash);
                    if (rightCharacter == (int) dialogue.name)
                    {
                        animator = rightAnimator;
                    }
                    else if (leftCharacter == (int) dialogue.name)
                    {
                        animator = leftAnimator;
                    }
                }

                // 만약 Animator가 지정되어 있는 경우
                if (animator != null)
                {
                    if (dialogue.name != CharacterType.Keep)
                    {
                        animator.SetInteger(CharacterHash, (int) dialogue.name);
                    }

                    if (dialogue.expression != Expression.Keep)
                    {
                        animator.SetInteger(ExpressionHash, (int) dialogue.expression - 1);
                    }

                    var state = Array.Find(dialogue.option, item => Enum.TryParse(item, out CharacterOption _));

                    Debug.Log(state + "  " + dialogue.name + " " + dialogue.expression);

                    SetDialogueCharacter(animator, state);
                }
            }
        }

        private void SetDialogueCharacter(Animator animator, string stateString)
        {
            if (!Enum.TryParse(stateString, out CharacterOption state))
            {
                state = CharacterOption.Active;
            }

            if (state.Equals(CharacterOption.None))
            {
                return;
            }

            // Character Name이 반대편인 경우 rightAnimatorState도 설정
            CharacterOption originState = default;
            if (animator == leftAnimator)
            {
                originState = _leftAnimatorState;
                _leftAnimatorState = state;
            }
            else if (animator == rightAnimator)
            {
                originState = _rightAnimatorState;
                _rightAnimatorState = state;
            }

            // Debug.Log($"{state}, {originState}");

            if (state == originState)
            {
                return;
            }

            // 전부다 초기화
            switch (state)
            {
                case CharacterOption.Appear:
                case CharacterOption.Active:
                {
                    if (animator == leftAnimator)
                    {
                        if (_rightAnimatorState.Equals(CharacterOption.Appear) ||
                            _rightAnimatorState.Equals(CharacterOption.Active))
                        {
                            rightAnimator.SetTrigger(InActiveHash);
                            _rightAnimatorState = CharacterOption.Inactive;
                        }
                    }
                    else if (animator == rightAnimator)
                    {
                        if (_leftAnimatorState.Equals(CharacterOption.Appear) ||
                            _leftAnimatorState.Equals(CharacterOption.Active))
                        {
                            leftAnimator.SetTrigger(InActiveHash);
                            _leftAnimatorState = CharacterOption.Inactive;
                        }
                    }

                    break;
                }
                case CharacterOption.Inactive:
                {
                    break;
                }
                case CharacterOption.Disappear:
                {
                    if (originState == CharacterOption.Active)
                    {
                        state = CharacterOption.DisappearActive;
                    }
                    else if (originState == CharacterOption.Inactive)
                    {
                        state = CharacterOption.DisappearInactive;
                    }
                    else if (originState == CharacterOption.Appear)
                    {
                        state = CharacterOption.DisappearActive;
                    }
                    else
                    {
                        return;
                    }

                    break;
                }
            }

            animator.SetTrigger(state.ToString());
        }

        private void StartDialoguePrint()
        {
            dialoguePanel.SetActive(true);
            _isUnfolding = true;
            blinkingIndicator.SetActive(false);
            subjectText.text = "";
            dialogueText.text = "";
            _printCoroutine = StartCoroutine(DialoguePrintCoroutine());
        }

        private IEnumerator DialoguePrintCoroutine()
        {
            var dialogueItem = _baseDialogueData.Peek().dialogueElements[_baseDialogueData.Peek().index];

            subjectText.text = dialogueItem.subject;
            _isSkipEnable = true;

            var characterPrintSpeed = 1f;
            if (dialogueItem.option != null)
            {
                var options = dialogueItem.option.Where(item => item.All(char.IsDigit))
                    .Select(item => (int) float.Parse(item)).ToArray();
                if (options.Length > 0)
                {
                    if (options.Contains(0))
                    {
                        Debug.Log($"대화 프린트 스킵 불가능!   {string.Join(", ", options)}");
                        _isSkipEnable = false;
                    }
                }

                if (dialogueItem.option.Any(item => item.Contains("(") && item.Contains(")")))
                {
                    var speedString = Array.Find(dialogueItem.option, item => item.Contains("("));
                    characterPrintSpeed = float.Parse(speedString.Replace("(", "").Replace(")", ""));
                    Debug.Log("속도: " + characterPrintSpeed);
                }
            }

            var waitForSec = new WaitForSeconds(textSpeed / characterPrintSpeed);

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

        public void EndDialogue(bool isEnd = true, int nextInteractionIndex = -1, DialogueData dialogueData = null)
        {
            Debug.Log($"대화 끝, 종료 여부: {isEnd}");

            dialoguePanel.SetActive(false);

            _isDialogue = false;

            InputManager.PopInputAction(_dialogueInputActions);

            skipButton.gameObject.SetActive(false);

            if (isEnd)
            {
                dialogueData ??= _baseDialogueData.Pop();

                Debug.Log(dialogueData.OnDialogueEnd);
                Debug.Log(dialogueData.OnDialogueEnd == null);
                Debug.Log(dialogueData.OnDialogueEnd == default);

                baseDialogueData.Remove(dialogueData);
                dialogueData.OnDialogueEnd?.Invoke(nextInteractionIndex);
            }
        }

        private void OnClickChoice(int choiceIndex, int choiceLen, int choiceContextLen)
        {
            Debug.Log($"Dialogue Index: {choiceIndex}\n" +
                      $"선택 개수: {choiceLen}\n" +
                      $"대화 길이: {choiceContextLen}");

            HighlightHelper.Instance.Pop(_choiceHighlighter);
            choicePanel.SetActive(false);

            var dialogueData = _baseDialogueData.Peek();

            var tendency = Array.Find(dialogueData.dialogueElements[choiceIndex].option,
                item => item.Contains("Tendency-"));
            if (!string.IsNullOrEmpty(tendency))
            {
                var tendencyName = tendency.Split("-")[1];
                TendencyManager.Instance.UpdateTendencyData(tendencyName);
            }

            if (choiceContextLen != 0)
            {
                var choiceDialogueData = new DialogueData
                {
                    index = 0,
                    dialogueElements = new DialogueElement[choiceContextLen]
                };

                Array.Copy(_baseDialogueData.Peek().dialogueElements, choiceIndex + choiceLen,
                    choiceDialogueData.dialogueElements, 0, choiceContextLen);

                Initialize(choiceDialogueData);

                ProgressDialogue();
            }
            else
            {
                InteractContinue();
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

            var choicedCount = 0;
            var currentDialogueData = _baseDialogueData.Peek();
            while (currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                   currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType !=
                   DialogueType.ChoiceEnd)
            {
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
                    var choiceIndex = currentDialogueData.index;
                    var choiceButton = choiceSelectors[choicedCount + i].button;

                    var highlightItem = _choiceHighlighter.HighlightItems.Find(item => item.button == choiceButton);
                    highlightItem.isEnable = true;

                    choiceButton.gameObject.SetActive(true);
                    choiceButton.GetComponentInChildren<TMP_Text>().text =
                        currentDialogueData.dialogueElements[choiceIndex + i].contents;

                    choiceButton.onClick.RemoveAllListeners();
                    choiceButton.onClick.AddListener(() =>
                    {
                        OnClickChoice(choiceIndex, choiceCount, choiceContextLen);
                    });
                }

                currentDialogueData.index += choiceCount + choiceContextLen;
                choicedCount += choiceCount;

                // Debug.Log($"다음 Index: {currentDialogueData.index}\n" +
                //           $"선택된 개수: {choicedCount}\n");
            }

            Debug.Log($"총 선택 개수: {choicedCount}");

            HighlightHelper.Instance.Push(_choiceHighlighter);
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
            Debug.Log($"{_playableDirector.time}\n" +
                      $"name: {_playableDirector.playableAsset.name}\n" +
                      $"duration: {_playableDirector.duration}");

            Debug.Log(
                $"Time: {Math.Abs(_playableDirector.duration - _playableDirector.time) <= 1 / ((TimelineAsset) _playableDirector.playableAsset).editorSettings.frameRate}\n" +
                $"Pause: {_playableDirector.state == PlayState.Paused}\n" +
                $"IsValid: {_playableDirector.playableGraph.IsValid()}\n" +
                $"IsCutSceneWorking {Math.Abs(_playableDirector.duration - _playableDirector.time) <= 1 / ((TimelineAsset) _playableDirector.playableAsset).editorSettings.frameRate || _playableDirector.state == PlayState.Paused && !_playableDirector.playableGraph.IsValid()}");


            var waitUntil = new WaitUntil(() =>
                Math.Abs(_playableDirector.duration - _playableDirector.time) <=
                1 / ((TimelineAsset) _playableDirector.playableAsset).editorSettings.frameRate ||
                _playableDirector.state == PlayState.Paused &&
                !_playableDirector.playableGraph.IsValid());

            yield return waitUntil;
            _isCutSceneSkipEnable = false;
            yield return null;

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