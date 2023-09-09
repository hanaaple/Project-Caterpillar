using UnityEngine;
using Utility.Core;

namespace Utility.Util
{
    public class SignalReceiveInterface : MonoBehaviour
    {
        public void FocusLetterBox()
        {
            PlayUIManager.Instance.dialogueController.SetFocusMode(false);
        }
    }
}
