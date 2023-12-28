using UnityEngine;

namespace Utility.Util
{
    /// <summary>
    /// This class does not work as bound
    /// only work by two field (offset, size)
    /// </summary>
    public class CameraRange2D : MonoBehaviour
    {
        [SerializeField] private Vector2 offset;
        [SerializeField] private Vector2 size;

#if UNITY_EDITOR
        [SerializeField] private Color color;
#endif
        public Vector2 Max => offset + Extent;
        public Vector2 Min => offset - Extent;
        
        private Vector2 Extent => size * .5f;
        private Vector2 LeftTop => offset + new Vector2(-Extent.x, Extent.y);
        private Vector2 RightBottom => offset + new Vector2(Extent.x, -Extent.y);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawLine(Min, LeftTop);
            Gizmos.DrawLine(Min, RightBottom);
            Gizmos.DrawLine(LeftTop, Max);
            Gizmos.DrawLine(RightBottom, Max);
        }
#endif
    }
}