using System;
using UnityEngine;

namespace Game.Camping
{
    public class Squirrel : CampingInteraction
    {
        [SerializeField]
        private DragItem egg;

        [SerializeField] private SpriteRenderer bird;
        [SerializeField] private Sprite birdDefault;
        [SerializeField] private Sprite birdClear;

        [SerializeField]
        private Animator note;
        
        [SerializeField]
        private DragItem[] apples;

        [SerializeField]
        private Collider2D squirrelCollider;
        
        [SerializeField]
        private Animator squirrel;
        
        private void Start()
        {
            egg.onFire = () =>
            {
                egg.gameObject.SetActive(false);
                bird.sprite = birdClear;
                note.SetBool("Fall", true);
            };
            foreach (var apple in apples)
            {
                apple.onFire = () =>
                {
                    Debug.Log("하이");
                    apple.gameObject.SetActive(false);
                    foreach (var dragItem in apples)
                    {
                        dragItem.GetComponent<Collider2D>().enabled = false;
                    }

                    squirrel.SetBool("Eat", true);
                    Appear();
                    squirrelCollider.enabled = false;
                };
            }
        }

        public override void Appear()
        {
            onAppear?.Invoke();
        }

        public override void Reset()
        {
            note.enabled = true;
            note.GetComponent<ShowInteractor>().setInteractable = setInteractable;
            egg.Reset();
            foreach (var apple in apples)
            {
                apple.Reset();
            }
            bird.sprite = birdDefault;
            squirrelCollider.enabled = true;
            note.SetBool("Fall", false);
            squirrel.SetBool("Eat", false);
        }
    }
}
