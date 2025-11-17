using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using SongOfTheStars.Missions;
using SongOfTheStars.UI;

namespace SongOfTheStars.Core
{
    /// <summary>
    /// Enhanced mission manager with objective tracking and scripted events
    /// 목표 추적 및 스크립트 이벤트가 있는 향상된 미션 관리자
    ///
    /// Replaces/enhances basic MissionManager with full mission system
    /// </summary>
    public class EnhancedMissionManager : MonoBehaviour
    {
        #region Singleton
        public static EnhancedMissionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        [Header("▶ Current Mission")]
        [Tooltip("Mission data for current mission")]
        public MissionData currentMission;

        [Header("▶ UI References")]
        [Tooltip("Mission objectives UI component")]
        public MissionObjectivesUI objectivesUI;

        [Header("▶ Events")]
        public UnityEvent<int> OnAlertLevelChanged;
        public UnityEvent<int> OnObjectiveCompleted;
        public UnityEvent<bool> OnMissionCompleted;
        public UnityEvent<string> OnScriptedEvent;

        #region Private State
        private bool _isMissionActive = false;
        private int _currentAlertLevel = 0;
        private float _missionStartTime;
        private float _missionElapsedTime;

        // Objective tracking
        private List<ObjectiveState> _objectiveStates = new List<ObjectiveState>();
        private int _completedPrimaryObjectives = 0;
        private int _completedOptionalObjectives = 0;

        // Scripted event tracking
        private HashSet<string> _triggeredEvents = new HashSet<string>();
        private List<ScriptedEventDefinition> _pendingTimerEvents = new List<ScriptedEventDefinition>();

        private class ObjectiveState
        {
            public ObjectiveDefinition definition;
            public int currentProgress;
            public bool isCompleted;
            public bool isFailed;
            public bool isPrimary;
            public int uiIndex; // Index in MissionObjectivesUI
        }
        #endregion

        #region Initialization
        void Start()
        {
            if (currentMission != null)
            {
                StartMission(currentMission);
            }
            else
            {
                Debug.LogWarning("EnhancedMissionManager: No mission data assigned!");
            }
        }

        /// <summary>
        /// Starts a mission with given data
        /// 주어진 데이터로 미션 시작
        /// </summary>
        public void StartMission(MissionData missionData)
        {
            if (_isMissionActive)
            {
                Debug.LogWarning("Mission already active!");
                return;
            }

            currentMission = missionData;
            _isMissionActive = true;
            _missionStartTime = Time.time;
            _missionElapsedTime = 0f;
            _currentAlertLevel = 0;
            _completedPrimaryObjectives = 0;
            _completedOptionalObjectives = 0;

            // Initialize objectives
            InitializeObjectives();

            // Apply difficulty settings
            ApplyDifficulty();

            // Setup skill loadout
            SetupSkillLoadout();

            // Start background music
            StartMissionMusic();

            // Process OnMissionStart events
            ProcessScriptedEvents(EventTrigger.OnMissionStart, 0);

            // Setup timer events
            SetupTimerEvents();

            Debug.Log($"Mission Started: {missionData.missionName}");
        }

        private void InitializeObjectives()
        {
            _objectiveStates.Clear();

            // Add primary objectives
            for (int i = 0; i < currentMission.primaryObjectives.Count; i++)
            {
                ObjectiveState state = new ObjectiveState
                {
                    definition = currentMission.primaryObjectives[i],
                    currentProgress = 0,
                    isCompleted = false,
                    isFailed = false,
                    isPrimary = true,
                    uiIndex = -1
                };
                _objectiveStates.Add(state);

                // Add to UI (if not secret)
                if (!state.definition.isSecret && objectivesUI != null)
                {
                    objectivesUI.AddPrimaryObjective(
                        state.definition.description,
                        state.definition.targetCount
                    );
                    state.uiIndex = objectivesUI.GetTotalCount() - 1;
                }
            }

            // Add optional objectives
            for (int i = 0; i < currentMission.optionalObjectives.Count; i++)
            {
                ObjectiveState state = new ObjectiveState
                {
                    definition = currentMission.optionalObjectives[i],
                    currentProgress = 0,
                    isCompleted = false,
                    isFailed = false,
                    isPrimary = false,
                    uiIndex = -1
                };
                _objectiveStates.Add(state);

                // Add to UI (if not secret)
                if (!state.definition.isSecret && objectivesUI != null)
                {
                    objectivesUI.AddOptionalObjective(
                        state.definition.description,
                        state.definition.targetCount
                    );
                    state.uiIndex = objectivesUI.GetTotalCount() - 1;
                }
            }
        }

