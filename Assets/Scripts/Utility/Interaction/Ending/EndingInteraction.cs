using System;
using System.Linq;
using UnityEngine;
using Utility.Core;
using Utility.Tendency;

namespace Utility.Interaction.Ending
{
    public class EndingInteraction : MonoBehaviour
    {
        [Serializable]
        private class Ending
        {
            public EndingType endingType;
            public Interaction interaction;
        }

        [SerializeField] private Ending[] endings;

        private void Awake()
        {
            foreach (var interaction in endings)
            {
                GameManager.Instance.AddInteraction(interaction.interaction);

                interaction.interaction.UpdateId();
            }
            
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }

        private void UpdateEnding()
        {
            var targetEnding = TendencyManager.Instance.GetEndingType();
            var tendencyData = TendencyManager.Instance.GetTendencyData();

            if (endings.All(item => item.endingType != targetEnding))
            {
                Debug.LogError(
                    $"해당 엔딩 타입이 없습니다. {targetEnding}, {tendencyData.ascent}, {tendencyData.descent}, {tendencyData.activation}, {tendencyData.inactive}");
                return;
            }
            
            Debug.LogWarning($"엔딩 타입 - {targetEnding}, {tendencyData.ascent}, {tendencyData.descent}, {tendencyData.activation}, {tendencyData.inactive}");

            foreach (var ending in endings)
            {
                ending.interaction.gameObject.SetActive(false);
            }

            var targetEndingInteraction = endings.First(item => item.endingType == targetEnding);
            targetEndingInteraction.interaction.gameObject.gameObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.isTrigger || !col.TryGetComponent(out Player.Player _))
            {
                return;
            }
            
            UpdateEnding();
        }
    }
}