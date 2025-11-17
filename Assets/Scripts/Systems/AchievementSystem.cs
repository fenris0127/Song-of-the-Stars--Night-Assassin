using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Achievement tracking and unlock system
    /// 업적 추적 및 잠금 해제 시스템
    ///
    /// Tracks player progress and unlocks achievements
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        [Header("▶ Achievement Settings")]
        public bool enableAchievements = true;
        public bool showNotifications = true;
        public float notificationDuration = 5f;

        [Header("▶ Events")]
        public UnityEvent<Achievement> OnAchievementUnlocked;
        public UnityEvent<Achievement, int> OnAchievementProgress;

        [Header("▶ Achievements Database")]
        public List<Achievement> allAchievements = new List<Achievement>();

        private AchievementProgress _progress;
        private const string SAVE_KEY = "AchievementProgress";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAchievements();
            LoadProgress();
        }

        private void InitializeAchievements()
        {
            if (allAchievements.Count > 0) return;

            // Define all achievements
            allAchievements = new List<Achievement>
            {
                // Completion Achievements
                new Achievement
                {
                    id = "first_mission",
                    name = "First Steps",
                    description = "Complete your first mission",
                    category = AchievementCategory.Completion,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 50,
                    icon = null
                },
                new Achievement
                {
                    id = "complete_10_missions",
                    name = "Seasoned Assassin",
                    description = "Complete 10 missions",
                    category = AchievementCategory.Completion,
                    type = AchievementType.Incremental,
                    requirement = 10,
                    rewardXP = 200,
                    icon = null
                },
                new Achievement
                {
                    id = "complete_50_missions",
                    name = "Master of Shadows",
                    description = "Complete 50 missions",
                    category = AchievementCategory.Completion,
                    type = AchievementType.Incremental,
                    requirement = 50,
                    rewardXP = 1000,
                    icon = null
                },

                // Skill Achievements
                new Achievement
                {
                    id = "first_skill",
                    name = "Constellation Power",
                    description = "Use your first skill",
                    category = AchievementCategory.Skills,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 25,
                    icon = null
                },
                new Achievement
                {
                    id = "use_all_skills",
                    name = "Master of the Stars",
                    description = "Use all 8 constellation skills",
                    category = AchievementCategory.Skills,
                    type = AchievementType.Collection,
                    requirement = 8,
                    rewardXP = 300,
                    icon = null
                },
                new Achievement
                {
                    id = "perfect_100_skills",
                    name = "Rhythm Expert",
                    description = "Activate 100 skills with Perfect timing",
                    category = AchievementCategory.Skills,
                    type = AchievementType.Incremental,
                    requirement = 100,
                    rewardXP = 500,
                    icon = null
                },

                // Stealth Achievements
                new Achievement
                {
                    id = "first_ghost",
                    name = "Ghost",
                    description = "Complete a mission without being detected",
                    category = AchievementCategory.Stealth,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 100,
                    icon = null
                },
                new Achievement
                {
                    id = "ghost_10_missions",
                    name = "Phantom",
                    description = "Complete 10 missions without detection",
                    category = AchievementCategory.Stealth,
                    type = AchievementType.Incremental,
                    requirement = 10,
                    rewardXP = 500,
                    icon = null
                },
                new Achievement
                {
                    id = "never_detected",
                    name = "Shadow Walker",
                    description = "Never be detected in 5 consecutive missions",
                    category = AchievementCategory.Stealth,
                    type = AchievementType.Streak,
                    requirement = 5,
                    rewardXP = 750,
                    icon = null
                },

                // Combat Achievements
                new Achievement
                {
                    id = "eliminate_100",
                    name = "Silent Reaper",
                    description = "Eliminate 100 targets",
                    category = AchievementCategory.Combat,
                    type = AchievementType.Incremental,
                    requirement = 100,
                    rewardXP = 400,
                    icon = null
                },
                new Achievement
                {
                    id = "orion_arrow_expert",
                    name = "Orion's Marksman",
                    description = "Eliminate 50 targets with Orion's Arrow",
                    category = AchievementCategory.Combat,
                    type = AchievementType.Incremental,
                    requirement = 50,
                    rewardXP = 300,
                    icon = null
                },

                // Rhythm Achievements
                new Achievement
                {
                    id = "perfect_100",
                    name = "Rhythm Master",
                    description = "Hit 100 Perfect inputs",
                    category = AchievementCategory.Rhythm,
                    type = AchievementType.Incremental,
                    requirement = 100,
                    rewardXP = 200,
                    icon = null
                },
                new Achievement
                {
                    id = "perfect_streak_50",
                    name = "In The Zone",
                    description = "Achieve a 50-hit Perfect combo",
                    category = AchievementCategory.Rhythm,
                    type = AchievementType.Single,
                    requirement = 50,
                    rewardXP = 500,
                    icon = null
                },
                new Achievement
                {
                    id = "no_misses",
                    name = "Flawless Rhythm",
                    description = "Complete a mission with 100% Perfect accuracy",
                    category = AchievementCategory.Rhythm,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 300,
                    icon = null
                },

                // Score Achievements
                new Achievement
                {
                    id = "rank_s",
                    name = "S-Rank Assassin",
                    description = "Achieve S-Rank on any mission",
                    category = AchievementCategory.Score,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 150,
                    icon = null
                },
                new Achievement
                {
                    id = "score_5000",
                    name = "High Scorer",
                    description = "Score 5000 points in a single mission",
                    category = AchievementCategory.Score,
                    type = AchievementType.Single,
                    requirement = 5000,
                    rewardXP = 400,
                    icon = null
                },

                // Challenge Achievements
                new Achievement
                {
                    id = "daily_7_streak",
                    name = "Dedicated",
                    description = "Complete daily challenges for 7 consecutive days",
                    category = AchievementCategory.Challenge,
                    type = AchievementType.Streak,
                    requirement = 7,
                    rewardXP = 500,
                    icon = null
                },
                new Achievement
                {
                    id = "all_daily_same_day",
                    name = "Triple Threat",
                    description = "Complete all 3 daily challenges in one day",
                    category = AchievementCategory.Challenge,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 250,
                    icon = null
                },

                // Speed Achievements
                new Achievement
                {
                    id = "speed_run_60s",
                    name = "Lightning Fast",
                    description = "Complete a mission in under 60 seconds",
                    category = AchievementCategory.Speed,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 300,
                    icon = null
                },

                // Collection Achievements
                new Achievement
                {
                    id = "collect_all_maps",
                    name = "Cartographer",
                    description = "Collect all star maps",
                    category = AchievementCategory.Collection,
                    type = AchievementType.Single,
                    requirement = 1,
                    rewardXP = 200,
                    icon = null
                }
            };
        }

        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                _progress = JsonUtility.FromJson<AchievementProgress>(json);
            }
            else
            {
                _progress = new AchievementProgress
                {
                    unlockedAchievements = new List<string>(),
                    achievementProgress = new Dictionary<string, int>()
                };
            }
        }

        private void SaveProgress()
        {
            string json = JsonUtility.ToJson(_progress, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        #region Progress Tracking

        public void TrackProgress(string achievementID, int amount = 1)
        {
            if (!enableAchievements) return;

            Achievement achievement = allAchievements.FirstOrDefault(a => a.id == achievementID);
            if (achievement == null)
            {
                Debug.LogWarning($"Achievement not found: {achievementID}");
                return;
            }

            if (_progress.unlockedAchievements.Contains(achievementID))
            {
                // Already unlocked
                return;
            }

            // Update progress
            if (!_progress.achievementProgress.ContainsKey(achievementID))
            {
                _progress.achievementProgress[achievementID] = 0;
            }

            int oldProgress = _progress.achievementProgress[achievementID];
            _progress.achievementProgress[achievementID] += amount;
            int newProgress = _progress.achievementProgress[achievementID];

            // Invoke progress event
            OnAchievementProgress?.Invoke(achievement, newProgress);

            // Check if unlocked
            if (newProgress >= achievement.requirement && oldProgress < achievement.requirement)
            {
                UnlockAchievement(achievementID);
            }

            SaveProgress();
        }

        private void UnlockAchievement(string achievementID)
        {
            Achievement achievement = allAchievements.FirstOrDefault(a => a.id == achievementID);
            if (achievement == null) return;

            _progress.unlockedAchievements.Add(achievementID);
            achievement.unlockedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            SaveProgress();

            Debug.Log($"🏆 Achievement Unlocked: {achievement.name} (+{achievement.rewardXP} XP)");

            // Invoke unlock event
            OnAchievementUnlocked?.Invoke(achievement);

            // Show notification
            if (showNotifications)
            {
                ShowNotification(achievement);
            }

            // Award XP (integrate with progression system)
            // ProgressionManager.Instance.AddExperience(achievement.rewardXP);
        }

        private void ShowNotification(Achievement achievement)
        {
            // This would trigger UI notification
            // For now, just log
            Debug.Log($"🎉 ACHIEVEMENT UNLOCKED!\n{achievement.name}\n{achievement.description}");
        }

        #endregion

        #region Public API

        public bool IsUnlocked(string achievementID)
        {
            return _progress.unlockedAchievements.Contains(achievementID);
        }

        public int GetProgress(string achievementID)
        {
            return _progress.achievementProgress.ContainsKey(achievementID)
                ? _progress.achievementProgress[achievementID]
                : 0;
        }

        public float GetProgressPercentage(string achievementID)
        {
            Achievement achievement = allAchievements.FirstOrDefault(a => a.id == achievementID);
            if (achievement == null) return 0f;

            int current = GetProgress(achievementID);
            return Mathf.Clamp01((float)current / achievement.requirement) * 100f;
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            return allAchievements
                .Where(a => _progress.unlockedAchievements.Contains(a.id))
                .ToList();
        }

        public List<Achievement> GetLockedAchievements()
        {
            return allAchievements
                .Where(a => !_progress.unlockedAchievements.Contains(a.id))
                .ToList();
        }

        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
        {
            return allAchievements.Where(a => a.category == category).ToList();
        }

        public int GetTotalUnlockedCount()
        {
            return _progress.unlockedAchievements.Count;
        }

        public float GetCompletionPercentage()
        {
            return (float)_progress.unlockedAchievements.Count / allAchievements.Count * 100f;
        }

        public int GetTotalRewardXP()
        {
            return GetUnlockedAchievements().Sum(a => a.rewardXP);
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        [TextArea(2, 4)]
        public string description;
        public AchievementCategory category;
        public AchievementType type;
        public int requirement; // Amount needed to unlock
        public int rewardXP;
        public Sprite icon;
        public string unlockedDate; // yyyy-MM-dd HH:mm:ss
    }

    [System.Serializable]
    public class AchievementProgress
    {
        public List<string> unlockedAchievements;
        public Dictionary<string, int> achievementProgress;
    }

    public enum AchievementCategory
    {
        Completion,   // Mission completion
        Skills,       // Skill usage
        Stealth,      // Stealth gameplay
        Combat,       // Combat/elimination
        Rhythm,       // Rhythm accuracy
        Score,        // High scores
        Challenge,    // Daily challenges
        Speed,        // Speed runs
        Collection    // Collectibles
    }

    public enum AchievementType
    {
        Single,       // Unlock once (e.g., first mission)
        Incremental,  // Accumulate over time (e.g., 100 eliminations)
        Streak,       // Consecutive count (e.g., 5 missions in a row)
        Collection    // Collect all items
    }

    #endregion
}
