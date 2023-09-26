using System;
using System.Linq;
using UnityEngine;
using Utility.Core;
using Utility.SaveSystem;

namespace Utility.Interaction
{
    public class Npc : MonoBehaviour
    {
        [Serializable]
        public class NpcInteraction
        {
            public Interaction interaction;
            public NpcState npcState;
        }

        public NpcType npcType;
        public NpcInteraction[] npcInteraction;

        private void Awake()
        {
            foreach (var interaction in npcInteraction)
            {
                GameManager.Instance.AddInteraction(interaction.interaction);

                interaction.interaction.UpdateId();
            }

            GameManager.Instance.AddMainFieldNpc(this);
        }

        // OnLoadSceneEnd, Save MainFieldData to GameManager

        public void SetNpcState(NpcState npcState)
        {
            var targetInteraction = Array.Find(npcInteraction, item => item.npcState == npcState).interaction;

            foreach (var interaction in npcInteraction)
            {
                interaction.interaction.gameObject.SetActive(false);
            }

            targetInteraction.gameObject.SetActive(true);
        }

        public NpcData GetSaveData()
        {
            var interaction = npcInteraction.First(item => item.interaction.gameObject.activeSelf);
            var mainFieldNpcSaveData = new NpcData
            {
                npcType = npcType,
                state = interaction.npcState
            };

            return mainFieldNpcSaveData;
        }
    }
}