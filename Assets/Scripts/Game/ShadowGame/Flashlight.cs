using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.ShadowGame
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light2D mainLight;
        [SerializeField] private Light2D subLight;

        [Range(5, 10)]
        [SerializeField] private float followSpeed;

        private float _originalRadius;

        private Vector3 _originPos;

        public void Init()
        {
            _originPos = mainLight.transform.position;
            var mainLightCollider = mainLight.GetComponent<CircleCollider2D>();
            mainLightCollider.radius = mainLight.pointLightOuterRadius;
            _originalRadius = mainLight.pointLightOuterRadius;
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

        public void UpdateLightRadius(float percantage)
        {
            var mainLightCollider = mainLight.GetComponent<CircleCollider2D>();
            mainLight.pointLightOuterRadius = _originalRadius * percantage;
            mainLightCollider.radius = _originalRadius * percantage;
        }

        public void Reset()
        {
            TeleportFlashLight(_originPos);
            UpdateLightRadius(1f);
        }
    
        private void TeleportFlashLight(Vector3 followPos)
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
