using UnityEngine;

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
        
        private static readonly int ClearHash = Animator.StringToHash("Clear");

        private void Start()
        {
            egg.OnFire = () =>
            {
                egg.gameObject.SetActive(false);
                // snakeAnimator.Set
                birdAnimator.SetBool(ClearHash, true);
                noteAnimator.SetBool(ClearHash, true);
            };
            
            foreach (var apple in apples)
            {
                apple.OnFire = () =>
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

        public override void ResetInteraction()
        {
            base.ResetInteraction();
            
            egg.Reset();
            foreach (var apple in apples)
            {
                apple.Reset();
            }

            birdAnimator.SetBool(ClearHash, false);
            noteAnimator.SetBool(ClearHash, false);
            squirrelAnimator.SetBool(ClearHash, false);
        }
    }
}