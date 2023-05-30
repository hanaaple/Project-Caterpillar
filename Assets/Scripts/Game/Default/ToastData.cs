using System;
using UnityEngine;

namespace Game.Default
{
    [Serializable]
    public class ToastData
    {
        [TextArea] public string[] toastContents;

        [NonSerialized] public bool isToasted;
    }
}