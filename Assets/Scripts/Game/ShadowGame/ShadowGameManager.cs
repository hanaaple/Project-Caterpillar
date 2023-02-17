using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

        [SerializeField] private int stageCount;
    
        [SerializeField] private SpriteRenderer messyObject;
    
        [SerializeField] private Sprite[] messyObjectSprites;
    
    
        [Space(20)]
        [Header("디버깅용")]
        [SerializeField]
        private int stageIndex;
        [SerializeField]
        private int mentality;
        [SerializeField]
        private int startStageIndex;

        private int _mentality
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

        void Start()
        {
            flashlight.Init();
            _camera = Camera.main;
            _minBounds = cameraBound.bounds.min;
            _maxBounds = cameraBound.bounds.max;
            _yScreenHalfSize = _camera.orthographicSize;
            _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;
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
                gameOverPanel.SetActive(false);
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
        }

        private void OnGameTutorial()
        {
            tutorialPanel.SetActive(true);
            globalLight.SetActive(true);
        }

        private void OnGameStart()
        {
            _isPlaying = false;
            OnGameTutorial();
        }
    
        private IEnumerator GameStart()
        {
            _camera.transform.position = Vector3.back;
            flashlight.Reset();
            Reset();
            shadowMonster.gameObject.SetActive(false);
        
            var t = 1f;
            var tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            while (t >= 0)
            {
                t -= Time.deltaTime;
                tutorialCanvasGroup.alpha = t;
                yield return null;
            }
            tutorialPanel.SetActive(false);
            tutorialCanvasGroup.alpha = 1f;
        
            flashlight.gameObject.SetActive(true);
        
            // 애니메이션 실행, 종료 기다리다가 시작
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
            }
            else if (stageIndex == 7)
            {
                batteryImage.sprite = batterySprites[2];
                flashlight.UpdateLightRadius(0.4f);
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
        
            yield return new WaitForSeconds(2);
            Debug.Log("6초, 실패");
            // 6초 괴물 효과음, 괴물 연출, 정신력 1 감소
            _mentality--;
        
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
        
            if (_mentality == 0)
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
            _isPlaying = false;
            playPanel.SetActive(false);
            gameEndPanel.SetActive(true);
        }
    
        private void GameOver()
        {
            _isPlaying = false;
            playPanel.SetActive(false);
            gameOverPanel.SetActive(true);
        }

        private void UpdateMentality()
        {
            for (int idx = 0; idx < _mentality; idx++)
            {
                var t = heartImages[idx].color;
                t.a = 1f;
                heartImages[idx].color = t;
            }
        
            for (int idx = _mentality; idx < 3; idx++)
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
        
            Vector3 input = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -_camera.transform.position.z);
                
            Vector3 point = _camera.ScreenToWorldPoint(input);
            flashlight.MoveFlashLight(point);
            CameraMove(input);
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
            _mentality = 3;
            stageIndex = startStageIndex;

            messyObject.sprite = messyObjectSprites[0];
        
            shadowMonster.Reset();
        }
    }
}
