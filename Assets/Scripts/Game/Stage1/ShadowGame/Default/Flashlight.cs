using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Stage1.ShadowGame.Default
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light2D mainLight;
        [SerializeField] private Light2D subLight;
        [SerializeField] private Light2D globalLight;

        [SerializeField] private float intensity;
        
        [Range(5, 10)]
        [SerializeField] private float followSpeed;

        [SerializeField] private float originalRadius;

        [SerializeField] private float lightRadiusPercentage;

        private Vector3 _originPos;

        private void OnValidate()
        {
            UpdateLight();
            SetFlashLightPos(mainLight.transform.position);
        }
        
        private void Update()
        {
            UpdateLight();
            SetFlashLightPos(mainLight.transform.position);
        }

        public void Init()
        {
            _originPos = mainLight.transform.position;
            originalRadius = mainLight.pointLightOuterRadius;
            
            SetLightRadiusPercentage(1f);
        }

        public void MoveFlashLight(Vector3 followPos)
        {
            followPos = Vector3.Lerp(mainLight.transform.position, followPos, Time.deltaTime * followSpeed);
            mainLight.transform.position = followPos;
    
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

        public void SetLightRadiusPercentage(float percentage)
        {
            lightRadiusPercentage = percentage;
            UpdateLight();
            SetFlashLightPos(mainLight.transform.position);
        }

        public void Reset()
        {
            SetFlashLightPos(_originPos);
            SetLightRadiusPercentage(1f);
        }
        
        private void UpdateLight()
        {
            var mainLightCollider = mainLight.GetComponent<CircleCollider2D>();
            mainLight.pointLightOuterRadius = originalRadius * lightRadiusPercentage;
            mainLightCollider.radius = originalRadius * lightRadiusPercentage;

            mainLight.intensity = intensity - globalLight.intensity;
            subLight.intensity = (intensity - globalLight.intensity) * 0.6f;
        }
    
        private void SetFlashLightPos(Vector3 followPos)
        {
            followPos = Vector3.Lerp(mainLight.transform.position, followPos, 1);
            mainLight.transform.position = followPos;
    
            subLight.transform.up = Vector2.Lerp(subLight.transform.up,
                (mainLight.transform.position - subLight.transform.position), 1);
    
    
            // 거리 계산
            subLight.pointLightOuterRadius = Vector2.Distance(mainLight.transform.position, subLight.transform.position);
        
            // 각도 계산
            // 각도 계산 시 mainLight의 scale 포함하여 계산
            // Debug.Log(2 * Mathf.Atan2(mainLight.pointLightOuterRadius, subLight.pointLightOuterRadius) * Mathf.Rad2Deg);
            var angle = 2 * Mathf.Atan2(mainLight.pointLightOuterRadius, subLight.pointLightOuterRadius) * Mathf.Rad2Deg;
            subLight.pointLightInnerAngle = angle;
            subLight.pointLightOuterAngle = angle;
        }
    }
}