        private void ApplyDifficulty()
        {
            if (currentMission == null) return;

            DifficultyManager difficultyManager = DifficultyManager.Instance;
            if (difficultyManager != null)
            {
                // Map mission difficulty to game difficulty
                switch (currentMission.difficulty)
                {
                    case DifficultyLevel.Tutorial:
                        difficultyManager.SetDifficulty(0); // Very Easy
                        break;
                    case DifficultyLevel.Easy:
                        difficultyManager.SetDifficulty(1);
                        break;
                    case DifficultyLevel.Normal:
                        difficultyManager.SetDifficulty(2);
                        break;
                    case DifficultyLevel.Hard:
                        difficultyManager.SetDifficulty(3);
                        break;
                }
            }
        }

        private void SetupSkillLoadout()
        {
            if (currentMission == null) return;

            SkillLoadoutManager loadoutManager = GameServices.SkillLoadout;
            if (loadoutManager != null && currentMission.defaultLoadout.Count > 0)
            {
                // Apply default loadout (up to 4 skills)
                loadoutManager.activeSkills.Clear();

                KeyCode[] keys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
                for (int i = 0; i < Mathf.Min(4, currentMission.defaultLoadout.Count); i++)
                {
                    if (currentMission.defaultLoadout[i] != null)
                    {
                        loadoutManager.activeSkills[keys[i]] = currentMission.defaultLoadout[i];
                    }
                }
            }
        }

        private void StartMissionMusic()
        {
            if (currentMission == null || currentMission.backgroundMusic == null) return;

            // Set music BPM for rhythm sync
            RhythmSyncManager rhythmManager = GameServices.RhythmManager;
            if (rhythmManager != null)
            {
                rhythmManager.musicBPM = currentMission.musicBPM;
            }

            // Play music
            MusicManager musicManager = FindObjectOfType<MusicManager>();
            if (musicManager != null)
            {
                musicManager.PlayMusic(currentMission.backgroundMusic);
            }
        }

        private void SetupTimerEvents()
        {
            if (currentMission == null) return;

            _pendingTimerEvents.Clear();
            foreach (var evt in currentMission.scriptedEvents)
            {
                if (evt.trigger == EventTrigger.OnTimer)
                {
                    _pendingTimerEvents.Add(evt);
                }
            }
        }
        #endregion

        #region Update
        void Update()
        {
            if (!_isMissionActive) return;

            // Update mission time
            _missionElapsedTime = Time.time - _missionStartTime;

            // Check time limit
            if (currentMission.timeLimit > 0 && _missionElapsedTime >= currentMission.timeLimit)
            {
                CompleteMission(false, "Time limit exceeded");
                return;
            }

            // Process timer events
            ProcessTimerEvents();

            // Update survival objectives
            UpdateSurvivalObjectives();
        }

        private void ProcessTimerEvents()
        {
            for (int i = _pendingTimerEvents.Count - 1; i >= 0; i--)
            {
                var evt = _pendingTimerEvents[i];
                if (_missionElapsedTime >= evt.triggerDelay)
                {
                    TriggerScriptedEvent(evt);
                    _pendingTimerEvents.RemoveAt(i);
                }
            }
        }

        private void UpdateSurvivalObjectives()
        {
            foreach (var state in _objectiveStates)
            {
                if (state.definition.type == ObjectiveType.Survive && !state.isCompleted)
                {
                    // Update progress based on time
                    int timeProgress = Mathf.FloorToInt(_missionElapsedTime);
                    if (timeProgress != state.currentProgress)
                    {
                        UpdateObjectiveProgress(state, timeProgress);
                    }
                }
            }
        }
        #endregion

