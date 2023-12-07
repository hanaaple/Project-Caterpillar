using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Stage1.Camping.Interaction.Show
{
    public class ShowPanel : MonoBehaviour
    {
        public Button exitButton;
        
        public Action onShow;

        public virtual void Initialize()
        {
            
        }

        public virtual void Show()
        {
            onShow?.Invoke();
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void ResetPanel()
        {
            gameObject.SetActive(false);
        }
    }
}