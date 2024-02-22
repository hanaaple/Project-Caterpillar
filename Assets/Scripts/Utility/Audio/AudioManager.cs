using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Util;
using Object = UnityEngine.Object;

namespace Utility.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private class AudioClipData
        {
            public AudioSource AudioSource;
            public float Volume;
            public bool IsFading;
        }

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

        /// <summary>
        /// Do not Set AudioSource Volume directly
        /// </summary>
        [Header("오디오 소스")] [SerializeField] private AudioSource bgm;

        /// <summary>
        /// Do not Set AudioSource Volume directly
        /// </summary>
        [SerializeField] private AudioSource sfx;

        [Header("Bgm PlayableDirector")] [SerializeField]
        private PlayableDirector bgmPlayableDirector;

        [Header("Debug")] [SerializeField] private bool isDebug;

        /// AudioSource(Bgm) - FadeIn 도중에 volume value가 도중에 바뀌는 경우 FadeIn이 제대로 작동하지 않는 것을 방지하기 위해 사용 
        private float BGMVolumeValue
        {
            get => _bgmVolumeValue;
            set
            {
                _bgmVolumeValue = value;
                UpdateVolume();
            }
        }

        private float _bgmVolumeValue;

        private float BGMPlayValue
        {
            get => _bgmPlayValue;
            set
            {
                _bgmPlayValue = value;
                UpdateVolume();
            }
        }

        private float _bgmPlayValue = 1f;

        private bool _isReduced;
        private Coroutine _fadeInCoroutine;
        private Coroutine _fadeOutCoroutine;

        // 문제점 1. 동일한 AudioClip, TimelineAsset을 사용하는 경우 어떻게 대처하느냐
        private readonly Dictionary<AudioClip, (float, Action)> _distinctSfxClipDictionary = new();

        // PlayableDirector, EndAction
        private readonly Dictionary<TimelineAsset, (PlayableDirector, Action)> _distinctSfxTimelineDictionary = new();

        // use for Get Volume on fadeOut
        private readonly Dictionary<PlayableDirector, float> _sfxAsBgmPlayableDirectors = new();
        private readonly List<AudioClipData> _sfxAsBgmAudioClipData = new();

        // Manage FadeOut
        private readonly Dictionary<Object, (Coroutine, Action)> _fadeOutSfxAsBgmCoroutine = new();

        // Manage IgnoreTimeScale AudioSources,  PlayableDirector Works by self
        private readonly List<AudioSource> _ignoreTimeScaleAudioSources = new();

        // Update Volume & Manage ObjectPool
        private readonly List<AudioSource> _sfxAudioSourcePool = new();
        private readonly List<PlayableDirector> _playableDirectorPool = new();

        private const float ReducePercentage = 0.2f;
        private const float FadeSec = .5f;

        private bool IsReduced
        {
            get => _isReduced;
            set
            {
                if (isDebug)
                {
                    Debug.Log($"Set Audio Reduce {value}");
                }

                _isReduced = value;
                UpdateVolume();
            }
        }

        private static AudioManager Create()
        {
            var sceneLoaderPrefab = Resources.Load<AudioManager>("AudioManager");
            return Instantiate(sceneLoaderPrefab);
        }

        public void PlaySfx(Object audioObject, float volume = 1f, bool isOneShot = true,
            bool ignoreTimeScale = false)
        {
            if (audioObject is AudioClip audioClip)
            {
                PlaySfx(audioClip, volume, isOneShot, ignoreTimeScale);
            }
            else if (audioObject is TimelineAsset timelineAsset)
            {
                PlaySfx(timelineAsset, volume, isOneShot, ignoreTimeScale);
            }
        }

        public void PlaySfx(TimelineAsset timelineAsset, float volume = 1f, bool isOneShot = true,
            bool ignoreTimeScale = false)
        {
            if (!timelineAsset)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (!isOneShot && IsPlayingSfx(timelineAsset))
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Play Sfx - {timelineAsset}");
            }

            var playableDirector = ObjectPoolHelper.Instance.Get<PlayableDirector>();
            _playableDirectorPool.Add(playableDirector);
            playableDirector.gameObject.name = timelineAsset.name;
            playableDirector.playableAsset = timelineAsset;

            if (ignoreTimeScale)
            {
                playableDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            }

            var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            foreach (var audioTrack in audioTracks)
            {
                playableDirector.SetGenericBinding(audioTrack, sfx);
            }

            SetAudioTrackVolume(timelineAsset, volume);

            playableDirector.Play();

            var coroutine = StartCoroutine(WaitEnd((float) playableDirector.duration, ignoreTimeScale, () =>
            {
                if (!isOneShot)
                {
                    if (_distinctSfxTimelineDictionary.TryGetValue(timelineAsset, out var keyValue))
                    {
                        keyValue.Item2?.Invoke();
                    }
                }
                else
                {
                    _playableDirectorPool.Remove(playableDirector);
                    ObjectPoolHelper.Instance.Release(playableDirector);
                }
            }));

            if (!isOneShot)
            {
                if (isDebug)
                {
                    Debug.LogWarning(
                        $"중복 불가능 추가, {Time.time}, {(float) playableDirector.duration}, {Time.timeScale}, {(float) playableDirector.duration / Time.timeScale}");
                }

                _distinctSfxTimelineDictionary.Add(timelineAsset, (playableDirector, () =>
                {
                    if (isDebug)
                    {
                        Debug.LogWarning($"삭제, {Time.time}, {playableDirector.duration}");
                    }

                    _distinctSfxTimelineDictionary.Remove(timelineAsset);
                    _playableDirectorPool.Remove(playableDirector);
                    ObjectPoolHelper.Instance.Release(playableDirector);
                    StopCoroutine(coroutine);
                }));
            }
        }

        public void PlaySfx(AudioClip audioClip, float volume = 1f, bool isOneShot = true, bool ignoreTimeScale = false)
        {
            if (!audioClip)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (!isOneShot && IsPlayingSfx(audioClip))
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Play Sfx - {audioClip}\n" +
                          $"length: {audioClip.length}\n" +
                          $"ignoreTimeScale: {ignoreTimeScale}");
            }

            AudioSource audioSource;
            if (ignoreTimeScale)
            {
                audioSource = ObjectPoolHelper.Instance.Get<AudioSource>();
                audioSource.mute = sfx.mute;
                _sfxAudioSourcePool.Add(audioSource);
                audioSource.outputAudioMixerGroup = sfx.outputAudioMixerGroup;
                UpdateVolume();
                audioSource.pitch = 1;
                _ignoreTimeScaleAudioSources.Add(audioSource);
            }
            else
            {
                audioSource = sfx;
            }

            audioSource.PlayOneShot(audioClip, volume);

            var coroutine = StartCoroutine(WaitEnd(audioClip.length, ignoreTimeScale,
                () =>
                {
                    if (!isOneShot)
                    {
                        if (_distinctSfxClipDictionary.TryGetValue(audioClip, out var keyValue))
                        {
                            keyValue.Item2?.Invoke();
                        }
                    }
                    else if (ignoreTimeScale)
                    {
                        _sfxAudioSourcePool.Remove(audioSource);
                        _ignoreTimeScaleAudioSources.Remove(audioSource);
                        ObjectPoolHelper.Instance.Release(audioSource);
                    }
                }));

            if (!isOneShot)
            {
                if (isDebug)
                {
                    Debug.LogWarning(
                        $"중복 불가능 추가, {Time.time}, {audioClip.length}, {Time.timeScale}, {audioClip.length / Time.timeScale}");
                }

                _distinctSfxClipDictionary.Add(audioClip, (Time.time, () =>
                {
                    if (isDebug)
                    {
                        Debug.LogWarning($"삭제, {Time.time}, {audioClip.length}");
                    }

                    if (ignoreTimeScale)
                    {
                        _sfxAudioSourcePool.Remove(audioSource);
                        _ignoreTimeScaleAudioSources.Remove(audioSource);
                        ObjectPoolHelper.Instance.Release(audioSource);
                    }

                    _distinctSfxClipDictionary.Remove(audioClip);
                    StopCoroutine(coroutine);
                }));
            }
        }

        public void PlaySfxAsBgm(Object audioObject, float volume = 1f, bool isFade = false,
            bool ignoreTimeScale = false, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (audioObject is AudioClip audioClip)
            {
                PlaySfxAsBgm(audioClip, volume, isFade, ignoreTimeScale, fadeSec, animationCurve);
            }
            else if (audioObject is TimelineAsset timelineAsset)
            {
                PlaySfxAsBgm(timelineAsset, volume, isFade, ignoreTimeScale, fadeSec, animationCurve);
            }
        }

        private void PlaySfxAsBgm(AudioClip audioClip, float volume = 1f, bool isFade = false,
            bool ignoreTimeScale = false, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (!audioClip)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (_fadeOutSfxAsBgmCoroutine.ContainsKey(audioClip))
            {
                // Debug.LogWarning($"Stop FadeOut Coroutine {audioClip}");
                StopCoroutine(_fadeOutSfxAsBgmCoroutine[audioClip].Item1);
                _fadeOutSfxAsBgmCoroutine[audioClip].Item2?.Invoke();
            }

            if (_sfxAsBgmAudioClipData.Any(item => item.AudioSource.clip == audioClip))
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Play Sfx - {audioClip}");
            }

            var audioSource = ObjectPoolHelper.Instance.Get<AudioSource>();
            var audioData = new AudioClipData {AudioSource = audioSource, Volume = volume};
            audioSource.outputAudioMixerGroup = sfx.outputAudioMixerGroup;
            UpdateVolume();
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.mute = sfx.mute;

            if (ignoreTimeScale)
            {
                audioSource.pitch = 1;
                _ignoreTimeScaleAudioSources.Add(audioSource);
            }
            else
            {
                if (TimeScaleHelper.GetIsStop())
                {
                    audioSource.pitch = TimeScaleHelper.GetTimeScale();
                }
            }

            // 1. X -> Fadein(new)
            _sfxAudioSourcePool.Add(audioSource);
            _sfxAsBgmAudioClipData.Add(audioData);
            audioSource.Play();

            if (isFade)
            {
                animationCurve ??= AnimationCurve.Linear(0, 0, 1, 1);

                if (fadeSec <= 0)
                {
                    fadeSec = FadeSec;
                }

                StartCoroutine(FadeInAudioSource(audioData, fadeSec,
                    animationCurve));
            }
        }

        private void PlaySfxAsBgm(TimelineAsset timelineAsset, float volume = 1f, bool isFade = false,
            bool ignoreTimeScale = false, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (!timelineAsset)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (_fadeOutSfxAsBgmCoroutine.ContainsKey(timelineAsset))
            {
                // Debug.LogWarning($"Stop FadeOut Coroutine {timelineAsset}");
                StopCoroutine(_fadeOutSfxAsBgmCoroutine[timelineAsset].Item1);
                _fadeOutSfxAsBgmCoroutine[timelineAsset].Item2?.Invoke();
            }

            if (_sfxAsBgmPlayableDirectors.Any(item => item.Key.playableAsset == timelineAsset))
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Play Sfx - {timelineAsset}");
            }

            var playableDirector = ObjectPoolHelper.Instance.Get<PlayableDirector>();
            _playableDirectorPool.Add(playableDirector);
            playableDirector.gameObject.name = timelineAsset.name;
            playableDirector.playableAsset = timelineAsset;
            playableDirector.extrapolationMode = DirectorWrapMode.Loop;

            if (ignoreTimeScale)
            {
                playableDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            }

            var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            foreach (var audioTrack in audioTracks)
            {
                playableDirector.SetGenericBinding(audioTrack, sfx);
            }

            // 1. X -> Fadein(new)
            _sfxAsBgmPlayableDirectors.Add(playableDirector, volume);
            playableDirector.Play();

            if (isFade)
            {
                animationCurve ??= AnimationCurve.Linear(0, 0, 1, 1);

                if (fadeSec <= 0)
                {
                    fadeSec = FadeSec;
                }

                StartCoroutine(FadeInPlayableDirectorSfx(playableDirector, fadeSec, volume, animationCurve));
            }
        }

        public void PlayBgmWithFade(Object audioObject, float volume = 1f, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (audioObject is AudioClip audioClip)
            {
                PlayBgmWithFade(audioClip, volume, fadeSec, animationCurve);
            }
            else if (audioObject is TimelineAsset timelineAsset)
            {
                PlayBgmWithFade(timelineAsset, volume, fadeSec, animationCurve);
            }
        }

        public void PlayBgmWithFade(TimelineAsset timelineAsset, float volume = 1f, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (!timelineAsset)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (fadeSec <= 0)
            {
                fadeSec = FadeSec;
            }

            animationCurve ??= AnimationCurve.Linear(0, 0, 1, 1);

            if ((bgmPlayableDirector.playableAsset == null && bgm.clip == null) || _fadeInCoroutine != null)
            {
                // FadeIn -> StopCoroutine -> PlayBgm (Contain Reset Volume) -> FadeIn
                if (_fadeInCoroutine != null)
                {
                    StopCoroutine(_fadeInCoroutine);
                    _fadeInCoroutine = null;
                }

                PlayBgm(timelineAsset, volume, fadeSec, animationCurve);
            }
            else
            {
                // FadeOut (뒤에 1번 Bgm을 새로 실행할 예정이었는데 또 2번 Bgm으로 PlayBgm을 실행함) -> FadeOut 이후 2번 Bgm을 실행
                // FadeOut -> Play & FadeIn ver2

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);

                    var t = Mathf.InverseLerp(0, GetBgmSourceVolume(), bgm.volume);

                    _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, animationCurve,
                        () => { PlayBgm(timelineAsset, volume, fadeSec, animationCurve); }, t));
                }
                else
                {
                    if (timelineAsset == bgmPlayableDirector.playableAsset as TimelineAsset)
                    {
                        PlayBgm(timelineAsset, volume, fadeSec, animationCurve);
                    }
                    else
                    {
                        _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, animationCurve,
                            () => { PlayBgm(timelineAsset, volume, fadeSec, animationCurve); }));
                    }
                }
            }
        }

        public void PlayBgmWithFade(AudioClip audioClip, float volume = 1f, float fadeSec = -1,
            AnimationCurve animationCurve = null)
        {
            if (!audioClip)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            if (fadeSec <= 0)
            {
                fadeSec = FadeSec;
            }

            animationCurve ??= AnimationCurve.Linear(0, 0, 1, 1);

            if ((bgmPlayableDirector.playableAsset == null && bgm.clip == null) || _fadeInCoroutine != null)
            {
                // FadeIn -> StopCoroutine -> PlayBgm (Contain Reset Volume) -> FadeIn
                if (_fadeInCoroutine != null)
                {
                    StopCoroutine(_fadeInCoroutine);
                    _fadeInCoroutine = null;
                }

                PlayBgm(audioClip, volume, fadeSec, animationCurve);
            }
            else
            {
                // FadeOut (뒤에 1번 Bgm을 새로 실행할 예정이었는데 또 2번 Bgm으로 PlayBgm을 실행함) -> FadeOut 이후 2번 Bgm을 실행
                // FadeOut -> Play & FadeIn ver2

                if (_fadeOutCoroutine != null)
                {
                    StopCoroutine(_fadeOutCoroutine);

                    var t = Mathf.InverseLerp(0, GetBgmSourceVolume(), bgm.volume);

                    _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, animationCurve,
                        () => { PlayBgm(audioClip, volume, fadeSec, animationCurve); }, t));
                }
                else
                {
                    // if (audioClip == bgm.clip) -> No Fade Out
                    if (audioClip == bgm.clip)
                    {
                        PlayBgm(audioClip, volume, fadeSec, animationCurve);
                    }
                    else
                    {
                        // FadeOut And Play Or Just Play
                        _fadeOutCoroutine = StartCoroutine(FadeOutBgm(fadeSec, animationCurve,
                            () => { PlayBgm(audioClip, volume, fadeSec, animationCurve); }
                        ));
                    }
                }
            }
        }

        private void PlayBgm(TimelineAsset timelineAsset, float volume, float fadeSec, AnimationCurve animationCurve)
        {
            if (!timelineAsset)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            BGMPlayValue = volume;
            if (isDebug)
            {
                Debug.Log($"Play bgm - {timelineAsset}");
            }

            if (bgmPlayableDirector.playableAsset != timelineAsset)
            {
                ResetBgm();

                var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();

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
                _fadeInCoroutine = StartCoroutine(FadeInBgm(fadeSec, animationCurve));
            }
        }

        private void PlayBgm(AudioClip audioClip, float volume, float fadeSec, AnimationCurve animationCurve)
        {
            if (!audioClip)
            {
                if (isDebug)
                {
                    Debug.LogWarning("Audio 비어있음");
                }

                return;
            }

            BGMPlayValue = volume;
            if (isDebug)
            {
                Debug.Log($"Play bgm - {audioClip}");
            }

            if (bgm.clip != audioClip)
            {
                ResetBgm();
                bgm.clip = audioClip;
            }

            if (IsReduced)
            {
                IsReduced = false;
            }

            // 항상 FadeIn
            bgm.Play();
            _fadeInCoroutine = StartCoroutine(FadeInBgm(fadeSec, animationCurve));
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
            bgm.volume = GetBgmSourceVolume();
            sfx.volume = sfx.volume;

            // sfxList - except sfxAsBgm
            var sfxAudioSources = _sfxAudioSourcePool.Except(_sfxAsBgmAudioClipData.Select(item => item.AudioSource));
            foreach (var sfxAudioSource in sfxAudioSources)
            {
                sfxAudioSource.volume = sfx.volume;
            }

            foreach (var audioData in _sfxAsBgmAudioClipData.Where(audioData => !audioData.IsFading))
            {
                audioData.AudioSource.volume = audioData.Volume * sfx.volume;
            }
        }

        private float GetBgmSourceVolume()
        {
            return _isReduced ? BGMPlayValue * BGMVolumeValue * ReducePercentage : BGMPlayValue * BGMVolumeValue;
        }

        public void UpdateTimeScale(float pitch)
        {
            sfx.pitch = pitch;
            bgm.pitch = pitch;

            // sfxList - except _ignoreTimeScaleAudioSources
            var query = _sfxAsBgmAudioClipData.Select(item => item.AudioSource).Except(_ignoreTimeScaleAudioSources);
            foreach (var audioSource in query)
            {
                audioSource.pitch = pitch;
            }
        }

        private void ResetBgm()
        {
            StopAllCoroutines();

            if (isDebug)
            {
                Debug.LogWarning("Reset Bgm");
            }

            bgmPlayableDirector.Stop();
            bgmPlayableDirector.playableAsset = null;
            bgmPlayableDirector.time = 0;
        }

        public void StopBgm(bool isContinue = true)
        {
            StopAllCoroutines();

            if (!isContinue)
            {
                IsReduced = false;
            }

            if (isDebug)
            {
                Debug.LogWarning("Stop Bgm");
            }

            if (!isContinue)
            {
                bgm.clip = null;
                bgm.Stop();
            }

            BGMPlayValue = 1f;

            bgmPlayableDirector.Stop();
            bgmPlayableDirector.playableAsset = null;
            bgmPlayableDirector.time = 0;
        }

        private void StopSfx()
        {
            if (isDebug)
            {
                Debug.LogWarning("Stop Sfx");
            }

            sfx.Stop();
        }

        public void StopSfxAsBgm(Object audioObject, bool isFade = false)
        {
            switch (audioObject)
            {
                case AudioClip audioClip:
                    StopSfxAsBgm(audioClip, isFade);
                    break;
                case TimelineAsset timelineAsset:
                    StopSfxAsBgm(timelineAsset, isFade);
                    break;
            }
        }

        private void StopSfxAsBgm(AudioClip audioClip, bool isFade = false)
        {
            var audioData = _sfxAsBgmAudioClipData.Find(item => item.AudioSource.clip == audioClip);

            if (audioData == null)
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Stop Sfx With Fade - {audioClip}, {isFade}");
            }

            if (isFade)
            {
                var coroutine = StartCoroutine(FadeOutAudioSource(audioData, () =>
                {
                    // Debug.LogWarning("Remove FadeOut Coroutine");
                    _fadeOutSfxAsBgmCoroutine.Remove(audioClip);
                    EndAction();
                }));

                // Debug.LogWarning("Add FadeOut Coroutine");
                _fadeOutSfxAsBgmCoroutine.Add(audioClip, (coroutine, () =>
                {
                    // Debug.LogWarning("Remove FadeOut Coroutine");
                    _fadeOutSfxAsBgmCoroutine.Remove(audioClip);
                    EndAction();
                }));
            }
            else
            {
                EndAction();
            }

            return;

            void EndAction()
            {
                if (_ignoreTimeScaleAudioSources.Contains(audioData.AudioSource))
                {
                    _ignoreTimeScaleAudioSources.Remove(audioData.AudioSource);
                }

                _sfxAsBgmAudioClipData.Remove(audioData);
                if (_sfxAudioSourcePool.Contains(audioData.AudioSource))
                {
                    _sfxAudioSourcePool.Remove(audioData.AudioSource);
                    ObjectPoolHelper.Instance.Release(audioData.AudioSource);
                }
            }
        }

        private void StopSfxAsBgm(TimelineAsset timelineAsset, bool isFade = false)
        {
            var playableDirector =
                _sfxAsBgmPlayableDirectors.FirstOrDefault(item => item.Key.playableAsset == timelineAsset);

            if (playableDirector.Key == null)
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log($"Stop Sfx With Fade - {timelineAsset}");
            }

            if (isFade)
            {
                var coroutine =
                    StartCoroutine(FadeOutPlayableDirector(playableDirector.Key, playableDirector.Value, () =>
                    {
                        // Debug.LogWarning("Remove FadeOut Coroutine");
                        _fadeOutSfxAsBgmCoroutine.Remove(timelineAsset);
                        EndAction();
                    }));
                // Debug.LogWarning("Add FadeOut Coroutine");
                _fadeOutSfxAsBgmCoroutine.Add(timelineAsset, (coroutine, () =>
                {
                    // Debug.LogWarning("Remove FadeOut Coroutine");
                    _fadeOutSfxAsBgmCoroutine.Remove(timelineAsset);
                    EndAction();
                }));
            }
            else
            {
                EndAction();
            }

            return;

            void EndAction()
            {
                if (isDebug)
                {
                    Debug.LogWarning($"FadeOut End - {timelineAsset}");
                }

                _sfxAsBgmPlayableDirectors.Remove(playableDirector.Key);
                _playableDirectorPool.Remove(playableDirector.Key);
                ObjectPoolHelper.Instance.Release(playableDirector.Key);
            }
        }

        public void StopAudio()
        {
            if (isDebug)
            {
                Debug.LogWarning("Stop Audio");
            }

            StopSfx();

            foreach (var sfxAudioSource in _sfxAudioSourcePool)
            {
                ObjectPoolHelper.Instance.Release(sfxAudioSource);
            }

            foreach (var playableDirector in _playableDirectorPool)
            {
                ObjectPoolHelper.Instance.Release(playableDirector);
            }

            _sfxAsBgmPlayableDirectors.Clear();
            _sfxAsBgmAudioClipData.Clear();
            _distinctSfxClipDictionary.Clear();
            _distinctSfxTimelineDictionary.Clear();
            _ignoreTimeScaleAudioSources.Clear();
            _sfxAudioSourcePool.Clear();
            _playableDirectorPool.Clear();

            StopBgm(false);
        }

        private bool IsPlayingSfx(TimelineAsset timelineAsset)
        {
            // if IsPlayingSfx Any SfxAudio return true
            if (!_distinctSfxTimelineDictionary.ContainsKey(timelineAsset))
            {
                if (isDebug)
                {
                    Debug.LogWarning("미보유 중");
                }

                return false;
            }

            var (playableDirector, endAction) = _distinctSfxTimelineDictionary[timelineAsset];

            if (Math.Abs(playableDirector.duration - playableDirector.time) <=
                1 / ((TimelineAsset) playableDirector.playableAsset).editorSettings.frameRate ||
                playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid())
            {
                if (isDebug)
                {
                    Debug.LogWarning("실행 종료된 상태임");
                }

                endAction?.Invoke();
                return false;
            }

            if (isDebug)
            {
                Debug.LogWarning("실행 중");
            }

            return true;
        }

        private bool IsPlayingSfx(AudioClip audioClip)
        {
            if (!_distinctSfxClipDictionary.ContainsKey(audioClip))
            {
                return false;
            }

            var (length, endAction) = _distinctSfxClipDictionary[audioClip];

            if (Time.time - length < audioClip.length)
            {
                return true;
            }

            endAction?.Invoke();
            return false;
        }

        private IEnumerator FadeInBgm(float fadeSec, AnimationCurve animationCurve)
        {
            var t = 0f;
            var bgmPlayValue = BGMPlayValue;
            while (t <= 1f)
            {
                t += Time.deltaTime / fadeSec;
                var value = animationCurve.Evaluate(t);
                BGMPlayValue = Mathf.Lerp(0, bgmPlayValue, value);
                yield return null;
            }

            BGMPlayValue = bgmPlayValue;
            _fadeInCoroutine = null;
        }

        private IEnumerator FadeOutBgm(float fadeSec, AnimationCurve animationCurve, Action onEndAction, float t = 1f)
        {
            var bgmPlayValue = BGMPlayValue;
            while (t >= 0f)
            {
                t -= Time.deltaTime / fadeSec;
                var value = animationCurve.Evaluate(t);
                BGMPlayValue = Mathf.Lerp(bgmPlayValue, 0, value);
                yield return null;
            }

            BGMPlayValue = bgmPlayValue;
            _fadeOutCoroutine = null;
            onEndAction.Invoke();
        }

        private IEnumerator FadeInAudioSource(AudioClipData audioClipData, float fadeSec,
            AnimationCurve animationCurve)
        {
            audioClipData.IsFading = true;

            var t = 0f;
            while (t <= 1f)
            {
                t += Time.deltaTime / fadeSec;
                var value = animationCurve.Evaluate(t);
                audioClipData.AudioSource.volume = Mathf.Lerp(0, sfx.volume * audioClipData.Volume, value);
                yield return null;
            }

            audioClipData.IsFading = false;
        }

        private IEnumerator FadeOutAudioSource(AudioClipData audioClipData, Action fadeEndAction)
        {
            audioClipData.IsFading = true;

            var t = 1f;
            while (t >= 0)
            {
                t -= Time.deltaTime / FadeSec;
                audioClipData.AudioSource.volume = Mathf.Lerp(0, sfx.volume * audioClipData.Volume, t);
                yield return null;
            }

            audioClipData.IsFading = false;
            fadeEndAction?.Invoke();
        }

        private static IEnumerator FadeInPlayableDirectorSfx(PlayableDirector playableDirector, float volume,
            float fadeSec, AnimationCurve animationCurve)
        {
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;

            if (timelineAsset == null)
            {
                yield break;
            }

            var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();

            var minSec = (float) Math.Min(timelineAsset.duration, fadeSec);

            foreach (var audioTrack in audioTracks)
            {
                if (audioTrack.curves != null)
                {
                    audioTrack.curves.ClearCurves();
                }

                audioTrack.CreateCurves("NameOfAnimationClip");

                var firstTime = animationCurve.keys[0].time;
                var lastTime = animationCurve.keys[animationCurve.length - 1].time;

                var minValue = animationCurve.keys.Min(item => item.value);
                var maxValue = animationCurve.keys.Max(item => item.value);

                // data -> new curve (0 ~ fadeSec, 0 ~ volume)

                foreach (var animationCurveKey in animationCurve.keys)
                {
                    var targetTime = (animationCurveKey.time - firstTime) / (lastTime + firstTime) * fadeSec;
                    var targetValue = (animationCurveKey.time - minValue) / (minValue + maxValue) * volume;

                    var newKeyFrame = animationCurveKey;
                    newKeyFrame.time = targetTime;
                    newKeyFrame.value = targetValue;

                    var index = Array.FindIndex(animationCurve.keys, item => item.Equals(animationCurveKey));
                    animationCurve.MoveKey(index, newKeyFrame);
                }

                // audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume", AnimationCurve.Linear(0, 0, minSec, 1));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume", animationCurve);
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "stereoPan",
                    AnimationCurve.Linear(0, 0, 1, 0));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "spatialBlend",
                    AnimationCurve.Linear(0, 0, 1, 0));
            }
            // timelineAsset duration보다 긴 경우 fadeSec가 긴 경우 무시, 최대값 timelineAsset duration으로  

            yield return YieldInstructionProvider.WaitForSeconds(minSec);

            SetAudioTrackVolume(timelineAsset, volume);
        }

        private static IEnumerator FadeOutPlayableDirector(PlayableDirector playableDirector, float volume,
            Action fadeEndAction)
        {
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;

            if (timelineAsset == null)
            {
                yield break;
            }

            var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>().ToArray();
            var minSec = (float) Math.Min(timelineAsset.duration, playableDirector.time + FadeSec);

            Debug.LogWarning($"fadeout - {minSec},   {timelineAsset.duration}, {playableDirector.time + FadeSec}");
            foreach (var audioTrack in audioTracks)
            {
                if (audioTrack.curves != null)
                {
                    audioTrack.curves.ClearCurves();
                }

                audioTrack.CreateCurves("NameOfAnimationClip");

                // data -> new curve (cur ~ (cur + fadeSec or end), 0 ~ volume)

                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume",
                    AnimationCurve.Linear((float) playableDirector.time, volume, minSec, 0));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "stereoPan",
                    AnimationCurve.Linear(0, 0, 1, 0));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "spatialBlend",
                    AnimationCurve.Linear(0, 0, 1, 0));
            }
            // timelineAsset duration보다 긴 경우 fadeSec가 긴 경우 무시, 최대값 timelineAsset duration으로  

            playableDirector.RebuildGraph();

            var minWaitSec = (float) Math.Min(timelineAsset.duration - playableDirector.time, FadeSec);
            yield return YieldInstructionProvider.WaitForSeconds(minWaitSec);

            foreach (var audioTrack in audioTracks)
            {
                if (audioTrack.curves != null)
                {
                    audioTrack.curves.ClearCurves();
                }
            }

            fadeEndAction?.Invoke();
        }

        private static IEnumerator WaitEnd(float waitSec, bool ignoreTimeScale, Action onEndAction)
        {
            if (ignoreTimeScale)
            {
                yield return YieldInstructionProvider.WaitForSecondsRealtime(waitSec);
            }
            else
            {
                yield return YieldInstructionProvider.WaitForSeconds(waitSec);
            }

            onEndAction?.Invoke();
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

        /// <param name="audioSourceType"> Bgm or Sfx </param>
        /// <param name="volumeValue"> 0 ~ 1</param>
        public void SetVolume(AudioSourceType audioSourceType, float volumeValue)
        {
            if (audioSourceType == AudioSourceType.Bgm)
            {
                BGMVolumeValue = volumeValue;
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

            foreach (var sfxAudioSource in _sfxAudioSourcePool)
            {
                sfxAudioSource.mute = isMute;
            }
        }

        // return 0 ~ 1 volume
        public float GetVolume(AudioSourceType audioSourceType)
        {
            return audioSourceType switch
            {
                AudioSourceType.Sfx => sfx.volume,
                AudioSourceType.Bgm => BGMVolumeValue,
                _ => -1
            };
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
            PlayerPrefs.SetFloat("Bgm", BGMVolumeValue);
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

        private static void SetAudioTrackVolume(TimelineAsset timelineAsset, float volume)
        {
            var audioTracks = timelineAsset.GetOutputTracks().OfType<AudioTrack>();

            foreach (var audioTrack in audioTracks)
            {
                if (audioTrack.curves != null)
                {
                    audioTrack.curves.ClearCurves();
                }

                audioTrack.CreateCurves("NameOfAnimationClip");

                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "volume",
                    AnimationCurve.Linear(0, volume, 1, volume));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "stereoPan",
                    AnimationCurve.Linear(0, 0, 1, 0));
                audioTrack.curves.SetCurve(string.Empty, typeof(AudioTrack), "spatialBlend",
                    AnimationCurve.Linear(0, 0, 1, 0));
            }
        }

        private void OnApplicationQuit()
        {
            SaveAudio();
        }
    }
}