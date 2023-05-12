using System;
using UnityEngine;

namespace Game.Camping
{
    public class Bag : MonoBehaviour
    {
        [SerializeField] private GameObject diary;
        [SerializeField] private GameObject diaryMask;

        private SpriteRenderer _spriteRenderer;
        
        [SerializeField] private Sprite openBag;
        [SerializeField] private Sprite closedBag;

        [NonSerialized] public bool isOut;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnMouseDown()
        {
            _spriteRenderer.sprite = diaryMask.activeSelf ? closedBag : openBag;
            if (!isOut)
            {
                diary.SetActive(!diary.activeSelf);
            }
            diaryMask.SetActive(!diaryMask.activeSelf);
        }

        public void OnFire()
        {
            // diaryMask.
            //GetComponent<Collider2D>().enabled = false;
        }
        
        public void Reset()
        {
            isOut = false;
            //GetComponent<Collider2D>().enabled = true;
        }
    }
}