        #region Objective Management
        /// <summary>
        /// Updates progress for an objective type
        /// </summary>
        public void NotifyObjectiveProgress(ObjectiveType type, string targetTag = "", int amount = 1)
        {
            foreach (var state in _objectiveStates)
            {
                if (state.isCompleted || state.isFailed) continue;
                if (state.definition.type != type) continue;

                // Check tag match (if applicable)
                if (!string.IsNullOrEmpty(targetTag) && state.definition.targetTag != targetTag)
                    continue;

                UpdateObjectiveProgress(state, state.currentProgress + amount);
            }
        }

        private void UpdateObjectiveProgress(ObjectiveState state, int newProgress)
        {
            state.currentProgress = newProgress;

            // Update UI
            if (state.uiIndex >= 0 && objectivesUI != null)
            {
                objectivesUI.UpdateObjectiveProgress(state.uiIndex, state.currentProgress);
            }

            // Check completion
            if (state.currentProgress >= state.definition.targetCount)
            {
                CompleteObjective(state);
            }
        }

        private void CompleteObjective(ObjectiveState state)
        {
            if (state.isCompleted) return;

            state.isCompleted = true;

            // Update UI
            if (state.uiIndex >= 0 && objectivesUI != null)
            {
                objectivesUI.CompleteObjective(state.uiIndex);
            }

            // Track completion
            if (state.isPrimary)
            {
                _completedPrimaryObjectives++;
            }
            else
            {
                _completedOptionalObjectives++;
            }

            // Fire event
            OnObjectiveCompleted?.Invoke(state.uiIndex);

            // Process scripted events
            int objectiveIndex = _objectiveStates.IndexOf(state);
            ProcessScriptedEvents(EventTrigger.OnObjectiveComplete, objectiveIndex);

            Debug.Log($"Objective Completed: {state.definition.description}");

            // Check mission completion
            if (_completedPrimaryObjectives >= currentMission.primaryObjectives.Count)
            {
                CompleteMission(true, "All objectives complete");
            }
        }

        /// <summary>
        /// Fails an objective (for timed/stealth objectives)
        /// </summary>
        public void FailObjective(int objectiveIndex)
        {
            if (objectiveIndex < 0 || objectiveIndex >= _objectiveStates.Count) return;

            ObjectiveState state = _objectiveStates[objectiveIndex];
            state.isFailed = true;

            // Update UI
            if (state.uiIndex >= 0 && objectivesUI != null)
            {
                objectivesUI.FailObjective(state.uiIndex);
            }

            // If primary objective failed, mission fails
            if (state.isPrimary)
            {
                CompleteMission(false, "Primary objective failed");
            }
        }
        #endregion

        #region Alert System
        public void IncreaseAlertLevel(int amount)
        {
            if (!_isMissionActive) return;

            int previousLevel = _currentAlertLevel;
            _currentAlertLevel = Mathf.Min(currentMission.maxAlertLevel, _currentAlertLevel + amount);

            if (_currentAlertLevel != previousLevel)
            {
                OnAlertLevelChanged?.Invoke(_currentAlertLevel);

                // Process alert level events
                ProcessScriptedEvents(EventTrigger.OnAlertLevel, _currentAlertLevel);
            }

            // Check stealth objectives
            CheckStealthObjectives();

            // Check mission failure
            if (_currentAlertLevel >= currentMission.maxAlertLevel)
            {
                CompleteMission(false, "Maximum alert level reached");
            }
        }

        public void DecreaseAlertLevel(int amount)
        {
            if (!_isMissionActive) return;

            int previousLevel = _currentAlertLevel;
            _currentAlertLevel = Mathf.Max(0, _currentAlertLevel - amount);

            if (_currentAlertLevel != previousLevel)
            {
                OnAlertLevelChanged?.Invoke(_currentAlertLevel);
            }
        }

        private void CheckStealthObjectives()
        {
            foreach (var state in _objectiveStates)
            {
                if (state.definition.type == ObjectiveType.Stealth && !state.isCompleted && !state.isFailed)
                {
                    // Fail if detected more than allowed
                    if (_currentAlertLevel > state.definition.maxDetections)
                    {
                        FailObjective(_objectiveStates.IndexOf(state));
                    }
                }
            }
        }
        #endregion

