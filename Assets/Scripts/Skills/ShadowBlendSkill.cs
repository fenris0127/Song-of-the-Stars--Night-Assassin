using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Shadow Blend skill - Stationary invisibility
    /// Shadow Blend 스킬 - 정지 상태 투명화
    ///
    /// Skill Details:
    /// - Category: Stealth
    /// - Cooldown: 8 beats
    /// - Focus Cost: 15
    /// - Input Pattern: 1 (single beat)
    /// - Duration: 8 beats
    ///
    /// Effect: Player becomes highly transparent while stationary.
    /// Movement breaks the effect.
    /// </summary>
    public class ShadowBlendSkill : MonoBehaviour
    {
        [Header("▶ Stealth Settings")]
        [Tooltip("Alpha transparency when Shadow Blend is active")]
        [Range(0f, 1f)]
        public float blendAlpha = 0.2f; // Nearly invisible

        [Tooltip("Duration in beats")]
        public int durationBeats = 8;

        [Tooltip("Should movement break Shadow Blend?")]
        public bool movementBreaksBlend = true;

        [Header("▶ Detection Settings")]
        [Tooltip("Detection range multiplier while in Shadow Blend")]
        [Range(0f, 1f)]
        public float detectionRangeMultiplier = 0.3f; // Guards detect at 30% range

        [Header("▶ VFX & SFX")]
        [Tooltip("VFX to play when activating Shadow Blend")]
        public GameObject activationVFXPrefab;

        [Tooltip("VFX to play when Shadow Blend breaks")]
        public GameObject breakVFXPrefab;

        [Tooltip("VFX to play when Shadow Blend ends normally")]
        public GameObject endVFXPrefab;

        [Tooltip("Sound effect when activating")]
        public AudioClip activationSFX;

        [Tooltip("Sound effect when blend breaks")]
        public AudioClip breakSFX;

        #region Private State
        private bool _isBlendActive = false;
        private float _blendEndTime;
        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;
        private RhythmSyncManager _rhythmManager;
        private Vector3 _lastPosition;
        private float _originalAlpha;

        // Guard detection modification
        private float _originalDetectionRangeMultiplier = 1f;
        #endregion

        #region Initialization
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerController = GetComponent<PlayerController>();
            _rhythmManager = GameServices.RhythmManager;
        }

        void Start()
        {
            if (_spriteRenderer != null)
            {
                _originalAlpha = _spriteRenderer.color.a;
            }
        }
        #endregion

        #region Update
        void Update()
        {
            if (!_isBlendActive) return;

            // Check if duration expired
            if (Time.time >= _blendEndTime)
            {
                DeactivateBlend(false); // Normal end
                return;
            }

            // Check if player moved (breaks blend)
            if (movementBreaksBlend && HasPlayerMoved())
            {
                DeactivateBlend(true); // Broken by movement
                return;
            }

            // Update last position
            _lastPosition = transform.position;
        }

        private bool HasPlayerMoved()
        {
            if (_playerController == null) return false;

            // Check if player is currently moving
            if (_playerController.isMoving) return true;

            // Check if position changed
            float distanceMoved = Vector3.Distance(transform.position, _lastPosition);
            return distanceMoved > 0.01f;
        }
        #endregion

        #region Skill Activation
        /// <summary>
        /// Activates the Shadow Blend skill
        /// Shadow Blend 스킬 활성화
        /// </summary>
        public void ActivateSkill()
        {
            // Don't reactivate if already active
            if (_isBlendActive)
            {
                Debug.Log("Shadow Blend already active!");
                return;
            }

            // Calculate duration based on rhythm
            float durationSeconds = CalculateDuration();

            // Activate blend
            _isBlendActive = true;
            _blendEndTime = Time.time + durationSeconds;
            _lastPosition = transform.position;

            // Apply visual effect
            ApplyBlendVisuals();

            // Modify detection (optional - if you have a detection system)
            ModifyGuardDetection(true);

            // Play VFX
            if (activationVFXPrefab != null)
            {
                PlayVFX(activationVFXPrefab, transform.position);
            }

            // Play SFX
            if (activationSFX != null)
            {
                PlaySFX(activationSFX);
            }

            Debug.Log($"Shadow Blend activated for {durationBeats} beats ({durationSeconds:F1}s)");
        }

        private float CalculateDuration()
        {
            if (_rhythmManager != null)
            {
                return durationBeats * _rhythmManager.beatInterval;
            }
            else
            {
                return durationBeats * 0.5f; // Fallback: assume 120 BPM (0.5s per beat)
            }
        }

        private void ApplyBlendVisuals()
        {
            if (_spriteRenderer == null) return;

            // Make player nearly invisible
            Color color = _spriteRenderer.color;
            color.a = blendAlpha;
            _spriteRenderer.color = color;
        }

        /// <summary>
        /// Deactivates Shadow Blend
        /// Shadow Blend 비활성화
        /// </summary>
        /// <param name="wasBroken">Was it broken by movement?</param>
        private void DeactivateBlend(bool wasBroken)
        {
            if (!_isBlendActive) return;

            _isBlendActive = false;

            // Restore visuals
            RestoreBlendVisuals();

            // Restore detection
            ModifyGuardDetection(false);

            // Play appropriate VFX
            if (wasBroken && breakVFXPrefab != null)
            {
                PlayVFX(breakVFXPrefab, transform.position);
            }
            else if (!wasBroken && endVFXPrefab != null)
            {
                PlayVFX(endVFXPrefab, transform.position);
            }

            // Play SFX
            if (wasBroken && breakSFX != null)
            {
                PlaySFX(breakSFX);
            }

            Debug.Log($"Shadow Blend ended ({(wasBroken ? "broken by movement" : "duration expired")})");
        }

        private void RestoreBlendVisuals()
        {
            if (_spriteRenderer == null) return;

            // Restore original alpha
            Color color = _spriteRenderer.color;
            color.a = _originalAlpha;
            _spriteRenderer.color = color;
        }

        private void ModifyGuardDetection(bool enable)
        {
            // This is a placeholder for modifying guard detection range
            // You would integrate this with your guard detection system
            // For example, you might have a global player detection multiplier

            if (enable)
            {
                // Reduce detection range for all guards
                // This could be implemented via a static modifier in GuardRhythmPatrol
                // or through a detection manager system
                Debug.Log($"Guard detection range reduced to {detectionRangeMultiplier * 100}%");
            }
            else
            {
                // Restore detection range
                Debug.Log("Guard detection range restored");
            }

            // TODO: Integrate with actual guard detection system when available
        }
        #endregion

        #region VFX & SFX
        private void PlayVFX(GameObject vfxPrefab, Vector3 position)
        {
            VFXManager vfxManager = VFXManager.Instance;
            if (vfxManager != null)
            {
                vfxManager.PlayVFXAt(vfxPrefab, position);
            }
            else
            {
                Instantiate(vfxPrefab, position, Quaternion.identity);
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            SFXManager sfxManager = SFXManager.Instance;
            if (sfxManager != null)
            {
                sfxManager.PlaySFX(clip);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Is Shadow Blend currently active?
        /// Shadow Blend가 현재 활성화되어 있는가?
        /// </summary>
        public bool IsActive()
        {
            return _isBlendActive;
        }

        /// <summary>
        /// Gets remaining duration in seconds
        /// 남은 지속 시간 (초 단위)
        /// </summary>
        public float GetRemainingDuration()
        {
            if (!_isBlendActive) return 0f;
            return Mathf.Max(0f, _blendEndTime - Time.time);
        }

        /// <summary>
        /// Manually deactivate Shadow Blend early
        /// Shadow Blend를 조기에 수동으로 비활성화
        /// </summary>
        public void DeactivateEarly()
        {
            DeactivateBlend(false);
        }
        #endregion

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Test Activate Shadow Blend")]
        private void TestActivate()
        {
            if (Application.isPlaying)
            {
                ActivateSkill();
            }
        }

        [ContextMenu("Test Deactivate Shadow Blend")]
        private void TestDeactivate()
        {
            if (Application.isPlaying)
            {
                DeactivateEarly();
            }
        }
        #endif

        void OnDrawGizmos()
        {
            if (_isBlendActive)
            {
                // Draw blend indicator
                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, 1f);

                // Draw remaining time
                #if UNITY_EDITOR
                float remaining = GetRemainingDuration();
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2f,
                    $"Shadow Blend: {remaining:F1}s"
                );
                #endif
            }
        }
        #endregion

        void OnDestroy()
        {
            // Restore visuals if destroyed while active
            if (_isBlendActive)
            {
                RestoreBlendVisuals();
                ModifyGuardDetection(false);
            }
        }
    }
}
