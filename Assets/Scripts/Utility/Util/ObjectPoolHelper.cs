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
                    var playableDirector = Instantiate(playableDirectorPrefab, transform);
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
                });
            
            _objectPools.Add(typeof(PlayableDirector), playableDirectorPool);
            
            var audioSourceObjectPool = new ObjectPool<AudioSource>(
                () =>
                {
                    var audioSource = Instantiate(audioSourcePrefab, transform);
                    return audioSource;
                },
                item =>
                {
                    item.gameObject.SetActive(true);
                },
                item =>
                {
                    item.Stop();
                    item.clip = null;
                    item.playOnAwake  = false;
                    item.outputAudioMixerGroup = null;
                    item.mute = false;
                    item.loop = false;
                    item.volume = 1;
                    item.pitch = 1;
                    item.time = 0;
                    item.gameObject.SetActive(false);
                });
            
            _objectPools.Add(typeof(AudioSource), audioSourceObjectPool);
        }

        [SerializeField] private PlayableDirector playableDirectorPrefab;
        [SerializeField] private AudioSource audioSourcePrefab;

        private Dictionary<Type, dynamic> _objectPools;

        public T Get<T>() where T : class
        {
            var returnObj = (_objectPools[typeof(T)] as ObjectPool<T>).Get();
            // Debug.LogWarning($"Get {returnObj.GetType()}");
            return returnObj;
        }
        
        public void Release<T>(T releaseObject) where T : class
        {
            // Debug.LogWarning($"Release {releaseObject.GetType()}");
            (_objectPools[typeof(T)] as ObjectPool<T>).Release(releaseObject);
        }
    }
}