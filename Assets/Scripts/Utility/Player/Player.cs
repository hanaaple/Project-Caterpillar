using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Utility.Audio;

namespace Utility.Player
{
    public class Player : MonoBehaviour
    {
        [Serializable]
        public struct StepProps
        {
            public AudioClip audioClip;
            public float timeInterval;
        }
        
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

        [SerializeField] private float stepOffset;
        [SerializeField] private StepProps[] stepProps;
        
        [Range(0, 1)] [SerializeField] private float volume;

        private Animator _animator;
        private int _stepIndex;
        private bool _isStepAudioPlaying;
        private float _stepOffsetTimer;

        private readonly int _isMove = Animator.StringToHash("isMove");

        // This function not work on animation
        // private void OnValidate()
        // {
        //     if (!Application.isPlaying || !gameObject.activeSelf)
        //     {
        //         return;
        //     }
        //
        //     UpdateControl();
        // }

        private void UpdateControl(bool isEnable = true)
        {
            if (!isEnable)
            {
                if (PlayerManager.Instance.IsPlayer(this))
                {
                    PlayerManager.Instance.SetPlayer(null);
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
                    PlayerManager.Instance.SetPlayer(null);
                    // False
                }
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            UpdateControl();
        }

        public void Move(Vector2 input)
        {
            RotateCharacter(input);
            SetCharacterAnimator(input != Vector2.zero);
            MoveCharacter(input);
            if (input != Vector2.zero)
            {
                if (stepOffset > _stepOffsetTimer)
                {
                    _stepOffsetTimer += Time.fixedDeltaTime;
                }
                else
                {
                    PlaySfx();
                }
            }
            else
            {
                _stepOffsetTimer = 0;
            }
        }

        public void RotateCharacter(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                return;
            }

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

        private void PlaySfx()
        {
            if (_isStepAudioPlaying || stepProps.Any(item => AudioManager.Instance.IsPlayingSfx(item.audioClip)))
            {
                return;
            }
            
            // Animator에서 하느냐 여기서 offset을 추가할 수 있게 하느냐
            
            _isStepAudioPlaying = true;
            
            var step = stepProps[_stepIndex];
            AudioManager.Instance.PlaySfx(step.audioClip, volume, false);
            
            StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            yield return stepProps[_stepIndex].audioClip.length + stepProps[_stepIndex].timeInterval; 
            _stepIndex = (_stepIndex + 1) % stepProps.Length;
            _isStepAudioPlaying = false;
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