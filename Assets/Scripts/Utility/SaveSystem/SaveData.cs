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
        
        public SerializableTransform playerSerializableTransform;
        
        // Npc들 상태
        public List<InteractionSaveData> interactionData;

        public string[] items;
        
        // Enum이 저장되던가 체크해야됨 그래서 string으로 저장해둠
        // public ItemManager.ItemType items;
        public TendencyManager.TendencyData tendencyData;
    }

    [Serializable]
    public struct SerializableTransform
    {
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
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
