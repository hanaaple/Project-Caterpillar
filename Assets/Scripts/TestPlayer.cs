using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public Vector3 input;

    [Range(1, 20f)]
    public float playerSpeed;
    
    [Range(1, 20f)]
    public float cameraSpeed;
  
    public BoxCollider2D boundBox;
    

    private Camera _camera;
    
    private Vector3 _minBounds;
    private Vector3 _maxBounds;
    
    private float _yScreenHalfSize;
    private float _xScreenHalfSize;
    
    void Start()
    {
        _camera = Camera.main;
        _minBounds = boundBox.bounds.min;
        _maxBounds = boundBox.bounds.max;
        _yScreenHalfSize = _camera.orthographicSize;
        _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;

        var playerActions = InputManager.inputControl.PlayerActions;
        playerActions.Enable();
        playerActions.Move.performed += delegate(InputAction.CallbackContext context)
        {
            input = context.ReadValue<Vector2>();
        };
        
        playerActions.Move.canceled += delegate(InputAction.CallbackContext context)
        {
            input = context.ReadValue<Vector2>();
        };
    }

    private void FixedUpdate()
    {
        if (input != Vector3.zero)
        {
            CharacterMove();
            CameraMove();
        }
    }

    private void Update()
    {
        CameraMove();
    }

    private void CharacterMove()
    {
        transform.Translate(input * playerSpeed * Time.fixedDeltaTime);
    }

    void CameraMove()
    {
        var cameraTransform = _camera.transform;
        var targetPos = transform.position;
        targetPos = new Vector3(targetPos.x, targetPos.y, cameraTransform.position.z);
        
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
