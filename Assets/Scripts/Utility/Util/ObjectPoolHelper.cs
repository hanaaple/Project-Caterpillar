using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;

namespace Utility.Util
{
    public class ObjectPoolHelper : MonoBehaviour
    {
        private static ObjectPoolHelper _instance;

        public static ObjectPoolHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<ObjectPoolHelper>();
                    if (obj != null)
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
        
        private static ObjectPoolHelper Create()
        {
            var prefab = Resources.Load<ObjectPoolHelper>("ObjectPoolHelper");
            return Instantiate(prefab);
        }

        private void Awake()
        {
            _objectPools = new Dictionary<Type, dynamic>();
            
            var playableDirectorPool = new ObjectPool<PlayableDirector>(
                () =>
                {
                    var playableDirector = Instantiate(playableDirectorPrefab);
                    playableDirector.playOnAwake = false;
                    return playableDirector;
                },
                item => { item.gameObject.SetActive(true); },
                item =>
                {
                    item.gameObject.SetActive(false);
                    item.playableAsset = null;
                    item.extrapolationMode = DirectorWrapMode.None;
                    item.time = 0;
                    item.Stop();
                },
                item => { Destroy(item.gameObject); });
            
            _objectPools.Add(typeof(PlayableDirector), playableDirectorPool);
            // Debug.Log(_objectPools[typeof(PlayableDirector)] as ObjectPool<PlayableDirector>);
            // Debug.Log(playableDirectorPool);
        }

        [SerializeField] private PlayableDirector playableDirectorPrefab;

        private Dictionary<Type, dynamic> _objectPools;

        public T Get<T>() where T : class
        {
            Debug.LogWarning($"Get {_objectPools[typeof(T)] as ObjectPool<T>}");
            return (_objectPools[typeof(T)] as ObjectPool<T>).Get();
        }
        
        public void Release<T>(T releaseObject) where T : class
        {
            Debug.LogWarning($"Release {_objectPools[typeof(T)] as ObjectPool<T>}");
            (_objectPools[typeof(T)] as ObjectPool<T>).Release(releaseObject);
        }
    }
}