using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility.SaveSystem
{
    public static class SaveManager
    {
        private static int _idx;

        
        private static readonly string SaveDirectoryPath = $"{Application.persistentDataPath}/saveData";
        private static string SaveFilePath => $"{Application.persistentDataPath}/saveData/saveData{_idx}.save";
        private static string SaveCoverFilePath => $"{Application.persistentDataPath}/saveData/saveCoverData{_idx}.save";

        private static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("abcdefg_abcdefg_");
        private static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("abcdefg_");

        private static readonly Dictionary<int, SaveData> SaveData;
        private static readonly Dictionary<int, SaveCoverData> SaveCoverData;
        
        static SaveManager()
        {
            Debug.Log(SaveFilePath);
            SaveData = new Dictionary<int, SaveData>();
            SaveCoverData = new Dictionary<int, SaveCoverData>();
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }

        public static void Save(int idx, SaveData saveData)
        {
            _idx = idx;
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            Remove(idx);
            
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData.saveCoverData);
                        AddSaveCoverData(idx, saveData.saveCoverData);

                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.LogWarning(e);
                        fileStream.Close();
                        rijn.Clear();
                        return;
                    }
                    fileStream.Close();
                }
                
                using (var fileStream = File.Open(SaveFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
                        new BinaryFormatter().Serialize(cryptoStream, saveData);
                        AddSaveData(idx, saveData);

                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.LogWarning(e);
                        fileStream.Close();
                        rijn.Clear();
                        return;
                    }
                    fileStream.Close();
                }
            }
            rijn.Clear();
        }

        public static void Load(int idx)
        {
            _idx = idx;
            if (!File.Exists(SaveFilePath) || IsLoaded(idx))
            {
                return;
            }
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            
            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveFilePath, FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        var saveData = (SaveData) new BinaryFormatter().Deserialize(cryptoStream);

                        AddSaveData(idx, saveData);

                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.LogWarning(e);
                        var saveData = new SaveData
                        {
                            saveCoverData = new SaveCoverData
                            {
                                describe = "불러오기 오류"
                            }
                        };
                        AddSaveData(idx, saveData);
                    }
                    fileStream.Close();
                }
            }

            rijn.Clear();
        }
        
        public static async Task LoadCoverAsync(int idx)
        {
            _idx = idx;
            if (!File.Exists(SaveCoverFilePath) || IsCoverLoaded(idx))
            {
                return;
            }

            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            using (ICryptoTransform decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                await using (var fileStream = File.Open(SaveCoverFilePath, FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        return;
                    }

                    try
                    {
                        Stream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
                        await Task.Run(() =>
                        {
                            var saveCoverData = (SaveCoverData) new BinaryFormatter().Deserialize(cryptoStream);
                            AddSaveCoverData(idx, saveCoverData);
                        });
                        
                        cryptoStream.Close();
                    }
                    catch (CryptographicException e)
                    {
                        Debug.LogWarning(e);

                        var saveCoverData = new SaveCoverData
                        {
                            describe = "불러오기에 실패",
                        };
                        AddSaveCoverData(idx, saveCoverData);
                    }

                    fileStream.Close();
                }

                rijn.Clear();
            }
        }

        public static int GetSaveIndex(int index)
        {
            var saveData = Directory.GetFiles(SaveDirectoryPath, "saveData*.save", SearchOption.AllDirectories);
            var saveCoverData = Directory
                .GetFiles(SaveDirectoryPath, "saveCoverData*.save", SearchOption.AllDirectories)
                .Select(item => item.Replace("Cover", "")).ToArray();

            var saveDataCount = 0;
            foreach (var data in saveData)
            {
                var coverData = Array.Find(saveCoverData, item => item == data);
                if (string.IsNullOrEmpty(coverData))
                {
                    continue;
                }
                
                if (saveDataCount == index)
                {
                    var saveDataIndex = int.Parse(new string(data.Where(char.IsDigit).ToArray()));
                    return saveDataIndex;
                }

                saveDataCount++;
            }

            Debug.LogWarning($"SaveData가 없어요 {index}번째 saveData 없음.");
            return -1;
        }

        public static int GetSaveDataLength()
        {
            var saveData = Directory.GetFiles(SaveDirectoryPath, "saveData*.save", SearchOption.AllDirectories);
            var saveCoverData = Directory.GetFiles(SaveDirectoryPath, "saveCoverData*.save", SearchOption.AllDirectories)
                .Select(item => item.Replace("Cover", "")).ToArray();

            var origin = _idx;
            _idx = 0;

            foreach (var data in saveData)
            {
                var coverData = Array.Find(saveCoverData, item => item == data);
                if (!string.IsNullOrEmpty(coverData))
                {
                    _idx++;
                }
            }
            
            var t = _idx;
            _idx = origin;

            return t;
        }
        
        public static SaveData GetSaveData(int idx)
        {
            _idx = idx;
            if (IsLoaded(idx))
            {
                return SaveData[idx];
            }

            return null;
        }
        
        public static SaveCoverData GetSaveCoverData(int idx)
        {
            _idx = idx;
            
            if (IsCoverLoaded(idx))
            {
                return SaveCoverData[idx];
            }

            return null;
        }

        private static void AddSaveData(int idx, SaveData saveData)
        {
            Debug.Log($"{idx} index Load");
            if (IsLoaded(idx))
            {
                SaveData[idx] = saveData;
            }
            else
            {
                SaveData.Add(idx, saveData);
            }
        }
        
        private static void AddSaveCoverData(int idx, SaveCoverData saveCoverData)
        {
            Debug.Log($"{idx} index Cover Load");
            if (IsCoverLoaded(idx))
            {
                SaveCoverData[idx] = saveCoverData;
            }
            else
            {
                SaveCoverData.Add(idx, saveCoverData);
            }
        }

        public static void Delete(int idx)
        {
            _idx = idx;
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
            
            if (File.Exists(SaveCoverFilePath))
            {
                File.Delete(SaveCoverFilePath);
            }

            Remove(idx);
        }
        
        private static void Remove(int idx)
        {
            if (IsLoaded(idx))
            {
                SaveData.Remove(idx);
            }
            if (IsCoverLoaded(idx))
            {
                SaveCoverData.Remove(idx);
            }
        }

        public static bool IsLoaded(int idx)
        {
            return SaveData.ContainsKey(idx);
        }
        
        public static bool IsCoverLoaded(int idx)
        {
            return SaveCoverData.ContainsKey(idx);
        }
        
        public static bool Exists(int idx)
        {
            _idx = idx;
            return File.Exists(SaveFilePath) && File.Exists(SaveCoverFilePath);
        }
    }
}