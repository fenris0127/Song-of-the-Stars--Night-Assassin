using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SongOfTheStars.UI
{
    /// <summary>
    /// Pause menu with settings, save/load, and mission controls
    /// 설정, 저장/로드 및 미션 컨트롤이 있는 일시중지 메뉴
    ///
    /// Features: Pause/Resume, Settings, Save/Load, Quit to Menu
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("▶ Menu Panels")]
        public GameObject pauseMenuPanel;
        public GameObject settingsPanel;
        public GameObject confirmQuitPanel;

        [Header("▶ Pause Menu Buttons")]
        public Button resumeButton;
        public Button settingsButton;
        public Button saveButton;
        public Button loadButton;
        public Button restartButton;
        public Button quitToMenuButton;

        [Header("▶ Settings")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Toggle vsyncToggle;
        public Toggle screenShakeToggle;
        public Dropdown qualityDropdown;
        public Button settingsBackButton;

        [Header("▶ Confirm Quit")]
        public Button confirmQuitButton;
        public Button cancelQuitButton;

        [Header("▶ Pause Settings")]
        public KeyCode pauseKey = KeyCode.Escape;
        public bool pauseOnStart = false;
        public bool allowPauseDuringCutscenes = false;

        [Header("▶ Events")]
        public UnityEvent OnPaused;
        public UnityEvent OnResumed;

        private bool _isPaused = false;
        private bool _canPause = true;
        private float _timeScaleBeforePause = 1f;

        void Start()
        {
            InitializeMenu();
            InitializeSettings();

            if (pauseOnStart)
            {
                Pause();
            }
            else
            {
                HideAllPanels();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(pauseKey) && _canPause)
            {
                TogglePause();
            }
        }

        #region Initialization

        private void InitializeMenu()
        {
            // Wire up pause menu buttons
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (saveButton != null)
                saveButton.onClick.AddListener(SaveGame);

            if (loadButton != null)
                loadButton.onClick.AddListener(LoadGame);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartMission);

            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(ShowQuitConfirmation);

            // Wire up settings buttons
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(CloseSettings);

            // Wire up confirm quit buttons
            if (confirmQuitButton != null)
                confirmQuitButton.onClick.AddListener(QuitToMainMenu);

            if (cancelQuitButton != null)
                cancelQuitButton.onClick.AddListener(HideQuitConfirmation);
        }

        private void InitializeSettings()
        {
            if (Systems.SettingsManager.Instance == null) return;

            // Master Volume
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = Systems.SettingsManager.Instance.GetMasterVolume();
                masterVolumeSlider.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetMasterVolume(value);
                });
            }

            // Music Volume
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = Systems.SettingsManager.Instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetMusicVolume(value);
                });
            }

            // SFX Volume
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = Systems.SettingsManager.Instance.GetSFXVolume();
                sfxVolumeSlider.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetSFXVolume(value);
                });
            }

            // VSync
            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = Systems.SettingsManager.Instance.GetCurrentSettings().vsyncEnabled;
                vsyncToggle.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetVSync(value);
                });
            }

            // Screen Shake
            if (screenShakeToggle != null)
            {
                screenShakeToggle.isOn = Systems.SettingsManager.Instance.GetScreenShakeEnabled();
                screenShakeToggle.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetScreenShake(value);
                });
            }

            // Quality
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.onValueChanged.AddListener((value) =>
                {
                    Systems.SettingsManager.Instance.SetQualityLevel(value);
                });
            }
        }

        #endregion

        #region Pause/Resume

        public void TogglePause()
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (_isPaused || !_canPause) return;

            _isPaused = true;
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;

            ShowPauseMenu();

            // Pause audio
            if (Systems.AudioManager.Instance != null)
            {
                Systems.AudioManager.Instance.PauseMusic();
            }

            OnPaused?.Invoke();

            Debug.Log("⏸️ Game paused");
        }

        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;
            Time.timeScale = _timeScaleBeforePause;

            HideAllPanels();

            // Resume audio
            if (Systems.AudioManager.Instance != null)
            {
                Systems.AudioManager.Instance.ResumeMusic();
            }

            OnResumed?.Invoke();

            Debug.Log("▶️ Game resumed");
        }

        public void SetCanPause(bool canPause)
        {
            _canPause = canPause;
        }

        #endregion

        #region Panel Management

        private void ShowPauseMenu()
        {
            HideAllPanels();

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void HideAllPanels()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            if (!_isPaused)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        #endregion

        #region Settings Menu

        private void OpenSettings()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            // Save settings
            if (Systems.SettingsManager.Instance != null)
            {
                Systems.SettingsManager.Instance.SaveSettings();
            }
        }

        #endregion

        #region Save/Load

        private void SaveGame()
        {
            if (Systems.SaveLoadManager.Instance != null)
            {
                bool success = Systems.SaveLoadManager.Instance.QuickSave();

                if (success)
                {
                    ShowNotification("Game Saved!", 2f);
                }
                else
                {
                    ShowNotification("Save Failed!", 2f);
                }
            }
            else
            {
                Debug.LogWarning("SaveLoadManager not found!");
            }
        }

        private void LoadGame()
        {
            if (Systems.SaveLoadManager.Instance != null)
            {
                bool success = Systems.SaveLoadManager.Instance.QuickLoad();

                if (success)
                {
                    ShowNotification("Game Loaded!", 2f);
                    Resume();
                }
                else
                {
                    ShowNotification("Load Failed!", 2f);
                }
            }
            else
            {
                Debug.LogWarning("SaveLoadManager not found!");
            }
        }

        #endregion

        #region Mission Controls

        private void RestartMission()
        {
            Time.timeScale = 1f;
            _isPaused = false;

            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            Debug.Log("🔄 Mission restarted");
        }

        private void ShowQuitConfirmation()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(true);
        }

        private void HideQuitConfirmation()
        {
            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        private void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            _isPaused = false;

            // Load main menu scene (assumed to be index 0)
            SceneManager.LoadScene(0);

            Debug.Log("🏠 Quit to main menu");
        }

        #endregion

        #region Notifications

        private void ShowNotification(string message, float duration)
        {
            // TODO: Implement notification UI
            Debug.Log($"📢 {message}");

            // Placeholder: Just log for now
            // In full implementation, this would show a temporary UI message
        }

        #endregion

        #region Public API

        public bool IsPaused() => _isPaused;

        public void ForcePause()
        {
            if (!_isPaused)
            {
                Pause();
            }
        }

        public void ForceResume()
        {
            if (_isPaused)
            {
                Resume();
            }
        }

        #endregion

        void OnDestroy()
        {
            // Ensure time scale is restored when menu is destroyed
            if (_isPaused)
            {
                Time.timeScale = _timeScaleBeforePause;
            }
        }
    }
}
