using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Rhythm-synchronized audio management system
    /// 리듬 동기화 오디오 관리 시스템
    ///
    /// Manages: Music playback, SFX, rhythm synchronization, beat callbacks
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("▶ Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;

        [Header("▶ Audio Mixer")]
        public AudioMixer audioMixer;

        [Header("▶ Rhythm Sync")]
        public bool enableRhythmSync = true;
        [Range(60, 200)]
        public float currentBPM = 120f;
        [Tooltip("Offset in seconds to sync beats")]
        public float beatOffset = 0f;

        [Header("▶ Audio Pools")]
        [Range(5, 50)]
        public int sfxPoolSize = 20;

        [Header("▶ Debug")]
        public bool showBeatVisualization = false;
        public bool logBeatEvents = false;

        // Events
        public UnityEvent OnBeat;
        public UnityEvent OnMeasure; // Every 4 beats

        // Rhythm tracking
        private float _songPosition = 0f;
        private float _songPositionInBeats = 0f;
        private int _currentBeat = 0;
        private int _lastReportedBeat = -1;
        private float _secPerBeat = 0f;

        // Audio pools
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private int _nextSFXSource = 0;

        // Music management
        private AudioClip _currentMusicClip;
        private bool _isMusicPlaying = false;
        private Coroutine _fadeCoroutine;

        // SFX library
        private Dictionary<string, AudioClip> _sfxLibrary = new Dictionary<string, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            InitializeSFXPool();
            UpdateBPM(currentBPM);
        }

        void Update()
        {
            if (enableRhythmSync && _isMusicPlaying)
            {
                UpdateRhythmTracking();
            }
        }

        #region Initialization

        private void InitializeAudioSources()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;

                if (audioMixer != null)
                {
                    musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
                }
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;

                if (audioMixer != null)
                {
                    sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                }
            }

            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;

                if (audioMixer != null)
                {
                    ambientSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                }
            }

            Debug.Log("🎵 Audio sources initialized");
        }

        private void InitializeSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject poolObj = new GameObject($"SFXPool_{i}");
                poolObj.transform.SetParent(transform);
                AudioSource source = poolObj.AddComponent<AudioSource>();
                source.playOnAwake = false;

                if (audioMixer != null)
                {
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                }

                _sfxPool.Add(source);
            }

            Debug.Log($"🎵 SFX pool initialized ({sfxPoolSize} sources)");
        }

        #endregion

        #region Music Playback

        public void PlayMusic(AudioClip clip, float fadeInDuration = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot play null audio clip");
                return;
            }

            // Stop existing fade
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _currentMusicClip = clip;

            if (fadeInDuration > 0f)
            {
                _fadeCoroutine = StartCoroutine(FadeInMusic(clip, fadeInDuration));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
                _isMusicPlaying = true;
                ResetRhythmTracking();
            }

            Debug.Log($"🎵 Playing music: {clip.name}");
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (fadeOutDuration > 0f)
            {
                _fadeCoroutine = StartCoroutine(FadeOutMusic(fadeOutDuration));
            }
            else
            {
                musicSource.Stop();
                _isMusicPlaying = false;
            }

            Debug.Log("🎵 Music stopped");
        }

        public void PauseMusic()
        {
            musicSource.Pause();
            _isMusicPlaying = false;
            Debug.Log("⏸️ Music paused");
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
            _isMusicPlaying = true;
            Debug.Log("▶️ Music resumed");
        }

        private IEnumerator FadeInMusic(AudioClip clip, float duration)
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            _isMusicPlaying = true;
            ResetRhythmTracking();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            musicSource.volume = 1f;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
            _isMusicPlaying = false;
            _fadeCoroutine = null;
        }

        #endregion

        #region SFX Playback

        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetNextSFXSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        public void PlaySFX(string clipName, float volume = 1f, float pitch = 1f)
        {
            if (_sfxLibrary.ContainsKey(clipName))
            {
                PlaySFX(_sfxLibrary[clipName], volume, pitch);
            }
            else
            {
                Debug.LogWarning($"SFX not found in library: {clipName}");
            }
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        public void PlaySFXOnBeat(AudioClip clip, float volume = 1f)
        {
            // Wait until next beat to play
            StartCoroutine(PlayOnNextBeat(clip, volume));
        }

        private IEnumerator PlayOnNextBeat(AudioClip clip, float volume)
        {
            float timeUntilNextBeat = _secPerBeat - (_songPosition % _secPerBeat);
            yield return new WaitForSeconds(timeUntilNextBeat);
            PlaySFX(clip, volume);
        }

        private AudioSource GetNextSFXSource()
        {
            AudioSource source = _sfxPool[_nextSFXSource];
            _nextSFXSource = (_nextSFXSource + 1) % _sfxPool.Count;
            return source;
        }

        public void RegisterSFX(string name, AudioClip clip)
        {
            if (!_sfxLibrary.ContainsKey(name))
            {
                _sfxLibrary.Add(name, clip);
            }
        }

        #endregion

        #region Ambient Audio

        public void PlayAmbient(AudioClip clip, float volume = 0.5f, float fadeInDuration = 2f)
        {
            if (clip == null) return;

            ambientSource.clip = clip;

            if (fadeInDuration > 0f)
            {
                StartCoroutine(FadeInAmbient(volume, fadeInDuration));
            }
            else
            {
                ambientSource.volume = volume;
                ambientSource.Play();
            }
        }

        public void StopAmbient(float fadeOutDuration = 2f)
        {
            if (fadeOutDuration > 0f)
            {
                StartCoroutine(FadeOutAmbient(fadeOutDuration));
            }
            else
            {
                ambientSource.Stop();
            }
        }

        private IEnumerator FadeInAmbient(float targetVolume, float duration)
        {
            ambientSource.volume = 0f;
            ambientSource.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            ambientSource.volume = targetVolume;
        }

        private IEnumerator FadeOutAmbient(float duration)
        {
            float startVolume = ambientSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            ambientSource.volume = 0f;
            ambientSource.Stop();
        }

        #endregion

        #region Rhythm Tracking

        private void UpdateRhythmTracking()
        {
            if (musicSource == null || !musicSource.isPlaying) return;

            _songPosition = (float)(AudioSettings.dspTime - musicSource.time) + beatOffset;
            _songPositionInBeats = _songPosition / _secPerBeat;

            int currentBeat = Mathf.FloorToInt(_songPositionInBeats);

            if (currentBeat != _lastReportedBeat)
            {
                _currentBeat = currentBeat;
                _lastReportedBeat = currentBeat;

                // Trigger beat event
                OnBeat?.Invoke();

                if (logBeatEvents)
                {
                    Debug.Log($"♪ Beat {_currentBeat}");
                }

                // Check for measure (every 4 beats)
                if (_currentBeat % 4 == 0)
                {
                    OnMeasure?.Invoke();

                    if (logBeatEvents)
                    {
                        Debug.Log($"♫ Measure {_currentBeat / 4}");
                    }
                }
            }
        }

        public void UpdateBPM(float bpm)
        {
            currentBPM = bpm;
            _secPerBeat = 60f / bpm;
            Debug.Log($"🎵 BPM updated: {bpm} ({_secPerBeat:F3}s per beat)");
        }

        private void ResetRhythmTracking()
        {
            _songPosition = 0f;
            _songPositionInBeats = 0f;
            _currentBeat = 0;
            _lastReportedBeat = -1;
        }

        public float GetSongPosition() => _songPosition;
        public float GetSongPositionInBeats() => _songPositionInBeats;
        public int GetCurrentBeat() => _currentBeat;
        public float GetSecPerBeat() => _secPerBeat;
        public float GetBPM() => currentBPM;

        public float GetTimingOffset(float inputTime)
        {
            // Calculate how far from perfect beat the input was
            float beatPosition = inputTime / _secPerBeat;
            float beatFraction = beatPosition - Mathf.Floor(beatPosition);

            // Normalize to -0.5 to 0.5 (beat center is 0)
            if (beatFraction > 0.5f)
            {
                beatFraction -= 1f;
            }

            return beatFraction * _secPerBeat;
        }

        public bool IsOnBeat(float threshold = 0.1f)
        {
            float beatFraction = _songPositionInBeats - Mathf.Floor(_songPositionInBeats);
            return beatFraction < threshold || beatFraction > (1f - threshold);
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", ConvertToDecibels(volume));
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", ConvertToDecibels(volume));
            }
            else
            {
                musicSource.volume = volume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("SFXVolume", ConvertToDecibels(volume));
            }
        }

        private float ConvertToDecibels(float linear)
        {
            if (linear <= 0f) return -80f;
            return Mathf.Clamp(20f * Mathf.Log10(linear), -80f, 0f);
        }

        #endregion

        #region Utilities

        public bool IsMusicPlaying() => _isMusicPlaying;
        public AudioClip GetCurrentMusic() => _currentMusicClip;

        public void StopAllAudio()
        {
            StopMusic(0f);
            StopAmbient(0f);

            foreach (AudioSource source in _sfxPool)
            {
                source.Stop();
            }

            Debug.Log("🔇 All audio stopped");
        }

        #endregion

        #region Debug Visualization

        void OnGUI()
        {
            if (!showBeatVisualization) return;

            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Box("Audio Manager - Rhythm Sync");

            if (_isMusicPlaying)
            {
                GUILayout.Label($"BPM: {currentBPM}");
                GUILayout.Label($"Beat: {_currentBeat}");
                GUILayout.Label($"Position: {_songPosition:F2}s");
                GUILayout.Label($"Beats: {_songPositionInBeats:F2}");

                // Beat indicator
                float beatFraction = _songPositionInBeats - Mathf.Floor(_songPositionInBeats);
                GUILayout.HorizontalSlider(beatFraction, 0f, 1f);

                if (IsOnBeat(0.1f))
                {
                    GUILayout.Label("🎵 ON BEAT");
                }
            }
            else
            {
                GUILayout.Label("No music playing");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
