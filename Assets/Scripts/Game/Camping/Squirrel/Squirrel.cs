using UnityEngine;

namespace Game.Camping
{
    public class Squirrel : CampingInteraction
    {
        [SerializeField]
        private DragItem egg;

        [SerializeField]
        private Animator note;
        
        [SerializeField]
        private DragItem apple;

        [SerializeField]
        private Animator squirrel;
        
        private void Start()
        {
            egg.onFire = () =>
            {
                egg.gameObject.SetActive(false);
                note.SetBool("Fall", true);
            };
            apple.onFire = () =>
            {
                apple.gameObject.SetActive(false);
                squirrel.SetBool("Eat", true);
                Appear();
            };
        }

        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void Reset()
        {
            note.enabled = true;
            note.GetComponent<ShowInteractor>().setEnable = setEnable;
            egg.Reset();
            apple.Reset();
            note.SetBool("Fall", false);
            squirrel.SetBool("Eat", false);
        }
    }
}
