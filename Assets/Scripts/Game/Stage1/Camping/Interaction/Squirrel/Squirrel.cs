using UnityEngine;
using Utility.Audio;

namespace Game.Stage1.Camping.Interaction.Squirrel
{
    public class Squirrel : CampingInteraction
    {
        // [SerializeField] private Animator snakeAnimator;
        [SerializeField] private Animator birdAnimator;
        [SerializeField] private Animator noteAnimator;
        [SerializeField] private Animator squirrelAnimator;
        
        [SerializeField] private DragItem egg;
        [SerializeField] private DragItem[] apples;
        
        [SerializeField] private AudioData takeAppleAudioData;
        
        private static readonly int ClearHash = Animator.StringToHash("Clear");

        private void Start()
        {
            egg.onFire = () =>
            {
                egg.gameObject.SetActive(false);
                // snakeAnimator.Set
                birdAnimator.SetBool(ClearHash, true);
                noteAnimator.SetBool(ClearHash, true);
                
            };
            
            foreach (var apple in apples)
            {
                apple.onTake = () =>
                {
                    takeAppleAudioData.Play();
                };
                apple.onFire = () =>
                {
                    apple.gameObject.SetActive(false);
                    foreach (var t in apples)
                    {
                        t.Collider2D.enabled = false;
                    }

                    squirrelAnimator.SetBool(ClearHash, true);
                    Appear();
                };
            }
        }

        public override void ResetInteraction(bool isGameReset = false)
        {
            base.ResetInteraction(isGameReset);
            
            egg.ResetDragItem();
            foreach (var apple in apples)
            {
                apple.ResetDragItem();
            }

            birdAnimator.SetBool(ClearHash, false);
            noteAnimator.SetBool(ClearHash, false);
            squirrelAnimator.SetBool(ClearHash, false);
        }
    }
}