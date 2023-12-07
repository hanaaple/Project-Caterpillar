using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class PhotoFramePanel : PagePanel
    {
        [SerializeField] private int targetIndex;
        [SerializeField] private AudioData audioData;
        
        protected override void SetPage(int changeValue)
        {
            base.SetPage(changeValue);

            if (Index == targetIndex)
            {
                audioData.Play();
            }
        }
    }
}