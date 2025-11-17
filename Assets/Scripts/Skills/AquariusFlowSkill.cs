using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Aquarius Flow skill - Speed boost
    /// Aquarius Flow 스킬 - 속도 증가
    ///
    /// Skill Details:
    /// - Category: Movement
    /// - Cooldown: 12 beats
    /// - Focus Cost: 25
    /// - Input Pattern: 1-2-3 (three beats)
    /// - Duration: 8 beats
    ///
    /// Effect: Grants significant movement speed boost.
    /// Player moves with flowing water-like grace.
    /// </summary>
    public class AquariusFlowSkill : MonoBehaviour
    {
        [Header("▶ Speed Boost Settings")]
        [Tooltip("Movement speed multiplier during Flow")]
        [Range(1.5f, 3f)]
        public float speedMultiplier = 2f; // 2x speed

        [Tooltip("Duration in beats")]
        public int durationBeats = 8;

        [Header("▶ Visual Effects")]
        [Tooltip("Sprite transparency during Flow")]
        [Range(0.5f, 1f)]
        public float flowAlpha = 0.9f; // Slightly transparent

        [Tooltip("Sprite color tint during Flow")]
        public Color flowTint = new Color(0.7f, 0.9f, 1f, 1f); // Light blue

        [Tooltip("Trail particles that follow player")]
        public GameObject flowTrailPrefab;

        [Tooltip("Aura particles around player")]
        public GameObject flowAuraPrefab;

        [Header("▶ VFX & SFX")]
        [Tooltip("VFX when activating Flow")]
        public GameObject activationVFXPrefab;

        [Tooltip("VFX when Flow ends")]
        public GameObject endVFXPrefab;

        [Tooltip("Sound effect when activating")]
        public AudioClip activationSFX;

        [Tooltip("Sound effect when flow ends")]
        public AudioClip endSFX;

        [Tooltip("Ambient whoosh sound while flowing")]
        public AudioClip flowAmbientLoop;

        [Header("▶ Bonus Effects")]
        [Tooltip("Enable dash cooldown reduction during Flow?")]
        public bool reduceDashCooldown = true;

        [Tooltip("Dash cooldown reduction percentage")]
        [Range(0f, 0.5f)]
        public float dashCooldownReduction = 0.25f; // 25% faster dash recharge

        #region Private State
        private bool _isFlowActive = false;
        private float _flowEndTime;
        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;
        private RhythmSyncManager _rhythmManager;

        private float _originalMoveSpeed;
        private Color _originalColor;
        private GameObject _flowTrailInstance;
        private GameObject _flowAuraInstance;
        private AudioSource _ambientAudioSource;
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
                _originalColor = _spriteRenderer.color;
            }

            if (_playerController != null)
            {
                _originalMoveSpeed = _playerController.moveSpeed;
            }
        }
        #endregion

        #region Update
        void Update()
        {
            if (!_isFlowActive) return;

            // Check if duration expired
            if (Time.time >= _flowEndTime)
            {
                DeactivateFlow();
                return;
            }

            // Update particle positions
            if (_flowTrailInstance != null)
            {
                _flowTrailInstance.transform.position = transform.position;
            }

            if (_flowAuraInstance != null)
            {
                _flowAuraInstance.transform.position = transform.position;
            }
        }
        #endregion

        #region Skill Activation
        /// <summary>
        /// Activates Aquarius Flow
        /// Aquarius Flow 활성화
        /// </summary>
        public void ActivateSkill()
        {
            // Don't reactivate if already active
            if (_isFlowActive)
            {
                Debug.Log("Aquarius Flow already active - extending duration!");
                // Optional: Extend duration instead
                ExtendDuration();
                return;
            }

            // Calculate duration based on rhythm
            float durationSeconds = CalculateDuration();

            // Activate flow
            _isFlowActive = true;
            _flowEndTime = Time.time + durationSeconds;

            // Apply speed boost
            if (_playerController != null)
            {
                _playerController.moveSpeed = _originalMoveSpeed * speedMultiplier;
            }

            // Apply visual effects
            ApplyFlowVisuals();

            // Spawn trail particles
            if (flowTrailPrefab != null)
            {
                _flowTrailInstance = Instantiate(flowTrailPrefab, transform.position, Quaternion.identity);
                _flowTrailInstance.transform.SetParent(transform);
            }

            // Spawn aura particles
            if (flowAuraPrefab != null)
            {
                _flowAuraInstance = Instantiate(flowAuraPrefab, transform.position, Quaternion.identity);
                _flowAuraInstance.transform.SetParent(transform);
            }

            // Play activation VFX
            if (activationVFXPrefab != null)
            {
                PlayVFX(activationVFXPrefab, transform.position);
            }

            // Play activation SFX
            if (activationSFX != null)
            {
                PlaySFX(activationSFX);
            }

            // Play ambient loop
            if (flowAmbientLoop != null)
            {
                PlayAmbientLoop();
            }

            // Apply bonus effects
            if (reduceDashCooldown)
            {
                ApplyDashCooldownBonus(true);
            }

            Debug.Log($"Aquarius Flow activated: {speedMultiplier}x speed for {durationBeats} beats ({durationSeconds:F1}s)");
        }

        private float CalculateDuration()
        {
            if (_rhythmManager != null)
            {
                return durationBeats * _rhythmManager.beatInterval;
            }
            else
            {
                return durationBeats * 0.5f; // Fallback: assume 120 BPM
            }
        }

        private void ExtendDuration()
        {
            // Extend flow duration (called when reactivated while active)
            float extensionSeconds = CalculateDuration() * 0.5f; // 50% extension
            _flowEndTime += extensionSeconds;

            Debug.Log($"Aquarius Flow extended by {extensionSeconds:F1}s");
        }

        private void ApplyFlowVisuals()
        {
            if (_spriteRenderer == null) return;

            // Apply color tint and transparency
            Color newColor = flowTint;
            newColor.a = flowAlpha;
            _spriteRenderer.color = newColor;
        }

        /// <summary>
        /// Deactivates Aquarius Flow
        /// Aquarius Flow 비활성화
        /// </summary>
        private void DeactivateFlow()
        {
            if (!_isFlowActive) return;

            _isFlowActive = false;

            // Restore movement speed
            if (_playerController != null)
            {
                _playerController.moveSpeed = _originalMoveSpeed;
            }

            // Restore visuals
            RestoreFlowVisuals();

            // Destroy particles
            if (_flowTrailInstance != null)
            {
                Destroy(_flowTrailInstance);
                _flowTrailInstance = null;
            }

            if (_flowAuraInstance != null)
            {
                Destroy(_flowAuraInstance);
                _flowAuraInstance = null;
            }

            // Play end VFX
            if (endVFXPrefab != null)
            {
                PlayVFX(endVFXPrefab, transform.position);
            }

            // Play end SFX
            if (endSFX != null)
            {
                PlaySFX(endSFX);
            }

            // Stop ambient loop
            StopAmbientLoop();

            // Remove bonus effects
            if (reduceDashCooldown)
            {
                ApplyDashCooldownBonus(false);
            }

            Debug.Log("Aquarius Flow ended");
        }

        private void RestoreFlowVisuals()
        {
            if (_spriteRenderer == null) return;

            // Restore original color
            _spriteRenderer.color = _originalColor;
        }

        private void ApplyDashCooldownBonus(bool enable)
        {
            // This is a placeholder for dash cooldown modification
            // Would integrate with skill system to reduce Pegasus Dash cooldown

            if (enable)
            {
                Debug.Log($"Dash cooldown reduced by {dashCooldownReduction * 100}%");
                // TODO: Integrate with RhythmPatternChecker or skill cooldown system
            }
            else
            {
                Debug.Log("Dash cooldown bonus removed");
            }
        }
        #endregion

        #region Audio
        private void PlayAmbientLoop()
        {
            if (flowAmbientLoop == null) return;

            // Create audio source for ambient loop
            _ambientAudioSource = gameObject.AddComponent<AudioSource>();
            _ambientAudioSource.clip = flowAmbientLoop;
            _ambientAudioSource.loop = true;
            _ambientAudioSource.volume = 0.4f;
            _ambientAudioSource.spatialBlend = 0f; // 2D sound
            _ambientAudioSource.Play();
        }

        private void StopAmbientLoop()
        {
            if (_ambientAudioSource != null)
            {
                _ambientAudioSource.Stop();
                Destroy(_ambientAudioSource);
                _ambientAudioSource = null;
            }
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
        /// Is Aquarius Flow currently active?
        /// Aquarius Flow가 현재 활성화되어 있는가?
        /// </summary>
        public bool IsActive()
        {
            return _isFlowActive;
        }

        /// <summary>
        /// Gets remaining duration in seconds
        /// 남은 지속 시간 (초 단위)
        /// </summary>
        public float GetRemainingDuration()
        {
            if (!_isFlowActive) return 0f;
            return Mathf.Max(0f, _flowEndTime - Time.time);
        }

        /// <summary>
        /// Gets current speed multiplier
        /// 현재 속도 배수 가져오기
        /// </summary>
        public float GetCurrentSpeedMultiplier()
        {
            return _isFlowActive ? speedMultiplier : 1f;
        }

        /// <summary>
        /// Manually deactivate Flow early
        /// Flow를 조기에 수동으로 비활성화
        /// </summary>
        public void DeactivateEarly()
        {
            DeactivateFlow();
        }
        #endregion

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Test Activate Aquarius Flow")]
        private void TestActivate()
        {
            if (Application.isPlaying)
            {
                ActivateSkill();
            }
        }

        [ContextMenu("Test Deactivate Aquarius Flow")]
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
            if (_isFlowActive)
            {
                // Draw flow indicator
                Gizmos.color = new Color(0.5f, 0.9f, 1f, 0.5f); // Light blue
                Gizmos.DrawWireSphere(transform.position, 1.2f);

                // Draw speed boost indicator
                Vector2 direction = Vector2.right;
                if (_spriteRenderer != null)
                {
                    direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(
                    transform.position,
                    (Vector2)transform.position + direction * 2f * speedMultiplier
                );

                // Draw remaining time
                #if UNITY_EDITOR
                float remaining = GetRemainingDuration();
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2.5f,
                    $"Aquarius Flow: {speedMultiplier}x speed, {remaining:F1}s"
                );
                #endif
            }
        }
        #endregion

        void OnDestroy()
        {
            // Cleanup if destroyed while active
            if (_isFlowActive)
            {
                RestoreFlowVisuals();

                if (_playerController != null)
                {
                    _playerController.moveSpeed = _originalMoveSpeed;
                }

                if (_flowTrailInstance != null)
                {
                    Destroy(_flowTrailInstance);
                }

                if (_flowAuraInstance != null)
                {
                    Destroy(_flowAuraInstance);
                }

                StopAmbientLoop();
            }
        }
    }
}
