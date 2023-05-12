using System;
using System.Collections;
using System.Collections.Generic;
using Game.Default;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.InputSystem;
using Utility.Scene;
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
        [Header("Camera")] [Range(1, 20f)] [SerializeField]
        private float cameraSpeed;
        [SerializeField] private BoxCollider2D cameraBound;


        [Space(10)] [Header("Light")] [SerializeField]
        protected Flashlight flashlight;
        // [SerializeField] private Light2D globalLight;
        // [SerializeField] private float globalLightIntensity;

        [Space(20)] [Header("Canvas")]
        [SerializeField] private Button tutorialExitButton;
        
        [SerializeField] private Button retryButton;
        [SerializeField] private Button giveUpButton;

        [Space(20)] [Header("Play UI")] [SerializeField] private Animator heartAnimator;
        [SerializeField] private SpeechBubble[] damagedTexts;
        [SerializeField] private SpeechBubble[] defeatedTexts;
        [SerializeField] private Animator batteryAnimator;
        [SerializeField] private Transform toastMessageParent;
        [SerializeField] private float textSec;
        [SerializeField] private float itemPopupSec;


        [Space(20)] [Header("스테이지")] [SerializeField]
        protected Animator gameAnimator;

        [SerializeField] private Animator stageAnimator;
        [SerializeField] private ShadowMonster shadowMonster;
        [SerializeField] private ShadowGameItem[] shadowGameItems;
        [SerializeField] private int stageCount;
        [SerializeField] private float stageSec;

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
        private Queue<string> _toastQueue;
        private Animator _toastMessageParentAnimator;
        
        private InputActions _inputActions;
        private InputActions _tutorialInputActions;
        private InputActions _popupInputActions;
        
        private Coroutine _stageUpdateCoroutine;
        private Coroutine _stageMonsterDefeatCoroutine;
        private Coroutine _itemTimer;
        private Coroutine _toastCoroutine;
        private Coroutine _toastDisappearCoroutine;
        
        private Action _onItemShowEnd;

        private static readonly int MentalityHash = Animator.StringToHash("Mentality");
        private static readonly int StageIndexHash = Animator.StringToHash("StageIndex");
        private static readonly int TutorialHash = Animator.StringToHash("Tutorial");
        private static readonly int PlayHash = Animator.StringToHash("Play");
        private static readonly int ResetHash = Animator.StringToHash("Reset");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int GameOverHash = Animator.StringToHash("GameOver");
        private static readonly int SecHash = Animator.StringToHash("Sec");
        private static readonly int IndexHash = Animator.StringToHash("Index");
        private static readonly int DisAppearHash = Animator.StringToHash("DisAppear");
        private static readonly int RecoveryHash = Animator.StringToHash("Recovery");

        private void Awake()
        {
            _onItemShowEnd = () =>
            {
                foreach (var toastContent in shadowGameItems[_selectedItemIndex].toastContents)
                {
                    _toastQueue.Enqueue(toastContent);
                }

                _toastCoroutine ??= StartCoroutine(StartToast());

                TimeScaleHelper.Pop();
                InputManager.PopInputAction(_popupInputActions);
                if (_itemTimer != null)
                {
                    StopCoroutine(_itemTimer);
                    _itemTimer = null;
                }
                shadowGameItems[_selectedItemIndex].uiPanel.gameObject.SetActive(false);
            };

            _inputActions = new InputActions("ShadowGameManager")
            {
                OnPause = _ => { PlayUIManager.Instance.pauseManager.onPause(); }
            };

            _tutorialInputActions = new InputActions("ShadowGame Tutorial")
            {
                OnPause = _ => { PlayUIManager.Instance.pauseManager.onPause(); }
            };

            _popupInputActions = new InputActions("Item Popup")
            {
                OnAnyKey = _ => _onItemShowEnd(),
                OnLeftClick = _ => _onItemShowEnd()
            };
        }

        private void Start()
        {
            _toastQueue = new Queue<string>();
            
            // globalLight.intensity = globalLightIntensity;
            flashlight.Init();
            _camera = Camera.main;
            _minBounds = cameraBound.bounds.min;
            _maxBounds = cameraBound.bounds.max;
            _yScreenHalfSize = _camera.orthographicSize;
            _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;

            _toastMessageParentAnimator = toastMessageParent.GetComponent<Animator>();
            
            tutorialExitButton.onClick.AddListener(() =>
            {
                InputManager.PopInputAction(_tutorialInputActions);
                StartCoroutine(StartGame());
            });
            giveUpButton.onClick.AddListener(() => { SceneLoader.Instance.LoadScene("MainScene"); });
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

        private void ResetSetting()
        {
            _camera.transform.position = Vector3.back;
            Mentality = 3;
            stageIndex = 0;
            shadowMonster.Reset();
        }

        public void Play()
        {
            InputManager.PushInputAction(_inputActions);
            StartTutorial();
        }

        private void StartTutorial()
        {
            ResetSetting();
            gameAnimator.SetTrigger(TutorialHash);
        }

        /// <summary>
        /// Use by Animation Event
        /// </summary>
        public void PushTutorialInputActions()
        {
            InputManager.PushInputAction(_tutorialInputActions);
        }

        private IEnumerator StartGame()
        {
            yield return new WaitForSeconds(1f);

            gameAnimator.SetTrigger(PlayHash);
            
            StartStage();
        }

        protected virtual void StartStage(bool isClear = true)
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
            gameAnimator.SetFloat(SecHash, 0f);
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
            Debug.Log("괴물 처치 체크 중");
            yield return new WaitUntil(() => shadowMonster.GetIsDefeated());
            Debug.Log("괴물 처치 완료");
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
                        _toastQueue.Enqueue(speechBubble.text);
                    }
                }

                _toastCoroutine ??= StartCoroutine(StartToast());
            }, OnStageEnd(true));
        }

        private IEnumerator StageUpdate()
        {
            shadowMonster.Appear(stageIndex);
            // 괴물 등장, 효과음

            var sec = 0f;
            while (sec <= stageSec)
            {
                gameAnimator.SetFloat(SecHash, sec / stageSec);
                sec += Time.deltaTime;
                yield return null;
            }
            _isPlaying = false;
            gameAnimator.SetTrigger(AttackHash);
            shadowMonster.Attack();
            
            Mentality--;

            if (_stageMonsterDefeatCoroutine != null)
            {
                StopCoroutine(_stageMonsterDefeatCoroutine);
                _stageMonsterDefeatCoroutine = null;
            }
            
            Debug.Log("제한시간 종료");

            yield return null;
            
            if (Mentality != 0)
            {
                // Recovery 후 진행
                gameAnimator.SetFloat(SecHash, 0);
                gameAnimator.SetTrigger(RecoveryHash);
            }
            
            
            yield return new WaitUntil(() => gameAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayDefault"));
            _isPlaying = true;
            
            // 회복 후 Sec가 0이 아니라 어두워졌다 밝아짐
            // PlayDefault -> Attack(Trigger) -> Recovery -> PlayDefault(대기)
            // 그른데 이게 게임이 끝난 경우에는 Recovery 없이 끝나야 되고
            // 게임이 이어지는 경우에는 Recovery 후 진행해야됨

            StartCoroutine(OnStageEnd(false));
        }

        protected virtual IEnumerator OnStageEnd(bool isClear)
        {
            // 배경 연출 이후 아이템 실행

            // 배경 연출
            Debug.Log(stageIndex + "스테이지 종료, 배경 연출 시작");
            stageAnimator.SetInteger(StageIndexHash, stageIndex);
            stageAnimator.SetTrigger(PlayHash);
            yield return new WaitUntil(() => stageAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));

            Debug.Log(stageIndex + "스테이지 종료");

            stageIndex++;

            if (Mentality == 0)
            {
                GameOver();
            }
            else if (stageIndex == stageCount)
            {
                ClearGame();
            }
            else if (stageIndex < stageCount)
            {
                StartStage(isClear);
            }
        }

        private void ClearGame()
        {
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }

            //gameAnimator.SetBool(PlayHash, false);
        }

        private void GameOver()
        {
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }

            gameAnimator.SetTrigger(GameOverHash);
        }

        private GameObject GetToastMessage()
        {
            for (var i = 0; i < toastMessageParent.childCount; i++)
            {
                if (!toastMessageParent.GetChild(i).gameObject.activeSelf)
                {
                    return toastMessageParent.GetChild(i).gameObject;
                }
            }

            var toastMessage = Instantiate(toastMessageParent.GetChild(0).gameObject, toastMessageParent);

            return toastMessage;
        }
        
        private IEnumerable<Animator> GetActiveToastMessages()
        {
            var toastMessages = new List<Animator>();
            for (var i = 0; i < toastMessageParent.childCount; i++)
            {
                if (toastMessageParent.GetChild(i).gameObject.activeSelf)
                {
                    toastMessages.Add(toastMessageParent.GetChild(i).GetComponent<Animator>());
                }
            }

            return toastMessages.ToArray();
        }
        
        private IEnumerator StartToast()
        {
            Debug.Log("StartToast");
            if(!toastMessageParent.gameObject.activeSelf)
            {
                toastMessageParent.gameObject.SetActive(true);
            }
            
            while (_toastQueue.Count > 0)
            {
                var toasts = GetActiveToastMessages();
                foreach (var t in toasts)
                {
                    var animator = t.GetComponent<Animator>();
                    var index = animator.GetInteger(IndexHash);
                    animator.SetInteger(IndexHash, index + 1);

                    if (index > 2)
                    {
                        t.gameObject.SetActive(false);
                    }
                }
                
                var toastMessage = GetToastMessage();
                var toastAnimator = toastMessage.GetComponent<Animator>();
                var toastText = toastMessage.GetComponentInChildren<TMP_Text>(true);
                toastText.text = "";
               
                toastMessage.transform.SetAsLastSibling();
                toastMessage.gameObject.SetActive(true);
                toastAnimator.SetInteger(IndexHash, 0);
                
                _toastMessageParentAnimator.SetTrigger(ResetHash);

                yield return new WaitUntil(() => toastAnimator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));
                
                
                // 전부 늘어날때까지 대기
                
                // 전부 보이면 Text Print
                var toastContent = _toastQueue.Dequeue();
                var waitTextSec = new WaitForSeconds(textSec);
                foreach (var t in toastContent)
                {
                    toastText.text += t;
                    yield return waitTextSec;
                }
                
                // 타이핑 완료 후??
                // Reset and Start Disappear
                
                _toastMessageParentAnimator.SetTrigger(DisAppearHash);

                _toastDisappearCoroutine ??= StartCoroutine(ToastDisappear());
            }

            _toastCoroutine = null;
        }

        private IEnumerator ToastDisappear()
        {
            yield return new WaitUntil(() => _toastMessageParentAnimator.GetCurrentAnimatorStateInfo(0).IsName("End"));
            // 전부 사라지게 만들기.
            _toastDisappearCoroutine = null;
            toastMessageParent.gameObject.SetActive(false);

            Debug.Log("Toast 종료");
            
            for (var idx = 0; idx < toastMessageParent.childCount; idx++)
            {
                toastMessageParent.GetChild(idx).gameObject.SetActive(false);
            }
        }

        private IEnumerator ItemTimer()
        {
            yield return new WaitForSecondsRealtime(itemPopupSec);
            _onItemShowEnd?.Invoke();
            _itemTimer = null;
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
                    _toastQueue.Enqueue(speechBubble.text);
                    _toastCoroutine ??= StartCoroutine(StartToast());
                }
            }
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            var input = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -_camera.transform.position.z);

            var point = _camera.ScreenToWorldPoint(input);
            flashlight.MoveFlashLight(point);
            CameraMove(input);

            if (Input.GetMouseButtonDown(0))
            {
                foreach (var shadowGameItem in shadowGameItems)
                {
                    if (shadowGameItem.gameObject.activeSelf)
                    {
                        shadowGameItem.CheckClick();
                    }
                }
            }
        }

        private void CameraMove(Vector3 input)
        {
            var cameraMoveVec = Vector3.zero;
            // 우측 이동
            if (Screen.currentResolution.width * 0.9f < input.x)
            {
                cameraMoveVec.x = 1;
            }
            // 좌측 이동
            else if (Screen.currentResolution.width * 0.1f > input.x)
            {
                cameraMoveVec.x = -1;
            }

            // 상단 이동
            if (Screen.currentResolution.height * 0.9f < input.y)
            {
                cameraMoveVec.y = 1;
            }
            // 하단 이동
            else if (Screen.currentResolution.height * 0.1f > input.y)
            {
                cameraMoveVec.y = -1;
            }

            var cameraTransform = _camera.transform;

            var targetPos = cameraTransform.position + cameraMoveVec;

            var clampX = cameraTransform.position.x;
            var clampY = cameraTransform.position.y;

            //Debug.Log($"old: {cameraTransform.position.x}, {cameraTransform.position.y} ");
            
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraSpeed * Time.deltaTime);
            
            // Debug.Log($"new: {cameraTransform.position.x}, {cameraTransform.position.y}");

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
            
            //Debug.Log($"t: {cameraSpeed * Time.deltaTime}, maxBound: {_maxBounds.x} {_maxBounds.y}  screenSize half: {_xScreenHalfSize}, {_yScreenHalfSize}");
            cameraTransform.position = new Vector3(clampX, clampY, cameraTransform.position.z);
            
            // Debug.Log($"new: {cameraTransform.position.x}, {cameraTransform.position.y}");
        }

        protected virtual void ReStartGame()
        {
            ResetSetting();
            gameAnimator.SetTrigger(ResetHash);
            // StartGame
            StartCoroutine(StartGame());
        }
    }
}