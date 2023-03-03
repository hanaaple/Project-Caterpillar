using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Core;
using Utility.SaveSystem;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ItemManager))]
[CanEditMultipleObjects]
public class GenerateButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        ItemManager generator = (ItemManager)target;
        if (GUILayout.Button("Add Random Item"))
        {
            if (!Application.isPlaying)
            {
                return;
            }
            generator.AddItem((ItemManager.ItemType)Random.Range(0, Enum.GetValues(typeof(ItemManager.ItemType)).Length));
        }
    }
}

#endif

namespace Utility.Core
{
    public class ItemManager : MonoBehaviour
    {
        public enum ItemType
        {
            None,
            Bag,
            Stone
        }

        private static ItemManager _instance;
        
        public static ItemManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    var obj = FindObjectOfType<ItemManager>();
                    if(obj != null)
                    {
                        _instance = obj;
                    }
                    else
                    {
                        _instance = Create();
                    }
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        [SerializeField] private List<ItemType> _items;
        
        private static ItemManager Create()
        {
            var itemManagerPrefab = Resources.Load<ItemManager>("ItemManager");
            return Instantiate(itemManagerPrefab);
        }

        public void Load(int saveIndex)
        {
            Debug.Log("Item Load");
            var saveData = SaveManager.GetSaveData(saveIndex);
            _items = saveData.items
                .Select(item => Enum.TryParse(item, true, out ItemType itemList) ? itemList : ItemType.None)
                .Where(item => item != ItemType.None)
                .ToList();

            if (_items.Count != _items.Distinct().Count())
            {
                Debug.LogWarning("중복 데이터 존재합니다.");
            }
        }

        public void AddItem(ItemType itemType)
        {
            if (Enum.GetValues(typeof(ItemType)).Length - 1 == _items.Count)
            {
                return;
            }

            while(true)
            {
                if (itemType == ItemType.None || _items.Contains(itemType))
                {
                    itemType = (ItemType)Random.Range(0, Enum.GetValues(typeof(ItemType)).Length);
                    continue;
                }

                _items.Add(itemType);
                break;
            }
        }

        public void SetItem(string[] options)
        {
            foreach (var option in options)
            {
                if (!option.Contains(":"))
                {
                    continue;
                }
                var t = option.Replace(" ", "").ToLower();
                var item = t.Split(":");
                var itemName = item[0];
                var addRemove = item[1];

                if(!Enum.TryParse(itemName, true, out ItemType itemType))
                {
                    Debug.LogWarning($"{itemName} 아이템이 없다는딥쇼 쓰앵님");
                    return;
                }
                
                switch(addRemove)
                {
                    case "add":
                    {
                        _items.Add(itemType);
                        break;
                    }
                    case "remove":
                    {
                        _items.Remove(itemType);
                        break;
                    }
                    default:
                    {
                        Debug.LogWarning("머선일인가");
                        break;
                    }
                }
            }
        }

        public T[] GetItem<T>()
        {
            if (typeof(T) == typeof(string))
            {
                return _items.Select(item => item.ToString().ToLower()).ToArray() as T[];    
            }
            
            if (typeof(T) == typeof(ItemType))
            {
                return _items.ToArray() as T[];
            }
            return null;
        }
    }
}
