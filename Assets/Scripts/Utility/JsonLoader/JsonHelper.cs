
using UnityEngine;

namespace Utility.JsonLoader
{
    public class JsonHelper : MonoBehaviour
    {
        public static T[] GetJsonArray<T>(string json)
        {
            var jsonData = JsonDataDecryptor.AesDecrypt256(json);
            var t = JsonUtility.FromJson<Wrapper<T>>("{\"wrapper\":" + jsonData + "}").wrapper;
            return t;
        }
    
        private class Wrapper<T>
        {
            public T[] wrapper;
        }
    }
}
