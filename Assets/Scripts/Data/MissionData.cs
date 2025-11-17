using UnityEngine;
using System.Collections.Generic;

namespace SongOfTheStars.Missions
{
    /// <summary>
    /// ScriptableObject defining a complete mission
    /// 미션을 정의하는 ScriptableObject
    ///
    /// Contains all mission parameters, objectives, and configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Mission_New", menuName = "Song of the Stars/Missions/Mission Data")]
    public class MissionData : ScriptableObject
    {
        [Header("▶ Mission Info")]
        [Tooltip("Internal mission ID for saving/loading")]
        public string missionID = "mission_01";

        [Tooltip("Display name shown to player")]
        public string missionName = "New Mission";

        [Tooltip("Mission briefing text")]
        [TextArea(3, 6)]
        public string briefing = "Your mission, should you choose to accept it...";

        [Tooltip("Mission difficulty (affects guard stats, timing windows)")]
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        [Header("▶ Scene")]
        [Tooltip("Scene name to load for this mission")]
        public string sceneName = "Mission_01";

        [Header("▶ Music")]
        [Tooltip("Background music for this mission")]
        public AudioClip backgroundMusic;

        [Tooltip("Music BPM (for rhythm sync)")]
        [Range(80, 160)]
        public float musicBPM = 120f;

        [Header("▶ Objectives")]
        [Tooltip("Primary objectives (must complete all)")]
        public List<ObjectiveDefinition> primaryObjectives = new List<ObjectiveDefinition>();

        [Tooltip("Optional objectives (bonus rewards)")]
        public List<ObjectiveDefinition> optionalObjectives = new List<ObjectiveDefinition>();

        [Header("▶ Fail Conditions")]
        [Tooltip("Maximum alert level before mission fails")]
        [Range(1, 10)]
        public int maxAlertLevel = 10;

        [Tooltip("Time limit in seconds (0 = no limit)")]
        public float timeLimit = 0f;

        [Tooltip("Can player continue after primary objectives complete?")]
        public bool allowContinueAfterComplete = false;

        [Header("▶ Skill Loadout")]
        [Tooltip("Skills unlocked for this mission")]
        public List<ConstellationSkillData> availableSkills = new List<ConstellationSkillData>();

        [Tooltip("Default skill loadout (max 4)")]
        public List<ConstellationSkillData> defaultLoadout = new List<ConstellationSkillData>();

        [Header("▶ Rewards")]
        [Tooltip("Experience points for completion")]
        public int experienceReward = 100;

        [Tooltip("Bonus XP for optional objectives")]
        public int bonusExperiencePerOptional = 50;

        [Tooltip("Unlocked skills after mission complete")]
        public List<ConstellationSkillData> unlockedSkills = new List<ConstellationSkillData>();

        [Header("▶ Scripted Events")]
        [Tooltip("Events that trigger during mission")]
        public List<ScriptedEventDefinition> scriptedEvents = new List<ScriptedEventDefinition>();

        #region Validation
        private void OnValidate()
        {
            // Ensure mission ID is valid
            if (string.IsNullOrEmpty(missionID))
            {
                missionID = name.ToLower().Replace(" ", "_");
            }

            // Limit default loadout to 4 skills
            if (defaultLoadout.Count > 4)
            {
                Debug.LogWarning($"MissionData '{name}': Default loadout has more than 4 skills. Only first 4 will be used.");
            }

            // Ensure primary objectives exist
            if (primaryObjectives.Count == 0)
            {
                Debug.LogWarning($"MissionData '{name}': No primary objectives defined!");
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets total number of objectives (primary + optional)
        /// </summary>
        public int GetTotalObjectiveCount()
        {
            return primaryObjectives.Count + optionalObjectives.Count;
        }

        /// <summary>
        /// Gets all objectives combined
        /// </summary>
        public List<ObjectiveDefinition> GetAllObjectives()
        {
            List<ObjectiveDefinition> all = new List<ObjectiveDefinition>();
            all.AddRange(primaryObjectives);
            all.AddRange(optionalObjectives);
            return all;
        }

        /// <summary>
        /// Calculates total possible XP (including bonus objectives)
        /// </summary>
        public int GetMaxExperience()
        {
            return experienceReward + (optionalObjectives.Count * bonusExperiencePerOptional);
        }
        #endregion
    }

    /// <summary>
    /// Defines a single mission objective
    /// 단일 미션 목표 정의
    /// </summary>
    [System.Serializable]
    public class ObjectiveDefinition
    {
        [Tooltip("Objective description shown to player")]
        public string description = "Complete the objective";

        [Tooltip("Type of objective")]
        public ObjectiveType type = ObjectiveType.Eliminate;

        [Tooltip("Target count for progress tracking")]
        public int targetCount = 1;

        [Tooltip("Is this a secret objective? (hidden until discovered)")]
        public bool isSecret = false;

        [Header("Type-Specific Data")]
        [Tooltip("For Eliminate/Reach objectives: Tag of target objects")]
        public string targetTag = "Guard";

        [Tooltip("For Collect objectives: Item ID to collect")]
        public string itemID = "";

        [Tooltip("For Survive objectives: Duration in seconds")]
        public float duration = 60f;

        [Tooltip("For Stealth objectives: Maximum times player can be spotted")]
        public int maxDetections = 0;
    }

    /// <summary>
    /// Types of objectives
    /// </summary>
    public enum ObjectiveType
    {
        Eliminate,      // Kill X targets
        Reach,          // Reach location/target
        Collect,        // Collect X items
        Survive,        // Survive for X seconds
        Stealth,        // Complete without detection
        Rescue,         // Rescue X targets
        Sabotage,       // Destroy X objects
        Investigate     // Discover X locations
    }

    /// <summary>
    /// Defines a scripted event that occurs during mission
    /// 미션 중 발생하는 스크립트 이벤트 정의
    /// </summary>
    [System.Serializable]
    public class ScriptedEventDefinition
    {
        [Tooltip("Event name/ID")]
        public string eventID = "event_01";

        [Tooltip("When does this event trigger?")]
        public EventTrigger trigger = EventTrigger.OnObjectiveComplete;

        [Tooltip("For ObjectiveComplete trigger: Which objective index?")]
        public int triggerObjectiveIndex = 0;

        [Tooltip("For Timer trigger: Delay in seconds")]
        public float triggerDelay = 10f;

        [Tooltip("Event type")]
        public EventType eventType = EventType.SpawnGuards;

        [Header("Event Data")]
        [Tooltip("For dialog events: Dialog text")]
        [TextArea(2, 4)]
        public string dialogText = "";

        [Tooltip("For spawn events: Number of units to spawn")]
        public int spawnCount = 3;

        [Tooltip("For spawn events: Spawn point tags")]
        public string spawnPointTag = "GuardSpawn";
    }

    public enum EventTrigger
    {
        OnMissionStart,         // When mission begins
        OnTimer,                // After X seconds
        OnObjectiveComplete,    // When specific objective completes
        OnAlertLevel,           // When alert reaches level
        OnPlayerEnterZone       // When player enters trigger zone
    }

    public enum EventType
    {
        SpawnGuards,        // Spawn additional guards
        ShowDialog,         // Display message to player
        ChangeMusic,        // Change background music
        UnlockDoor,         // Open locked door/path
        SetObjective,       // Add new objective
        CameraFocus         // Focus camera on location
    }

    public enum DifficultyLevel
    {
        Tutorial,
        Easy,
        Normal,
        Hard
    }
}
