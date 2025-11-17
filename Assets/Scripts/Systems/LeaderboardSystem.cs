using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Local leaderboard system for tracking high scores
    /// 하이스코어 추적을 위한 로컬 리더보드 시스템
    ///
    /// Tracks: Global scores, per-mission scores, daily/weekly/monthly
    /// </summary>
    public class LeaderboardSystem : MonoBehaviour
    {
        public static LeaderboardSystem Instance { get; private set; }

        [Header("▶ Leaderboard Settings")]
        public int maxEntriesPerCategory = 100;
        public bool autoSubmitScores = true;

        [Header("▶ Time Periods")]
        public bool trackDailyLeaderboards = true;
        public bool trackWeeklyLeaderboards = true;
        public bool trackMonthlyLeaderboards = true;

        private LeaderboardData _leaderboards;

        private const string LEADERBOARD_SAVE_KEY = "LeaderboardData";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLeaderboards();
            CleanupOldEntries();
        }

        private void LoadLeaderboards()
        {
            if (PlayerPrefs.HasKey(LEADERBOARD_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(LEADERBOARD_SAVE_KEY);
                _leaderboards = JsonUtility.FromJson<LeaderboardData>(json);
            }
            else
            {
                _leaderboards = new LeaderboardData
                {
                    globalEntries = new List<LeaderboardEntry>(),
                    missionEntries = new Dictionary<string, List<LeaderboardEntry>>(),
                    dailyEntries = new List<LeaderboardEntry>(),
                    weeklyEntries = new List<LeaderboardEntry>(),
                    monthlyEntries = new List<LeaderboardEntry>()
                };
            }
        }

        private void SaveLeaderboards()
        {
            string json = JsonUtility.ToJson(_leaderboards, true);
            PlayerPrefs.SetString(LEADERBOARD_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void CleanupOldEntries()
        {
            System.DateTime now = System.DateTime.Now;

            // Clean up daily entries (older than 7 days)
            if (trackDailyLeaderboards)
            {
                _leaderboards.dailyEntries.RemoveAll(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp);
                    return (now - entryDate).TotalDays > 7;
                });
            }

            // Clean up weekly entries (older than 4 weeks)
            if (trackWeeklyLeaderboards)
            {
                _leaderboards.weeklyEntries.RemoveAll(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp);
                    return (now - entryDate).TotalDays > 28;
                });
            }

            // Clean up monthly entries (older than 6 months)
            if (trackMonthlyLeaderboards)
            {
                _leaderboards.monthlyEntries.RemoveAll(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp);
                    return (now - entryDate).TotalDays > 180;
                });
            }
        }

        public void SubmitScore(
            string playerName,
            string missionID,
            string missionName,
            int score,
            float completionTime,
            string rank,
            bool wasGhostRun,
            float accuracy)
        {
            LeaderboardEntry entry = new LeaderboardEntry
            {
                playerName = playerName,
                missionID = missionID,
                missionName = missionName,
                score = score,
                completionTime = completionTime,
                rank = rank,
                wasGhostRun = wasGhostRun,
                accuracy = accuracy,
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Submit to global leaderboard
            AddToLeaderboard(_leaderboards.globalEntries, entry);

            // Submit to mission-specific leaderboard
            if (!_leaderboards.missionEntries.ContainsKey(missionID))
            {
                _leaderboards.missionEntries[missionID] = new List<LeaderboardEntry>();
            }
            AddToLeaderboard(_leaderboards.missionEntries[missionID], entry);

            // Submit to time-period leaderboards
            if (trackDailyLeaderboards)
            {
                AddToLeaderboard(_leaderboards.dailyEntries, entry);
            }

            if (trackWeeklyLeaderboards)
            {
                AddToLeaderboard(_leaderboards.weeklyEntries, entry);
            }

            if (trackMonthlyLeaderboards)
            {
                AddToLeaderboard(_leaderboards.monthlyEntries, entry);
            }

            SaveLeaderboards();

            Debug.Log($"📊 Score submitted to leaderboards\n" +
                      $"Player: {playerName}, Score: {score}, Rank: {rank}");
        }

        private void AddToLeaderboard(List<LeaderboardEntry> leaderboard, LeaderboardEntry entry)
        {
            leaderboard.Add(entry);

            // Sort by score (descending)
            leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Limit entries
            if (leaderboard.Count > maxEntriesPerCategory)
            {
                leaderboard.RemoveRange(maxEntriesPerCategory, leaderboard.Count - maxEntriesPerCategory);
            }
        }

        #region Get Leaderboards

        public List<LeaderboardEntry> GetGlobalLeaderboard(int count = 10)
        {
            return _leaderboards.globalEntries.Take(count).ToList();
        }

        public List<LeaderboardEntry> GetMissionLeaderboard(string missionID, int count = 10)
        {
            if (_leaderboards.missionEntries.ContainsKey(missionID))
            {
                return _leaderboards.missionEntries[missionID].Take(count).ToList();
            }
            return new List<LeaderboardEntry>();
        }

        public List<LeaderboardEntry> GetDailyLeaderboard(int count = 10)
        {
            System.DateTime today = System.DateTime.Now.Date;

            return _leaderboards.dailyEntries
                .Where(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp).Date;
                    return entryDate == today;
                })
                .OrderByDescending(e => e.score)
                .Take(count)
                .ToList();
        }

        public List<LeaderboardEntry> GetWeeklyLeaderboard(int count = 10)
        {
            System.DateTime now = System.DateTime.Now;
            System.DateTime weekStart = now.Date.AddDays(-(int)now.DayOfWeek);

            return _leaderboards.weeklyEntries
                .Where(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp).Date;
                    return entryDate >= weekStart;
                })
                .OrderByDescending(e => e.score)
                .Take(count)
                .ToList();
        }

        public List<LeaderboardEntry> GetMonthlyLeaderboard(int count = 10)
        {
            System.DateTime now = System.DateTime.Now;
            System.DateTime monthStart = new System.DateTime(now.Year, now.Month, 1);

            return _leaderboards.monthlyEntries
                .Where(e =>
                {
                    System.DateTime entryDate = System.DateTime.Parse(e.timestamp).Date;
                    return entryDate >= monthStart;
                })
                .OrderByDescending(e => e.score)
                .Take(count)
                .ToList();
        }

        #endregion

        #region Player Rankings

        public int GetPlayerGlobalRank(string playerName)
        {
            LeaderboardEntry bestScore = _leaderboards.globalEntries
                .Where(e => e.playerName == playerName)
                .OrderByDescending(e => e.score)
                .FirstOrDefault();

            if (bestScore == null) return -1;

            return _leaderboards.globalEntries
                .OrderByDescending(e => e.score)
                .ToList()
                .IndexOf(bestScore) + 1;
        }

        public int GetPlayerMissionRank(string playerName, string missionID)
        {
            if (!_leaderboards.missionEntries.ContainsKey(missionID))
                return -1;

            LeaderboardEntry bestScore = _leaderboards.missionEntries[missionID]
                .Where(e => e.playerName == playerName)
                .OrderByDescending(e => e.score)
                .FirstOrDefault();

            if (bestScore == null) return -1;

            return _leaderboards.missionEntries[missionID]
                .OrderByDescending(e => e.score)
                .ToList()
                .IndexOf(bestScore) + 1;
        }

        public LeaderboardEntry GetPlayerBestScore(string playerName)
        {
            return _leaderboards.globalEntries
                .Where(e => e.playerName == playerName)
                .OrderByDescending(e => e.score)
                .FirstOrDefault();
        }

        public LeaderboardEntry GetPlayerBestMissionScore(string playerName, string missionID)
        {
            if (!_leaderboards.missionEntries.ContainsKey(missionID))
                return null;

            return _leaderboards.missionEntries[missionID]
                .Where(e => e.playerName == playerName)
                .OrderByDescending(e => e.score)
                .FirstOrDefault();
        }

        #endregion

        #region Statistics

        public LeaderboardStats GetPlayerStats(string playerName)
        {
            var allPlayerEntries = _leaderboards.globalEntries
                .Where(e => e.playerName == playerName)
                .ToList();

            if (allPlayerEntries.Count == 0)
            {
                return new LeaderboardStats
                {
                    totalMissionsPlayed = 0,
                    averageScore = 0,
                    bestScore = 0,
                    averageRank = "N/A",
                    ghostRunCount = 0,
                    averageAccuracy = 0f
                };
            }

            return new LeaderboardStats
            {
                totalMissionsPlayed = allPlayerEntries.Count,
                averageScore = (int)allPlayerEntries.Average(e => e.score),
                bestScore = allPlayerEntries.Max(e => e.score),
                averageRank = GetAverageRank(allPlayerEntries),
                ghostRunCount = allPlayerEntries.Count(e => e.wasGhostRun),
                averageAccuracy = allPlayerEntries.Average(e => e.accuracy)
            };
        }

        private string GetAverageRank(List<LeaderboardEntry> entries)
        {
            Dictionary<string, int> rankValues = new Dictionary<string, int>
            {
                { "F", 0 }, { "D", 1 }, { "C", 2 }, { "B", 3 }, { "A", 4 }, { "S", 5 }
            };

            int totalValue = entries.Sum(e => rankValues.ContainsKey(e.rank) ? rankValues[e.rank] : 0);
            int avgValue = totalValue / entries.Count;

            return rankValues.FirstOrDefault(kvp => kvp.Value == avgValue).Key ?? "C";
        }

        public List<string> GetTopPlayers(int count = 10)
        {
            return _leaderboards.globalEntries
                .GroupBy(e => e.playerName)
                .OrderByDescending(g => g.Max(e => e.score))
                .Take(count)
                .Select(g => g.Key)
                .ToList();
        }

        #endregion

        #region Clear Methods

        public void ClearAllLeaderboards()
        {
            _leaderboards = new LeaderboardData
            {
                globalEntries = new List<LeaderboardEntry>(),
                missionEntries = new Dictionary<string, List<LeaderboardEntry>>(),
                dailyEntries = new List<LeaderboardEntry>(),
                weeklyEntries = new List<LeaderboardEntry>(),
                monthlyEntries = new List<LeaderboardEntry>()
            };

            SaveLeaderboards();
            Debug.Log("🗑️ All leaderboards cleared");
        }

        public void ClearMissionLeaderboard(string missionID)
        {
            if (_leaderboards.missionEntries.ContainsKey(missionID))
            {
                _leaderboards.missionEntries.Remove(missionID);
                SaveLeaderboards();
                Debug.Log($"🗑️ Leaderboard cleared for mission: {missionID}");
            }
        }

        #endregion
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> globalEntries;
        public Dictionary<string, List<LeaderboardEntry>> missionEntries;
        public List<LeaderboardEntry> dailyEntries;
        public List<LeaderboardEntry> weeklyEntries;
        public List<LeaderboardEntry> monthlyEntries;
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public string missionID;
        public string missionName;
        public int score;
        public float completionTime;
        public string rank; // S, A, B, C, D, F
        public bool wasGhostRun;
        public float accuracy;
        public string timestamp; // yyyy-MM-dd HH:mm:ss
    }

    [System.Serializable]
    public class LeaderboardStats
    {
        public int totalMissionsPlayed;
        public int averageScore;
        public int bestScore;
        public string averageRank;
        public int ghostRunCount;
        public float averageAccuracy;
    }
}
