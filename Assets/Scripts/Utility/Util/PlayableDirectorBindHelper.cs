using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Audio;
using Utility.Core;
using Utility.Scene;

namespace Utility.Util
{
    public class PlayableDirectorBindHelper : MonoBehaviour
    {
        [SerializeField] private PlayableDirector[] playableDirectors;

        private void Start()
        {
            foreach (var playableDirector in playableDirectors)
            {
                var tracks = (playableDirector.playableAsset as TimelineAsset)?.GetOutputTracks()
                    .Where(item => item is AnimationTrack or ActivationTrack or AudioTrack);
                if (tracks == null)
                {
                    return;
                }

                foreach (var temp in tracks)
                {
                    UnityEngine.Object bindObject = temp switch
                    {
                        AnimationTrack => SceneHelper.Instance.GetBindObject<Animator>(temp.name),
                        ActivationTrack => SceneHelper.Instance.GetBindObject<GameObject>(temp.name),
                        AudioTrack => AudioManager.Instance.GetAudioSource(temp.name),
                        _ => null
                    };

                    if (bindObject)
                    {
                        playableDirector.SetGenericBinding(temp, bindObject);
                    }
                    else
                    {
                        playableDirector.SetGenericBinding(temp,
                            PlayUIManager.Instance.dialogueController.defaultCutSceneAnimator);
                    }
                }
            }
        }
    }
}
