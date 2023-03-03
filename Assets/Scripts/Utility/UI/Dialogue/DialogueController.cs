using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
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

namespace Utility.UI.Dialogue
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

        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueInputArea;

        [Header("CutScene")] [SerializeField] private PlayableDirector playableDirector;

        [Header("Choice")] [SerializeField] private ChoiceSelector[] choiceSelectors;

        [Header("깜빡이는 애니메이션 들어간 ui")] [SerializeField]
        private GameObject blinkingIndicator;

        [Header("좌 애니메이터")] [SerializeField] private Animator leftAnimator;

        [Header("우 애니메이터")] [SerializeField] private Animator rightAnimator;

        [SerializeField] private float textSpeed;

        [Space(10)] [Header("디버깅용")] [SerializeField]
        private DialogueData currentDialogueData;

        [SerializeField] private DialogueData baseDialogueData;
        [SerializeField] private DialogueData choicedDialogueData;

        [NonSerialized] public UnityAction onDialogueEnd;
        [NonSerialized] public bool isDialogue;

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
            dialogueInputArea.onClick.AddListener(() =>
            {
                OnInputDialogue();
            });
            baseDialogueData = new DialogueData();
            choicedDialogueData = new DialogueData();
        }

        private async void Initialize(string jsonAsset)
        {
            isDialogue = true;
            currentDialogueData = baseDialogueData;
            baseDialogueData.Load(jsonAsset);
            _isUnfolding = false;

            rightAnimator.SetInteger(CharacterHash, 0);
            rightAnimator.SetInteger(ExpressionHash, 0);

            leftAnimator.SetInteger(CharacterHash, 0);
            leftAnimator.SetInteger(ExpressionHash, 0);

            dialoguePanel.SetActive(true);

            await Task.Delay((int)(Time.deltaTime * 1000f));

            InputManager.SetUiAction(true);
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Dialogue.performed += OnInputDialogue;
        }

        public void StartDialogue(string jsonAsset)
        {
            if (isDialogue)
            {
                return;
            }

            Initialize(jsonAsset);

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
                currentDialogueData.index++;
                ProgressDialogue();
            }
        }

        private void ProgressDialogue()
        {
            if (IsDialogueEnd())
            {
                if (choicedDialogueData == currentDialogueData)
                {
                    choicedDialogueData.Reset();
                    currentDialogueData = baseDialogueData;
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
            var dialogue = currentDialogueData.dialogueElements[currentDialogueData.index];
            ItemManager.Instance.SetItem(dialogue.option);
            switch(dialogue.dialogueType)
            {
                case DialogueType.Script:
                {
                    StartDialoguePrint();

                    if (dialogue.option is { Length: >= 1 })
                    {
                        for(var index = 0; index < dialogue.option.Length; index++)
                        {
                            dialogue.option[index] = dialogue.option[index].Replace(" ", "");
                        }

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
                                animator.SetInteger(CharacterHash, (int)dialogue.name);
                            }

                            if (dialogue.expression != Expression.Keep)
                            {
                                animator.SetInteger(ExpressionHash, (int)dialogue.expression);
                            }
                        }
                    }
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
                        var uiActions = InputManager.inputControl.Ui;
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
                    var playableAsset = Resources.Load<TimelineAsset>($"Timeline/{dialogue.contents}");
                    Debug.Log(playableAsset);
                    playableDirector.playableAsset = playableAsset;

                    if (dialogue.option.Contains("Hold", StringComparer.OrdinalIgnoreCase))
                    {
                        playableDirector.extrapolationMode = DirectorWrapMode.Hold;
                    }
                    else if (dialogue.option.Contains("None", StringComparer.OrdinalIgnoreCase))
                    {
                        playableDirector.extrapolationMode = DirectorWrapMode.None;
                    }
                    // Binding - Animator 한개만? 아마도? 카메라 흔들리는 트랙, 클립 만들고
                    var timelineAsset = (TimelineAsset)playableDirector.playableAsset;
                    if (timelineAsset != null)
                    {
                        var tracks = timelineAsset.GetOutputTracks();
                        foreach (var temp in tracks)
                        {
                            if (temp is AnimationTrack && dialogueCanvas.TryGetComponent(out Animator animator))
                            {
                                playableDirector.SetGenericBinding(temp, animator);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Task 타임라인 오류");
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
                    else
                    {
                        Debug.LogWarning($"엑셀 세팅 이상함 - 길이: {enumerable.Length}");
                        foreach (var option in dialogue.option)
                        {
                            Debug.Log("option - " + option);
                        }
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartDialoguePrint()
        {
            Debug.Log("프린트 시작");
            _isUnfolding = true;
            blinkingIndicator.SetActive(false);
            dialogueText.text = "";
            _printCoroutine = StartCoroutine(DialoguePrint());
        }

        private IEnumerator DialoguePrint()
        {
            var dialogueItem = currentDialogueData.dialogueElements[currentDialogueData.index];

            var wordSpeed = 1f;

            _isSkipEnable = true;

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

            CompleteDialogue();
        }

        private void CompleteDialogue()
        {
            _printCoroutine = null;
            _isUnfolding = false;
            var dialogueItem = currentDialogueData.dialogueElements[currentDialogueData.index];
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

        private void EndDialogue()
        {
            Debug.Log("대화 끝");
            currentDialogueData = default;

            dialoguePanel.SetActive(false);

            isDialogue = false;

            onDialogueEnd?.Invoke();
            onDialogueEnd = () => {};

            rightAnimator.SetTrigger(DisappearHash);
            leftAnimator.SetTrigger(DisappearHash);

            InputManager.SetUiAction(false);
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Dialogue.performed -= OnInputDialogue;
        }

        private void OnClickChoice(int curIdx, int choiceLen, int choiceContextLen)
        {
            Debug.Log(curIdx);
            Debug.Log("선택 개수: " + choiceLen);
            Debug.Log("선택 대화 길이: " + choiceContextLen);

            InitChoiceDialogue(curIdx + choiceLen, choiceContextLen);

            choicePanel.SetActive(false);
            ProgressDialogue();
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

            var uiActions = InputManager.inputControl.Ui;
            uiActions.Dialogue.performed -= OnInputDialogue;

            HighlightHelper.Instance.Push(_choiceHighlighter);

            var choicedCount = 0;
            while(currentDialogueData.index < currentDialogueData.dialogueElements.Length &&
                  currentDialogueData.dialogueElements[currentDialogueData.index].dialogueType != DialogueType.ChoiceEnd)
            {
                Debug.Log(currentDialogueData.index);

                var choiceCount = 0;
                var choiceContextLen = 0;

                while(currentDialogueData.index + choiceCount < currentDialogueData.dialogueElements.Length &&
                      currentDialogueData.dialogueElements[currentDialogueData.index + choiceCount].dialogueType == DialogueType.Choice)
                {
                    choiceCount++;
                }
                while(currentDialogueData.index + choiceCount + choiceContextLen < currentDialogueData.dialogueElements.Length)
                {
                    var dialogueElement = currentDialogueData.dialogueElements[currentDialogueData.index + choiceCount + choiceContextLen];
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

                    choiceButton.onClick.AddListener(() =>
                    {
                        OnClickChoice(curIdx, choiceCount, choiceContextLen);
                    });
                }

                currentDialogueData.index += choiceCount + choiceContextLen;
                choicedCount += choiceCount;

                Debug.Log($"다음 Index: {currentDialogueData.index}\n" +
                          $"선택된 개수: {choicedCount}\n");
            }
        }

        private void InitChoiceDialogue(int nextIndex, int choiceContextLen)
        {
            var uiActions = InputManager.inputControl.Ui;
            uiActions.Dialogue.performed += OnInputDialogue;

            HighlightHelper.Instance.Pop(_choiceHighlighter);

            if (choiceContextLen == 0)
            {
                return;
            }
            choicedDialogueData.index = 0;
            choicedDialogueData.dialogueElements = new DialogueElement[choiceContextLen];
            Array.Copy(baseDialogueData.dialogueElements, nextIndex, choicedDialogueData.dialogueElements, 0, choiceContextLen);
            currentDialogueData = choicedDialogueData;
        }

        private IEnumerator WaitInput(float sec)
        {
            yield return new WaitForSeconds(sec);
            _isCutSceneSkipEnable = true;
            OnInputDialogue();
        }

        private bool IsDialogueEnd()
        {
            return currentDialogueData.index >= currentDialogueData.dialogueElements.Length;
        }
    }
}