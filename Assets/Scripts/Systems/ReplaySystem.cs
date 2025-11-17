using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SongOfTheStars.Systems
{
    /// <summary>
    /// Records and replays player gameplay for sharing and analysis
    /// 플레이어 게임플레이를 기록하고 재생하여 공유 및 분석
    ///
    /// Records: Inputs, positions, skill usage, timing accuracy
    /// </summary>
    public class ReplaySystem : MonoBehaviour
    {
        public static ReplaySystem Instance { get; private set; }

        [Header("▶ Recording Settings")]
        public bool recordingEnabled = true;
        public bool autoRecordMissions = true;
        [Range(10, 120)]
        public int recordingFPS = 30; // Samples per second

        [Header("▶ Playback Settings")]
        public bool showGhostPlayer = true;
        public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);
        public GameObject ghostPlayerPrefab;

        [Header("▶ Storage")]
        public int maxReplaysStored = 50;
        public bool autoSaveBestRuns = true;

        [Header("▶ Debug")]
        public bool showDebugInfo = false;

        private bool _isRecording = false;
        private bool _isPlaying = false;
        private ReplayData _currentRecording;
        private ReplayData _currentPlayback;
        private float _recordTimer = 0f;
        private float _playbackTimer = 0f;
        private int _playbackFrameIndex = 0;

        private List<ReplayData> _savedReplays = new List<ReplayData>();
        private GameObject _ghostPlayerInstance;

        private const string REPLAY_SAVE_PATH = "Replays/";
        private const string REPLAY_FILE_EXTENSION = ".replay";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedReplays();
        }

        void Update()
        {
            if (_isRecording)
            {
                UpdateRecording();
            }

            if (_isPlaying)
            {
                UpdatePlayback();
            }
        }

        #region Recording

        public void StartRecording(string missionID, string missionName)
        {
            if (_isRecording)
            {
                Debug.LogWarning("Already recording!");
                return;
            }

            _currentRecording = new ReplayData
            {
                replayID = System.Guid.NewGuid().ToString(),
                missionID = missionID,
                missionName = missionName,
                recordedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                recordingFPS = recordingFPS,
                frames = new List<ReplayFrame>()
            };

            _isRecording = true;
            _recordTimer = 0f;

            Debug.Log($"▶ Recording started: {missionName}");
        }

        public void StopRecording(bool wasMissionSuccessful, int finalScore, float completionTime)
        {
            if (!_isRecording)
            {
                Debug.LogWarning("Not recording!");
                return;
            }

            _isRecording = false;

            // Fill metadata
            _currentRecording.wasSuccessful = wasMissionSuccessful;
            _currentRecording.finalScore = finalScore;
            _currentRecording.completionTime = completionTime;
            _currentRecording.totalFrames = _currentRecording.frames.Count;
            _currentRecording.duration = _recordTimer;

            // Calculate stats
            CalculateReplayStats(_currentRecording);

            // Save replay
            SaveReplay(_currentRecording);

            Debug.Log($"■ Recording stopped. Frames: {_currentRecording.totalFrames}, " +
                      $"Duration: {_currentRecording.duration:F2}s");

            _currentRecording = null;
        }

        private void UpdateRecording()
        {
            _recordTimer += Time.deltaTime;

            // Record at fixed intervals
            float frameInterval = 1f / recordingFPS;
            if (_recordTimer >= frameInterval * _currentRecording.frames.Count)
            {
                RecordFrame();
            }
        }

        private void RecordFrame()
        {
            // Get player data (you'll need to reference your player controller)
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;

            ReplayFrame frame = new ReplayFrame
            {
                timestamp = _recordTimer,
                position = playerTransform.position,
                rotation = playerTransform.rotation.eulerAngles.z,
                // Add more data as needed
                velocityX = 0f, // Get from player controller
                velocityY = 0f,
                currentState = 0, // Player state enum
                facingDirection = 0 // Direction enum
            };

            _currentRecording.frames.Add(frame);
        }

        public void RecordInput(string inputType, Vector2 inputDirection, float timing)
        {
            if (!_isRecording) return;

            ReplayInput input = new ReplayInput
            {
                timestamp = _recordTimer,
                inputType = inputType,
                direction = inputDirection,
                timingOffset = timing
            };

            _currentRecording.inputs.Add(input);
        }

        public void RecordSkillUsage(string skillName, Vector2 position, float focusCost)
        {
            if (!_isRecording) return;

            ReplaySkillUsage skill = new ReplaySkillUsage
            {
                timestamp = _recordTimer,
                skillName = skillName,
                position = position,
                focusCost = focusCost
            };

            _currentRecording.skillUsages.Add(skill);
        }

        public void RecordEvent(string eventType, string eventData)
        {
            if (!_isRecording) return;

            ReplayEvent evt = new ReplayEvent
            {
                timestamp = _recordTimer,
                eventType = eventType,
                eventData = eventData
            };

            _currentRecording.events.Add(evt);
        }

        #endregion

        #region Playback

        public void StartPlayback(ReplayData replay)
        {
            if (_isPlaying)
            {
                StopPlayback();
            }

            _currentPlayback = replay;
            _isPlaying = true;
            _playbackTimer = 0f;
            _playbackFrameIndex = 0;

            // Spawn ghost player
            if (showGhostPlayer && ghostPlayerPrefab != null)
            {
                _ghostPlayerInstance = Instantiate(ghostPlayerPrefab);
                _ghostPlayerInstance.name = "GhostPlayer_Replay";

                // Set ghost appearance
                SpriteRenderer renderer = _ghostPlayerInstance.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = ghostColor;
                }

                // Disable ghost physics/collisions
                Collider2D collider = _ghostPlayerInstance.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }

            Debug.Log($"▶ Playback started: {replay.missionName} " +
                      $"({replay.totalFrames} frames, {replay.duration:F2}s)");
        }

        public void StopPlayback()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            _currentPlayback = null;

            if (_ghostPlayerInstance != null)
            {
                Destroy(_ghostPlayerInstance);
                _ghostPlayerInstance = null;
            }

            Debug.Log("■ Playback stopped");
        }

        private void UpdatePlayback()
        {
            _playbackTimer += Time.deltaTime;

            // Check if playback finished
            if (_playbackTimer >= _currentPlayback.duration)
            {
                StopPlayback();
                return;
            }

            // Update ghost player position
            if (_ghostPlayerInstance != null && _currentPlayback.frames.Count > 0)
            {
                // Find frame to display
                while (_playbackFrameIndex < _currentPlayback.frames.Count - 1)
                {
                    ReplayFrame nextFrame = _currentPlayback.frames[_playbackFrameIndex + 1];
                    if (nextFrame.timestamp > _playbackTimer)
                        break;
                    _playbackFrameIndex++;
                }

                if (_playbackFrameIndex < _currentPlayback.frames.Count)
                {
                    ReplayFrame currentFrame = _currentPlayback.frames[_playbackFrameIndex];

                    // Interpolate to next frame if available
                    if (_playbackFrameIndex < _currentPlayback.frames.Count - 1)
                    {
                        ReplayFrame nextFrame = _currentPlayback.frames[_playbackFrameIndex + 1];
                        float t = (_playbackTimer - currentFrame.timestamp) /
                                  (nextFrame.timestamp - currentFrame.timestamp);
                        t = Mathf.Clamp01(t);

                        Vector3 interpolatedPos = Vector3.Lerp(currentFrame.position, nextFrame.position, t);
                        float interpolatedRot = Mathf.LerpAngle(currentFrame.rotation, nextFrame.rotation, t);

                        _ghostPlayerInstance.transform.position = interpolatedPos;
                        _ghostPlayerInstance.transform.rotation = Quaternion.Euler(0f, 0f, interpolatedRot);
                    }
                    else
                    {
                        _ghostPlayerInstance.transform.position = currentFrame.position;
                        _ghostPlayerInstance.transform.rotation = Quaternion.Euler(0f, 0f, currentFrame.rotation);
                    }
                }
            }
        }

        #endregion

        #region Stats Calculation

        private void CalculateReplayStats(ReplayData replay)
        {
            if (replay.inputs.Count == 0) return;

            // Calculate accuracy
            int perfectCount = replay.inputs.Count(i => Mathf.Abs(i.timingOffset) <= 0.04f);
            int greatCount = replay.inputs.Count(i => Mathf.Abs(i.timingOffset) > 0.04f && Mathf.Abs(i.timingOffset) <= 0.08f);
            int missCount = replay.inputs.Count - perfectCount - greatCount;

            replay.perfectInputs = perfectCount;
            replay.greatInputs = greatCount;
            replay.missedInputs = missCount;

            replay.accuracy = replay.inputs.Count > 0
                ? (float)perfectCount / replay.inputs.Count * 100f
                : 0f;

            // Calculate average timing offset
            float totalOffset = replay.inputs.Sum(i => Mathf.Abs(i.timingOffset));
            replay.averageTimingOffset = replay.inputs.Count > 0
                ? totalOffset / replay.inputs.Count * 1000f // Convert to ms
                : 0f;

            // Skills used
            replay.skillsUsed = replay.skillUsages.Count;

            // Total focus spent
            replay.totalFocusSpent = replay.skillUsages.Sum(s => s.focusCost);
        }

        #endregion

        #region Save/Load

        private void SaveReplay(ReplayData replay)
        {
            _savedReplays.Add(replay);

            // Limit stored replays
            if (_savedReplays.Count > maxReplaysStored)
            {
                // Remove oldest non-best replay
                ReplayData toRemove = _savedReplays
                    .Where(r => !r.isBestRun)
                    .OrderBy(r => r.recordedDate)
                    .FirstOrDefault();

                if (toRemove != null)
                {
                    _savedReplays.Remove(toRemove);
                    DeleteReplayFile(toRemove.replayID);
                }
            }

            // Mark as best run if applicable
            if (autoSaveBestRuns && replay.wasSuccessful)
            {
                var missionReplays = _savedReplays.Where(r => r.missionID == replay.missionID).ToList();
                var bestReplay = missionReplays.OrderByDescending(r => r.finalScore).FirstOrDefault();

                if (bestReplay == replay)
                {
                    replay.isBestRun = true;
                }
            }

            // Save to file
            SaveReplayToFile(replay);
        }

        private void SaveReplayToFile(ReplayData replay)
        {
            string directory = Path.Combine(Application.persistentDataPath, REPLAY_SAVE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filePath = Path.Combine(directory, replay.replayID + REPLAY_FILE_EXTENSION);
            string json = JsonUtility.ToJson(replay, true);
            File.WriteAllText(filePath, json);

            Debug.Log($"💾 Replay saved: {filePath}");
        }

        private void LoadSavedReplays()
        {
            string directory = Path.Combine(Application.persistentDataPath, REPLAY_SAVE_PATH);
            if (!Directory.Exists(directory))
                return;

            string[] replayFiles = Directory.GetFiles(directory, "*" + REPLAY_FILE_EXTENSION);

            foreach (string filePath in replayFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    ReplayData replay = JsonUtility.FromJson<ReplayData>(json);
                    _savedReplays.Add(replay);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load replay: {filePath}\n{e.Message}");
                }
            }

            Debug.Log($"📁 Loaded {_savedReplays.Count} replays");
        }

        private void DeleteReplayFile(string replayID)
        {
            string directory = Path.Combine(Application.persistentDataPath, REPLAY_SAVE_PATH);
            string filePath = Path.Combine(directory, replayID + REPLAY_FILE_EXTENSION);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region Public API

        public List<ReplayData> GetAllReplays()
        {
            return new List<ReplayData>(_savedReplays);
        }

        public List<ReplayData> GetReplaysForMission(string missionID)
        {
            return _savedReplays.Where(r => r.missionID == missionID).ToList();
        }

        public ReplayData GetBestReplayForMission(string missionID)
        {
            return _savedReplays
                .Where(r => r.missionID == missionID && r.wasSuccessful)
                .OrderByDescending(r => r.finalScore)
                .FirstOrDefault();
        }

        public void DeleteReplay(string replayID)
        {
            ReplayData replay = _savedReplays.FirstOrDefault(r => r.replayID == replayID);
            if (replay != null)
            {
                _savedReplays.Remove(replay);
                DeleteReplayFile(replayID);
            }
        }

        public bool IsRecording() => _isRecording;
        public bool IsPlaying() => _isPlaying;

        #endregion

        void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Box("Replay System");

            if (_isRecording)
            {
                GUILayout.Label($"⏺ Recording: {_recordTimer:F2}s");
                GUILayout.Label($"Frames: {_currentRecording.frames.Count}");
                GUILayout.Label($"Inputs: {_currentRecording.inputs.Count}");
            }

            if (_isPlaying)
            {
                GUILayout.Label($"▶ Playing: {_playbackTimer:F2}s / {_currentPlayback.duration:F2}s");
                GUILayout.Label($"Frame: {_playbackFrameIndex + 1} / {_currentPlayback.totalFrames}");
            }

            if (!_isRecording && !_isPlaying)
            {
                GUILayout.Label($"Saved Replays: {_savedReplays.Count}");
            }

            GUILayout.EndArea();
        }
    }

    #region Data Structures

    [System.Serializable]
    public class ReplayData
    {
        public string replayID;
        public string missionID;
        public string missionName;
        public string recordedDate;

        public bool wasSuccessful;
        public int finalScore;
        public float completionTime;
        public bool isBestRun;

        public int recordingFPS;
        public int totalFrames;
        public float duration;

        public List<ReplayFrame> frames = new List<ReplayFrame>();
        public List<ReplayInput> inputs = new List<ReplayInput>();
        public List<ReplaySkillUsage> skillUsages = new List<ReplaySkillUsage>();
        public List<ReplayEvent> events = new List<ReplayEvent>();

        // Stats
        public int perfectInputs;
        public int greatInputs;
        public int missedInputs;
        public float accuracy;
        public float averageTimingOffset; // in ms
        public int skillsUsed;
        public float totalFocusSpent;
    }

    [System.Serializable]
    public class ReplayFrame
    {
        public float timestamp;
        public Vector3 position;
        public float rotation;
        public float velocityX;
        public float velocityY;
        public int currentState;
        public int facingDirection;
    }

    [System.Serializable]
    public class ReplayInput
    {
        public float timestamp;
        public string inputType; // "Move", "Skill", "Assassinate"
        public Vector2 direction;
        public float timingOffset; // How far from perfect beat
    }

    [System.Serializable]
    public class ReplaySkillUsage
    {
        public float timestamp;
        public string skillName;
        public Vector2 position;
        public float focusCost;
    }

    [System.Serializable]
    public class ReplayEvent
    {
        public float timestamp;
        public string eventType; // "Detected", "Eliminated", "ObjectiveComplete"
        public string eventData;
    }

    #endregion
}
