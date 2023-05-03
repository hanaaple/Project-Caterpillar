using System;
using System.Collections.Generic;
using UnityEngine;
using Utility.Player;
using Utility.SaveSystem;

namespace Utility.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<GameManager>();
                    if (obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }
        
        [NonSerialized] public TestPlayer Player;

        [NonSerialized] public List<Interaction.Interaction> InteractionObjects;

        private static GameManager Create()
        {
            var gameManagerPrefab = Resources.Load<GameManager>("GameManager");
            return Instantiate(gameManagerPrefab);
        }

        private void Awake()
        {
            InteractionObjects = new List<Interaction.Interaction>();
        }

        public void AddInteraction(Interaction.Interaction interaction)
        {
            InteractionObjects.Add(interaction);
        }

        public void Load(int saveDataIndex)
        {
            var saveData = SaveManager.GetSaveData(saveDataIndex);
            if (saveData.interactionData != null)
            {
                foreach (var interactionSaveData in saveData.interactionData)
                {
                    var interaction = InteractionObjects.Find(item => item.id == interactionSaveData.id);

                    foreach (var interactionData in interaction.interactionData)
                    {
                        var loadedSerializedData = interactionSaveData.serializedInteractionData.Find(item =>
                            item.id == interactionData.serializedInteractionData.id);

                        interactionData.serializedInteractionData = loadedSerializedData;
                    }
                }
            }

            if (Player)
            {
                Player.transform.position = saveData.playerSerializableTransform.position;
                Player.transform.rotation = saveData.playerSerializableTransform.rotation;
            }
        }
    }
}