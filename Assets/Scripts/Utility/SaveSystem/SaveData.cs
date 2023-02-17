using Utility.Serialize;

namespace Utility.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public SaveCoverData saveCoverData;
        
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
    }
    
    [System.Serializable]
    public class SaveCoverData
    {
        public string describe;
        
        public string sceneName;
        
        public int playTime;
    }
}
