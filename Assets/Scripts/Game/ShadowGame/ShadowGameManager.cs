using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShadowGameManager : MonoBehaviour
{
    [System.Serializable]
    class StageProps
    {
        public ShadowMonster shadowMonster;
    }
    
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
    
    public Animation animation;
    
    [Space(20)]
    [Header("Canvas")]
    [SerializeField]
    private GameObject tutorialPanel;
    [SerializeField]
    private Button tutorialButton;
    [SerializeField]
    private GameObject playPanel;
    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private Image[] heartImages;
    
    [Space(20)]
    [Header("스테이지")]
    [SerializeField]
    private StageProps[] stageProps;
    
    [Space(20)]
    [Header("디버깅용")]
    [SerializeField]
    private int _stage;
    [SerializeField] 
    private int mentality;

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
        Debug.Log(animation.clip.length);
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
    }

    private void OnGameTutorial()
    {
        tutorialPanel.SetActive(true);
        globalLight.SetActive(true);
    }

    private void OnGameStart()
    {
        _isPlaying = false;
        _mentality = 0;
        OnGameTutorial();
    }
    
    private IEnumerator GameStart()
    {
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
        _mentality = 3;
        OnStartStage();
    }

    private void OnStartStage()
    {
        Debug.Log(_stage + " 스테이지 시작");
        _stageCoroutine = StartCoroutine(StageUpdate());
        _stageCheckCoroutine = StartCoroutine(CheckDefeat());
        _isPlaying = true;
     
        // 4스테이지
        if (_stage == 0)
        {
            flashlight.UpdateLightRadius(1f);
        }
        else if (_stage == 3)
        {
            flashlight.UpdateLightRadius(0.7f);
        }
        // 8스테이지
        else if (_stage == 7)
        {
            flashlight.UpdateLightRadius(0.4f);
        }
    }
    
    private IEnumerator CheckDefeat()
    {
        Debug.Log("괴물 처치 체크 중");
        yield return new WaitUntil(() => stageProps[_stage].shadowMonster.GetIsDefeated());
        OnDefeatShadowMonster();
    }
    
    private void OnDefeatShadowMonster()
    {
        stageProps[_stage].shadowMonster.gameObject.SetActive(false);
        
        RemoveCoroutine();
        Debug.Log("괴물 처치");
        // 괴물 처치
        OnStageEnd();
    }

    private IEnumerator StageUpdate()
    {
        stageProps[_stage].shadowMonster.Init();
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
        stageProps[_stage].shadowMonster.gameObject.SetActive(false);
        // 괴물 연출 후 삭제
        RemoveCoroutine();
        OnStageEnd();
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
        Debug.Log(_stage + "스테이지 종료");
        _stage++;
        _isPlaying = false;

        if (_mentality == 0)
        {
            GameOver();
        }
        else if (_stage < stageProps.Length)
        {
            OnStartStage();
        }
    }
    
    private void GameOver()
    {
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
}
