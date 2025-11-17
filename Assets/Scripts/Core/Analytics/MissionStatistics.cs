using UnityEngine;
using System.Collections.Generic;

namespace SongOfTheStars.Analytics
{
    /// <summary>
    /// Tracks detailed statistics for a single mission playthrough
    /// 단일 미션 플레이스루에 대한 상세 통계 추적
    ///
    /// Used for post-mission summary and analytics
    /// </summary>
    [System.Serializable]
    public class MissionStatistics
    {
        #region Mission Info
        [Header("▶ Mission Info")]
        public string missionID = "";
        public string missionName = "";
        public float startTime = 0f;
        public float endTime = 0f;
        public bool wasSuccessful = false;
        #endregion

        #region Rhythm Stats
        [Header("▶ Rhythm Performance")]
        public int totalInputs = 0;
        public int perfectInputs = 0;
        public int greatInputs = 0;
        public int missInputs = 0;
        public int longestCombo = 0;
        public int currentCombo = 0;
        public float averageTimingOffset = 0f; // Average ms from perfect timing
        #endregion

        #region Combat Stats
        [Header("▶ Combat")]
        public int guardsEliminated = 0;
        public int guardsStunned = 0;
        public int skillsUsed = 0;
        public Dictionary<string, int> skillUsageCount = new Dictionary<string, int>();
        public int perfectKills = 0; // Kills with perfect rhythm timing
        #endregion

        #region Stealth Stats
        [Header("▶ Stealth")]
        public int timesDetected = 0;
        public int timesSpotted = 0; // Seen but not fully detected
        public int alertsTriggered = 0;
        public float timeInShadows = 0f; // Time spent in stealth/hiding
        public int bodiesHidden = 0;
        public int noisesCreated = 0;
        #endregion

        #region Movement Stats
        [Header("▶ Movement")]
        public float distanceTraveled = 0f;
        public int dashesUsed = 0;
        public int teleportsUsed = 0;
        public Vector3 lastPosition = Vector3.zero;
        #endregion

        #region Objective Stats
        [Header("▶ Objectives")]
        public int primaryObjectivesCompleted = 0;
        public int primaryObjectivesTotal = 0;
        public int optionalObjectivesCompleted = 0;
        public int optionalObjectivesTotal = 0;
        public float firstObjectiveTime = 0f; // Time to first objective
        public float lastObjectiveTime = 0f; // Time to last objective
        #endregion

        #region Focus Stats
        [Header("▶ Focus Management")]
        public float totalFocusGenerated = 0f;
        public float totalFocusSpent = 0f;
        public float maxFocusReached = 0f;
        public int timesMaxFocus = 0; // Times hit max focus
        #endregion

        #region Score Calculation
        [Header("▶ Score")]
        public int baseScore = 0;
        public int rhythmBonus = 0;
        public int stealthBonus = 0;
        public int speedBonus = 0;
        public int optionalBonus = 0;
        public int finalScore = 0;
        #endregion

        #region Methods
        /// <summary>
        /// Starts tracking statistics for a mission
        /// </summary>
        public void StartMission(string id, string name)
        {
            missionID = id;
            missionName = name;
            startTime = Time.time;
            lastPosition = Vector3.zero;

            // Reset all stats
            ResetStats();
        }

        /// <summary>
        /// Ends tracking and calculates final score
        /// </summary>
        public void EndMission(bool success, int primaryTotal, int optionalTotal)
        {
            endTime = Time.time;
            wasSuccessful = success;
            primaryObjectivesTotal = primaryTotal;
            optionalObjectivesTotal = optionalTotal;

            CalculateFinalScore();
        }

        private void ResetStats()
        {
            totalInputs = 0;
            perfectInputs = 0;
            greatInputs = 0;
            missInputs = 0;
            longestCombo = 0;
            currentCombo = 0;
            averageTimingOffset = 0f;

            guardsEliminated = 0;
            guardsStunned = 0;
            skillsUsed = 0;
            skillUsageCount.Clear();
            perfectKills = 0;

            timesDetected = 0;
            timesSpotted = 0;
            alertsTriggered = 0;
            timeInShadows = 0f;
            bodiesHidden = 0;
            noisesCreated = 0;

            distanceTraveled = 0f;
            dashesUsed = 0;
            teleportsUsed = 0;

            primaryObjectivesCompleted = 0;
            optionalObjectivesCompleted = 0;

            totalFocusGenerated = 0f;
            totalFocusSpent = 0f;
            maxFocusReached = 0f;
            timesMaxFocus = 0;
        }

