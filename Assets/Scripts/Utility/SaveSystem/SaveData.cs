using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Interaction;
using Utility.Serialize;

namespace Utility.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;

        [FormerlySerializedAs("playerTransform")] public SerializableTransform playerSerializableTransform;
        
        public List<InteractionSaveData> interactionData;

        public string[] items;
        
        // Enum이 저장되던가 체크해야됨 그래서 string으로 저장해둠
        // public ItemManager.ItemType itemss;
        public TendencyData tendencyData;
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
