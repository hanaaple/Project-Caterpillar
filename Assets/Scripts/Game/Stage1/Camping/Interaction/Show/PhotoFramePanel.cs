using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class PhotoFramePanel : PagePanel
    {
        [SerializeField] private int targetIndex;
        [SerializeField] private AudioClip audioClip;
        
        protected override void SetPage(int changeValue)
        {
            base.SetPage(changeValue);

            if (Index == targetIndex)
            {
                AudioManager.Instance.PlaySfx(audioClip);
            }
        }
    }
}