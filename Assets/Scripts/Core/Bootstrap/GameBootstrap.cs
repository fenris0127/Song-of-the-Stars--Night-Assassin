using UnityEngine;
using UnityEngine.SceneManagement;
using SongOfTheStars.Core;

namespace SongOfTheStars.Bootstrap
{
    /// <summary>
    /// Initializes all core game systems in the correct order
    /// 모든 핵심 게임 시스템을 올바른 순서로 초기화
    ///
    /// Place this on a GameObject in the first scene (usually MainMenu or Bootstrap scene)
    /// This ensures all singleton managers are created before gameplay starts
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("▶ Initialization")]
        [Tooltip("Should we auto-initialize on Awake?")]
        public bool autoInitialize = true;

        [Tooltip("Show debug logs during initialization?")]
        public bool debugMode = true;

        [Header("▶ Manager Prefabs")]
        [Tooltip("Assign manager prefabs that need to be instantiated")]
        public GameObject progressionManagerPrefab;
        public GameObject statisticsManagerPrefab;

        [Header("▶ Startup Scene")]
        [Tooltip("Scene to load after initialization (leave empty to stay in current scene)")]
        public string startupSceneName = "";

        private static bool _isInitialized = false;

        void Awake()
        {
            if (autoInitialize && !_isInitialized)
            {
                InitializeGame();
            }
        }

        /// <summary>
        /// Initializes all game systems
        /// Call this manually if autoInitialize is false
        /// </summary>
        public void InitializeGame()
        {
            if (_isInitialized)
            {
                Log("Game already initialized. Skipping.");
                return;
            }

            Log("=== Game Bootstrap Starting ===");

            // Step 1: Create persistent managers
            CreatePersistentManagers();

            // Step 2: Initialize GameServices
            InitializeGameServices();

            // Step 3: Load player progression
            LoadPlayerData();

            // Step 4: Apply default settings
            ApplySettings();

            // Step 5: Setup audio
            SetupAudio();

            _isInitialized = true;
            Log("=== Game Bootstrap Complete ===");

            // Step 6: Load startup scene if specified
            if (!string.IsNullOrEmpty(startupSceneName))
            {
                Log($"Loading startup scene: {startupSceneName}");
                SceneManager.LoadScene(startupSceneName);
            }
        }

        #region Initialization Steps

        /// <summary>
        /// Step 1: Create manager GameObjects that need to persist
        /// </summary>
        private void CreatePersistentManagers()
        {
            Log("Creating persistent managers...");

            // Create ProgressionManager if not exists
            if (ProgressionManager.Instance == null && progressionManagerPrefab != null)
            {
                Instantiate(progressionManagerPrefab);
                Log("- ProgressionManager created");
            }
            else if (ProgressionManager.Instance != null)
            {
                Log("- ProgressionManager already exists");
            }

            // Create StatisticsManager if not exists
            if (FindObjectOfType<Analytics.StatisticsManager>() == null && statisticsManagerPrefab != null)
            {
                Instantiate(statisticsManagerPrefab);
                Log("- StatisticsManager created");
            }

            // Note: Other managers (RhythmSyncManager, DifficultyManager, etc.)
            // are typically placed in scene or created by their scenes
        }

        /// <summary>
        /// Step 2: Initialize GameServices references
        /// </summary>
        private void InitializeGameServices()
        {
            Log("Initializing GameServices...");

            // GameServices will automatically cache references when accessed
            // We just verify they exist or log warnings

            if (GameServices.RhythmManager == null)
                LogWarning("- RhythmSyncManager not found (will be created in gameplay scene)");
            else
                Log("- RhythmSyncManager found");

            if (GameServices.UIManager == null)
                LogWarning("- UIManager not found (will be created in gameplay scene)");
            else
                Log("- UIManager found");

            if (GameServices.Player == null)
                LogWarning("- PlayerController not found (will be created in gameplay scene)");
            else
                Log("- PlayerController found");

            // These are optional and may not exist yet
            Log("- GameServices initialized");
        }

        /// <summary>
        /// Step 3: Load player progression data
        /// </summary>
        private void LoadPlayerData()
        {
            Log("Loading player data...");

            if (ProgressionManager.Instance != null)
            {
                // ProgressionManager loads automatically in Awake
                var progression = ProgressionManager.Instance.progression;
                if (progression != null)
                {
                    Log($"- Player Level: {progression.currentLevel}");
                    Log($"- Unlocked Skills: {progression.unlockedSkills.Count}/8");
                    Log($"- Completed Missions: {progression.completedMissions.Count}");
                }
            }
            else
            {
                LogWarning("- ProgressionManager not available");
            }
        }

        /// <summary>
        /// Step 4: Apply default settings (graphics, audio, etc.)
        /// </summary>
        private void ApplySettings()
        {
            Log("Applying settings...");

            // Set target framerate
            Application.targetFrameRate = 60;
            Log("- Target FPS: 60");

            // Set quality settings
            QualitySettings.vSyncCount = 0; // Disable VSync for consistent rhythm timing
            Log("- VSync: Disabled (for rhythm accuracy)");

            // Set audio settings
            AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
            audioConfig.dspBufferSize = 512; // Lower latency for rhythm game
            AudioSettings.Reset(audioConfig);
            Log($"- Audio DSP Buffer: {audioConfig.dspBufferSize}");
        }

        /// <summary>
        /// Step 5: Setup audio system
        /// </summary>
        private void SetupAudio()
        {
            Log("Setting up audio...");

            // Ensure AudioListener exists
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener == null)
            {
                gameObject.AddComponent<AudioListener>();
                Log("- AudioListener created");
            }
            else
            {
                Log("- AudioListener found");
            }

            // Set master volume from saved preferences
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = masterVolume;
            Log($"- Master Volume: {masterVolume * 100f}%");
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[GameBootstrap] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[GameBootstrap] {message}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Resets the game to initial state (for new game)
        /// </summary>
        [ContextMenu("Reset Game")]
        public void ResetGame()
        {
            Log("Resetting game...");

            // Reset progression
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.ResetProgression();
            }

            // Clear PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            Log("Game reset complete");
        }

        /// <summary>
        /// Gets initialization status
        /// </summary>
        public static bool IsInitialized()
        {
            return _isInitialized;
        }

        #endregion

        #region Editor Debug

