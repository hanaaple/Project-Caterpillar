using System.Collections.Generic;
using System.IO;
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

        private static string SaveFilePath => $"{Application.persistentDataPath}/saveData{_idx}.save";
        private static string SaveCoverFilePath => $"{Application.persistentDataPath}/saveDataCover{_idx}.save";

        private static readonly byte[] EncryptKey = Encoding.UTF8.GetBytes("abcdefg_abcdefg_");
        private static readonly byte[] EncryptIv = Encoding.UTF8.GetBytes("abcdefg_");

        private static readonly Dictionary<int, SaveData> SaveDatas;
        private static readonly Dictionary<int, SaveCoverData> SaveCoverDatas;
        
        static SaveManager()
        {
            Debug.Log(SaveFilePath);
            SaveDatas = new Dictionary<int, SaveData>();
            SaveCoverDatas = new Dictionary<int, SaveCoverData>();
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
        
        public static SaveData GetSaveData(int idx)
        {
            _idx = idx;
            if (IsLoaded(idx))
            {
                return SaveDatas[idx];
            }

            return null;
        }
        
        public static SaveCoverData GetSaveCoverData(int idx)
        {
            _idx = idx;
            if (IsCoverLoaded(idx))
            {
                return SaveCoverDatas[idx];
            }

            return null;
        }

        private static void AddSaveData(int idx, SaveData saveData)
        {
            Debug.Log($"{idx} index Load");
            if (IsLoaded(idx))
            {
                SaveDatas[idx] = saveData;
            }
            else
            {
                SaveDatas.Add(idx, saveData);
            }
        }
        
        private static void AddSaveCoverData(int idx, SaveCoverData saveCoverData)
        {
            Debug.Log($"{idx} index Cover Load");
            if (IsCoverLoaded(idx))
            {
                SaveCoverDatas[idx] = saveCoverData;
            }
            else
            {
                SaveCoverDatas.Add(idx, saveCoverData);
            }
        }

        private static void Delete(int idx)
        {
            _idx = idx;
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }

            Remove(idx);
        }
        
        private static void Remove(int idx)
        {
            if (IsLoaded(idx))
            {
                SaveDatas.Remove(idx);
            }
            if (IsCoverLoaded(idx))
            {
                SaveCoverDatas.Remove(idx);
            }
        }


        public static bool IsLoaded(int idx)
        {
            return SaveDatas.ContainsKey(idx);
        }
        
        public static bool IsCoverLoaded(int idx)
        {
            return SaveCoverDatas.ContainsKey(idx);
        }
        
        public static bool Exists(int idx)
        {
            _idx = idx;
            return File.Exists(SaveFilePath) && File.Exists(SaveCoverFilePath);
        }
    }
}