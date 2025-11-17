using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SongOfTheStars.UI
{
    /// <summary>
    /// Visual metronome/beat timeline for rhythm feedback
    /// 리듬 피드백을 위한 비트 타임라인 UI
    ///
    /// Features:
    /// - Scrolling beat indicators showing upcoming beats
    /// - Color-coded timing zones (Perfect/Great)
    /// - Current beat highlight
    /// - Minimal latency (<5ms)
    /// </summary>
    public class BeatTimelineUI : MonoBehaviour
    {
        #region Inspector References
        [Header("▶ Timeline Container")]
        [Tooltip("The parent RectTransform containing all beat markers")]
        public RectTransform timelineContainer;

        [Tooltip("The indicator line showing current beat position")]
        public RectTransform currentBeatIndicator;

        [Header("▶ Beat Marker Prefabs")]
        [Tooltip("Prefab for beat markers")]
        public GameObject beatMarkerPrefab;

        [Header("▶ Timing Zone Visuals")]
        [Tooltip("Background image for Perfect timing zone")]
        public Image perfectZoneImage;

        [Tooltip("Background image for Great timing zone")]
        public Image greatZoneImage;

        [Header("▶ Visual Settings")]
        [Tooltip("Number of beats to show ahead")]
        [Range(2, 8)]
        public int beatsAhead = 4;

        [Tooltip("Number of beats to show behind")]
        [Range(0, 4)]
        public int beatsBehind = 1;

        [Tooltip("Distance between beat markers in pixels")]
        [Range(50f, 200f)]
        public float beatSpacing = 100f;

        [Header("▶ Colors")]
        public Color perfectZoneColor = new Color(0f, 1f, 0f, 0.2f); // Green transparent
        public Color greatZoneColor = new Color(1f, 1f, 0f, 0.15f);  // Yellow transparent
        public Color currentBeatColor = Color.white;
        public Color upcomingBeatColor = new Color(1f, 1f, 1f, 0.6f);
        public Color passedBeatColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Header("▶ Animation")]
        [Tooltip("Enable beat marker pulse animation")]
        public bool enableBeatPulse = true;

        [Tooltip("Scale multiplier for beat pulse")]
        [Range(1f, 2f)]
        public float beatPulseScale = 1.3f;

        [Tooltip("Pulse animation duration")]
        [Range(0.05f, 0.3f)]
        public float pulseDuration = 0.1f;
        #endregion

        #region Private State
        private RhythmSyncManager _rhythmManager;
        private Queue<BeatMarker> _activeBeatMarkers = new Queue<BeatMarker>();
        private Stack<BeatMarker> _inactiveBeatMarkers = new Stack<BeatMarker>();

        private int _lastBeatCount = -1;
        private float _timelineWidth;
        private Vector2 _perfectZoneSize;
        private Vector2 _greatZoneSize;

        private class BeatMarker
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public Image image;
            public int beatNumber;
            public float targetXPosition;
        }
        #endregion

        #region Initialization
        void Start()
        {
            _rhythmManager = GameServices.RhythmManager;

            if (_rhythmManager == null)
            {
                Debug.LogError("BeatTimelineUI: RhythmSyncManager not found!");
                enabled = false;
                return;
            }

            if (timelineContainer == null)
            {
                Debug.LogError("BeatTimelineUI: Timeline container not assigned!");
                enabled = false;
                return;
            }

            if (beatMarkerPrefab == null)
            {
                Debug.LogError("BeatTimelineUI: Beat marker prefab not assigned!");
                enabled = false;
                return;
            }

            InitializeTimeline();
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeTimeline()
        {
            // Calculate timeline width from container
            _timelineWidth = timelineContainer.rect.width;

            // Calculate timing zone sizes based on rhythm settings
            CalculateTimingZones();

            // Setup timing zone visuals
            SetupTimingZoneVisuals();

            // Pre-instantiate beat markers for pooling
            PreInstantiateBeatMarkers(beatsAhead + beatsBehind + 2);

            Debug.Log($"BeatTimelineUI initialized: Width={_timelineWidth}, BeatsAhead={beatsAhead}");
        }

        private void CalculateTimingZones()
        {
            if (_rhythmManager == null) return;

            // Convert timing windows to pixel widths
            // beatSpacing represents one beat interval
            float perfectWindowRatio = _rhythmManager.perfectTolerance * 2f / _rhythmManager.beatInterval;
            float greatWindowRatio = _rhythmManager.beatTolerance * 2f / _rhythmManager.beatInterval;

            _perfectZoneSize = new Vector2(beatSpacing * perfectWindowRatio, timelineContainer.rect.height * 0.8f);
            _greatZoneSize = new Vector2(beatSpacing * greatWindowRatio, timelineContainer.rect.height * 0.6f);
        }

        private void SetupTimingZoneVisuals()
        {
            // Position and size Perfect zone
            if (perfectZoneImage != null)
            {
                perfectZoneImage.rectTransform.sizeDelta = _perfectZoneSize;
                perfectZoneImage.color = perfectZoneColor;
                perfectZoneImage.rectTransform.anchoredPosition = Vector2.zero; // Centered
            }

            // Position and size Great zone
            if (greatZoneImage != null)
            {
                greatZoneImage.rectTransform.sizeDelta = _greatZoneSize;
                greatZoneImage.color = greatZoneColor;
                greatZoneImage.rectTransform.anchoredPosition = Vector2.zero; // Centered
            }

            // Position current beat indicator
            if (currentBeatIndicator != null)
            {
                currentBeatIndicator.GetComponent<Image>().color = currentBeatColor;
            }
        }

        private void PreInstantiateBeatMarkers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewBeatMarker();
            }
        }

        private BeatMarker CreateNewBeatMarker()
        {
            GameObject markerObj = Instantiate(beatMarkerPrefab, timelineContainer);
            markerObj.SetActive(false);

            BeatMarker marker = new BeatMarker
            {
                gameObject = markerObj,
                rectTransform = markerObj.GetComponent<RectTransform>(),
                image = markerObj.GetComponent<Image>()
            };

            _inactiveBeatMarkers.Push(marker);
            return marker;
        }
        #endregion

        #region Event Handling
        private void SubscribeToEvents()
        {
            if (_rhythmManager != null)
            {
                _rhythmManager.OnBeatCounted.AddListener(OnBeatCounted);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_rhythmManager != null)
            {
                _rhythmManager.OnBeatCounted.RemoveListener(OnBeatCounted);
            }
        }

        private void OnBeatCounted(int beatCount)
        {
            // Update last beat count
            _lastBeatCount = beatCount;

            // Pulse current beat indicator
            if (enableBeatPulse && currentBeatIndicator != null)
            {
                StartCoroutine(PulseBeatIndicator());
            }

            // Add new beat markers for upcoming beats
            SpawnUpcomingBeatMarkers(beatCount);

            // Clean up old markers
            CleanupPassedMarkers(beatCount);
        }

        private System.Collections.IEnumerator PulseBeatIndicator()
        {
            Vector3 originalScale = currentBeatIndicator.localScale;
            Vector3 targetScale = originalScale * beatPulseScale;

            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                currentBeatIndicator.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            currentBeatIndicator.localScale = originalScale;
        }
        #endregion

        #region Beat Marker Management
        private void SpawnUpcomingBeatMarkers(int currentBeat)
        {
            // Spawn markers for upcoming beats
            for (int i = 1; i <= beatsAhead; i++)
            {
                int futureBeat = currentBeat + i;

                // Check if marker already exists for this beat
                bool exists = false;
                foreach (var marker in _activeBeatMarkers)
                {
                    if (marker.beatNumber == futureBeat)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    SpawnBeatMarker(futureBeat);
                }
            }
        }

        private void SpawnBeatMarker(int beatNumber)
        {
            BeatMarker marker = GetBeatMarker();
            marker.beatNumber = beatNumber;

            // Calculate target position (to the right for future beats)
            int beatsFromCurrent = beatNumber - _lastBeatCount;
            marker.targetXPosition = beatsFromCurrent * beatSpacing;

            // Set initial position
            marker.rectTransform.anchoredPosition = new Vector2(marker.targetXPosition, 0f);

            // Set color based on distance
            UpdateMarkerColor(marker, beatsFromCurrent);

            // Activate marker
            marker.gameObject.SetActive(true);

            // Add to active queue
            _activeBeatMarkers.Enqueue(marker);
        }

        private void CleanupPassedMarkers(int currentBeat)
        {
            // Remove markers that are too far in the past
            while (_activeBeatMarkers.Count > 0)
            {
                BeatMarker marker = _activeBeatMarkers.Peek();
                int beatsFromCurrent = marker.beatNumber - currentBeat;

                if (beatsFromCurrent < -beatsBehind)
                {
                    // Deactivate and return to pool
                    _activeBeatMarkers.Dequeue();
                    marker.gameObject.SetActive(false);
                    _inactiveBeatMarkers.Push(marker);
                }
                else
                {
                    break; // Queue is ordered, so we can stop here
                }
            }
        }

        private BeatMarker GetBeatMarker()
        {
            if (_inactiveBeatMarkers.Count > 0)
            {
                return _inactiveBeatMarkers.Pop();
            }
            else
            {
                return CreateNewBeatMarker();
            }
        }

        private void UpdateMarkerColor(BeatMarker marker, int beatsFromCurrent)
        {
            if (marker.image == null) return;

            if (beatsFromCurrent == 0)
            {
                marker.image.color = currentBeatColor;
            }
            else if (beatsFromCurrent > 0)
            {
                // Future beat - fade based on distance
                float fadeAmount = 1f - (beatsFromCurrent / (float)beatsAhead * 0.4f);
                Color color = upcomingBeatColor;
                color.a *= fadeAmount;
                marker.image.color = color;
            }
            else
            {
                marker.image.color = passedBeatColor;
            }
        }
        #endregion

        #region Update
        void Update()
        {
            if (_rhythmManager == null) return;

            // Smoothly scroll all beat markers based on rhythm progress
            UpdateBeatMarkerPositions();
        }

        private void UpdateBeatMarkerPositions()
        {
            if (_activeBeatMarkers.Count == 0) return;

            // Calculate scroll offset based on rhythm progress within current beat
            float beatProgress = _rhythmManager.GetBeatProgress();
            float scrollOffset = beatProgress * beatSpacing;

            // Update all active markers
            foreach (var marker in _activeBeatMarkers)
            {
                int beatsFromCurrent = marker.beatNumber - _lastBeatCount;
                float targetX = beatsFromCurrent * beatSpacing - scrollOffset;

                marker.rectTransform.anchoredPosition = new Vector2(targetX, 0f);

                // Update color based on current position
                UpdateMarkerColor(marker, beatsFromCurrent);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Updates timing zone visuals when rhythm settings change
        /// 리듬 설정 변경 시 타이밍 존 비주얼 업데이트
        /// </summary>
        public void RefreshTimingZones()
        {
            CalculateTimingZones();
            SetupTimingZoneVisuals();
        }

        /// <summary>
        /// Clears all beat markers (useful for scene transitions)
        /// 모든 비트 마커 제거 (씬 전환 시 유용)
        /// </summary>
        public void ClearAllMarkers()
        {
            while (_activeBeatMarkers.Count > 0)
            {
                BeatMarker marker = _activeBeatMarkers.Dequeue();
                marker.gameObject.SetActive(false);
                _inactiveBeatMarkers.Push(marker);
            }
        }

        /// <summary>
        /// Resets the timeline to current beat
        /// 타임라인을 현재 비트로 리셋
        /// </summary>
        public void ResetTimeline()
        {
            ClearAllMarkers();
            if (_rhythmManager != null)
            {
                _lastBeatCount = _rhythmManager.currentBeatCount;
                SpawnUpcomingBeatMarkers(_lastBeatCount);
            }
        }
        #endregion

        #region Debug
        void OnValidate()
        {
            // Ensure colors have proper alpha for visibility
            if (perfectZoneColor.a < 0.05f)
                perfectZoneColor.a = 0.2f;

            if (greatZoneColor.a < 0.05f)
                greatZoneColor.a = 0.15f;
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Beat Pulse")]
        private void TestBeatPulse()
        {
            if (currentBeatIndicator != null && Application.isPlaying)
            {
                StartCoroutine(PulseBeatIndicator());
            }
        }

        [ContextMenu("Refresh Timeline")]
        private void TestRefreshTimeline()
        {
            if (Application.isPlaying)
            {
                RefreshTimingZones();
                ResetTimeline();
            }
        }
        #endif
        #endregion
    }
}
