using System;
using System.Linq;
using UnityEngine;
using Utility.Tendency;

namespace Utility.Interaction
{
    public class TendencyNpc : MonoBehaviour
    {
        [Serializable]
        public class NpcInteraction
        {
            public InputInteraction interaction;
            public int diff;
        }

        [SerializeField] private int ascent;
        [SerializeField] private int active;
        public NpcInteraction[] npcInteractions;

        private void OnEnable()
        {
            TendencyManager.Instance.OnTendencyUpdate += UpdateInteraction; 
            UpdateInteraction();
        }
        
        private void OnDisable()
        {
            TendencyManager.Instance.OnTendencyUpdate -= UpdateInteraction;
        }
      
        private void UpdateInteraction()
        {
            foreach (var npcInteraction in npcInteractions)
            {
                npcInteraction.interaction.gameObject.SetActive(false);
            }
            
            // Get Percentage
            var tendencyData = TendencyManager.Instance.GetTendencyData();

            var ascentData = tendencyData.ascent - tendencyData.descent;
            var activeData = tendencyData.activation - tendencyData.inactive;

            var diff = Mathf.Abs(ascentData - ascent) + Mathf.Abs(activeData - active);


            var a = npcInteractions.Count(item => item.diff < diff);
            if (a == 0)
            {
                var first = npcInteractions.OrderBy(item => item.diff).Last();
                first.interaction.gameObject.SetActive(true);
            }
            else
            {
                var target = npcInteractions.Where(item => item.diff > diff).OrderBy(item => item.diff).First();
                target.interaction.gameObject.SetActive(true);
            }
        }
    }
}