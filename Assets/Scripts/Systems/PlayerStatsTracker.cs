using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Comprehensive player statistics tracking system
    /// 포괄적인 플레이어 통계 추적 시스템
    ///
    /// Tracks: Mission stats, combat stats, rhythm stats, skill usage, playtime
    /// </summary>
    public class PlayerStatsTracker : MonoBehaviour
    {
        public static PlayerStatsTracker Instance { get; private set; }

        [Header("▶ Tracking Settings")]
        public bool enableTracking = true;
        public bool saveOnUpdate = false; // Save every update (performance heavy)

        private PlayerStatistics _stats;
        private const string STATS_SAVE_KEY = "PlayerStatistics";

        // Session tracking
        private float _sessionStartTime;
        private MissionSessionStats _currentMissionStats;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadStatistics();
            _sessionStartTime = Time.time;
        }

        void OnApplicationQuit()
        {
            SaveStatistics();
        }

        #region Load/Save

        private void LoadStatistics()
        {
            if (PlayerPrefs.HasKey(STATS_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(STATS_SAVE_KEY);
                _stats = JsonUtility.FromJson<PlayerStatistics>(json);
                Debug.Log("📊 Player statistics loaded");
            }
            else
            {
                _stats = new PlayerStatistics
                {
                    // Mission Stats
                    totalMissionsAttempted = 0,
                    totalMissionsCompleted = 0,
                    totalMissionsFailed = 0,
                    perfectMissions = 0,
                    ghostRunMissions = 0,

                    // Combat Stats
                    totalEliminationsMade = 0,
                    stealthEliminationsCount = 0,
                    skillEliminationsCount = 0,
                    enemiesDetectedByCount = 0,
                    alarmsTriggeredCount = 0,

                    // Rhythm Stats
                    totalInputsAttempted = 0,
                    perfectInputsCount = 0,
                    greatInputsCount = 0,
                    goodInputsCount = 0,
                    missedInputsCount = 0,
                    longestPerfectStreak = 0,

                    // Skill Stats
                    skillUsageCount = new Dictionary<string, int>(),
                    totalSkillsUsed = 0,
                    totalFocusSpent = 0f,

                    // Score Stats
                    totalScore = 0,
                    highestSingleMissionScore = 0,
                    sRankMissions = 0,
                    aRankMissions = 0,

                    // Time Stats
                    totalPlayTimeSeconds = 0f,
                    fastestMissionCompletionTime = float.MaxValue,

                    // Collectibles
                    starMapsCollected = 0,
                    secretsFound = 0,

                    // Meta
                    firstPlayDate = System.DateTime.Now.ToString("yyyy-MM-dd"),
                    lastPlayDate = System.DateTime.Now.ToString("yyyy-MM-dd")
                };

                Debug.Log("📊 New player statistics created");
            }

            UpdatePlaytime();
        }

        public void SaveStatistics()
        {
            if (!enableTracking) return;

            UpdatePlaytime();

            _stats.lastPlayDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            string json = JsonUtility.ToJson(_stats, true);
            PlayerPrefs.SetString(STATS_SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("💾 Player statistics saved");
        }

        private void UpdatePlaytime()
        {
            float sessionTime = Time.time - _sessionStartTime;
            _stats.totalPlayTimeSeconds += sessionTime;
            _sessionStartTime = Time.time;
        }

        #endregion

        #region Mission Tracking

        public void OnMissionStarted(string missionID)
        {
            _currentMissionStats = new MissionSessionStats
            {
                missionID = missionID,
                startTime = Time.time,
                eliminationsMade = 0,
                timesDetected = 0,
                skillsUsed = 0,
                perfectInputs = 0,
                missedInputs = 0
            };

            _stats.totalMissionsAttempted++;

            Debug.Log($"📊 Mission tracking started: {missionID}");
        }

        public void OnMissionCompleted(string missionID, int score, string rank, bool wasGhostRun)
        {
            if (_currentMissionStats == null || _currentMissionStats.missionID != missionID)
            {
                Debug.LogWarning("Mission completed without proper tracking!");
                return;
            }

            _stats.totalMissionsCompleted++;

            // Completion time
            float completionTime = Time.time - _currentMissionStats.startTime;
            if (completionTime < _stats.fastestMissionCompletionTime)
            {
                _stats.fastestMissionCompletionTime = completionTime;
            }

            // Score
            _stats.totalScore += score;
            if (score > _stats.highestSingleMissionScore)
            {
                _stats.highestSingleMissionScore = score;
            }

            // Rank
            switch (rank)
            {
                case "S":
                    _stats.sRankMissions++;
                    break;
                case "A":
                    _stats.aRankMissions++;
                    break;
            }

            // Ghost run
            if (wasGhostRun)
            {
                _stats.ghostRunMissions++;
            }

            // Perfect mission (no misses, no detections)
            if (_currentMissionStats.missedInputs == 0 && _currentMissionStats.timesDetected == 0)
            {
                _stats.perfectMissions++;
            }

            _currentMissionStats = null;

            SaveStatistics();

            Debug.Log($"📊 Mission completed tracked: {missionID}, Rank: {rank}, Score: {score}");
        }

        public void OnMissionFailed(string missionID)
        {
            _stats.totalMissionsFailed++;
            _currentMissionStats = null;

            SaveStatistics();

            Debug.Log($"📊 Mission failed tracked: {missionID}");
        }

        #endregion

        #region Combat Tracking

        public void OnEliminationMade(bool wasStealth, bool wasSkill)
        {
            _stats.totalEliminationsMade++;

            if (wasStealth)
            {
                _stats.stealthEliminationsCount++;
            }

            if (wasSkill)
            {
                _stats.skillEliminationsCount++;
            }

            if (_currentMissionStats != null)
            {
                _currentMissionStats.eliminationsMade++;
            }

            if (saveOnUpdate) SaveStatistics();
        }

        public void OnDetectedByEnemy()
        {
            _stats.enemiesDetectedByCount++;

            if (_currentMissionStats != null)
            {
                _currentMissionStats.timesDetected++;
            }

            if (saveOnUpdate) SaveStatistics();
        }

        public void OnAlarmTriggered()
        {
            _stats.alarmsTriggeredCount++;

            if (saveOnUpdate) SaveStatistics();
        }

        #endregion

        #region Rhythm Tracking

        public void OnInputAttempted(TimingRating rating)
        {
            _stats.totalInputsAttempted++;

            switch (rating)
            {
                case TimingRating.Perfect:
                    _stats.perfectInputsCount++;
                    if (_currentMissionStats != null)
                    {
                        _currentMissionStats.perfectInputs++;
                    }
                    break;

                case TimingRating.Great:
                    _stats.greatInputsCount++;
                    break;

                case TimingRating.Good:
                    _stats.goodInputsCount++;
                    break;

                case TimingRating.Miss:
                    _stats.missedInputsCount++;
                    if (_currentMissionStats != null)
                    {
                        _currentMissionStats.missedInputs++;
                    }
                    break;
            }

            if (saveOnUpdate) SaveStatistics();
        }

        public void OnPerfectStreak(int streakLength)
        {
            if (streakLength > _stats.longestPerfectStreak)
            {
                _stats.longestPerfectStreak = streakLength;
                SaveStatistics();
            }
        }

        #endregion

        #region Skill Tracking

        public void OnSkillUsed(string skillName, float focusCost)
        {
            _stats.totalSkillsUsed++;
            _stats.totalFocusSpent += focusCost;

            if (!_stats.skillUsageCount.ContainsKey(skillName))
            {
                _stats.skillUsageCount[skillName] = 0;
            }
            _stats.skillUsageCount[skillName]++;

            if (_currentMissionStats != null)
            {
                _currentMissionStats.skillsUsed++;
            }

            if (saveOnUpdate) SaveStatistics();
        }

        public Dictionary<string, int> GetSkillUsageStats()
        {
            return new Dictionary<string, int>(_stats.skillUsageCount);
        }

        public string GetMostUsedSkill()
        {
            if (_stats.skillUsageCount.Count == 0) return "None";

            return _stats.skillUsageCount
                .OrderByDescending(kvp => kvp.Value)
                .First()
                .Key;
        }

        #endregion

        #region Collectibles Tracking

        public void OnStarMapCollected()
        {
            _stats.starMapsCollected++;
            SaveStatistics();
        }

        public void OnSecretFound()
        {
            _stats.secretsFound++;
            SaveStatistics();
        }

        #endregion

        #region Statistics Getters

        public PlayerStatistics GetStatistics()
        {
            UpdatePlaytime();
            return _stats;
        }

        public float GetAccuracy()
        {
            if (_stats.totalInputsAttempted == 0) return 0f;

            return (float)_stats.perfectInputsCount / _stats.totalInputsAttempted * 100f;
        }

        public float GetCompletionRate()
        {
            if (_stats.totalMissionsAttempted == 0) return 0f;

            return (float)_stats.totalMissionsCompleted / _stats.totalMissionsAttempted * 100f;
        }

        public float GetGhostRunRate()
        {
            if (_stats.totalMissionsCompleted == 0) return 0f;

            return (float)_stats.ghostRunMissions / _stats.totalMissionsCompleted * 100f;
        }

        public string GetFormattedPlayTime()
        {
            int hours = Mathf.FloorToInt(_stats.totalPlayTimeSeconds / 3600f);
            int minutes = Mathf.FloorToInt((_stats.totalPlayTimeSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(_stats.totalPlayTimeSeconds % 60f);

            return $"{hours}h {minutes}m {seconds}s";
        }

        public Dictionary<TimingRating, int> GetTimingDistribution()
        {
            return new Dictionary<TimingRating, int>
            {
                { TimingRating.Perfect, _stats.perfectInputsCount },
                { TimingRating.Great, _stats.greatInputsCount },
                { TimingRating.Good, _stats.goodInputsCount },
                { TimingRating.Miss, _stats.missedInputsCount }
            };
        }

        #endregion

        #region Reset

        public void ResetAllStatistics()
        {
            PlayerPrefs.DeleteKey(STATS_SAVE_KEY);
            LoadStatistics();
            Debug.Log("🔄 All statistics reset");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class PlayerStatistics
    {
        [Header("Mission Stats")]
        public int totalMissionsAttempted;
        public int totalMissionsCompleted;
        public int totalMissionsFailed;
        public int perfectMissions;
        public int ghostRunMissions;

        [Header("Combat Stats")]
        public int totalEliminationsMade;
        public int stealthEliminationsCount;
        public int skillEliminationsCount;
        public int enemiesDetectedByCount;
        public int alarmsTriggeredCount;

        [Header("Rhythm Stats")]
        public int totalInputsAttempted;
        public int perfectInputsCount;
        public int greatInputsCount;
        public int goodInputsCount;
        public int missedInputsCount;
        public int longestPerfectStreak;

        [Header("Skill Stats")]
        public Dictionary<string, int> skillUsageCount;
        public int totalSkillsUsed;
        public float totalFocusSpent;

        [Header("Score Stats")]
        public int totalScore;
        public int highestSingleMissionScore;
        public int sRankMissions;
        public int aRankMissions;

        [Header("Time Stats")]
        public float totalPlayTimeSeconds;
        public float fastestMissionCompletionTime;

        [Header("Collectibles")]
        public int starMapsCollected;
        public int secretsFound;

        [Header("Meta")]
        public string firstPlayDate;
        public string lastPlayDate;
    }

    [System.Serializable]
    public class MissionSessionStats
    {
        public string missionID;
        public float startTime;
        public int eliminationsMade;
        public int timesDetected;
        public int skillsUsed;
        public int perfectInputs;
        public int missedInputs;
    }

    public enum TimingRating
    {
        Perfect,
        Great,
        Good,
        Miss
    }

    #endregion
}
