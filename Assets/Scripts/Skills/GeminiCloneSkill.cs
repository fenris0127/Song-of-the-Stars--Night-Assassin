using UnityEngine;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Gemini Clone skill - Creates a moving decoy clone
    /// Gemini Clone 스킬 - 이동하는 분신 생성
    ///
    /// Skill Details:
    /// - Category: Lure
    /// - Cooldown: 12 beats
    /// - Focus Cost: 20
    /// - Input Pattern: 1-2 (two beats)
    /// - Duration: 8 beats
    ///
    /// Effect: Spawns a semi-transparent clone that patrols and attracts guard attention
    /// </summary>
    public class GeminiCloneSkill : MonoBehaviour
    {
        [Header("▶ Clone Prefab")]
        [Tooltip("Prefab for the clone (should have CloneController component)")]
        public GameObject clonePrefab;

        [Header("▶ Spawn Settings")]
        [Tooltip("Offset from player position to spawn clone")]
        public Vector2 spawnOffset = new Vector2(2f, 0f);

        [Tooltip("Should clone spawn in player's facing direction?")]
        public bool spawnInFacingDirection = true;

        [Tooltip("Maximum number of active clones allowed")]
        [Range(1, 3)]
        public int maxActiveClones = 1;

        [Header("▶ Clone Configuration")]
        [Tooltip("Movement pattern for the clone")]
        public CloneController.CloneMovementPattern movementPattern = CloneController.CloneMovementPattern.BackAndForth;

        [Tooltip("Clone movement speed")]
        public float cloneMoveSpeed = 3f;

        [Tooltip("Clone patrol distance")]
        public float clonePatrolDistance = 4f;

        [Tooltip("Clone lifetime in beats")]
        public int cloneLifetimeBeats = 8;

        [Header("▶ VFX & SFX")]
        [Tooltip("VFX to play at player position when activating skill")]
        public GameObject activationVFXPrefab;

        [Tooltip("Sound effect when spawning clone")]
        public AudioClip spawnSFX;

        [Header("▶ Layer Settings")]
        [Tooltip("Layer mask for guards (for clone to detect)")]
        public LayerMask guardMask;

        #region Private State
        private Transform _playerTransform;
        private SpriteRenderer _playerSpriteRenderer;
        private System.Collections.Generic.List<CloneController> _activeClones = new System.Collections.Generic.List<CloneController>();
        #endregion

        #region Initialization
        void Awake()
        {
            _playerTransform = transform;
            _playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (clonePrefab == null)
            {
                Debug.LogError("GeminiCloneSkill: Clone prefab not assigned!");
                enabled = false;
                return;
            }

            // Validate clone prefab has CloneController component
            CloneController cloneController = clonePrefab.GetComponent<CloneController>();
            if (cloneController == null)
            {
                Debug.LogError("GeminiCloneSkill: Clone prefab missing CloneController component!");
                enabled = false;
                return;
            }
        }
        #endregion

        #region Skill Activation
        /// <summary>
        /// Activates the Gemini Clone skill
        /// Gemini Clone 스킬 활성화
        /// </summary>
        public void ActivateSkill()
        {
            // Check if we've reached max active clones
            CleanupDestroyedClones();
            if (_activeClones.Count >= maxActiveClones)
            {
                Debug.Log($"Gemini Clone: Max active clones reached ({maxActiveClones})");
                return;
            }

            // Get spawn position and direction
            Vector2 spawnPosition = GetSpawnPosition();
            Vector2 facingDirection = GetPlayerFacingDirection();

            // Spawn clone
            GameObject cloneObj = SpawnClone(spawnPosition, facingDirection);
            if (cloneObj != null)
            {
                // Play activation VFX at player position
                if (activationVFXPrefab != null)
                {
                    PlayVFX(activationVFXPrefab, _playerTransform.position);
                }

                // Play spawn SFX
                if (spawnSFX != null)
                {
                    PlaySFX(spawnSFX);
                }

                Debug.Log($"Gemini Clone spawned at {spawnPosition}");
            }
        }

        private Vector2 GetSpawnPosition()
        {
            Vector2 playerPos = _playerTransform.position;

            if (spawnInFacingDirection)
            {
                // Spawn in front of player based on facing direction
                Vector2 direction = GetPlayerFacingDirection();
                return playerPos + direction * spawnOffset.magnitude;
            }
            else
            {
                // Use fixed offset
                return playerPos + spawnOffset;
            }
        }

        private Vector2 GetPlayerFacingDirection()
        {
            // Check if player sprite is flipped to determine facing direction
            if (_playerSpriteRenderer != null)
            {
                return _playerSpriteRenderer.flipX ? Vector2.left : Vector2.right;
            }

            // Fallback: default right
            return Vector2.right;
        }

        private GameObject SpawnClone(Vector2 position, Vector2 facingDirection)
        {
            GameObject cloneObj = null;

            // Try to get from object pool
            if (ObjectPoolManager.Instance != null)
            {
                cloneObj = ObjectPoolManager.Instance.GetPooledObject(clonePrefab);
                if (cloneObj != null)
                {
                    cloneObj.transform.position = position;
                    cloneObj.SetActive(true);
                }
            }

            // Fallback: Instantiate
            if (cloneObj == null)
            {
                cloneObj = Instantiate(clonePrefab, position, Quaternion.identity);
            }

            // Configure clone controller
            CloneController cloneController = cloneObj.GetComponent<CloneController>();
            if (cloneController != null)
            {
                // Copy player's sprite to clone
                SpriteRenderer cloneSpriteRenderer = cloneObj.GetComponent<SpriteRenderer>();
                if (cloneSpriteRenderer != null && _playerSpriteRenderer != null)
                {
                    cloneSpriteRenderer.sprite = _playerSpriteRenderer.sprite;
                    cloneSpriteRenderer.flipX = _playerSpriteRenderer.flipX;
                }

                // Configure clone settings
                cloneController.movementPattern = movementPattern;
                cloneController.moveSpeed = cloneMoveSpeed;
                cloneController.patrolDistance = clonePatrolDistance;
                cloneController.lifetimeBeats = cloneLifetimeBeats;
                cloneController.guardMask = guardMask;

                // Initialize clone
                cloneController.Initialize(position, facingDirection);

                // Add to active clones list
                _activeClones.Add(cloneController);
            }
            else
            {
                Debug.LogError("GeminiCloneSkill: Spawned clone missing CloneController component!");
                Destroy(cloneObj);
                return null;
            }

            return cloneObj;
        }

        private void CleanupDestroyedClones()
        {
            // Remove null references (destroyed clones)
            _activeClones.RemoveAll(clone => clone == null);
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
        /// Gets the number of currently active clones
        /// 현재 활성 분신 수 가져오기
        /// </summary>
        public int GetActiveCloneCount()
        {
            CleanupDestroyedClones();
            return _activeClones.Count;
        }

        /// <summary>
        /// Despawns all active clones
        /// 모든 활성 분신 제거
        /// </summary>
        public void DespawnAllClones()
        {
            foreach (var clone in _activeClones)
            {
                if (clone != null)
                {
                    clone.DespawnEarly();
                }
            }
            _activeClones.Clear();
        }

        /// <summary>
        /// Can the skill be activated? (checks max clones)
        /// 스킬을 활성화할 수 있는가? (최대 분신 수 확인)
        /// </summary>
        public bool CanActivate()
        {
            CleanupDestroyedClones();
            return _activeClones.Count < maxActiveClones;
        }
        #endregion

        #region Debug
        void OnDrawGizmos()
        {
            // Draw spawn position preview in editor
            if (!Application.isPlaying && _playerTransform != null)
            {
                Vector2 previewSpawnPos = (Vector2)_playerTransform.position + spawnOffset;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(previewSpawnPos, 0.5f);
                Gizmos.DrawLine(_playerTransform.position, previewSpawnPos);
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Spawn Clone")]
        private void TestSpawnClone()
        {
            if (Application.isPlaying)
            {
                ActivateSkill();
            }
        }

        [ContextMenu("Test Despawn All Clones")]
        private void TestDespawnAllClones()
        {
            if (Application.isPlaying)
            {
                DespawnAllClones();
            }
        }
        #endif
        #endregion

        void OnDestroy()
        {
            // Cleanup all clones when skill component is destroyed
            DespawnAllClones();
        }
    }
}
