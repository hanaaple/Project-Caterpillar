using System;
using System.Collections;
using System.Collections.Generic;
using Game.Default;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.Audio;
using Utility.Core;
using Utility.InputSystem;
using Utility.SaveSystem;
using Utility.Scene;
using Utility.Tutorial;
using Utility.UI.Check;
using Utility.UI.Highlight;
using Utility.Util;

namespace Game.Stage1.ShadowGame.Default
{
    [Serializable]
    public class SpeechBubble
    {
        public int index;
        [TextArea] public string text;
    }

    public class ShadowGameManager : MonoBehaviour, IGamePlayable
    {
        [Header("Tutorial")] [SerializeField] private TutorialHelper tutorialHelper;

        [Header("Audio")] [SerializeField] private AudioData bgmAudioData;

        /// <summary>
        /// sfx, loop, fade
        /// </summary>
        [SerializeField] private AudioData crisisAudioData;

        [Header("Camera")] [Range(1, 20f)] [SerializeField]
        private float cameraSpeed;

        [SerializeField] private BoxCollider2D cameraBound;

        [Space(10)] [Header("Light")] [SerializeField]
        private Flashlight flashlight;

        [Space(20)] [Header("Play UI")] [SerializeField]
        private Animator heartAnimator;

        [SerializeField] private SpeechBubble[] damagedTexts;
        [SerializeField] private SpeechBubble[] defeatedTexts;
        [SerializeField] private Animator batteryAnimator;
        [SerializeField] private float itemPopupSec;

        [Space(20)] [Header("GameOver")] [SerializeField]
        private Button retryButton;

        [SerializeField] private Button giveUpButton;
        [SerializeField] private SelectHighlightItem[] gameOverHighlightItems;
        [SerializeField] private CheckUIManager checkUIManager;

        [Space(20)] [Header("스테이지")] [SerializeField]
        protected Animator gameAnimator;

        [SerializeField] private Animator stageAnimator;
        [SerializeField] protected ShadowMonster shadowMonster;
        [SerializeField] private ShadowGameItem[] shadowGameItems;
        [SerializeField] private int stageCount;
        [SerializeField] private float stageSec;
        [SerializeField] private float crisisSec = 7;

        [Space(20)] [Header("디버깅용")] [SerializeField]
        protected int stageIndex;

        private int _mentality;

        private int Mentality
        {
            get => _mentality;
            set
            {
                _mentality = value;
                UpdateMentality();
            }
        }

        private Camera _camera;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private float _yScreenHalfSize;
        private float _xScreenHalfSize;

        private bool _isPlaying;
        private int _selectedItemIndex;

        private InputActions _inputActions;
        private InputActions _popupInputActions;

        private Highlighter _gameOverHighlighter;

        private Coroutine _stageUpdateCoroutine;
        private Coroutine _stageMonsterDefeatCoroutine;
        private Coroutine _itemTimer;

        private static readonly int MentalityHash = Animator.StringToHash("Mentality");
        private static readonly int StageIndexHash = Animator.StringToHash("StageIndex");
        private static readonly int PlayHash = Animator.StringToHash("Play");
        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int LastAttackHash = Animator.StringToHash("LastAttack");
        private static readonly int SecHash = Animator.StringToHash("Sec");
        private static readonly int ClearHash = Animator.StringToHash("Clear");

        protected string NextScene;

        private void Awake()
        {
            _inputActions = new InputActions("ShadowGameManager")
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause(); },
                OnLeftClick = () =>
                {
                    if (!_isPlaying)
                    {
                        return;
                    }

                    foreach (var shadowGameItem in shadowGameItems)
                    {
                        shadowGameItem.TryClick(_camera);
                    }
                }
            };

            _popupInputActions = new InputActions("Item Popup")
            {
                OnEsc = () => { PlayUIManager.Instance.pauseManager.onPause(); },
                OnLeftClick = PopDownItem
            };

            _gameOverHighlighter = new Highlighter("GameOver Highlight")
            {
                HighlightItems = new List<HighlightItem>(gameOverHighlightItems),
                highlightType = Highlighter.HighlightType.HighlightIsSelect
            };

            _gameOverHighlighter.onPush = () => { _gameOverHighlighter.Select(0); };

            foreach (var highlightItem in gameOverHighlightItems)
            {
                highlightItem.Init(highlightItem.button.GetComponentInChildren<Animator>(true));
            }

