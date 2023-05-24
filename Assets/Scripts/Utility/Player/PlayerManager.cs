using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.InputSystem;
using Utility.Scene;

namespace Utility.Player
{
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager _instance;

        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<PlayerManager>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        [NonSerialized] public Player Player;
        private Camera _camera;
        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private float _yScreenHalfSize;
        private float _xScreenHalfSize;

        private Vector3 _input;
        private InputActions _inputActions;

        private static PlayerManager Create()
        {
            var playerManagerPrefab = Resources.Load<PlayerManager>("PlayerManager");
            return Instantiate(playerManagerPrefab);
        }

        private void OnEnable()
        {
            InputManager.PushInputAction(_inputActions);
        }

        private void OnDisable()
        {
            InputManager.PopInputAction(_inputActions);
        }

        private void Awake()
        {
            _inputActions = new InputActions(nameof(Utility.Player.Player))
            {
                OnInteract = _ =>
                {
                    if (Player != null)
                    {
                        Player.OnInteractAction?.Invoke();
                    }
                },
                OnMovePerformed = Input,
                OnMoveCanceled = Input,
                OnPause = _ => { PlayUIManager.Instance.pauseManager.onPause?.Invoke(); },
                OnActive = isActive =>
                {
                    if (!isActive)
                    {
                        _input = Vector3.zero;
                    }
                }
            };
        }

        private void Start()
        {
            _camera = Camera.main;
            var bounds = SceneHelper.Instance.boundBox.bounds;
            _minBounds = bounds.min;
            _maxBounds = bounds.max;
            if (_camera != null)
            {
                _yScreenHalfSize = _camera.orthographicSize;
                _xScreenHalfSize = _yScreenHalfSize * _camera.aspect;
            }
        }

        public void SetPlayer(Player mPlayer = null)
        {
            Debug.LogWarning($"기존: {Player}, 새로운 플레이 캐릭터: {mPlayer}");
            Player = mPlayer;
        }

        public bool IsPlayer(Player mPlayer)
        {
            return Player == mPlayer;
        }

        private void Input(InputAction.CallbackContext _)
        {
            _input = _.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            if (!Player)
            {
                return;
            }

            Vector2 input = _input;
            switch (SceneHelper.Instance.playerMoveType)
            {
                case PlayerMoveType.Vertical:
                    input.x = 0;
                    break;
                case PlayerMoveType.Horizontal:
                    input.y = 0;
                    break;
                case PlayerMoveType.None:
                    input = Vector2.zero;
                    break;
            }

            Player.Move(input);
            CameraMove();
        }

        private void CameraMove()
        {
            if (!Player || !SceneHelper.Instance.isCameraMove)
            {
                return;
            }

            var targetPos = Player.transform.position;

            var cameraTransform = _camera.transform;
            // targetPos = new Vector3(targetPos.x, targetPos.y, cameraTransform.position.z);

            // cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, cameraSpeed * Time.fixedDeltaTime);

            float clampX, clampY;

            if (_maxBounds.x - _xScreenHalfSize > 0)
            {
                clampX = Mathf.Clamp(targetPos.x, _minBounds.x + _xScreenHalfSize,
                    _maxBounds.x - _xScreenHalfSize);
            }
            else
            {
                clampX = Mathf.Clamp(targetPos.x, 0, 0);
            }

            if (_maxBounds.y - _yScreenHalfSize > 0)
            {
                clampY = Mathf.Clamp(targetPos.y, _minBounds.y + _yScreenHalfSize,
                    _maxBounds.y - _yScreenHalfSize);
            }
            else
            {
                clampY = Mathf.Clamp(targetPos.y, 0, 0);
            }

            cameraTransform.position = new Vector3(clampX, clampY, cameraTransform.position.z);
        }
    }
}