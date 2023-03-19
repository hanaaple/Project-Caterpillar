using UnityEngine;
using UnityEngine.InputSystem;
using Utility.Core;
using Utility.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public Vector3 input;

    [Range(1, 20f)] public float playerSpeed;

    [Range(1, 20f)] public float cameraSpeed;

    public BoxCollider2D boundBox;


    private Camera _camera;

    private Vector3 _minBounds;
    private Vector3 _maxBounds;

    private float _yScreenHalfSize;
    private float _xScreenHalfSize;

    private Animator _animator;
    private static readonly int IsMove = Animator.StringToHash("isMove");
    
    private bool _wasPositive;

    private void OnEnable()
    {
        InputManager.SetPlayerAction(true);
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Move.performed += Input;
        playerActions.Move.canceled += Input;
    }

    private void OnDisable()
    {
        InputManager.SetPlayerAction(false);
        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Move.performed -= Input;
        playerActions.Move.canceled -= Input;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main;
        _minBounds = boundBox.bounds.min;
        _maxBounds = boundBox.bounds.max;
        _yScreenHalfSize = _camera.orthographicSize;
        _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;

        if (transform.localScale.x > 0)
        {
            _wasPositive = true;
        }
    }

    private void FixedUpdate()
    {
        if (input == Vector3.zero || !GameManager.Instance.IsCharacterControlEnable())
        {
            if (_animator.GetBool(IsMove))
            {
                _animator.SetBool(IsMove, false);
            }
            return;
        }
        
        if (!_animator.GetBool(IsMove))
        {
            _animator.SetBool(IsMove, true);
        }
        
        if (!_wasPositive && input.x > 0)
        {
            var scale = transform.localScale;
            scale.x = scale.x * -1;
            transform.localScale = scale;
            
            _wasPositive = true;
        }
        else if (_wasPositive && input.x < 0)
        {
            var scale = transform.localScale;
            scale.x = scale.x * -1;
            transform.localScale = scale;

            _wasPositive = false;
        }

        CharacterMove();
        CameraMove();
    }

    private void CharacterMove()
    {
        transform.Translate(input * playerSpeed * Time.fixedDeltaTime);
    }

    private void CameraMove()
    {
        var cameraTransform = _camera.transform;
        var targetPos = transform.position;
        // targetPos = new Vector3(targetPos.x, targetPos.y, cameraTransform.position.z);

        // cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraSpeed * Time.fixedDeltaTime);

        float clampX = targetPos.x;
        float clampY = targetPos.y;

        if (_maxBounds.x - _xScreenHalfSize > 0)
        {
            clampX = Mathf.Clamp(targetPos.x, _minBounds.x + _xScreenHalfSize,
                _maxBounds.x - _xScreenHalfSize);
        }

        if (_maxBounds.y - _yScreenHalfSize > 0)
        {
            clampY = Mathf.Clamp(targetPos.y, _minBounds.y + _yScreenHalfSize,
                _maxBounds.y - _yScreenHalfSize);
        }

        cameraTransform.position = new Vector3(clampX, clampY, cameraTransform.position.z);
    }

    private void Input(InputAction.CallbackContext _)
    {
        input = _.ReadValue<Vector2>();
    }
}