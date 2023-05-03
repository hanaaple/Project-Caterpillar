using System;
using System.Collections.Concurrent;
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
        private static readonly string SaveDirectoryPath = $"{Application.persistentDataPath}/saveData";
        private static string SaveFilePath(int index) => $"{Application.persistentDataPath}/saveData/saveData{index}.save";
        private static string SaveCoverFilePath(int index) => $"{Application.persistentDataPath}/saveData/saveCoverData{index}.save";

        private static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("abcdefg_abcdefg_");
        private static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("abcdefg_");

        private static readonly ConcurrentDictionary<int, SaveData> SaveData;
        private static readonly ConcurrentDictionary<int, SaveCoverData> SaveCoverData;
        
        static SaveManager()
        {
            Debug.Log(SaveDirectoryPath);
            SaveData = new ConcurrentDictionary<int, SaveData>();
            SaveCoverData = new ConcurrentDictionary<int, SaveCoverData>();
#if UNITY_IPHONE
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }

        public static void Save(int idx, SaveData saveData)
        {
            RijndaelManaged rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;

            Remove(idx);
            
            using (ICryptoTransform encryptor = rijn.CreateEncryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
                
                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
            if (!File.Exists(SaveFilePath(idx)) || IsLoaded(idx))
            {
                return;
            }
            var rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            
            using (var decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                using (var fileStream = File.Open(SaveFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        fileStream.Close();
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
            if (!File.Exists(SaveCoverFilePath(idx)) || IsCoverLoaded(idx))
            {
                return;
            }

            var rijn = new RijndaelManaged();
            rijn.Mode = CipherMode.ECB;
            rijn.Padding = PaddingMode.Zeros;
            rijn.BlockSize = 256;
            
            Debug.Log($"{idx} Load 시도");

            using (var decryptor = rijn.CreateDecryptor(EncryptKey, EncryptIv))
            {
                await using (var fileStream = File.Open(SaveCoverFilePath(idx), FileMode.Open))
                {
                    if (fileStream.Length <= 0)
                    {
                        Debug.Log("Load 오류 발생");
                        fileStream.Close();
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
        
        public static int GetNewSaveIndex()
        {
            var saveData = Directory.GetFiles(SaveDirectoryPath, "saveData*.save", SearchOption.AllDirectories)
                .Select(item => item.Replace(SaveDirectoryPath, ""))
                .Select(item => item.Replace("\\", ""));
            
            var saveCoverData = Directory
                .GetFiles(SaveDirectoryPath, "saveCoverData*.save", SearchOption.AllDirectories)
                .Select(item => item.Replace(SaveDirectoryPath, ""))
                .Select(item => item.Replace("\\", ""))
                .Select(item => item.Replace("Cover", "")).ToArray();

            var indexArray = saveData.Join(saveCoverData, data => data, coverData => coverData,
                (data, coverData) => int.Parse(new string(data.Where(char.IsDigit).ToArray()))).ToArray();

            indexArray = indexArray.OrderBy(item => item).ToArray();
            
            return indexArray.Last() + 1;
        }

        public static int GetSaveDataLength()
        {
            var saveData = Directory.GetFiles(SaveDirectoryPath, "saveData*.save", SearchOption.AllDirectories);
            var saveCoverData = Directory.GetFiles(SaveDirectoryPath, "saveCoverData*.save", SearchOption.AllDirectories)
                .Select(item => item.Replace("Cover", "")).ToArray();

            var count = saveData.Select(data => Array.Find(saveCoverData, item => item == data))
                .Count(coverData => !string.IsNullOrEmpty(coverData));
            return count;
        }
        
        public static SaveData GetSaveData(int idx)
        {
            return IsLoaded(idx) ? SaveData[idx] : null;
        }
        
        public static SaveCoverData GetSaveCoverData(int idx)
        {
            return IsCoverLoaded(idx) ? SaveCoverData[idx] : null;
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
                SaveData.AddOrUpdate(idx, saveData, (_, _) => saveData);
            }
        }
        
        private static void AddSaveCoverData(int idx, SaveCoverData saveCoverData)
        {
            // Debug.Log($"{idx} index Cover Load");
            if (IsCoverLoaded(idx))
            {
                SaveCoverData[idx] = saveCoverData;
            }
            else
            {
                SaveCoverData.AddOrUpdate(idx, saveCoverData, (_, _) => saveCoverData);
            }
        }

        public static void Delete(int idx)
        {
            if (File.Exists(SaveFilePath(idx)))
            {
                File.Delete(SaveFilePath(idx));
            }
            
            if (File.Exists(SaveCoverFilePath(idx)))
            {
                File.Delete(SaveCoverFilePath(idx));
            }

            Remove(idx);
        }
        
        private static void Remove(int idx)
        {
            if (IsLoaded(idx))
            {
                SaveData.TryRemove(idx, out var saveData);
            }
            if (IsCoverLoaded(idx))
            {
                SaveCoverData.TryRemove(idx, out var saveCoverData);
            }
        }

        public static bool IsLoaded(int idx)
        {
            return SaveData.ContainsKey(idx);
        }

        private static bool IsCoverLoaded(int idx)
        {
            // Debug.Log($"idx: {idx}");
            return SaveCoverData.ContainsKey(idx);
        }
        
        public static bool Exists(int idx)
        {
            return File.Exists(SaveFilePath(idx)) && File.Exists(SaveCoverFilePath(idx));
        }
    }
}