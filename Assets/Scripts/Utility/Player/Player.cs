using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utility.Player
{
    public class Player : MonoBehaviour
    {
        public bool IsCharacterControllable
        {
            get => isCharacterControllable;
            set
            {
                isCharacterControllable = value;

                if (!Application.isPlaying || !gameObject.activeSelf)
                {
                    return;
                }

                UpdateControl();
            }
        }

        /// <summary>
        /// Do not change In Animation
        /// </summary>
        [SerializeField] private bool isCharacterControllable;

        [SerializeField] private bool frontIsRight;

        [Header("Setting")] [Range(1, 20f)] [SerializeField]
        private float playerSpeed;

        internal Action OnInteractAction;

        private Animator _animator;
        private Action<InputAction.CallbackContext> _onInteract;

        private readonly int _isMove = Animator.StringToHash("isMove");

        // This function not work on animation
        private void OnValidate()
        {
            if (!Application.isPlaying || !gameObject.activeSelf)
            {
                return;
            }

            UpdateControl();
        }

        private void UpdateControl(bool isEnable = true)
        {
            if (!isEnable)
            {
                if (PlayerManager.Instance.IsPlayer(this))
                {
                    PlayerManager.Instance.SetPlayer();
                }

                return;
            }

            if (isCharacterControllable)
            {
                if (PlayerManager.Instance.IsPlayer(this))
                {
                    return;
                }

                PlayerManager.Instance.SetPlayer(this);
                // True
            }
            else
            {
                if (PlayerManager.Instance.IsPlayer(this))
                {
                    PlayerManager.Instance.SetPlayer();
                    // False
                }
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();

            UpdateControl();
        }

        public void Move(Vector2 input)
        {
            RotateCharacter(input);
            SetCharacterAnimator(input != Vector2.zero);
            MoveCharacter(input);
        }

        public void RotateCharacter(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                return;
            }
            
            // Debug.Log($"이동: {input}");

            if (input.x < 0)
            {
                var scale = transform.localScale;
                scale.x = frontIsRight ? -1 : 1;
                SetScale(scale);
            }
            else if (input.x > 0)
            {
                var scale = transform.localScale;
                scale.x = frontIsRight ? 1 : -1;
                SetScale(scale);
            }
        }

        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void SetCharacterAnimator(bool isInput)
        {
            if (!isInput)
            {
                if (_animator.GetBool(_isMove))
                {
                    _animator.SetBool(_isMove, false);
                }
            }
            else if (!_animator.GetBool(_isMove))
            {
                _animator.SetBool(_isMove, true);
            }
        }

        private void MoveCharacter(Vector2 input)
        {
            transform.Translate(input * (playerSpeed * Time.fixedDeltaTime));
        }

        private void OnEnable()
        {
            UpdateControl();
        }

        private void OnDisable()
        {
            if (isActiveAndEnabled)
            {
                UpdateControl(false);
            }
        }
    }
}