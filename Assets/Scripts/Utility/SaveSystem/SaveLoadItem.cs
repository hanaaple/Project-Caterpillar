using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.SaveSystem
{
    public class SaveLoadItem : MonoBehaviour
    {
        public Button deleteButton;
        
        public TMP_Text text;
        
        public bool isEmpty;
        
        [NonSerialized] public Animator Animator;
    }
}
