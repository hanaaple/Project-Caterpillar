using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Util;

namespace Utility.Audio
{
    public enum AudioSourceType
    {
        Bgm,
        Sfx
    }

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
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

        [Header("Bgm PlayableDirector")] [SerializeField]
        private PlayableDirector bgmPlayableDirector;

        /// AudioSource(Bgm) - FadeIn 도중에 volume value가 도중에 바뀌는 경우 FadeIn이 제대로 작동하지 않는 것을 방지하기 위해 사용 
        private float _bgmVolumeValue;

        private bool _isReduced;
        private Coroutine _fadeInCoroutine;
        private Coroutine _fadeOutCoroutine;

        private const float ReducePercentage = 0.1f;

        private bool IsReduced
        {
            get => _isReduced;
            set
            {
                Debug.Log($"Set Audio Reduce {value}");
                _isReduced = value;
                UpdateVolume();
            }
        }

        private readonly Dictionary<AudioClip, float> _sfxClipDictionary = new();
        private readonly Dictionary<TimelineAsset, PlayableDirector> _sfxTimelineDictionary = new();

        private const float FadeSec = .5f;

        private static AudioManager Create()
        {
            var sceneLoaderPrefab = Resources.Load<AudioManager>("AudioManager");
            return Instantiate(sceneLoaderPrefab);
        }

        public void SetVolume(AudioSourceType audioSourceType, float volumeValue)
        {
            if (audioSourceType == AudioSourceType.Bgm)
            {
                _bgmVolumeValue = volumeValue;
            }
            else if (audioSourceType == AudioSourceType.Sfx)
            {
                sfx.volume = volumeValue;
            }

            UpdateVolume();
        }

        public void SetMute(AudioSourceType audioSourceType, bool isMute)
        {
            var audioSource = GetAudioSource(audioSourceType);
            audioSource.mute = isMute;
        }

        public void PlaySfx(TimelineAsset timelineAsset, float volume = 1f, bool isOneShot = true)
        {
            CheckGarbageCollect();

            if (!timelineAsset)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }
            
            Debug.Log($"Play Sfx - {timelineAsset}");

