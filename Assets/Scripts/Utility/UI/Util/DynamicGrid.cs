using UnityEngine;
using UnityEngine.UI;

namespace Utility.UI.Util
{
    public class DynamicGrid : MonoBehaviour
    {
        public void UpdateRectSize()
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var grid = gameObject.GetComponent<GridLayoutGroup>();
            
            var rows = grid.transform.childCount / grid.constraintCount;
            
            if (grid.transform.childCount % 2 != 0)
            {
                rows++;
            }
            
            var height = grid.padding.top + grid.padding.bottom + grid.spacing.y * (rows - 1) + grid.cellSize.y * rows;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }
}