using UnityEngine;

public class ShadowMonster : MonoBehaviour
{
    private CircleCollider2D _circleCollider2D;

    private void Start()
    {
        _circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var circleCollider2D = other.GetComponent<CircleCollider2D>();
        var a = _circleCollider2D.radius; //원1의 반지
        var b = circleCollider2D.radius; //원2의 반지름
        var d = Vector2.Distance(circleCollider2D.transform.position, transform.position); // 두 원 사이의 거리

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
            Debug.Log("전체 넓이: " + monsterS + ", 겹친 넓이: " + S);
            // float target = target r ^ 2 * PI;
            percantage = S / monsterS;
        }

        Debug.Log("0 ~ 1에서 퍼센트: " + percantage);
    }
}
