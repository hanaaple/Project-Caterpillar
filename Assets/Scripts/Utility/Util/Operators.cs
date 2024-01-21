using UnityEngine;

namespace Utility.Util
{
    public static class Operators
    {
        public static Vector2 WindowToCanvasVector2 => new Vector2(1920, 1080) / new Vector2(Screen.width, Screen.height);
    }
}