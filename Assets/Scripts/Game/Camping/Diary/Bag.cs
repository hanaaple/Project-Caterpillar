using UnityEngine;

namespace Game.Camping
{
    public class Bag : MonoBehaviour
    {
        [SerializeField] private GameObject diary;
        private void OnMouseDown()
        {
            diary.SetActive(!diary.activeSelf);
        }

        public void OnFire()
        {
            GetComponent<Collider2D>().enabled = false;
        }
        
        public void Reset()
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }
}
