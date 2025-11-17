using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Andromeda's Veil skill - Full invisibility with movement
    /// Andromeda's Veil 스킬 - 이동 가능한 완전 투명화
    ///
    /// Skill Details:
    /// - Category: Stealth
    /// - Cooldown: 16 beats
    /// - Focus Cost: 35
    /// - Input Pattern: 1-2-3 (three beats)
    /// - Duration: 8 beats
    ///
    /// Effect: Player becomes fully invisible and can move slowly.
    /// Premium stealth skill with higher cost and complexity.
    /// </summary>
    public class AndromedaVeilSkill : MonoBehaviour
    {
        [Header("▶ Stealth Settings")]
        [Tooltip("Alpha transparency when Veil is active")]
        [Range(0f, 1f)]
        public float veilAlpha = 0.05f; // Nearly fully invisible

        [Tooltip("Duration in beats")]
        public int durationBeats = 8;

        [Tooltip("Movement speed multiplier while veiled")]
        [Range(0.1f, 1f)]
        public float moveSpeedMultiplier = 0.6f; // 60% speed

        [Header("▶ Detection Settings")]
        [Tooltip("Detection range multiplier while veiled")]
        [Range(0f, 1f)]
        public float detectionRangeMultiplier = 0.1f; // Guards detect at 10% range

        [Tooltip("Vision cone width multiplier for guards")]
        [Range(0f, 1f)]
        public float visionConeMultiplier = 0.5f; // Guards have 50% narrower vision

        [Header("▶ VFX & SFX")]
        [Tooltip("VFX to play when activating Veil")]
        public GameObject activationVFXPrefab;

        [Tooltip("VFX to play when Veil ends")]
        public GameObject endVFXPrefab;

        [Tooltip("Particle effect that follows player while veiled")]
        public GameObject veilParticlesPrefab;

        [Tooltip("Sound effect when activating")]
        public AudioClip activationSFX;

        [Tooltip("Sound effect when veil ends")]
        public AudioClip endSFX;

        [Tooltip("Ambient sound loop while veiled")]
        public AudioClip veilAmbientLoop;

        #region Private State
        private bool _isVeilActive = false;
        private float _veilEndTime;
        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;
        private RhythmSyncManager _rhythmManager;
        private float _originalAlpha;
        private float _originalMoveSpeed;
        private GameObject _veilParticlesInstance;
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
                _originalAlpha = _spriteRenderer.color.a;
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
            if (!_isVeilActive) return;

            // Check if duration expired
            if (Time.time >= _veilEndTime)
            {
                DeactivateVeil();
                return;
            }

            // Update particle position if active
            if (_veilParticlesInstance != null)
            {
                _veilParticlesInstance.transform.position = transform.position;
            }
        }
        #endregion

        #region Skill Activation
        /// <summary>
        /// Activates Andromeda's Veil
        /// Andromeda's Veil 활성화
        /// </summary>
        public void ActivateSkill()
        {
            // Don't reactivate if already active
            if (_isVeilActive)
            {
                Debug.Log("Andromeda's Veil already active!");
                return;
            }

            // Calculate duration based on rhythm
            float durationSeconds = CalculateDuration();

            // Activate veil
            _isVeilActive = true;
            _veilEndTime = Time.time + durationSeconds;

            // Apply visual effect
            ApplyVeilVisuals();

            // Reduce movement speed
            if (_playerController != null)
            {
                _playerController.moveSpeed = _originalMoveSpeed * moveSpeedMultiplier;
            }

            // Spawn veil particles
            if (veilParticlesPrefab != null)
            {
                _veilParticlesInstance = Instantiate(veilParticlesPrefab, transform.position, Quaternion.identity);
                _veilParticlesInstance.transform.SetParent(transform); // Parent to player
            }

            // Modify guard detection
            ModifyGuardDetection(true);

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
            if (veilAmbientLoop != null)
            {
                PlayAmbientLoop();
            }

            Debug.Log($"Andromeda's Veil activated for {durationBeats} beats ({durationSeconds:F1}s)");
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

        private void ApplyVeilVisuals()
        {
            if (_spriteRenderer == null) return;

            // Make player nearly fully invisible
            Color color = _spriteRenderer.color;
            color.a = veilAlpha;
            _spriteRenderer.color = color;
        }

        /// <summary>
        /// Deactivates Andromeda's Veil
        /// Andromeda's Veil 비활성화
        /// </summary>
        private void DeactivateVeil()
        {
            if (!_isVeilActive) return;

            _isVeilActive = false;

            // Restore visuals
            RestoreVeilVisuals();

            // Restore movement speed
            if (_playerController != null)
            {
                _playerController.moveSpeed = _originalMoveSpeed;
            }

            // Destroy veil particles
            if (_veilParticlesInstance != null)
            {
                Destroy(_veilParticlesInstance);
                _veilParticlesInstance = null;
            }

            // Restore guard detection
            ModifyGuardDetection(false);

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

            Debug.Log("Andromeda's Veil ended");
        }

        private void RestoreVeilVisuals()
        {
            if (_spriteRenderer == null) return;

            // Restore original alpha
            Color color = _spriteRenderer.color;
            color.a = _originalAlpha;
            _spriteRenderer.color = color;
        }

        private void ModifyGuardDetection(bool enable)
        {
            // This is a placeholder for modifying guard detection
            // Integration with guard detection system

            if (enable)
            {
                // Dramatically reduce detection range and vision cone
                Debug.Log($"Guard detection: Range={detectionRangeMultiplier * 100}%, Vision={visionConeMultiplier * 100}%");
            }
            else
            {
                // Restore detection
                Debug.Log("Guard detection restored");
            }

            // TODO: Integrate with actual guard detection system
            // This could be done via:
            // 1. Static player detection multiplier in GuardRhythmPatrol
            // 2. Event system that notifies guards when veil is active
            // 3. Detection manager that tracks player stealth state
        }
        #endregion

        #region Audio
        private void PlayAmbientLoop()
        {
            if (veilAmbientLoop == null) return;

            // Create audio source for ambient loop
            _ambientAudioSource = gameObject.AddComponent<AudioSource>();
            _ambientAudioSource.clip = veilAmbientLoop;
            _ambientAudioSource.loop = true;
            _ambientAudioSource.volume = 0.3f;
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
        /// Is Andromeda's Veil currently active?
        /// Andromeda's Veil가 현재 활성화되어 있는가?
        /// </summary>
        public bool IsActive()
        {
            return _isVeilActive;
        }

        /// <summary>
        /// Gets remaining duration in seconds
        /// 남은 지속 시간 (초 단위)
        /// </summary>
        public float GetRemainingDuration()
        {
            if (!_isVeilActive) return 0f;
            return Mathf.Max(0f, _veilEndTime - Time.time);
        }

        /// <summary>
        /// Manually deactivate veil early
        /// 베일을 조기에 수동으로 비활성화
        /// </summary>
        public void DeactivateEarly()
        {
            DeactivateVeil();
        }
        #endregion

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Test Activate Andromeda's Veil")]
        private void TestActivate()
        {
            if (Application.isPlaying)
            {
                ActivateSkill();
            }
        }

        [ContextMenu("Test Deactivate Andromeda's Veil")]
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
            if (_isVeilActive)
            {
                // Draw veil indicator
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.4f); // Purple
                Gizmos.DrawWireSphere(transform.position, 1.5f);

                // Draw remaining time
                #if UNITY_EDITOR
                float remaining = GetRemainingDuration();
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2.5f,
                    $"Andromeda's Veil: {remaining:F1}s"
                );
                #endif
            }
        }
        #endregion

        void OnDestroy()
        {
            // Cleanup if destroyed while active
            if (_isVeilActive)
            {
                RestoreVeilVisuals();

                if (_playerController != null)
                {
                    _playerController.moveSpeed = _originalMoveSpeed;
                }

                if (_veilParticlesInstance != null)
                {
                    Destroy(_veilParticlesInstance);
                }

                StopAmbientLoop();
                ModifyGuardDetection(false);
            }
        }
    }
}