        #region Record Methods
        public void RecordInput(RhythmJudgment judgment, float timingOffset)
        {
            totalInputs++;

            switch (judgment)
            {
                case RhythmJudgment.Perfect:
                    perfectInputs++;
                    currentCombo++;
                    break;
                case RhythmJudgment.Great:
                    greatInputs++;
                    currentCombo++;
                    break;
                case RhythmJudgment.Miss:
                    missInputs++;
                    currentCombo = 0;
                    break;
            }

            // Update longest combo
            if (currentCombo > longestCombo)
            {
                longestCombo = currentCombo;
            }

            // Update average timing offset
            averageTimingOffset = ((averageTimingOffset * (totalInputs - 1)) + Mathf.Abs(timingOffset)) / totalInputs;
        }

        public void RecordGuardEliminated(bool wasPerfectTiming)
        {
            guardsEliminated++;
            if (wasPerfectTiming) perfectKills++;
        }

        public void RecordGuardStunned()
        {
            guardsStunned++;
        }

        public void RecordSkillUsed(string skillName)
        {
            skillsUsed++;

            if (!skillUsageCount.ContainsKey(skillName))
            {
                skillUsageCount[skillName] = 0;
            }
            skillUsageCount[skillName]++;
        }

        public void RecordDetection()
        {
            timesDetected++;
            alertsTriggered++;
        }

        public void RecordSpotted()
        {
            timesSpotted++;
        }

        public void RecordMovement(Vector3 currentPosition)
        {
            if (lastPosition != Vector3.zero)
            {
                distanceTraveled += Vector3.Distance(lastPosition, currentPosition);
            }
            lastPosition = currentPosition;
        }

        public void RecordDash()
        {
            dashesUsed++;
        }

        public void RecordTeleport()
        {
            teleportsUsed++;
        }

        public void RecordObjectiveComplete(bool isPrimary)
        {
            if (isPrimary)
            {
                primaryObjectivesCompleted++;

                // Record first objective time
                if (primaryObjectivesCompleted == 1)
                {
                    firstObjectiveTime = Time.time - startTime;
                }
            }
            else
            {
                optionalObjectivesCompleted++;
            }

            // Always update last objective time
            lastObjectiveTime = Time.time - startTime;
        }

        public void RecordFocusGenerated(float amount)
        {
            totalFocusGenerated += amount;
        }

        public void RecordFocusSpent(float amount)
        {
            totalFocusSpent += amount;
        }

        public void RecordFocusLevel(float currentFocus, float maxFocus)
        {
            if (currentFocus > maxFocusReached)
            {
                maxFocusReached = currentFocus;
            }

            if (currentFocus >= maxFocus)
            {
                timesMaxFocus++;
            }
        }
        #endregion

        #region Score Calculation
        private void CalculateFinalScore()
        {
            // Base score: Mission completion
            baseScore = wasSuccessful ? 200 : 0;

            // Rhythm bonus: Accuracy and combo
            float accuracy = GetAccuracy();
            rhythmBonus = Mathf.RoundToInt(accuracy * 2f); // Max 200
            rhythmBonus += longestCombo * 10; // Combo multiplier

            // Stealth bonus: Avoid detection
            stealthBonus = 0;
            if (timesDetected == 0)
            {
                stealthBonus = 300; // Ghost run
            }
            else if (timesDetected <= 2)
            {
                stealthBonus = 100; // Low profile
            }

            // Speed bonus: Fast completion
            float completionTime = GetCompletionTime();
            if (completionTime > 0)
            {
                // Bonus for completing under 3 minutes (180s)
                if (completionTime <= 180f)
                {
                    speedBonus = 200;
                }
                else if (completionTime <= 300f)
                {
                    speedBonus = 100;
                }
            }

            // Optional objectives bonus
            optionalBonus = optionalObjectivesCompleted * 50;

            // Calculate final score
            finalScore = baseScore + rhythmBonus + stealthBonus + speedBonus + optionalBonus;

            Debug.Log($"Score Breakdown: Base={baseScore}, Rhythm={rhythmBonus}, Stealth={stealthBonus}, Speed={speedBonus}, Optional={optionalBonus}, Total={finalScore}");
        }
        #endregion

        #region Getters
        public float GetCompletionTime()
        {
            return endTime - startTime;
        }

        public float GetAccuracy()
        {
            if (totalInputs == 0) return 0f;
            return (float)perfectInputs / totalInputs * 100f;
        }

        public float GetPerfectRate()
        {
            if (totalInputs == 0) return 0f;
            return (float)perfectInputs / totalInputs;
        }