#if UNITY_EDITOR
        [ContextMenu("Print System Status")]
        private void PrintSystemStatus()
        {
            Debug.Log("=== System Status ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"ProgressionManager: {(ProgressionManager.Instance != null ? "✓" : "✗")}");
            Debug.Log($"StatisticsManager: {(FindObjectOfType<Analytics.StatisticsManager>() != null ? "✓" : "✗")}");
            Debug.Log($"RhythmManager: {(GameServices.RhythmManager != null ? "✓" : "✗")}");
            Debug.Log($"Player: {(GameServices.Player != null ? "✓" : "✗")}");
            Debug.Log($"Target FPS: {Application.targetFrameRate}");
            Debug.Log($"VSync: {QualitySettings.vSyncCount}");
            Debug.Log($"Audio DSP Buffer: {AudioSettings.GetConfiguration().dspBufferSize}");
            Debug.Log($"Master Volume: {AudioListener.volume * 100f}%");

            if (ProgressionManager.Instance != null)
            {
                var p = ProgressionManager.Instance.progression;
                Debug.Log($"\nPlayer Progress:");
                Debug.Log($"- Level: {p.currentLevel}");
                Debug.Log($"- XP: {p.currentExperience}/{p.experienceToNextLevel}");
                Debug.Log($"- Skills: {p.unlockedSkills.Count}/8");
                Debug.Log($"- Missions: {p.completedMissions.Count}");
                Debug.Log($"- Achievements: {p.unlockedAchievements.Count}");
            }
        }

        [ContextMenu("Force Initialize")]
        private void ForceInitialize()
        {
            _isInitialized = false;
            InitializeGame();
        }
#endif

        #endregion
    }
}
