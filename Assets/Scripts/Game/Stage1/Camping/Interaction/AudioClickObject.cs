using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction
{
    public class AudioClickObject : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private AudioData audioData;
        [SerializeField] private bool isLoop;

        private bool _isPlayed;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isLoop && _isPlayed)
            {
                return;
            }

            _isPlayed = true;
            
            audioData.Play();
        }
    }
}