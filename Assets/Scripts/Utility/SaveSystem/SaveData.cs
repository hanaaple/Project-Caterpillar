using System;
using System.Collections.Generic;
using Utility.Interaction;
using Utility.Serialize;
using Utility.Tendency;

namespace Utility.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        // public SerializableTransform playerSerializableTransform;

        public List<SceneSaveData> sceneSaveData;

        public string[] items;

        public TendencyManager.TendencyData tendencyData;
    }

    [Serializable]
    public class NpcData
    {
        public NpcType npcType;
        public NpcState state;
    }
    
    public enum NpcType
    {
        None,
        Photographer,
    }

    public enum NpcState
    {
        Default,
        Fail,
        End
    }

    [Serializable]
    public struct SerializableTransform
    {
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public struct SceneSaveData
    {
        public string sceneName;
        public List<InteractionSaveData> interactionData;
        public List<NpcData> npcData;
        
        public static bool operator ==(SceneSaveData op1,  SceneSaveData op2) 
        {
            return op1.Equals(op2);
        }

        public static bool operator !=(SceneSaveData op1, SceneSaveData op2)
        {
            return !(op1 == op2);
        }
    }

    [Serializable]
    public struct InteractionSaveData
    {
        public string id;

        public int interactionIndex;

        public List<SerializedInteractionData> serializedInteractionData;
    }

    [Serializable]
    public class SaveCoverData
    {
        public string describe;

        public string sceneName;

        public int playTime;
    }
}