            _gameOverHighlighter.Init(Highlighter.ArrowType.Horizontal);
        }

        private void Start()
        {
            NextScene = "CampingScene";

            flashlight.Init();
            _camera = Camera.main;
            _minBounds = cameraBound.bounds.min;
            _maxBounds = cameraBound.bounds.max;
            _yScreenHalfSize = _camera.orthographicSize;
            _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;

            checkUIManager.Initialize();
            checkUIManager.SetText("이야기를 포기할 경우, 재 진행이 어렵습니다.\n이 기억의 이야기를 포기 하시겠습니까?");

            checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.Yes, () =>
            {
                checkUIManager.Pop();
                HighlightHelper.Instance.Pop(_gameOverHighlighter);
                SaveHelper.SetNpcData(NpcType.Photographer, NpcState.Fail);
                SceneLoader.Instance.LoadScene("MainScene");
            });

            checkUIManager.SetOnClickListener(CheckHighlightItem.ButtonType.No, () => { checkUIManager.Pop(); });

            giveUpButton.onClick.AddListener(() => { checkUIManager.Push(); });

            retryButton.onClick.AddListener(ReStartGame);

            for (var idx = 0; idx < shadowGameItems.Length; idx++)
            {
                var shadowGameItem = shadowGameItems[idx];
                var itemIndex = idx;
                shadowGameItem.OnClick = () =>
                {
                    shadowGameItem.uiPanel.gameObject.SetActive(true);
                    _selectedItemIndex = itemIndex;
                    shadowGameItem.gameObject.SetActive(false);
                    TimeScaleHelper.Push(0f);
                    InputManager.PushInputAction(_popupInputActions);
                    _itemTimer = StartCoroutine(ItemTimer());
                };
            }

            Play();
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            var worldPoint = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            flashlight.MoveFlashLight(worldPoint);
            CameraMove(Mouse.current.position.ReadValue());
        }

        private void ResetSetting()
        {
            _camera.transform.position = Vector3.back;
            Mentality = 3;
            stageIndex = 0;

            flashlight.SetFlashLightPos(Vector3.zero);

            stageAnimator.SetTrigger(ResetHash);
            stageAnimator.SetInteger(StageIndexHash, stageIndex);
            gameAnimator.SetFloat(SecHash, 0);
        }

        public void Play()
        {
            PlayUIManager.Instance.tutorialManager.StartTutorial(tutorialHelper, () =>
            {
                InputManager.PushInputAction(_inputActions);
                ResetSetting();
                StartCoroutine(StartGame());
            });
        }

        private IEnumerator StartGame()
        {
            yield return new WaitUntil(() => gameAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));
            yield return YieldInstructionProvider.WaitForSeconds(1f);

            bgmAudioData.Play();
            gameAnimator.SetTrigger(PlayHash);
            StartStage();
        }

        private void StartStage(bool isClear = true)
        {
            Debug.Log(stageIndex + " 스테이지 시작");

            // 아이템
            if (isClear)
            {
                foreach (var shadowGameItem in shadowGameItems)
                {
                    if (shadowGameItem.IsEnable(stageIndex))
                    {
                        shadowGameItem.gameObject.SetActive(true);
                    }
                }
            }

            _isPlaying = true;
            _stageUpdateCoroutine = StartCoroutine(StageUpdate());
            _stageMonsterDefeatCoroutine = StartCoroutine(CheckDefeat());

            batteryAnimator.SetInteger(StageIndexHash, stageIndex);

            switch (stageIndex)
            {
                case 0:
                    flashlight.SetLightRadiusPercentage(1f);
                    break;
                case 3:
                    flashlight.SetLightRadiusPercentage(0.7f);
                    break;
                case 7:
                    flashlight.SetLightRadiusPercentage(0.4f);
                    break;
            }
        }

        private IEnumerator CheckDefeat()
        {
            yield return new WaitUntil(() => shadowMonster.GetIsDefeated());

            if (_stageUpdateCoroutine != null)
            {
                StopCoroutine(_stageUpdateCoroutine);
                _stageUpdateCoroutine = null;
            }

            shadowMonster.Defeat(() =>
            {
                foreach (var speechBubble in defeatedTexts)
                {
                    if (stageIndex == speechBubble.index)
                    {
                        SceneHelper.Instance.toastManager.Enqueue(speechBubble.text);
                    }
                }
            }, () => { StartCoroutine(OnStageEnd(true)); });
        }

        private IEnumerator StageUpdate()
        {
            shadowMonster.Appear(stageIndex);

            yield return new WaitUntil(
                () => shadowMonster.monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wait"));
            // Set Attack Enable
            shadowMonster.SetEnable(true);


            // 괴물 등장, 효과음

            var sec = 0f;
            while (sec <= stageSec)
            {
                if (sec > crisisSec)
                {
                    crisisAudioData.Play();
                }

                gameAnimator.SetFloat(SecHash, sec / stageSec);
                sec += Time.deltaTime;
                yield return null;
            }

            crisisAudioData.Stop();

            _isPlaying = false;
            Mentality--;

            gameAnimator.SetTrigger(Mentality > 0 ? AttackHash : LastAttackHash);

            shadowMonster.Attack();

            if (_stageMonsterDefeatCoroutine != null)
            {
                StopCoroutine(_stageMonsterDefeatCoroutine);
                _stageMonsterDefeatCoroutine = null;
            }

            Debug.Log("제한시간 종료");

            if (Mentality > 0)
            {
                yield return null;

                gameAnimator.SetFloat(SecHash, 0);

                yield return new WaitUntil(() => gameAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayDefault"));

                _isPlaying = true;

                StartCoroutine(OnStageEnd(false));
            }
            else
            {
                GameOver();
            }
        }

        protected virtual IEnumerator OnStageEnd(bool isClear)
        {
            // 배경 연출 대기
            stageAnimator.SetInteger(StageIndexHash, stageIndex);
            stageAnimator.SetTrigger(PlayHash);
            yield return new WaitUntil(() =>
                stageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty") && shadowMonster.monsterAnimator
                    .GetCurrentAnimatorStateInfo(0).IsName("Default"));

            Debug.Log(stageIndex + "스테이지 종료");

            stageIndex++;

            if (stageIndex == stageCount)
            {
                ClearGame();
            }
            else if (stageIndex < stageCount)
            {
                var t = gameAnimator.GetFloat(SecHash);
                while (t > 0f)
                {
                    t -= Time.deltaTime;
                    gameAnimator.SetFloat(SecHash, t);
                    yield return null;
                }

                StartStage(isClear);
            }
        }

        protected virtual void ClearGame()
        {
            InputManager.PopInputAction(_inputActions);
            gameAnimator.SetTrigger(ClearHash);
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }

            SceneLoader.Instance.LoadScene(NextScene);
        }

        private void GameOver()
        {
            InputManager.PopInputAction(_inputActions);
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Animator Event
        /// </summary>
        public void GameOverPush()
        {
            HighlightHelper.Instance.Push(_gameOverHighlighter);
        }

        private IEnumerator ItemTimer()
        {
            yield return YieldInstructionProvider.WaitForSecondsRealtime(itemPopupSec);
            PopDownItem();
        }

        private void UpdateMentality()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            heartAnimator.SetInteger(MentalityHash, Mentality);

            Debug.Log($"Update Mental {Mentality} {heartAnimator.GetInteger(MentalityHash)}");

            foreach (var speechBubble in damagedTexts)
            {
                if (Mentality == speechBubble.index)
                {
                    SceneHelper.Instance.toastManager.Enqueue(speechBubble.text);
                }
            }
        }

        private void CameraMove(Vector2 input)
        {
            // Screen.currentResolution.width did not always work 

            var cameraMoveVec = Vector2.zero;
            // 우측 이동
            if (Screen.width * 0.9f < input.x)
            {
                cameraMoveVec.x = 1;
            }
            // 좌측 이동
            else if (Screen.width * 0.1f > input.x)
            {
                cameraMoveVec.x = -1;
            }

            // 상단 이동
            if (Screen.height * 0.9f < input.y)
            {
                cameraMoveVec.y = 1;
            }
            // 하단 이동
            else if (Screen.height * 0.1f > input.y)
            {
                cameraMoveVec.y = -1;
            }

            var cameraTransform = _camera.transform;
            var targetPos = cameraTransform.position + (Vector3) cameraMoveVec;
            var clampX = cameraTransform.position.x;
            var clampY = cameraTransform.position.y;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraSpeed * Time.deltaTime);

            if (_maxBounds.x - _xScreenHalfSize > 0)
            {
                clampX = Mathf.Clamp(cameraTransform.position.x, _minBounds.x + _xScreenHalfSize,
                    _maxBounds.x - _xScreenHalfSize);
            }

            if (_maxBounds.y - _yScreenHalfSize > 0)
            {
                clampY = Mathf.Clamp(cameraTransform.position.y, _minBounds.y + _yScreenHalfSize,
                    _maxBounds.y - _yScreenHalfSize);
            }

            cameraTransform.position = new Vector3(clampX, clampY, cameraTransform.position.z);
        }

        private void ReStartGame()
        {
            HighlightHelper.Instance.Pop(_gameOverHighlighter);
            ResetSetting();
            gameAnimator.SetTrigger(ResetHash);
            StartCoroutine(StartGame());
        }

        private void PopDownItem()
        {
            foreach (var toastContent in shadowGameItems[_selectedItemIndex].toastContents)
            {
                SceneHelper.Instance.toastManager.Enqueue(toastContent);
            }

            TimeScaleHelper.Pop();
            InputManager.PopInputAction(_popupInputActions);
            if (_itemTimer != null)
            {
                StopCoroutine(_itemTimer);
                _itemTimer = null;
            }

            shadowGameItems[_selectedItemIndex].uiPanel.gameObject.SetActive(false);
        }
    }
}