using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SongOfTheStars.Missions;

namespace SongOfTheStars.Core
{
    /// <summary>
    /// Tracks player progression, unlocks, and achievements
    /// 플레이어 진행 상황, 잠금 해제 및 업적 추적
    ///
    /// Persists between sessions via SaveSystem
    /// </summary>
    [System.Serializable]
    public class PlayerProgression
    {
        #region Player Stats
        [Header("▶ Player Level")]
        public int currentLevel = 1;
        public int currentExperience = 0;
        public int experienceToNextLevel = 100;

        [Header("▶ Mission Progress")]
        public List<string> completedMissions = new List<string>();
        public List<MissionScore> missionScores = new List<MissionScore>();
        public string currentMissionID = "";

        [Header("▶ Skill Unlocks")]
        public List<string> unlockedSkills = new List<string>();
        public List<string> equippedSkills = new List<string>(); // Max 4

        [Header("▶ Achievements")]
        public List<string> unlockedAchievements = new List<string>();
        public int totalPerfectInputs = 0;
        public int totalMissInputs = 0;
        public int totalGuardsEliminated = 0;
        public int totalTimesDetected = 0;

        [Header("▶ Statistics")]
        public float totalPlayTime = 0f; // Seconds
        public int totalMissionsAttempted = 0;
        public int totalMissionsCompleted = 0;
        public int ghostRunsCompleted = 0; // Missions completed without detection
        public int speedRunsCompleted = 0; // Missions completed under par time
        #endregion

        #region Initialization
        /// <summary>
        /// Creates new player progression with tutorial skills
        /// </summary>
        public static PlayerProgression CreateNew()
        {
            PlayerProgression progression = new PlayerProgression();

            // Start with tutorial skills
            progression.unlockedSkills.Add("Capricorn Trap");

            // Default loadout
            progression.equippedSkills.Add("Capricorn Trap");

            return progression;
        }
        #endregion

