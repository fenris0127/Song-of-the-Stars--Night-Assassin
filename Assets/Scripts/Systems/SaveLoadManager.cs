using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Centralized save/load system with multiple save slots
    /// 여러 저장 슬롯이 있는 중앙 집중식 저장/로드 시스템
    ///
    /// Manages: Player progress, achievements, replays, settings, statistics
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        [Header("▶ Save Settings")]
        public int maxSaveSlots = 3;
        public bool enableAutoSave = true;
        [Range(30f, 600f)]
        public float autoSaveInterval = 120f; // 2 minutes

        [Header("▶ Debug")]
        public bool showDebugLogs = true;

        private int _currentSaveSlot = 0;
        private float _autoSaveTimer = 0f;

        private const string SAVE_FOLDER = "SaveData";
        private const string SAVE_FILE_PREFIX = "Save_Slot_";
        private const string SAVE_FILE_EXTENSION = ".sav";
        private const string SAVE_INFO_FILE = "SaveInfo.json";

        private SaveInfo _saveInfo;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSaveSystem();
        }

        void Update()
        {
            if (enableAutoSave)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= autoSaveInterval)
                {
                    AutoSave();
                    _autoSaveTimer = 0f;
                }
            }
        }

        private void InitializeSaveSystem()
        {
            string saveDirectory = GetSaveDirectory();
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                Log("📁 Save directory created");
            }

            LoadSaveInfo();
        }

        #region Save Operations

        public bool SaveGame(int slotIndex = -1)
        {
            if (slotIndex == -1) slotIndex = _currentSaveSlot;

            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return false;
            }

            try
            {
                SaveData saveData = CollectSaveData();
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetSaveFilePath(slotIndex);

                File.WriteAllText(filePath, json);

                // Update save info
                UpdateSaveInfo(slotIndex, saveData);

                Log($"💾 Game saved to slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                return false;
            }
        }

        private SaveData CollectSaveData()
        {
            SaveData data = new SaveData
            {
                saveVersion = Application.version,
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                playTime = Time.time, // Would be from PlayerStats

                // Player Stats
                playerLevel = 1, // From ProgressionManager
                playerXP = 0,
                totalScore = 0,

                // Progress
                unlockedMissions = new List<string>(),
                completedMissions = new List<string>(),
                currentMissionID = "",

                // Achievements
                unlockedAchievements = new List<string>(),
                achievementProgress = new Dictionary<string, int>(),

                // Statistics
                totalMissionsCompleted = 0,
                totalPlayTime = 0f,
                perfectInputsTotal = 0,
                skillsUsedTotal = 0
            };

            // Collect from various systems
            CollectProgressionData(ref data);
            CollectAchievementData(ref data);
            CollectStatisticsData(ref data);

            return data;
        }

        private void CollectProgressionData(ref SaveData data)
        {
            // TODO: Integrate with actual ProgressionManager
            // For now, placeholder
        }

        private void CollectAchievementData(ref SaveData data)
        {
            if (AchievementSystem.Instance != null)
            {
                var unlockedAchievements = AchievementSystem.Instance.GetUnlockedAchievements();
                foreach (var achievement in unlockedAchievements)
                {
                    data.unlockedAchievements.Add(achievement.id);
                }
            }
        }

        private void CollectStatisticsData(ref SaveData data)
        {
            // TODO: Collect from PlayerStatsTracker
        }

        private void AutoSave()
        {
            if (!SettingsManager.Instance.GetAutoSaveEnabled()) return;

            if (SaveGame(_currentSaveSlot))
            {
                Log("💾 Auto-saved");
            }
        }

        #endregion

        #region Load Operations

        public bool LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file found in slot {slotIndex}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                ApplySaveData(data);

                _currentSaveSlot = slotIndex;

                Log($"📂 Game loaded from slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        private void ApplySaveData(SaveData data)
        {
            // Apply to various systems
            ApplyProgressionData(data);
            ApplyAchievementData(data);
            ApplyStatisticsData(data);

            Log($"✅ Save data applied (Version: {data.saveVersion}, Date: {data.saveDate})");
        }

        private void ApplyProgressionData(SaveData data)
        {
            // TODO: Apply to ProgressionManager
        }

        private void ApplyAchievementData(SaveData data)
        {
            // Note: Achievement progress is already saved separately by AchievementSystem
            // This is for cross-slot verification
        }

        private void ApplyStatisticsData(SaveData data)
        {
            // TODO: Apply to PlayerStatsTracker
        }

        #endregion

        #region Delete Operations

        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file to delete in slot {slotIndex}");
                return false;
            }

            try
            {
                File.Delete(filePath);

                // Update save info
                if (_saveInfo.slotInfos.ContainsKey(slotIndex))
                {
                    _saveInfo.slotInfos.Remove(slotIndex);
                    SaveSaveInfo();
                }

                Log($"🗑️ Save slot {slotIndex} deleted");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        public void DeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSave(i);
            }

            _saveInfo = new SaveInfo
            {
                slotInfos = new Dictionary<int, SlotInfo>()
            };
            SaveSaveInfo();

            Log("🗑️ All saves deleted");
        }

        #endregion

        #region Save Info Management

        private void LoadSaveInfo()
        {
            string infoPath = Path.Combine(GetSaveDirectory(), SAVE_INFO_FILE);

            if (File.Exists(infoPath))
            {
                try
                {
                    string json = File.ReadAllText(infoPath);
                    _saveInfo = JsonUtility.FromJson<SaveInfo>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load save info: {e.Message}");
                    _saveInfo = new SaveInfo { slotInfos = new Dictionary<int, SlotInfo>() };
                }
            }
            else
            {
                _saveInfo = new SaveInfo { slotInfos = new Dictionary<int, SlotInfo>() };
            }
        }

        private void SaveSaveInfo()
        {
            try
            {
                string json = JsonUtility.ToJson(_saveInfo, true);
                string infoPath = Path.Combine(GetSaveDirectory(), SAVE_INFO_FILE);
                File.WriteAllText(infoPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save info: {e.Message}");
            }
        }

        private void UpdateSaveInfo(int slotIndex, SaveData saveData)
        {
            SlotInfo info = new SlotInfo
            {
                slotIndex = slotIndex,
                saveDate = saveData.saveDate,
                playerLevel = saveData.playerLevel,
                playTime = saveData.playTime,
                currentMission = saveData.currentMissionID,
                completedMissions = saveData.completedMissions.Count
            };

            _saveInfo.slotInfos[slotIndex] = info;
            SaveSaveInfo();
        }

        public SlotInfo GetSlotInfo(int slotIndex)
        {
            if (_saveInfo.slotInfos.ContainsKey(slotIndex))
            {
                return _saveInfo.slotInfos[slotIndex];
            }
            return null;
        }

        public bool IsSlotOccupied(int slotIndex)
        {
            return File.Exists(GetSaveFilePath(slotIndex));
        }

        public List<SlotInfo> GetAllSlotInfos()
        {
            List<SlotInfo> infos = new List<SlotInfo>();
            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (IsSlotOccupied(i))
                {
                    infos.Add(GetSlotInfo(i));
                }
            }
            return infos;
        }

        #endregion

        #region Quick Save/Load

        public bool QuickSave()
        {
            return SaveGame(_currentSaveSlot);
        }

        public bool QuickLoad()
        {
            return LoadGame(_currentSaveSlot);
        }

        #endregion

        #region Export/Import (Future: Cloud Save)

        public string ExportSaveToJSON(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file in slot {slotIndex}");
                return null;
            }

            return File.ReadAllText(filePath);
        }

        public bool ImportSaveFromJSON(int slotIndex, string json)
        {
            try
            {
                // Validate JSON
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                string filePath = GetSaveFilePath(slotIndex);
                File.WriteAllText(filePath, json);

                UpdateSaveInfo(slotIndex, data);

                Log($"📥 Save imported to slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to import save: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Utilities

        private string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        }

        private string GetSaveFilePath(int slotIndex)
        {
            string fileName = SAVE_FILE_PREFIX + slotIndex + SAVE_FILE_EXTENSION;
            return Path.Combine(GetSaveDirectory(), fileName);
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        public int GetCurrentSaveSlot() => _currentSaveSlot;
        public void SetCurrentSaveSlot(int slot) => _currentSaveSlot = Mathf.Clamp(slot, 0, maxSaveSlots - 1);

        #endregion

        #region Save Data Verification

        public bool VerifySaveIntegrity(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (!File.Exists(filePath)) return false;

            try
            {
                string json = File.ReadAllText(filePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Basic validation
                if (string.IsNullOrEmpty(data.saveVersion)) return false;
                if (string.IsNullOrEmpty(data.saveDate)) return false;

                Log($"✅ Save slot {slotIndex} verified");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Save verification failed: {e.Message}");
                return false;
            }
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class SaveData
    {
        [Header("Meta")]
        public string saveVersion;
        public string saveDate;
        public float playTime;

        [Header("Player")]
        public int playerLevel;
        public int playerXP;
        public int totalScore;

        [Header("Progress")]
        public List<string> unlockedMissions;
        public List<string> completedMissions;
        public string currentMissionID;

        [Header("Achievements")]
        public List<string> unlockedAchievements;
        public Dictionary<string, int> achievementProgress;

        [Header("Statistics")]
        public int totalMissionsCompleted;
        public float totalPlayTime;
        public int perfectInputsTotal;
        public int skillsUsedTotal;
    }

    [System.Serializable]
    public class SaveInfo
    {
        public Dictionary<int, SlotInfo> slotInfos;
    }

    [System.Serializable]
    public class SlotInfo
    {
        public int slotIndex;
        public string saveDate;
        public int playerLevel;
        public float playTime;
        public string currentMission;
        public int completedMissions;

        public string GetFormattedPlayTime()
        {
            int hours = Mathf.FloorToInt(playTime / 3600f);
            int minutes = Mathf.FloorToInt((playTime % 3600f) / 60f);
            return $"{hours}h {minutes}m";
        }

        public string GetFormattedDate()
        {
            try
            {
                DateTime date = DateTime.Parse(saveDate);
                return date.ToString("MMM dd, yyyy HH:mm");
            }
            catch
            {
                return saveDate;
            }
        }
    }

    #endregion
}
