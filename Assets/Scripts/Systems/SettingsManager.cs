using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Centralized settings and options management
    /// 중앙 집중식 설정 및 옵션 관리
    ///
    /// Manages: Graphics, Audio, Gameplay, Controls, Accessibility
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("▶ Audio")]
        public AudioMixer audioMixer;
        [Tooltip("AudioMixer parameter names")]
        public string masterVolumeParam = "MasterVolume";
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";

        [Header("▶ Default Settings")]
        [Range(0f, 1f)]
        public float defaultMasterVolume = 0.8f;
        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.7f;
        [Range(0f, 1f)]
        public float defaultSFXVolume = 0.8f;

        private GameSettings _currentSettings;
        private const string SETTINGS_SAVE_KEY = "GameSettings";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            ApplySettings();
        }

        #region Load/Save

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_SAVE_KEY);
                _currentSettings = JsonUtility.FromJson<GameSettings>(json);
                Debug.Log("⚙️ Settings loaded");
            }
            else
            {
                // Create default settings
                _currentSettings = new GameSettings
                {
                    // Graphics
                    resolutionWidth = Screen.currentResolution.width,
                    resolutionHeight = Screen.currentResolution.height,
                    fullscreenMode = FullScreenMode.FullScreenWindow,
                    vsyncEnabled = true,
                    qualityLevel = QualitySettings.GetQualityLevel(),
                    targetFrameRate = 60,

                    // Audio
                    masterVolume = defaultMasterVolume,
                    musicVolume = defaultMusicVolume,
                    sfxVolume = defaultSFXVolume,
                    audioEnabled = true,

                    // Gameplay
                    showTimingIndicators = true,
                    screenShakeEnabled = true,
                    screenShakeIntensity = 1f,
                    showDamageNumbers = true,
                    autoSaveEnabled = true,
                    difficultyMultiplier = 1f,

                    // Accessibility
                    colorblindMode = ColorblindMode.None,
                    highContrastMode = false,
                    reducedMotion = false,
                    largeText = false,
                    subtitlesEnabled = true,

                    // Controls
                    mouseSensitivity = 1f,
                    invertYAxis = false,
                    controllerVibration = true
                };

                Debug.Log("⚙️ Default settings created");
            }
        }

        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(_currentSettings, true);
            PlayerPrefs.SetString(SETTINGS_SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("💾 Settings saved");
        }

        public void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(SETTINGS_SAVE_KEY);
            LoadSettings();
            ApplySettings();
            Debug.Log("🔄 Settings reset to defaults");
        }

        #endregion

        #region Apply Settings

        private void ApplySettings()
        {
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyGameplaySettings();
            ApplyAccessibilitySettings();
        }

        private void ApplyGraphicsSettings()
        {
            // Resolution
            Screen.SetResolution(
                _currentSettings.resolutionWidth,
                _currentSettings.resolutionHeight,
                _currentSettings.fullscreenMode
            );

            // Quality
            QualitySettings.SetQualityLevel(_currentSettings.qualityLevel, true);

            // VSync
            QualitySettings.vSyncCount = _currentSettings.vsyncEnabled ? 1 : 0;

            // Target FPS
            Application.targetFrameRate = _currentSettings.targetFrameRate;

            Debug.Log($"🎨 Graphics applied: {_currentSettings.resolutionWidth}x{_currentSettings.resolutionHeight}, " +
                      $"Quality: {_currentSettings.qualityLevel}, FPS: {_currentSettings.targetFrameRate}");
        }

        private void ApplyAudioSettings()
        {
            if (audioMixer == null)
            {
                Debug.LogWarning("AudioMixer not assigned!");
                return;
            }

            if (_currentSettings.audioEnabled)
            {
                // Convert 0-1 range to decibels (-80 to 0)
                float masterDB = ConvertToDecibels(_currentSettings.masterVolume);
                float musicDB = ConvertToDecibels(_currentSettings.musicVolume);
                float sfxDB = ConvertToDecibels(_currentSettings.sfxVolume);

                audioMixer.SetFloat(masterVolumeParam, masterDB);
                audioMixer.SetFloat(musicVolumeParam, musicDB);
                audioMixer.SetFloat(sfxVolumeParam, sfxDB);
            }
            else
            {
                // Mute all
                audioMixer.SetFloat(masterVolumeParam, -80f);
            }

            Debug.Log($"🔊 Audio applied: Master={_currentSettings.masterVolume:F2}, " +
                      $"Music={_currentSettings.musicVolume:F2}, SFX={_currentSettings.sfxVolume:F2}");
        }

        private float ConvertToDecibels(float linear)
        {
            // Convert 0-1 linear to -80 to 0 dB
            if (linear <= 0f) return -80f;
            return Mathf.Clamp(20f * Mathf.Log10(linear), -80f, 0f);
        }

        private void ApplyGameplaySettings()
        {
            // These would be accessed by other systems
            // e.g., CameraController.Instance.SetShakeIntensity(_currentSettings.screenShakeIntensity);

            Debug.Log($"🎮 Gameplay settings applied");
        }

        private void ApplyAccessibilitySettings()
        {
            // Apply colorblind filter
            // Apply high contrast mode
            // etc.

            Debug.Log($"♿ Accessibility settings applied: Colorblind={_currentSettings.colorblindMode}");
        }

        #endregion

        #region Graphics Settings

        public void SetResolution(int width, int height, FullScreenMode mode)
        {
            _currentSettings.resolutionWidth = width;
            _currentSettings.resolutionHeight = height;
            _currentSettings.fullscreenMode = mode;
            Screen.SetResolution(width, height, mode);
            SaveSettings();
        }

        public void SetQualityLevel(int level)
        {
            _currentSettings.qualityLevel = level;
            QualitySettings.SetQualityLevel(level, true);
            SaveSettings();
        }

        public void SetVSync(bool enabled)
        {
            _currentSettings.vsyncEnabled = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            SaveSettings();
        }

        public void SetTargetFrameRate(int fps)
        {
            _currentSettings.targetFrameRate = fps;
            Application.targetFrameRate = fps;
            SaveSettings();
        }

        public List<Resolution> GetAvailableResolutions()
        {
            List<Resolution> resolutions = new List<Resolution>();
            foreach (var res in Screen.resolutions)
            {
                resolutions.Add(res);
            }
            return resolutions;
        }

        #endregion

        #region Audio Settings

        public void SetMasterVolume(float volume)
        {
            _currentSettings.masterVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(masterVolumeParam, ConvertToDecibels(volume));
            }
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            _currentSettings.musicVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(musicVolumeParam, ConvertToDecibels(volume));
            }
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            _currentSettings.sfxVolume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(sfxVolumeParam, ConvertToDecibels(volume));
            }
            SaveSettings();
        }

        public void SetAudioEnabled(bool enabled)
        {
            _currentSettings.audioEnabled = enabled;
            ApplyAudioSettings();
            SaveSettings();
        }

        public float GetMasterVolume() => _currentSettings.masterVolume;
        public float GetMusicVolume() => _currentSettings.musicVolume;
        public float GetSFXVolume() => _currentSettings.sfxVolume;
        public bool IsAudioEnabled() => _currentSettings.audioEnabled;

        #endregion

        #region Gameplay Settings

        public void SetShowTimingIndicators(bool show)
        {
            _currentSettings.showTimingIndicators = show;
            SaveSettings();
        }

        public void SetScreenShake(bool enabled, float intensity = 1f)
        {
            _currentSettings.screenShakeEnabled = enabled;
            _currentSettings.screenShakeIntensity = Mathf.Clamp01(intensity);
            SaveSettings();
        }

        public void SetShowDamageNumbers(bool show)
        {
            _currentSettings.showDamageNumbers = show;
            SaveSettings();
        }

        public void SetAutoSave(bool enabled)
        {
            _currentSettings.autoSaveEnabled = enabled;
            SaveSettings();
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            _currentSettings.difficultyMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
            SaveSettings();
        }

        public bool GetShowTimingIndicators() => _currentSettings.showTimingIndicators;
        public bool GetScreenShakeEnabled() => _currentSettings.screenShakeEnabled;
        public float GetScreenShakeIntensity() => _currentSettings.screenShakeIntensity;
        public bool GetShowDamageNumbers() => _currentSettings.showDamageNumbers;
        public bool GetAutoSaveEnabled() => _currentSettings.autoSaveEnabled;
        public float GetDifficultyMultiplier() => _currentSettings.difficultyMultiplier;

        #endregion

        #region Accessibility Settings

        public void SetColorblindMode(ColorblindMode mode)
        {
            _currentSettings.colorblindMode = mode;
            ApplyAccessibilitySettings();
            SaveSettings();
        }

        public void SetHighContrastMode(bool enabled)
        {
            _currentSettings.highContrastMode = enabled;
            ApplyAccessibilitySettings();
            SaveSettings();
        }

        public void SetReducedMotion(bool enabled)
        {
            _currentSettings.reducedMotion = enabled;
            SaveSettings();
        }

        public void SetLargeText(bool enabled)
        {
            _currentSettings.largeText = enabled;
            SaveSettings();
        }

        public void SetSubtitles(bool enabled)
        {
            _currentSettings.subtitlesEnabled = enabled;
            SaveSettings();
        }

        public ColorblindMode GetColorblindMode() => _currentSettings.colorblindMode;
        public bool GetHighContrastMode() => _currentSettings.highContrastMode;
        public bool GetReducedMotion() => _currentSettings.reducedMotion;
        public bool GetLargeText() => _currentSettings.largeText;
        public bool GetSubtitlesEnabled() => _currentSettings.subtitlesEnabled;

        #endregion

        #region Controls Settings

        public void SetMouseSensitivity(float sensitivity)
        {
            _currentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
            SaveSettings();
        }

        public void SetInvertYAxis(bool invert)
        {
            _currentSettings.invertYAxis = invert;
            SaveSettings();
        }

        public void SetControllerVibration(bool enabled)
        {
            _currentSettings.controllerVibration = enabled;
            SaveSettings();
        }

        public float GetMouseSensitivity() => _currentSettings.mouseSensitivity;
        public bool GetInvertYAxis() => _currentSettings.invertYAxis;
        public bool GetControllerVibration() => _currentSettings.controllerVibration;

        #endregion

        #region Public Getters

        public GameSettings GetCurrentSettings()
        {
            return _currentSettings;
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class GameSettings
    {
        [Header("Graphics")]
        public int resolutionWidth;
        public int resolutionHeight;
        public FullScreenMode fullscreenMode;
        public bool vsyncEnabled;
        public int qualityLevel;
        public int targetFrameRate;

        [Header("Audio")]
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public bool audioEnabled;

        [Header("Gameplay")]
        public bool showTimingIndicators;
        public bool screenShakeEnabled;
        public float screenShakeIntensity;
        public bool showDamageNumbers;
        public bool autoSaveEnabled;
        public float difficultyMultiplier;

        [Header("Accessibility")]
        public ColorblindMode colorblindMode;
        public bool highContrastMode;
        public bool reducedMotion;
        public bool largeText;
        public bool subtitlesEnabled;

        [Header("Controls")]
        public float mouseSensitivity;
        public bool invertYAxis;
        public bool controllerVibration;
    }

    public enum ColorblindMode
    {
        None,
        Protanopia,    // Red-green (red weak)
        Deuteranopia,  // Red-green (green weak)
        Tritanopia     // Blue-yellow
    }

    #endregion
}
