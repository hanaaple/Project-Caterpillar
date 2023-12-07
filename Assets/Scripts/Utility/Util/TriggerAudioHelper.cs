using UnityEngine;
using Utility.Audio;
using Utility.Core;

namespace Utility.Util
{
    public sealed class TriggerAudioHelper : MonoBehaviour
    {
        [SerializeField] private Trigger[] triggers;
        [SerializeField] private AudioData audioData;

        private int _triggerCount;

        [SerializeField] private bool isPlayOnDialogue;
        [SerializeField] private bool isDebug;

        private void Start()
        {
            foreach (var trigger in triggers)
            {
                trigger.onTriggerEnter2D = () =>
                {
                    _triggerCount++;
                    if (isDebug)
                    {
                        Debug.LogWarning($"Enter {_triggerCount}  {trigger.gameObject}, {audioData}");
                    }

                    if (!PlayUIManager.Instance.dialogueController.IsDialogue() || isPlayOnDialogue)
                    {
                        audioData.Play();   
                    }
                };

                if ((audioData.audioSourceType == AudioSourceType.Sfx && audioData.isLoop) || audioData.audioSourceType == AudioSourceType.Bgm)
                {
                    trigger.onTriggerExit2D = () =>
                    {
                        _triggerCount--;
                        
                        if (isDebug)
                        {
                            Debug.LogWarning($"Exit {_triggerCount}  {trigger.gameObject}, {audioData}");
                        }
                        
                        if (_triggerCount > 0)
                        {
                            return;
                        }

                        audioData.Stop();
                    };
                }
            }
        }
    }
}