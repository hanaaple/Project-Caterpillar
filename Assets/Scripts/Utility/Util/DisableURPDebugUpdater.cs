using UnityEngine;
using UnityEngine.Rendering;

namespace Utility.Util
{
    public class DisableURPDebugUpdater : MonoBehaviour
    {
        private void Awake()
        {
            DebugManager.instance.enableRuntimeUI = false;
        }
    }
}
