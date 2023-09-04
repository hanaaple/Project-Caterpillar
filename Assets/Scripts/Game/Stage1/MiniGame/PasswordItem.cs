using TMPro;
using UnityEngine;

namespace Game.Stage1.MiniGame
{
    public class PasswordItem : MonoBehaviour
    {
        [SerializeField] private Animator animators;
        [SerializeField] private TMP_Text text;
        
        private static readonly int SelectHash = Animator.StringToHash("Select");
        private static readonly int BlinkHash = Animator.StringToHash("Blink");

        public void Select()
        {
            animators.SetBool(SelectHash, true);
            animators.SetBool(BlinkHash, true);
        }
        
        public void DeSelect()
        {
            animators.SetBool(SelectHash, false);
            animators.SetBool(BlinkHash, false);
        }

        public void DeBlink()
        {
            animators.SetBool(BlinkHash, false);
        }
        

        public void SetText(string key)
        {
            text.text = key;
        }

        public void Remove()
        {
            animators.SetBool(SelectHash, false);
            text.text = "";
        }
    }
}