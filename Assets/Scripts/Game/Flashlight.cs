using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light2D mainLight;
    [SerializeField] private Light2D subLight;
    
    [SerializeField] private GameObject followObj;

    private void Start()
    {
        var mainLightCollider = mainLight.GetComponent<CircleCollider2D>();
        mainLightCollider.radius = mainLight.pointLightOuterRadius;
    }

    void Update()
    {
        mainLight.transform.position = followObj.transform.position;
    
        subLight.transform.up = Vector2.Lerp(subLight.transform.up,
            (mainLight.transform.position - subLight.transform.position), 0.05f);
    
    
        // 거리 계산
        subLight.pointLightOuterRadius = Vector2.Distance(mainLight.transform.position, subLight.transform.position);
        
        // 각도 계산
        // 각도 계산 시 mainLight의 scale 포함하여 계산
        // Debug.Log(2 * Mathf.Atan2(mainLight.pointLightOuterRadius, subLight.pointLightOuterRadius) * Mathf.Rad2Deg);
        var angle = 2 * Mathf.Atan2(mainLight.pointLightOuterRadius, subLight.pointLightOuterRadius) * Mathf.Rad2Deg;
        subLight.pointLightInnerAngle = angle;
        subLight.pointLightOuterAngle = angle;
    }
    
    
    public void UpdateLightRadius(float radius)
    {
        var mainLightCollider = mainLight.GetComponent<CircleCollider2D>();
        mainLight.pointLightOuterRadius = radius;
        mainLightCollider.radius = radius;
    }
}
