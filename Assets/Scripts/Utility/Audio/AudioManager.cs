using UnityEngine;

namespace Utility.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        private static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = FindObjectOfType<AudioManager>();
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

        [Header("오디오 소스")] [SerializeField] private AudioSource bgm;
        [SerializeField] private AudioSource sfx;

        private bool _isPaused;

        private static AudioManager Create()
        {
            var sceneLoaderPrefab = Resources.Load<AudioManager>("AudioManager");
            return Instantiate(sceneLoaderPrefab);
        }

        public static void SetVolume(string sourceName, float value)
        {
            var audioSource = GetAudioSource(sourceName);
            audioSource.volume = value;
        }

        public static void SetMute(string sourceName, bool isMute)
        {
            if (Instance.bgm.name == sourceName)
            {
                Instance.bgm.mute = isMute;
            }
            else if (Instance.sfx.name == sourceName)
            {
                Instance.sfx.mute = isMute;
            }
        }

        public static void PlaySfx(AudioClip audioClip)
        {
            Instance.sfx.PlayOneShot(audioClip);
        }

        public static void PlayBgm(AudioClip audioClip)
        {
            if (Instance.bgm.clip != audioClip)
            {
                Instance.bgm.clip = audioClip;
            }

            if (Instance._isPaused)
            {
                Instance.bgm.UnPause();
            }
            else
            {
                Instance.bgm.Play();
            }
        }

        public static void PauseBgm()
        {
            Instance._isPaused = true;
            Instance.bgm.Pause();
        }

        public static void StopBgm()
        {
            Instance._isPaused = false;
            Instance.bgm.clip = null;
            Instance.bgm.Stop();
        }

        public static AudioSource GetAudioSource(string sourceName)
        {
            if (Instance.bgm.name == sourceName)
            {
                return Instance.bgm;
            }

            if (Instance.sfx.name == sourceName)
            {
                return Instance.sfx;
            }

            return null;
        }

        private static void SaveAudio()
        {
            PlayerPrefs.SetString("SfxMute", Instance.sfx.mute.ToString());
            PlayerPrefs.SetString("BgmMute", Instance.bgm.mute.ToString());
            PlayerPrefs.SetFloat("Sfx", Instance.sfx.volume);
            PlayerPrefs.SetFloat("Bgm", Instance.bgm.volume);
        }

        public static void LoadAudio()
        {
            if (PlayerPrefs.HasKey("SfxMute"))
            {
                var sfxMuteString = PlayerPrefs.GetString("SfxMute");

                Instance.sfx.mute = bool.TryParse(sfxMuteString, out var isSfxMute) && isSfxMute;
            }

            if (PlayerPrefs.HasKey("BgmMute"))
            {
                var bgmMuteString = PlayerPrefs.GetString("BgmMute");

                Instance.bgm.mute = bool.TryParse(bgmMuteString, out var isBgmMute) && isBgmMute;
            }

            if (PlayerPrefs.HasKey("Sfx"))
            {
                var sfxValue = PlayerPrefs.GetFloat("Sfx");

                Instance.sfx.volume = sfxValue;
            }

            if (PlayerPrefs.HasKey("Bgm"))
            {
                var bgmValue = PlayerPrefs.GetFloat("Bgm");

                Instance.bgm.volume = bgmValue;
            }
        }

        private void OnApplicationQuit()
        {
            SaveAudio();
        }
    }
}