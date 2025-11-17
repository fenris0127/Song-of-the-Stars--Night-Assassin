using UnityEngine;
using UnityEngine.Events;
using SongOfTheStars.Missions;

namespace SongOfTheStars.Core
{
    /// <summary>
    /// Manages player progression and integrates with save system
    /// 플레이어 진행 상황 관리 및 저장 시스템 통합
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        #region Singleton
        public static ProgressionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgression();
        }
        #endregion

        [Header("▶ Progression Data")]
        public PlayerProgression progression;

        [Header("▶ Events")]
        public UnityEvent<int> OnLevelUp;
        public UnityEvent<int> OnExperienceGained;
        public UnityEvent<string> OnSkillUnlocked;
        public UnityEvent<string> OnAchievementUnlocked;
        public UnityEvent<string, int> OnMissionCompleted;

        #region Save/Load
        private const string SAVE_KEY = "PlayerProgression";

        /// <summary>
        /// Loads progression from PlayerPrefs
        /// </summary>
        private void LoadProgression()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                progression = JsonUtility.FromJson<PlayerProgression>(json);
                Debug.Log($"Progression loaded: Level {progression.currentLevel}, {progression.unlockedSkills.Count} skills");
            }
            else
            {
                // Create new progression
                progression = PlayerProgression.CreateNew();
                SaveProgression();
                Debug.Log("New progression created");
            }
        }

        /// <summary>
        /// Saves progression to PlayerPrefs
        /// </summary>
        public void SaveProgression()
        {
            if (progression == null) return;

            string json = JsonUtility.ToJson(progression, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("Progression saved");
        }

        /// <summary>
        /// Resets all progression (for debugging or new game)
        /// </summary>
        public void ResetProgression()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            progression = PlayerProgression.CreateNew();
            SaveProgression();
            Debug.Log("Progression reset");
        }
        #endregion

        #region Experience Management
        /// <summary>
        /// Adds experience and saves
        /// </summary>
        public void AddExperience(int amount)
        {
            int previousLevel = progression.currentLevel;
            progression.AddExperience(amount);

            OnExperienceGained?.Invoke(amount);

            // Check for level up
            if (progression.currentLevel > previousLevel)
            {
                OnLevelUp?.Invoke(progression.currentLevel);
            }

            SaveProgression();
        }
        #endregion

        #region Mission Management
        /// <summary>
        /// Called when mission is completed
        /// </summary>
        public void OnMissionComplete(MissionData mission, int finalScore, float completionTime,
                                       bool wasGhostRun, bool wasSpeedRun, int optionalObjectives)
        {
            // Record mission completion
            progression.CompleteMission(
                mission.missionID,
                finalScore,
                completionTime,
                wasGhostRun,
                wasSpeedRun,
                optionalObjectives
            );

            // Award experience
            int experienceGained = mission.experienceReward;
            experienceGained += optionalObjectives * mission.bonusExperiencePerOptional;
            AddExperience(experienceGained);

            // Unlock skills
            foreach (var skill in mission.unlockedSkills)
            {
                if (skill != null)
                {
                    UnlockSkill(skill.skillName);
                }
            }

            // Check achievements
            progression.CheckAchievements();

            // Fire event
            OnMissionCompleted?.Invoke(mission.missionID, finalScore);

            SaveProgression();
        }

        /// <summary>
        /// Gets mission high score
        /// </summary>
        public MissionScore GetMissionHighScore(string missionID)
        {
            return progression.GetMissionScore(missionID);
        }
        #endregion

        #region Skill Management
        /// <summary>
        /// Unlocks a skill
        /// </summary>
        public void UnlockSkill(string skillName)
        {
            if (progression.UnlockSkill(skillName))
            {
                OnSkillUnlocked?.Invoke(skillName);
                SaveProgression();
            }
        }

        /// <summary>
        /// Checks if skill is unlocked
        /// </summary>
        public bool IsSkillUnlocked(string skillName)
        {
            return progression.IsSkillUnlocked(skillName);
        }

        /// <summary>
        /// Equips skill to loadout
        /// </summary>
        public void EquipSkill(string skillName, int slot)
        {
            if (progression.EquipSkill(skillName, slot))
            {
                SaveProgression();
            }
        }
        #endregion

        #region Statistics Tracking
        /// <summary>
        /// Records gameplay statistics during mission
        /// </summary>
        public void RecordPerfectInput()
        {
            progression.RecordPerfectInput();
        }

        public void RecordMiss()
        {
            progression.RecordMiss();
        }

        public void RecordGuardElimination()
        {
            progression.RecordGuardElimination();
        }

        public void RecordDetection()
        {
            progression.RecordDetection();
        }

        /// <summary>
        /// Adds play time (call in Update or on mission end)
        /// </summary>
        public void AddPlayTime(float seconds)
        {
            progression.totalPlayTime += seconds;
        }
        #endregion

        #region Achievements
        /// <summary>
        /// Manually unlock achievement (for special conditions)
        /// </summary>
        public void UnlockAchievement(string achievementID)
        {
            if (progression.UnlockAchievement(achievementID))
            {
                OnAchievementUnlocked?.Invoke(achievementID);
                SaveProgression();
            }
        }

        /// <summary>
        /// Check all achievement conditions
        /// </summary>
        public void CheckAchievements()
        {
            progression.CheckAchievements();
        }
        #endregion

        #region Public Getters
        public int GetCurrentLevel() => progression.currentLevel;
        public int GetCurrentExperience() => progression.currentExperience;
        public float GetLevelProgress() => progression.GetLevelProgress();
        public float GetAccuracy() => progression.GetAccuracy();
        public int GetTotalMissionsCompleted() => progression.totalMissionsCompleted;
        public int GetGhostRunsCompleted() => progression.ghostRunsCompleted;
        public int GetTotalGuardsEliminated() => progression.totalGuardsEliminated;
        public float GetTotalPlayTime() => progression.totalPlayTime;
        #endregion

        #region Auto-Save
        void OnApplicationQuit()
        {
            SaveProgression();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveProgression();
            }
        }
        #endregion

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Add 100 XP")]
        private void DebugAddXP()
        {
            AddExperience(100);
        }

        [ContextMenu("Unlock All Skills")]
        private void DebugUnlockAllSkills()
        {
            string[] allSkills = new string[]
            {
                "Capricorn Trap", "Orion's Arrow",
                "Decoy", "Gemini Clone",
                "Shadow Blend", "Andromeda's Veil",
                "Pegasus Dash", "Aquarius Flow"
            };

            foreach (string skill in allSkills)
            {
                UnlockSkill(skill);
            }
        }

        [ContextMenu("Reset Progression")]
        private void DebugResetProgression()
        {
            ResetProgression();
        }

        [ContextMenu("Print Stats")]
        private void DebugPrintStats()
        {
            Debug.Log($"=== Player Stats ===");
            Debug.Log($"Level: {progression.currentLevel}");
            Debug.Log($"XP: {progression.currentExperience}/{progression.experienceToNextLevel}");
            Debug.Log($"Accuracy: {progression.GetAccuracy():F1}%");
            Debug.Log($"Missions: {progression.totalMissionsCompleted}/{progression.totalMissionsAttempted}");
            Debug.Log($"Skills Unlocked: {progression.unlockedSkills.Count}/8");
            Debug.Log($"Achievements: {progression.unlockedAchievements.Count}");
            Debug.Log($"Play Time: {progression.totalPlayTime / 3600f:F1} hours");
        }
        #endif
        #endregion
    }
}
