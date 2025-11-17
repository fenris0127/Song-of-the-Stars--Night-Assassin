using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Daily challenge system with seed-based procedural missions
    /// 시드 기반 프로시저럴 미션으로 일일 챌린지 시스템
    ///
    /// Generates same challenge for all players on same day
    /// </summary>
    public class DailyChallengeSystem : MonoBehaviour
    {
        public static DailyChallengeSystem Instance { get; private set; }

        [Header("▶ Challenge Settings")]
        public bool enableDailyChallenges = true;
        public int challengesPerDay = 3; // Easy, Normal, Hard
        public int hoursUntilRefresh = 24;

        [Header("▶ Rewards")]
        public int easyReward = 100;
        public int normalReward = 250;
        public int hardReward = 500;
        public int bonusForAllThree = 200; // Bonus if all 3 completed

        [Header("▶ Streak Bonuses")]
        public bool enableStreakBonus = true;
        [Tooltip("Bonus XP per consecutive day")]
        public int streakBonusPerDay = 50;
        public int maxStreakBonus = 500;

        private DailyChallengeData _todaysChallenges;
        private DailyChallengeProgress _progress;

        private const string PROGRESS_SAVE_KEY = "DailyChallengeProgress";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
            CheckAndGenerateChallenges();
        }

        private void CheckAndGenerateChallenges()
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            // Check if we need to generate new challenges
            if (_todaysChallenges == null ||
                DateTime.Parse(_todaysChallenges.date).Date != today)
            {
                GenerateDailyChallenges();
            }

            // Check if streak should reset
            if (_progress.lastCompletedDate != null)
            {
                DateTime lastCompleted = DateTime.Parse(_progress.lastCompletedDate).Date;
                TimeSpan daysSince = today - lastCompleted;

                if (daysSince.TotalDays > 1)
                {
                    // Streak broken
                    Debug.Log($"Streak broken! Was: {_progress.currentStreak} days");
                    _progress.currentStreak = 0;
                    SaveProgress();
                }
            }
        }

        private void GenerateDailyChallenges()
        {
            DateTime today = DateTime.Now.Date;
            int todaySeed = GetDailySeed(today);

            _todaysChallenges = new DailyChallengeData
            {
                date = today.ToString("yyyy-MM-dd"),
                seed = todaySeed,
                challenges = new List<DailyChallenge>()
            };

            // Generate 3 challenges: Easy, Normal, Hard
            _todaysChallenges.challenges.Add(GenerateChallenge(0, "Easy", todaySeed + 1, easyReward));
            _todaysChallenges.challenges.Add(GenerateChallenge(1, "Normal", todaySeed + 2, normalReward));
            _todaysChallenges.challenges.Add(GenerateChallenge(2, "Hard", todaySeed + 3, hardReward));

            Debug.Log($"✨ Daily challenges generated for {today:yyyy-MM-dd}\n" +
                      $"Seed: {todaySeed}\n" +
                      $"Easy: {_todaysChallenges.challenges[0].missionName}\n" +
                      $"Normal: {_todaysChallenges.challenges[1].missionName}\n" +
                      $"Hard: {_todaysChallenges.challenges[2].missionName}");
        }

        private DailyChallenge GenerateChallenge(int index, string difficulty, int seed, int reward)
        {
            Random.InitState(seed);

            string[] nameTemplates = new string[]
            {
                "Daily {0}: {1}",
                "Challenge {0}: {1}",
                "Today's {0}: {1}"
            };

            string[] missionTypes = new string[]
            {
                "Assassination", "Infiltration", "Heist", "Extraction",
                "Sabotage", "Reconnaissance", "Strike", "Operation"
            };

            string missionType = missionTypes[Random.Range(0, missionTypes.Length)];
            string challengeName = string.Format(nameTemplates[Random.Range(0, nameTemplates.Length)],
                                                  difficulty, missionType);

            DailyChallenge challenge = new DailyChallenge
            {
                challengeID = $"daily_{DateTime.Now:yyyyMMdd}_{index}",
                missionName = challengeName,
                difficulty = difficulty,
                seed = seed,
                experienceReward = reward,
                isCompleted = false
            };

            // Generate objectives based on difficulty
            int objectiveCount = difficulty switch
            {
                "Easy" => Random.Range(2, 4),
                "Normal" => Random.Range(3, 5),
                "Hard" => Random.Range(4, 7),
                _ => 3
            };

            challenge.objectives = new List<string>();
            for (int i = 0; i < objectiveCount; i++)
            {
                challenge.objectives.Add(GenerateRandomObjective());
            }

            // Add special modifiers for harder challenges
            if (difficulty == "Hard")
            {
                challenge.modifiers = GenerateRandomModifiers(Random.Range(2, 4));
            }
            else if (difficulty == "Normal")
            {
                challenge.modifiers = GenerateRandomModifiers(Random.Range(1, 3));
            }

            return challenge;
        }

        private string GenerateRandomObjective()
        {
            string[] objectiveTemplates = new string[]
            {
                "Eliminate {0} target(s)",
                "Collect {0} item(s)",
                "Reach extraction point",
                "Complete without detection",
                "Finish in under {0} seconds",
                "Maintain {0}% accuracy",
                "Use only {0} skill(s)"
            };

            string template = objectiveTemplates[Random.Range(0, objectiveTemplates.Length)];

            // Fill in numbers
            if (template.Contains("{0}"))
            {
                int value = Random.Range(1, 5);
                return string.Format(template, value);
            }

            return template;
        }

        private List<string> GenerateRandomModifiers(int count)
        {
            List<string> allModifiers = new List<string>
            {
                "Reduced Focus (-50%)",
                "Increased Guard Vision (+30%)",
                "Tight Timing Windows (-30%)",
                "Limited Skills (2 max)",
                "No Stealth Skills",
                "Faster BPM (+20%)",
                "Reduced Movement Speed (-20%)",
                "Longer Cooldowns (+50%)",
                "Perfect Timing Required",
                "One Detection = Fail"
            };

            List<string> selected = new List<string>();
            for (int i = 0; i < count && allModifiers.Count > 0; i++)
            {
                int index = Random.Range(0, allModifiers.Count);
                selected.Add(allModifiers[index]);
                allModifiers.RemoveAt(index);
            }

            return selected;
        }

        private int GetDailySeed(DateTime date)
        {
            // Generate seed from date (same for all players worldwide)
            int year = date.Year;
            int dayOfYear = date.DayOfYear;
            return year * 1000 + dayOfYear;
        }

        public void CompleteChallenge(string challengeID, int score, float completionTime)
        {
            DailyChallenge challenge = _todaysChallenges.challenges
                .FirstOrDefault(c => c.challengeID == challengeID);

            if (challenge == null)
            {
                Debug.LogWarning($"Challenge not found: {challengeID}");
                return;
            }

            if (challenge.isCompleted)
            {
                Debug.LogWarning($"Challenge already completed: {challengeID}");
                return;
            }

            challenge.isCompleted = true;
            challenge.completionTime = completionTime;
            challenge.achievedScore = score;

            int reward = challenge.experienceReward;

            // Apply streak bonus
            if (enableStreakBonus && _progress.currentStreak > 0)
            {
                int streakBonus = Mathf.Min(_progress.currentStreak * streakBonusPerDay, maxStreakBonus);
                reward += streakBonus;
                Debug.Log($"🔥 Streak bonus: +{streakBonus} XP (Streak: {_progress.currentStreak} days)");
            }

            // Check if all challenges completed
            bool allCompleted = _todaysChallenges.challenges.All(c => c.isCompleted);
            if (allCompleted)
            {
                reward += bonusForAllThree;
                Debug.Log($"⭐ All challenges complete! Bonus: +{bonusForAllThree} XP");

                // Update streak
                DateTime today = DateTime.Now.Date;
                DateTime lastCompleted = _progress.lastCompletedDate != null
                    ? DateTime.Parse(_progress.lastCompletedDate).Date
                    : DateTime.MinValue;

                if (today == lastCompleted.AddDays(1))
                {
                    _progress.currentStreak++;
                }
                else if (today != lastCompleted)
                {
                    _progress.currentStreak = 1;
                }

                if (_progress.currentStreak > _progress.longestStreak)
                {
                    _progress.longestStreak = _progress.currentStreak;
                }

                _progress.lastCompletedDate = today.ToString("yyyy-MM-dd");
                _progress.totalChallengesCompleted += challengesPerDay;
            }
            else
            {
                _progress.totalChallengesCompleted++;
            }

            SaveProgress();

            Debug.Log($"✅ Challenge completed: {challenge.missionName}\n" +
                      $"Score: {score}, Time: {completionTime:F2}s\n" +
                      $"Reward: {reward} XP");

            // TODO: Award XP to player
        }

        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey(PROGRESS_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(PROGRESS_SAVE_KEY);
                _progress = JsonUtility.FromJson<DailyChallengeProgress>(json);
            }
            else
            {
                _progress = new DailyChallengeProgress
                {
                    currentStreak = 0,
                    longestStreak = 0,
                    totalChallengesCompleted = 0,
                    lastCompletedDate = null
                };
            }
        }

        private void SaveProgress()
        {
            string json = JsonUtility.ToJson(_progress);
            PlayerPrefs.SetString(PROGRESS_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public DailyChallengeData GetTodaysChallenges()
        {
            CheckAndGenerateChallenges();
            return _todaysChallenges;
        }

        public DailyChallengeProgress GetProgress()
        {
            return _progress;
        }

        public TimeSpan GetTimeUntilRefresh()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.Date.AddDays(1);
            return tomorrow - now;
        }

        public string GetFormattedTimeUntilRefresh()
        {
            TimeSpan time = GetTimeUntilRefresh();
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }

    [System.Serializable]
    public class DailyChallengeData
    {
        public string date;
        public int seed;
        public List<DailyChallenge> challenges;
    }

    [System.Serializable]
    public class DailyChallenge
    {
        public string challengeID;
        public string missionName;
        public string difficulty;
        public int seed;
        public int experienceReward;
        public List<string> objectives;
        public List<string> modifiers;

        public bool isCompleted;
        public float completionTime;
        public int achievedScore;
    }

    [System.Serializable]
    public class DailyChallengeProgress
    {
        public int currentStreak;
        public int longestStreak;
        public int totalChallengesCompleted;
        public string lastCompletedDate; // yyyy-MM-dd
    }
}
