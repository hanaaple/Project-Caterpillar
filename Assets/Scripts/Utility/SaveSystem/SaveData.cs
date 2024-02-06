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

        public List<SceneData> sceneSaveData;

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
    public class SceneData
    {
        public string sceneName;
        public List<InteractionSaveData> interactionData;
        public List<NpcData> npcData;
        
        public SerializableTransform playerSerializableTransform;

        public void UpdateNpcData(NpcType npcType, NpcState npcState)
        {
            var npc = npcData.Find(item => item.npcType == npcType);
            npc.state = npcState;
        }

        public void UpdateInteractionData(InteractionSaveData interactionSaveData)
        {
            var data = interactionData.Find(item => item.id == interactionSaveData.id);

            data.id = interactionSaveData.id;
            data.serializedInteractionData = interactionSaveData.serializedInteractionData;
        }
        
        // default, null 비교

        // public static bool operator ==(SceneData op1, SceneData op2)
        // {
        //     return Equals(op1, op2);
        // }
        //
        // public static bool operator !=(SceneData op1, SceneData op2)
        // {
        //     return !(op1 == op2);
        // }
        //
        // protected bool Equals(SceneData other)
        // {
        //     return sceneName == other.sceneName && Equals(interactionData, other.interactionData) && Equals(npcData, other.npcData) && playerSerializableTransform.Equals(other.playerSerializableTransform);
        // }
        //
        // public override bool Equals(object obj)
        // {
        //     if (ReferenceEquals(null, obj)) return false;
        //     if (ReferenceEquals(this, obj)) return true;
        //     if (obj.GetType() != GetType()) return false;
        //     return Equals((SceneData) obj);
        // }
        //
        // public override int GetHashCode()
        // {
        //     return HashCode.Combine(sceneName, interactionData, npcData, playerSerializableTransform);
        // }
    }

    [Serializable]
    public class InteractionSaveData
    {
        public string name;
        public string id;
        public int interactionIndex;
        public List<SerializedInteractionData> serializedInteractionData;
    }

    [Serializable]
    public class SaveCoverData
    {
        public string describe;
        public string sceneName;
        public string stageText;
        public TimeSpan playTime;
        public string date;
        public string lastPlayTime;
    }
}