        #region Scripted Events
        private void ProcessScriptedEvents(EventTrigger trigger, int triggerValue)
        {
            if (currentMission == null) return;

            foreach (var evt in currentMission.scriptedEvents)
            {
                if (evt.trigger != trigger) continue;

                // Check trigger-specific conditions
                bool shouldTrigger = false;
                switch (trigger)
                {
                    case EventTrigger.OnMissionStart:
                        shouldTrigger = true;
                        break;
                    case EventTrigger.OnObjectiveComplete:
                        shouldTrigger = (evt.triggerObjectiveIndex == triggerValue);
                        break;
                    case EventTrigger.OnAlertLevel:
                        shouldTrigger = (triggerValue >= evt.triggerObjectiveIndex);
                        break;
                }

                if (shouldTrigger && !_triggeredEvents.Contains(evt.eventID))
                {
                    TriggerScriptedEvent(evt);
                }
            }
        }

        private void TriggerScriptedEvent(ScriptedEventDefinition evt)
        {
            _triggeredEvents.Add(evt.eventID);
            OnScriptedEvent?.Invoke(evt.eventID);

            switch (evt.eventType)
            {
                case EventType.ShowDialog:
                    ShowDialog(evt.dialogText);
                    break;

                case EventType.SpawnGuards:
                    SpawnGuards(evt.spawnPointTag, evt.spawnCount);
                    break;

                // Other event types can be implemented as needed
                default:
                    Debug.Log($"Scripted Event: {evt.eventID} ({evt.eventType})");
                    break;
            }
        }

        private void ShowDialog(string text)
        {
            // TODO: Integrate with dialog system
            Debug.Log($"[Dialog] {text}");
        }

        private void SpawnGuards(string spawnPointTag, int count)
        {
            // Find spawn points
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(spawnPointTag);
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning($"No spawn points found with tag: {spawnPointTag}");
                return;
            }

            // Spawn guards at random points
            for (int i = 0; i < count; i++)
            {
                GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                // TODO: Instantiate guard prefab at spawn point
                Debug.Log($"Spawning guard at: {spawnPoint.name}");
            }
        }
        #endregion

        #region Mission Completion
        private void CompleteMission(bool success, string reason)
        {
            if (!_isMissionActive) return;

            _isMissionActive = false;

            // Calculate final score
            int experience = success ? currentMission.experienceReward : 0;
            experience += _completedOptionalObjectives * currentMission.bonusExperiencePerOptional;

            // Fire event
            OnMissionCompleted?.Invoke(success);

            if (success)
            {
                Debug.Log($"⭐ Mission Success! XP: {experience}, Reason: {reason}");
                // Unlock new skills
                UnlockSkills();
            }
            else
            {
                Debug.Log($"💀 Mission Failed! Reason: {reason}");
            }

            // Return to menu after delay
            if (!currentMission.allowContinueAfterComplete)
            {
                Invoke(nameof(ReturnToMainMenu), 5f);
            }
        }

        private void UnlockSkills()
        {
            if (currentMission == null || currentMission.unlockedSkills.Count == 0) return;

            foreach (var skill in currentMission.unlockedSkills)
            {
                if (skill != null)
                {
                    Debug.Log($"Skill Unlocked: {skill.skillName}");
                    // TODO: Save unlocked skill to player progression
                }
            }
        }

        private void ReturnToMainMenu()
        {
            Debug.Log("Returning to main menu...");
            SceneManager.LoadScene("MainMenu");
        }
        #endregion

        #region Public API
        public bool IsMissionActive() => _isMissionActive;
        public int GetCurrentAlertLevel() => _currentAlertLevel;
        public float GetElapsedTime() => _missionElapsedTime;
        public int GetCompletedObjectivesCount() => _completedPrimaryObjectives + _completedOptionalObjectives;
        public int GetTotalObjectivesCount() => currentMission != null ? currentMission.GetTotalObjectiveCount() : 0;
        #endregion
    }
}
