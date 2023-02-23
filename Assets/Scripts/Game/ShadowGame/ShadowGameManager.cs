using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.InputSystem;
using Utility.SceneLoader;

namespace Game.ShadowGame
{
    public class ShadowGameManager : MonoBehaviour
    {
        [SerializeField]
        private Button[] campingButtons;
    
        [Header("Camera")]
        [Range(1, 20f)]
        [SerializeField]
        private float cameraSpeed;
        [SerializeField]
        private BoxCollider2D cameraBound;
    
        [SerializeField]
        private Flashlight flashlight;
        [SerializeField]
        private GameObject globalLight;
    
        [SerializeField]
        private Animation animation;
    
        [Space(20)]
        [Header("Canvas")]
        [SerializeField]
        private GameObject tutorialPanel;
        [SerializeField]
        private Button tutorialButton;
        [SerializeField]
        private GameObject playPanel;
    
        [Space(10)]
        [SerializeField]
        private GameObject gameOverPanel;
        [SerializeField]
        private Button retryButton;
        [SerializeField]
        private Button giveUpButton;
    
        [Space(10)]
        [SerializeField]
        private GameObject gameEndPanel;
        [SerializeField]
        private Image[] heartImages;
        [SerializeField]
        private Image batteryImage;
        [SerializeField]
        private Sprite[] batterySprites;

        [Space(20)] [Header("스테이지")] [SerializeField]
        private ShadowMonster shadowMonster;
        
        [SerializeField] private ShadowGameItem[] shadowGameItems;

        [SerializeField] private int stageCount;
    
        [SerializeField] private SpriteRenderer messyObject;
    
        [SerializeField] private Sprite[] messyObjectSprites;

        [SerializeField] private Animator uiAnimator;
    
    
        [Space(20)]
        [Header("디버깅용")]
        [SerializeField]
        private int stageIndex;
        [SerializeField]
        private int mentality;
        [SerializeField]
        private int startStageIndex;

        private int Mentality
        {
            get => mentality;
            set
            {
                mentality = value;
                UpdateMentality();
            }
        }

        private Coroutine _stageCoroutine;
        private Coroutine _stageCheckCoroutine;

        private Camera _camera;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private float _yScreenHalfSize;
        private float _xScreenHalfSize;

        private bool _isPlaying;

        private Action<InputAction.CallbackContext> _onItemShowEnd;
        private Coroutine _itemTimer;

        private int _selectedItemIndex;

        private void Awake()
        {
            var playerActions = InputManager.InputControl.PlayerActions;
            _onItemShowEnd = delegate
            {
                Time.timeScale = 1f;
                StopCoroutine(_itemTimer);
                _itemTimer = null;
                shadowGameItems[_selectedItemIndex].gameObject.SetActive(false);
                shadowGameItems[_selectedItemIndex].uiPanel.gameObject.SetActive(false);
                playerActions.Interact.performed -= _onItemShowEnd;
                InputManager.SetPlayerAction(false);
            };
        }
        
        private void Start()
        {
            OnGameStart();
            tutorialButton.onClick.AddListener(() =>
            {
                StartCoroutine(GameStart());
            });

            giveUpButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("MainScene");
            });
            
            retryButton.onClick.AddListener(() =>
            {
                uiAnimator.SetTrigger("Reset");
                shadowMonster.Reset();
                StartCoroutine(GameStart());
            });
            
            foreach (var campingButton in campingButtons)
            {
                campingButton.onClick.AddListener(() =>
                {
                    SceneLoader.Instance.LoadScene("CampingGameTest");
                });
            }