            if (isOneShot)
            {
                var playableDirector = ObjectPoolHelper.Instance.Get<PlayableDirector>();
                playableDirector.gameObject.name = timelineAsset.name;
                playableDirector.playableAsset = timelineAsset;

                var audioTracks = timelineAsset.GetOutputTracks().Where(item => item is AudioTrack);

                foreach (var audioTrack in audioTracks)
                {
                    playableDirector.SetGenericBinding(audioTrack, sfx);

                    audioTrack.CreateCurves("NameOfAnimationClip");
                    audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume",
                        AnimationCurve.Linear(0, volume, 1, volume));
                    audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "stereoPan",
                        AnimationCurve.Linear(0, 0, 1, 0));
                    audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "spatialBlend",
                        AnimationCurve.Linear(0, 0, 1, 0));
                }

                playableDirector.Play();
            }
            else
            {
                if (!IsPlayingSfx(timelineAsset))
                {
                    var playableDirector = ObjectPoolHelper.Instance.Get<PlayableDirector>();
                    playableDirector.gameObject.name = timelineAsset.name;
                    playableDirector.playableAsset = timelineAsset;

                    var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();

                    foreach (var audioTrack in audioTracks)
                    {
                        playableDirector.SetGenericBinding(audioTrack, sfx);
                        audioTrack.CreateCurves("NameOfAnimationClip");

                        audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume",
                            AnimationCurve.Linear(0, volume, 1, volume));
                        audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "stereoPan",
                            AnimationCurve.Linear(0, 0, 1, 0));
                        audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "spatialBlend",
                            AnimationCurve.Linear(0, 0, 1, 0));
                    }

                    _sfxTimelineDictionary.Add(timelineAsset, playableDirector);
                    playableDirector.Play();
                }
            }
        }

        public void PlaySfx(AudioClip audioClip, float volume = 1f, bool isOneShot = true,
            bool isWithoutTimeScale = false)
        {
            if (!audioClip)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }
            
            Debug.Log($"Play Sfx - {audioClip}");

            if (isWithoutTimeScale)
            {
                sfx.pitch = 1;
            }

            CheckGarbageCollect();

            if (isOneShot)
            {
                sfx.PlayOneShot(audioClip);
            }
            else
            {
                // SfxTimeline is not Playing & SfxClip is Not Playing
                if (!IsPlayingSfx(audioClip))
                {
                    _sfxClipDictionary.Add(audioClip, Time.time);
                    sfx.PlayOneShot(audioClip, volume);
                }
            }
        }

        public void PlayBgmWithFade(TimelineAsset timelineAsset)
        {
            if (!timelineAsset)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }

            if ((bgmPlayableDirector.playableAsset == null && bgm.clip == null) || _fadeInCoroutine != null)
            {
                // FadeIn -> StopCoroutine -> PlayBgm (Contain Reset Volume) -> FadeIn
                if (_fadeInCoroutine != null)
                {
                    StopCoroutine(_fadeInCoroutine);
                    _fadeInCoroutine = null;
                }

                PlayBgm(timelineAsset);
            }
            else
            {
                // FadeOut (뒤에 1번 Bgm을 새로 실행할 예정이었는데 또 2번 Bgm으로 PlayBgm을 실행함) -> FadeOut 이후 2번 Bgm을 실행
                // FadeOut -> Play & FadeIn ver2

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);

                    var t = Mathf.InverseLerp(0, _bgmVolumeValue, bgm.volume);

                    _fadeOutCoroutine = StartCoroutine(FadeOutBgm(FadeSec, () => { PlayBgm(timelineAsset); }, t));
                }
                else
                {
                    if (timelineAsset == bgmPlayableDirector.playableAsset as TimelineAsset)
                    {
                        PlayBgm(timelineAsset);
                    }
                    else
                    {
                        _fadeOutCoroutine = StartCoroutine(FadeOutBgm(FadeSec, () => { PlayBgm(timelineAsset); }));
                    }
                }
            }
        }
        
        public void PlayBgmWithFade(AudioClip audioClip, float fadeSec = -1)
        {
            if (!audioClip)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }

            if (fadeSec < 0)
            {
                fadeSec = FadeSec;
            }

            if ((bgmPlayableDirector.playableAsset == null && bgm.clip == null) || _fadeInCoroutine != null)
            {
                // FadeIn -> StopCoroutine -> PlayBgm (Contain Reset Volume) -> FadeIn
                if (_fadeInCoroutine != null)
                {
                    StopCoroutine(_fadeInCoroutine);
                    _fadeInCoroutine = null;
                }

                PlayBgm(audioClip);
            }
            else
            {
                // FadeOut (뒤에 1번 Bgm을 새로 실행할 예정이었는데 또 2번 Bgm으로 PlayBgm을 실행함) -> FadeOut 이후 2번 Bgm을 실행
                // FadeOut -> Play & FadeIn ver2

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);

                    var t = Mathf.InverseLerp(0, _bgmVolumeValue, bgm.volume);

                    _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, () => { PlayBgm(audioClip); }, t));
                }
                else
                {
                    // if (audioClip == bgm.clip) -> No Fade Out
                    if (audioClip == bgm.clip)
                    {
                        PlayBgm(audioClip);
                    }
                    else
                    {
                        // FadeOut And Play Or Just Play
                        _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, () => { PlayBgm(audioClip); }
                        ));
                    }
                }
            }
        }

        private void PlayBgm(TimelineAsset timelineAsset)
        {
            if (!timelineAsset)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }

            Debug.Log($"Play bgm - {timelineAsset}");

            if (bgmPlayableDirector.playableAsset != timelineAsset)
            {
                StopBgm();

                var audioTracks = timelineAsset.GetOutputTracks().Where(item => item is AudioTrack);

                foreach (var audioTrack in audioTracks)
                {
                    bgmPlayableDirector.SetGenericBinding(audioTrack, bgm);
                }

                bgmPlayableDirector.extrapolationMode = DirectorWrapMode.Loop;
                bgmPlayableDirector.playableAsset = timelineAsset;
            }

            if (IsReduced)
            {
                // 원래대로 돌리기
                IsReduced = false;
            }
            else
            {
                // 항상 FadeIn
                bgmPlayableDirector.Play();
                _fadeInCoroutine = StartCoroutine(FadeInBgm(FadeSec));
            }
        }

        private void PlayBgm(AudioClip audioClip)
        {
            if (!audioClip)
            {
                Debug.LogWarning("Audio 비어있음");
                return;
            }

            Debug.Log($"Play bgm - {audioClip}");

            if (bgm.clip != audioClip)
            {
                StopBgm();
                bgm.clip = audioClip;
            }

            if (IsReduced)
            {
                IsReduced = false;
            }
            else
            {
                // 항상 FadeIn
                bgm.Play();
                _fadeInCoroutine = StartCoroutine(FadeInBgm(FadeSec));
            }
        }

        public void ReturnVolume()
        {
            IsReduced = false;
        }

        public void ReduceVolume()
        {
            IsReduced = true;
        }

        private void UpdateVolume()
        {
            bgm.volume = _isReduced ? _bgmVolumeValue * ReducePercentage : _bgmVolumeValue;
            sfx.volume = sfx.volume;
            
            Debug.Log($"Bgm(Reduce - {_isReduced}) - {bgm.volume}, Sfx - {sfx.volume}");
        }

        private void StopBgm(bool isContinue = true)
        {
            StopAllCoroutines();

            if (!isContinue)
            {
                IsReduced = false;
            }

            bgm.clip = null;
            bgm.Stop();
            bgmPlayableDirector.Stop();
            bgmPlayableDirector.playableAsset = null;
            bgmPlayableDirector.time = 0;
        }

        private void StopSfx()
        {
            sfx.Stop();
        }

        public void StopAudio()
        {
            StopSfx();
            StopBgm(false);
        }

        private bool IsPlayingSfx(TimelineAsset timelineAsset)
        {
            // if IsPlayingSfx Any SfxAudio return true
            if (!_sfxTimelineDictionary.ContainsKey(timelineAsset))
            {
                return false;
            }

            var playableDirector = _sfxTimelineDictionary[timelineAsset];

            // Debug.LogWarning($"{playableDirector.duration},   {playableDirector.time}");
            // Debug.LogWarning(Math.Abs(playableDirector.duration - playableDirector.time) <= 1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate);
            // Debug.LogWarning($"{playableDirector.state},  {playableDirector.playableGraph.IsValid()}");
            // Debug.LogWarning(playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid());

            if (Math.Abs(playableDirector.duration - playableDirector.time) <=
                1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate ||
                playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid())
            {
                return false;
            }

            return true;
        }

        public bool IsPlayingSfx(AudioClip audioClip)
        {
            if (!_sfxClipDictionary.ContainsKey(audioClip) ||
                Time.time - _sfxClipDictionary[audioClip] >= audioClip.length)
            {
                return false;
            }

            return true;
        }

        private void CheckGarbageCollect()
        {
            var toRemoveClip = new List<AudioClip>();
            foreach (var keyValuePair in _sfxClipDictionary)
            {
                if (!IsPlayingSfx(keyValuePair.Key))
                {
                    toRemoveClip.Add(keyValuePair.Key);
                }
            }

            foreach (var audioClip in toRemoveClip)
            {
                _sfxClipDictionary.Remove(audioClip);
            }


            var toRemoveTimeline = new List<TimelineAsset>();
            foreach (var keyValuePair in _sfxTimelineDictionary)
            {
                if (!IsPlayingSfx(keyValuePair.Key))
                {
                    toRemoveTimeline.Add(keyValuePair.Key);
                }
            }

            foreach (var timelineAsset in toRemoveTimeline)
            {
                ObjectPoolHelper.Instance.Release(_sfxTimelineDictionary[timelineAsset]);
                _sfxTimelineDictionary.Remove(timelineAsset);
            }
        }

        private IEnumerator FadeInBgm(float fadeSec)
        {
            var t = 0f;
            while (t <= 1f)
            {
                t += Time.deltaTime / fadeSec;
                bgm.volume = Mathf.Lerp(0, _bgmVolumeValue, t);
                yield return null;
            }

            _fadeInCoroutine = null;
        }

        private IEnumerator FadeOutBgm(float fadeSec, Action onEndAction, float t = 1f)
        {
            while (t >= 0f)
            {
                t -= Time.deltaTime / fadeSec;
                bgm.volume = Mathf.Lerp(_bgmVolumeValue, 0, t);
                yield return null;
            }

            _fadeOutCoroutine = null;
            onEndAction.Invoke();
        }

        public AudioSource GetAudioSource(AudioSourceType audioSourceType)
        {
            return audioSourceType switch
            {
                AudioSourceType.Bgm => bgm,
                AudioSourceType.Sfx => sfx,
                _ => null
            };
        }

        public AudioSource GetAudioSource(string audioSource)
        {
            return Enum.TryParse<AudioSourceType>(audioSource, out var result) ? GetAudioSource(result) : bgm;
        }

        public float GetVolume(AudioSourceType audioSourceType)
        {
            if (audioSourceType == AudioSourceType.Sfx)
            {
                return sfx.volume;
            }

            if (audioSourceType == AudioSourceType.Bgm)
            {
                return _bgmVolumeValue;
            }

            return -1;
        }

        public bool GetIsReduced()
        {
            return IsReduced;
        }

        private void SaveAudio()
        {
            PlayerPrefs.SetString("SfxMute", sfx.mute.ToString());
            PlayerPrefs.SetString("BgmMute", bgm.mute.ToString());
            PlayerPrefs.SetFloat("Sfx", sfx.volume);
            PlayerPrefs.SetFloat("Bgm", _bgmVolumeValue);
        }

        public void LoadAudio()
        {
            if (PlayerPrefs.HasKey("SfxMute"))
            {
                var sfxMuteString = PlayerPrefs.GetString("SfxMute");

                SetMute(AudioSourceType.Sfx, bool.TryParse(sfxMuteString, out var isSfxMute) && isSfxMute);
            }
            else
            {
                SetMute(AudioSourceType.Sfx, false);
            }

            if (PlayerPrefs.HasKey("BgmMute"))
            {
                var bgmMuteString = PlayerPrefs.GetString("BgmMute");

                SetMute(AudioSourceType.Bgm, bool.TryParse(bgmMuteString, out var isBgmMute) && isBgmMute);
            }
            else
            {
                SetMute(AudioSourceType.Bgm, false);
            }

            if (PlayerPrefs.HasKey("Sfx"))
            {
                var sfxValue = PlayerPrefs.GetFloat("Sfx");

                SetVolume(AudioSourceType.Sfx, sfxValue);
            }
            else
            {
                SetVolume(AudioSourceType.Sfx, .5f);
            }

            if (PlayerPrefs.HasKey("Bgm"))
            {
                var bgmValue = PlayerPrefs.GetFloat("Bgm");
                SetVolume(AudioSourceType.Bgm, bgmValue);
            }
            else
            {
                SetVolume(AudioSourceType.Bgm, .5f);
            }
        }

        private void OnApplicationQuit()
        {
            SaveAudio();
        }
    }
}