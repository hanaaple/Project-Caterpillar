using TMPro;
using UnityEngine;

namespace Game.Stage1.MiniGame
{
    public class PasswordItem : MonoBehaviour
    {
        [SerializeField] private Animator animators;
        [SerializeField] private TMP_Text text;
        
        private static readonly int IsEmptyHash = Animator.StringToHash("IsEmpty");

        public void Select()
        {
            animators.SetBool(IsEmptyHash, false);
        }
        
        public void DeSelect()
        {
            animators.SetBool(IsEmptyHash, true);
        }

        public void SetText(string key)
        {
            text.text = key;
        }

        public void Remove()
        {
            animators.SetBool(IsEmptyHash, true);
            text.text = "";
        }
    }
}