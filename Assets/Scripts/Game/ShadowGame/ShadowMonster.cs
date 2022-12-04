using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ShadowMonster : MonoBehaviour
{
    [SerializeField] private CircleCollider2D circleCollider2D;
    [SerializeField] private Animator animator;

    [Header("커튼")] [SerializeField] private SpriteRenderer[] curtain;
    [SerializeField] private Sprite[] curtainSprites;
    
    
    private float _damagedTime;
    private float _judgmentTime;

    private float _judgmentPercantage;

    private CircleCollider2D _otherCollider;
    
    public void Appear(int stageIndex)
    {
        gameObject.SetActive(true);
        
        _judgmentTime = 3;
        _damagedTime = 0;
        _judgmentPercantage = 0.5f;
        animator.SetInteger("Stage", stageIndex);
        animator.SetTrigger("Appear");
    }

    public void OpenCurtain()
    {
        curtain[0].sprite = curtainSprites[0];
        curtain[1].sprite = curtainSprites[1];
    }

    public bool GetIsDefeated()
    {
        return _damagedTime >= _judgmentTime;
    }

    public void Attack(UnityAction unityAction)
    {
        StartCoroutine(DefeatCoroutine(unityAction));
    }
    
    public void Defeat(UnityAction unityAction)
    {
        StartCoroutine(DefeatCoroutine(unityAction));
    }

    private IEnumerator DefeatCoroutine(UnityAction unityAction)
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("Defeat");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Default"));
        animator.applyRootMotion = false;
        unityAction();
    }

    void Update()
    {
        if (_otherCollider)
        {
            var a = circleCollider2D.radius; //원1의 반지
            var b = _otherCollider.radius; //원2의 반지름
            var d = Vector2.Distance(_otherCollider.transform.position, transform.position); // 두 원 사이의 거리

            float percantage = 0f;
            if (d >= a + b)
            {
                percantage = 0;
            }
            else if (Mathf.Abs(a - b) >= d) ///큰원에 작은원이 포함된 경우
            {
                percantage = 1;
            }
            else
            {
                float thetaA = Mathf.Acos((Mathf.Pow(a, 2) + Mathf.Pow(d, 2) - Mathf.Pow(b, 2)) / (2 * a * d)) * 2;
                float thetaB = Mathf.Acos((Mathf.Pow(b, 2) + Mathf.Pow(d, 2) - Mathf.Pow(a, 2)) / (2 * b * d)) * 2;


                var s1 = Mathf.Pow(a, 2) * (thetaA - Mathf.Sin(thetaA)) / 2;
                var s2 = Mathf.Pow(b, 2) * (thetaB - Mathf.Sin(thetaB)) / 2;
                var S = s1 + s2;
                float monsterS = Mathf.Pow(a, 2) * Mathf.PI;
                // Debug.Log("전체 넓이: " + monsterS + ", 겹친 넓이: " + S);
                // float target = target r ^ 2 * PI;
                percantage = S / monsterS;
                percantage = Mathf.Clamp(percantage, 0, 1);
            }

            // Debug.Log("0 ~ 1에서 퍼센트: " + percantage);
            Debug.Log(_damagedTime/_judgmentTime);
            if (percantage >= _judgmentPercantage)
            {
                _damagedTime += Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        _otherCollider = other.GetComponent<CircleCollider2D>();
    }  
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        _otherCollider = null;
    }
}