            for(var idx = 0; idx < shadowGameItems.Length; idx++)
            {
                var shadowGameItem = shadowGameItems[idx];
                var itemIndex = idx;
                shadowGameItem.onClick = () =>
                {
                    _selectedItemIndex = itemIndex;
                    Time.timeScale = 0f;
                    shadowGameItem.uiPanel.gameObject.SetActive(true);
                    _itemTimer = StartCoroutine(ItemTimer());
                };
            }
        }

        private IEnumerator ItemTimer()
        {
            InputManager.SetPlayerAction(true);
            var playerActions = InputManager.InputControl.PlayerActions;
            playerActions.Interact.performed += _onItemShowEnd;
            yield return new WaitForSecondsRealtime(2f);
            _onItemShowEnd?.Invoke(default);
        }

        private void OnGameTutorial()
        {
            uiAnimator.SetBool("IsTutorial", true);
            globalLight.SetActive(true);
        }

        private void OnGameStart()
        {
            flashlight.Init();
            _camera = Camera.main;
            _minBounds = cameraBound.bounds.min;
            _maxBounds = cameraBound.bounds.max;
            _yScreenHalfSize = _camera.orthographicSize;
            _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;
            
            uiAnimator.SetTrigger("Reset");
            _isPlaying = false;
            OnGameTutorial();
        }
    
        private IEnumerator GameStart()
        {
            _camera.transform.position = Vector3.back;
            flashlight.Reset();
            Reset();
            shadowMonster.gameObject.SetActive(false);

            yield return new WaitForSeconds(1f);
            tutorialPanel.SetActive(false);
        
            flashlight.gameObject.SetActive(true);
            uiAnimator.SetBool("IsPlay", true);
            uiAnimator.SetBool("IsTutorial", false);
            
            yield return new WaitForSeconds(1f);
            animation.Play();
            yield return new WaitForSeconds(animation.clip.length);
        
            playPanel.SetActive(true);
            OnStartStage();
        }

        private void OnStartStage()
        {
            Debug.Log(stageIndex + " 스테이지 시작");
            _isPlaying = true;
            _stageCoroutine = StartCoroutine(StageUpdate());
            _stageCheckCoroutine = StartCoroutine(CheckDefeat());
     
            if (stageIndex == 0)
            { 
                batteryImage.sprite = batterySprites[0];
                flashlight.UpdateLightRadius(1f);
            }
            else if (stageIndex == 3)
            {
                batteryImage.sprite = batterySprites[1];
                flashlight.UpdateLightRadius(0.7f);
                
                shadowGameItems[0].gameObject.SetActive(true);
            }
            else if (stageIndex == 7)
            {
                batteryImage.sprite = batterySprites[2];
                flashlight.UpdateLightRadius(0.4f);
                
                shadowGameItems[1].gameObject.SetActive(true);
            }
            else if (stageIndex == 9)
            {
                shadowGameItems[2].gameObject.SetActive(true);
            }

            if (stageIndex == 4)
            {
                messyObject.sprite = messyObjectSprites[1];
            }
        }
    
        private IEnumerator CheckDefeat()
        {
            Debug.Log("괴물 처치 체크 중");
            yield return new WaitUntil(() => shadowMonster.GetIsDefeated());
            OnDefeatShadowMonster();
        }
    
        private void OnDefeatShadowMonster()
        {
            RemoveCoroutine();
            shadowMonster.Defeat(OnStageEnd);
        }

        private IEnumerator StageUpdate()
        {
            shadowMonster.Appear(stageIndex);
            // 괴물 등장, 효과음
        
            yield return new WaitForSeconds(2);
            Debug.Log("2초");
            // 2초 괴물 효과음
            
            yield return new WaitForSeconds(2);
            Debug.Log("4초");
            // 4초 위급한 화면 연출
            //uiAnimator.SetTrigger("");
        
            yield return new WaitForSeconds(2);
            Debug.Log("6초, 실패");
            // 6초 괴물 효과음, 괴물 연출, 정신력 1 감소
            //uiAnimator.SetTrigger("");
            Mentality--;
        
            RemoveCoroutine();
        
            shadowMonster.Attack(OnStageEnd);
        }

        private void RemoveCoroutine()
        {
            if (_stageCoroutine != null)
            {
                StopCoroutine(_stageCoroutine);
                _stageCoroutine = null;
            }
            if (_stageCheckCoroutine != null)
            {
                StopCoroutine(_stageCheckCoroutine);
                _stageCheckCoroutine = null;
            }
        }

        private void OnStageEnd()
        {
            Debug.Log(stageIndex + "스테이지 종료");

            stageIndex++;

            if (Mentality == 0)
            {
                GameOver();
            }
            else if (stageIndex == stageCount)
            {
                OnGameEnd();
            }
            else if (stageIndex < stageCount)
            {
                OnStartStage();
            }
        }

        private void OnGameEnd()
        {
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }
            uiAnimator.SetBool("IsPlay", false);
            _isPlaying = false;
            playPanel.SetActive(false);
            gameEndPanel.SetActive(true);
        }
    
        private void GameOver()
        {
            foreach (var shadowGameItem in shadowGameItems)
            {
                shadowGameItem.gameObject.SetActive(false);
            }
            uiAnimator.SetBool("IsPlay", false);
            _isPlaying = false;
            playPanel.SetActive(false);
            uiAnimator.SetTrigger("GameOver");
        }

        private void UpdateMentality()
        {
            for (int idx = 0; idx < Mentality; idx++)
            {
                var t = heartImages[idx].color;
                t.a = 1f;
                heartImages[idx].color = t;
            }
        
            for (int idx = Mentality; idx < 3; idx++)
            {
                var t = heartImages[idx].color;
                t.a = 110/255f;
                heartImages[idx].color = t;
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
            Vector3 cameraMoveVec = Vector3.zero;
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
        
            float clampX = cameraTransform.position.x;
            float clampY = cameraTransform.position.y;
        
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

        private void Reset()
        {
            Mentality = 3;
            stageIndex = startStageIndex;

            messyObject.sprite = messyObjectSprites[0];
        
            shadowMonster.Reset();
        }
    }
}
