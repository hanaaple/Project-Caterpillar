using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Utility.SaveSystem
{
    public static class SaveManager
    {
        private static string SaveFileName = $"{Application.persistentDataPath}/saveData.save";
        private static SaveData _saveData;

        public static void Init()
        {
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }

        public static void Save()
        {
            using var file = File.Create(SaveFileName);
            new BinaryFormatter().Serialize(file, _saveData);

            file.
            
        }

        public static void Load()
        {

        }
    }
}