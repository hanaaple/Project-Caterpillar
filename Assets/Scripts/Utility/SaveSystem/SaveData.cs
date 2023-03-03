using Utility.Serialize;

namespace Utility.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;
        
        public SerializableVector3 position;
        public SerializableQuaternion rotation;

        public string[] items;
        
        // Enum이 저장되던가 체크해야됨
        // public ItemManager.ItemType itemss;
    }
    
    [System.Serializable]
    public class SaveCoverData
    {
        public string describe;
        
        public string sceneName;
        
        public int playTime;
    }
}