        #region Experience & Leveling
        /// <summary>
        /// Adds experience and handles level ups
        /// </summary>
        public void AddExperience(int amount)
        {
            currentExperience += amount;

            // Check for level up
            while (currentExperience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;

            // Calculate next level requirement (exponential growth)
            experienceToNextLevel = CalculateExperienceRequired(currentLevel);

            Debug.Log($"Level Up! Now level {currentLevel}. Next level: {experienceToNextLevel} XP");
        }

        private int CalculateExperienceRequired(int level)
        {
            // Formula: 100 * (level ^ 1.5)
            return Mathf.RoundToInt(100f * Mathf.Pow(level, 1.5f));
        }

        /// <summary>
        /// Gets progress to next level (0-1)
        /// </summary>
        public float GetLevelProgress()
        {
            return (float)currentExperience / experienceToNextLevel;
        }
        #endregion

        #region Mission Tracking
        /// <summary>
        /// Records mission completion
        /// </summary>
        public void CompleteMission(string missionID, int score, float completionTime,
                                     bool wasGhostRun, bool wasSpeedRun, int optionalObjectivesCompleted)
        {
            // Add to completed list
            if (!completedMissions.Contains(missionID))
            {
                completedMissions.Add(missionID);
                totalMissionsCompleted++;
            }

            // Update or add score
            MissionScore existingScore = missionScores.FirstOrDefault(s => s.missionID == missionID);
            if (existingScore != null)
            {
                // Update if better score
                if (score > existingScore.score)
                {
                    existingScore.score = score;
                    existingScore.completionTime = completionTime;
                    existingScore.wasGhostRun = wasGhostRun;
                    existingScore.wasSpeedRun = wasSpeedRun;
                    existingScore.optionalObjectivesCompleted = optionalObjectivesCompleted;
                }
            }
            else
            {
                // Add new score
                missionScores.Add(new MissionScore
                {
                    missionID = missionID,
                    score = score,
                    completionTime = completionTime,
                    wasGhostRun = wasGhostRun,
                    wasSpeedRun = wasSpeedRun,
                    optionalObjectivesCompleted = optionalObjectivesCompleted
                });
            }

            // Update statistics
            if (wasGhostRun) ghostRunsCompleted++;
            if (wasSpeedRun) speedRunsCompleted++;

            Debug.Log($"Mission '{missionID}' completed! Score: {score}, Time: {completionTime:F1}s");
        }

        /// <summary>
        /// Checks if mission is unlocked
        /// </summary>
        public bool IsMissionUnlocked(string missionID)
        {
            // Tutorial always unlocked
            if (missionID.Contains("tutorial")) return true;

            // Check if previous mission is completed
            // TODO: Implement proper mission dependency chain
            return true;
        }

        /// <summary>
        /// Gets best score for a mission
        /// </summary>
        public MissionScore GetMissionScore(string missionID)
        {
            return missionScores.FirstOrDefault(s => s.missionID == missionID);
        }
        #endregion

        #region Skill Management
        /// <summary>
        /// Unlocks a new skill
        /// </summary>
        public bool UnlockSkill(string skillName)
        {
            if (unlockedSkills.Contains(skillName))
            {
                Debug.LogWarning($"Skill '{skillName}' already unlocked!");
                return false;
            }

            unlockedSkills.Add(skillName);
            Debug.Log($"Skill Unlocked: {skillName}");
            return true;
        }

        /// <summary>
        /// Checks if skill is unlocked
        /// </summary>
        public bool IsSkillUnlocked(string skillName)
        {
            return unlockedSkills.Contains(skillName);
        }

        /// <summary>
        /// Equips a skill to loadout slot (0-3)
        /// </summary>
        public bool EquipSkill(string skillName, int slot)
        {
            if (!IsSkillUnlocked(skillName))
            {
                Debug.LogWarning($"Cannot equip locked skill: {skillName}");
                return false;
            }

            if (slot < 0 || slot > 3)
            {
                Debug.LogWarning($"Invalid slot: {slot}. Must be 0-3.");
                return false;
            }

            // Ensure list is large enough
            while (equippedSkills.Count <= slot)
            {
                equippedSkills.Add("");
            }

            equippedSkills[slot] = skillName;
            Debug.Log($"Equipped '{skillName}' to slot {slot}");
            return true;
        }

        /// <summary>
        /// Gets equipped skill at slot
        /// </summary>
        public string GetEquippedSkill(int slot)
        {
            if (slot < 0 || slot >= equippedSkills.Count) return "";
            return equippedSkills[slot];
        }
        #endregion

        #region Achievements
        /// <summary>
        /// Unlocks an achievement
        /// </summary>
        public bool UnlockAchievement(string achievementID)
        {
            if (unlockedAchievements.Contains(achievementID))
            {
                return false;
            }

            unlockedAchievements.Add(achievementID);
            Debug.Log($"🏆 Achievement Unlocked: {achievementID}");
            return true;
        }

        /// <summary>
        /// Checks achievement conditions and unlocks if met
        /// </summary>
        public void CheckAchievements()
        {
            // Perfect Assassin: 100 perfect inputs
            if (totalPerfectInputs >= 100)
            {
                UnlockAchievement("perfect_assassin");
            }

            // Ghost: Complete 5 missions without detection
            if (ghostRunsCompleted >= 5)
            {
                UnlockAchievement("shadow_master");
            }

            // Speed Demon: Complete 5 speed runs
            if (speedRunsCompleted >= 5)
            {
                UnlockAchievement("speed_demon");
            }

            // Mission Master: Complete all missions
            if (completedMissions.Count >= 10) // Adjust based on total missions
            {
                UnlockAchievement("mission_master");
            }

            // Skill Collector: Unlock all 8 skills
            if (unlockedSkills.Count >= 8)
            {
                UnlockAchievement("skill_collector");
            }

            // Level 10: Reach level 10
            if (currentLevel >= 10)
            {
                UnlockAchievement("seasoned_assassin");
            }

            // Eliminator: Eliminate 50 guards
            if (totalGuardsEliminated >= 50)
            {
                UnlockAchievement("eliminator");
            }
        }
        #endregion

        #region Statistics
        /// <summary>
        /// Records a perfect rhythm input
        /// </summary>
        public void RecordPerfectInput()
        {
            totalPerfectInputs++;
        }

        /// <summary>
        /// Records a missed rhythm input
        /// </summary>
        public void RecordMiss()
        {
            totalMissInputs++;
        }

        /// <summary>
        /// Records guard elimination
        /// </summary>
        public void RecordGuardElimination()
        {
            totalGuardsEliminated++;
        }

        /// <summary>
        /// Records detection
        /// </summary>
        public void RecordDetection()
        {
            totalTimesDetected++;
        }

        /// <summary>
        /// Gets accuracy percentage
        /// </summary>
        public float GetAccuracy()
        {
            int totalInputs = totalPerfectInputs + totalMissInputs;
            if (totalInputs == 0) return 0f;
            return (float)totalPerfectInputs / totalInputs * 100f;
        }

        /// <summary>
        /// Gets average mission completion time
        /// </summary>
        public float GetAverageMissionTime()
        {
            if (missionScores.Count == 0) return 0f;
            return missionScores.Average(s => s.completionTime);
        }
        #endregion
    }

    /// <summary>
    /// Stores score data for a completed mission
    /// </summary>
    [System.Serializable]
    public class MissionScore
    {
        public string missionID;
        public int score;
        public float completionTime;
        public bool wasGhostRun;
        public bool wasSpeedRun;
        public int optionalObjectivesCompleted;

        /// <summary>
        /// Gets rank based on score (S, A, B, C, D)
        /// </summary>
        public string GetRank()
        {
            if (score >= 1000) return "S";
            if (score >= 800) return "A";
            if (score >= 600) return "B";
            if (score >= 400) return "C";
            return "D";
        }
    }
}
