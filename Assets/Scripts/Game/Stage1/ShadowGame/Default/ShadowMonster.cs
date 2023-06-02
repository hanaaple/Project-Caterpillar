using System;
using System.Collections;
using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.ShadowGame.Default
{
    public class ShadowMonster : MonoBehaviour
    {
        public Animator monsterAnimator;
        
        [SerializeField] private CircleCollider2D monsterCollider;
        [SerializeField] private CircleCollider2D flashLightCollider;
        
        [SerializeField] private float judgmentSec;
        [SerializeField] private float judgmentPercentage;  // 0.5

        private float _attackedTime;
        private bool _isEnable;
        
        private static readonly int StageHash = Animator.StringToHash("Stage");
        private static readonly int AppearHash = Animator.StringToHash("Appear");
        private static readonly int AttackedSecHash = Animator.StringToHash("AttackedSec");
        private static readonly int DisappearHash = Animator.StringToHash("Disappear");

        public void Appear(int stageIndex)
        {
            gameObject.SetActive(true);
            monsterAnimator.SetFloat(AttackedSecHash, 0);
            _attackedTime = 0;
            monsterAnimator.SetInteger(StageHash, stageIndex);
            monsterAnimator.SetTrigger(AppearHash);
        }
        
        public void Attack()
        {
            SetActive(false);

            StartCoroutine(DisappearCoroutine());
        }

        // Defeat Animation 타이밍 조정 필요 -> 미사용?
        public void Defeat(Action onDefeatStart, Action onDisappearEnd)
        {
            SetActive(false);
            onDefeatStart?.Invoke();
            StartCoroutine(DisappearCoroutine(onDisappearEnd));
        }

        private IEnumerator DisappearCoroutine(Action onDisappearEnd = null)
        {
            monsterAnimator.SetTrigger(DisappearHash);
            yield return null;
            yield return new WaitUntil(() => monsterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"));
            
            onDisappearEnd?.Invoke();
        }

        private void Update()
        {
            if (!_isEnable)
            {
                return;
            }

            var a = monsterCollider.radius; // 몬스터 원의 반지름
            var b = flashLightCollider.radius; // 손전등 원의 반지름
            var d = Vector2.Distance(flashLightCollider.transform.position, transform.position); // 두 원 사이의 거리

            float percentage;
            if (d >= a + b)
            {
                percentage = 0;
            }
            else if (Mathf.Abs(a - b) >= d) // 큰원에 작은원이 포함된 경우
            {
                percentage = 1;
            }
            else
            {
                var thetaA = Mathf.Acos((Mathf.Pow(a, 2) + Mathf.Pow(d, 2) - Mathf.Pow(b, 2)) / (2 * a * d)) * 2;
                var thetaB = Mathf.Acos((Mathf.Pow(b, 2) + Mathf.Pow(d, 2) - Mathf.Pow(a, 2)) / (2 * b * d)) * 2;


                var s1 = Mathf.Pow(a, 2) * (thetaA - Mathf.Sin(thetaA)) / 2;
                var s2 = Mathf.Pow(b, 2) * (thetaB - Mathf.Sin(thetaB)) / 2;
                var s = s1 + s2;
                var monsterS = Mathf.Pow(a, 2) * Mathf.PI;
                // Debug.Log("전체 넓이: " + monsterS + ", 겹친 넓이: " + S);
                // float target = target r ^ 2 * PI;
                percentage = s / monsterS;
                percentage = Mathf.Clamp(percentage, 0, 1);
            }
            
            if (percentage >= judgmentPercentage)
            {
                _attackedTime += Time.deltaTime;
            }
            else
            {
                _attackedTime = Mathf.Clamp(_attackedTime - Time.deltaTime, 0f, judgmentSec);
            }
            
            monsterAnimator.SetFloat(AttackedSecHash, _attackedTime / judgmentSec);
        }
        
        public void PlayOneShot(AudioClip audioClip)
        {
            AudioManager.PlaySfx(audioClip);
        }

        public void SetActive(bool isActive)
        {
            // Defeat 
            _isEnable = isActive;
        }
        
        public bool GetIsDefeated()
        {
            return _attackedTime >= judgmentSec;
        }
    }
}