        public string GetRank()
        {
            if (finalScore >= 1000) return "S";
            if (finalScore >= 800) return "A";
            if (finalScore >= 600) return "B";
            if (finalScore >= 400) return "C";
            return "D";
        }

        public bool IsGhostRun()
        {
            return timesDetected == 0;
        }

        public bool IsSpeedRun()
        {
            return GetCompletionTime() <= 180f;
        }

        public bool IsPerfectRun()
        {
            return missInputs == 0 && timesDetected == 0;
        }
        #endregion

        #region Debug Print
        public void PrintSummary()
        {
            Debug.Log($"=== Mission Statistics: {missionName} ===");
            Debug.Log($"Result: {(wasSuccessful ? "SUCCESS" : "FAILED")}");
            Debug.Log($"Time: {GetCompletionTime():F1}s");
            Debug.Log($"Rank: {GetRank()} (Score: {finalScore})");
            Debug.Log($"");
            Debug.Log($"Rhythm: {perfectInputs}P / {greatInputs}G / {missInputs}M ({GetAccuracy():F1}% accuracy)");
            Debug.Log($"Longest Combo: {longestCombo}");
            Debug.Log($"Combat: {guardsEliminated} eliminated ({perfectKills} perfect)");
            Debug.Log($"Stealth: {timesDetected} detections, {timesSpotted} spotted");
            Debug.Log($"Objectives: {primaryObjectivesCompleted}/{primaryObjectivesTotal} primary, {optionalObjectivesCompleted}/{optionalObjectivesTotal} optional");
            Debug.Log($"Focus: {totalFocusGenerated:F0} generated, {totalFocusSpent:F0} spent");
            Debug.Log($"Distance: {distanceTraveled:F1}m");
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Manager for tracking mission statistics
    /// </summary>
    public class StatisticsManager : MonoBehaviour
    {
        public static StatisticsManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        [Header("▶ Current Mission")]
        public MissionStatistics currentStats = new MissionStatistics();

        private bool _isTracking = false;
        private Vector3 _lastPlayerPosition;
        private float _lastPositionUpdateTime;

        #region Mission Lifecycle
        public void StartTracking(string missionID, string missionName)
        {
            currentStats.StartMission(missionID, missionName);
            _isTracking = true;
            _lastPositionUpdateTime = Time.time;

            Debug.Log($"Statistics tracking started for: {missionName}");
        }

        public void StopTracking(bool success, int primaryTotal, int optionalTotal)
        {
            if (!_isTracking) return;

            currentStats.EndMission(success, primaryTotal, optionalTotal);
            _isTracking = false;

            // Print summary
            currentStats.PrintSummary();

            // Report to progression manager
            ReportToProgression();
        }

        private void ReportToProgression()
        {
            var progressionManager = Core.ProgressionManager.Instance;
            if (progressionManager != null)
            {
                // Mission completion is already handled by EnhancedMissionManager
                // Here we just report statistics
                Debug.Log($"Mission stats reported to progression system");
            }
        }
        #endregion

        #region Update
        void Update()
        {
            if (!_isTracking) return;

            // Track player movement every 0.5s
            if (Time.time - _lastPositionUpdateTime >= 0.5f)
            {
                var player = GameServices.Player;
                if (player != null)
                {
                    currentStats.RecordMovement(player.transform.position);
                    _lastPositionUpdateTime = Time.time;
                }
            }
        }
        #endregion

        #region Public Recording Methods
        // These are called by other systems during gameplay

        public void OnRhythmInput(RhythmJudgment judgment, float timingOffset)
        {
            if (!_isTracking) return;
            currentStats.RecordInput(judgment, timingOffset);
        }

        public void OnGuardEliminated(bool wasPerfectTiming)
        {
            if (!_isTracking) return;
            currentStats.RecordGuardEliminated(wasPerfectTiming);
        }

        public void OnSkillUsed(string skillName)
        {
            if (!_isTracking) return;
            currentStats.RecordSkillUsed(skillName);
        }

        public void OnDetection()
        {
            if (!_isTracking) return;
            currentStats.RecordDetection();
        }

        public void OnObjectiveComplete(bool isPrimary)
        {
            if (!_isTracking) return;
            currentStats.RecordObjectiveComplete(isPrimary);
        }

        public void OnFocusGenerated(float amount)
        {
            if (!_isTracking) return;
            currentStats.RecordFocusGenerated(amount);
        }

        public void OnFocusSpent(float amount)
        {
            if (!_isTracking) return;
            currentStats.RecordFocusSpent(amount);
        }
        #endregion

        #region Getters
        public MissionStatistics GetCurrentStats() => currentStats;
        public bool IsTracking() => _isTracking;
        #endregion
    }
}
