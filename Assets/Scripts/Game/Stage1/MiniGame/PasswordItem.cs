using TMPro;
using UnityEngine;

namespace Game.Stage1.MiniGame
{
    public class PasswordItem : MonoBehaviour
    {
        [SerializeField] private Animator animators;
        [SerializeField] private TMP_Text text;
        
        private static readonly int SelectHash = Animator.StringToHash("Select");

        public void Select()
        {
            animators.SetBool(SelectHash, true);
        }
        
        public void DeSelect()
        {
            animators.SetBool(SelectHash, false);
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