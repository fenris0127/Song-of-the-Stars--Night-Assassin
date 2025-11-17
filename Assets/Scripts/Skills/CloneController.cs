using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Controls a moving clone decoy created by Gemini Clone skill
    /// Gemini Clone 스킬로 생성된 이동하는 분신 제어
    ///
    /// Features:
    /// - Patrols in a pattern (back and forth or circular)
    /// - Attracts guard attention
    /// - Auto-despawns after duration or if guard gets too close
    /// - Semi-transparent visual
    /// </summary>
    public class CloneController : MonoBehaviour
    {
        [Header("▶ Movement Settings")]
        [Tooltip("Movement pattern type")]
        public CloneMovementPattern movementPattern = CloneMovementPattern.BackAndForth;

        [Tooltip("Movement speed in units per second")]
        public float moveSpeed = 3f;

        [Tooltip("Distance to move in each direction")]
        public float patrolDistance = 4f;

        [Tooltip("Pause duration at patrol endpoints (seconds)")]
        public float pauseDuration = 0.5f;

        [Header("▶ Lifetime Settings")]
        [Tooltip("Clone lifetime in beats (synced to rhythm)")]
        public int lifetimeBeats = 8;

        [Tooltip("Minimum distance to guards before despawning")]
        public float guardProximityThreshold = 1.5f;

        [Header("▶ Visual Settings")]
        [Tooltip("Clone transparency (0 = invisible, 1 = opaque)")]
        [Range(0f, 1f)]
        public float transparency = 0.6f;

        [Tooltip("Layer mask for guards")]
        public LayerMask guardMask;

        [Header("▶ VFX")]
        [Tooltip("VFX to play when clone spawns")]
        public GameObject spawnVFXPrefab;

        [Tooltip("VFX to play when clone despawns")]
        public GameObject despawnVFXPrefab;

        #region Private State
        private Vector2 _startPosition;
        private Vector2 _patrolPointA;
        private Vector2 _patrolPointB;
        private Vector2 _currentTarget;
        private bool _movingToPointB = true;
        private bool _isPaused = false;
        private float _despawnTime;
        private SpriteRenderer _spriteRenderer;
        private RhythmSyncManager _rhythmManager;
        private Collider2D[] _guardCheckResults = new Collider2D[10];
        #endregion

        public enum CloneMovementPattern
        {
            BackAndForth,
            Circular,
            Stationary
        }

        #region Initialization
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rhythmManager = GameServices.RhythmManager;
        }

        /// <summary>
        /// Initializes the clone at spawn position with specified direction
        /// 스폰 위치와 방향으로 분신 초기화
        /// </summary>
        public void Initialize(Vector2 spawnPosition, Vector2 facingDirection)
        {
            _startPosition = spawnPosition;
            transform.position = spawnPosition;

            // Setup patrol points based on facing direction
            SetupPatrolPoints(facingDirection);

            // Setup visuals
            SetupVisuals();

            // Calculate despawn time based on rhythm
            if (_rhythmManager != null)
            {
                float beatsToSeconds = lifetimeBeats * _rhythmManager.beatInterval;
                _despawnTime = Time.time + beatsToSeconds;
            }
            else
            {
                _despawnTime = Time.time + 10f; // Fallback: 10 seconds
            }

            // Spawn VFX
            if (spawnVFXPrefab != null)
            {
                SpawnVFX(spawnVFXPrefab, spawnPosition);
            }

            // Start movement
            StartCoroutine(PatrolRoutine());

            Debug.Log($"Clone initialized at {spawnPosition}, lifetime: {lifetimeBeats} beats");
        }

        private void SetupPatrolPoints(Vector2 facingDirection)
        {
            Vector2 normalizedDirection = facingDirection.normalized;

            switch (movementPattern)
            {
                case CloneMovementPattern.BackAndForth:
                    // Patrol back and forth along facing direction
                    _patrolPointA = _startPosition + normalizedDirection * patrolDistance;
                    _patrolPointB = _startPosition - normalizedDirection * patrolDistance;
                    _currentTarget = _patrolPointA;
                    break;

                case CloneMovementPattern.Circular:
                    // For circular, we'll use patrol points as reference
                    _patrolPointA = _startPosition + new Vector2(patrolDistance, 0f);
                    _patrolPointB = _startPosition + new Vector2(0f, patrolDistance);
                    _currentTarget = _patrolPointA;
                    break;

                case CloneMovementPattern.Stationary:
                    _patrolPointA = _startPosition;
                    _patrolPointB = _startPosition;
                    _currentTarget = _startPosition;
                    break;
            }
        }

        private void SetupVisuals()
        {
            if (_spriteRenderer != null)
            {
                // Make clone semi-transparent
                Color color = _spriteRenderer.color;
                color.a = transparency;
                _spriteRenderer.color = color;

                // Optional: Add slight color tint to distinguish from player
                _spriteRenderer.color = new Color(0.7f, 0.7f, 1f, transparency); // Slight blue tint
            }
        }

        private void SpawnVFX(GameObject vfxPrefab, Vector2 position)
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
        #endregion

        #region Update
        void Update()
        {
            // Check for lifetime expiration
            if (Time.time >= _despawnTime)
            {
                Despawn(false);
                return;
            }

            // Check guard proximity
            if (IsGuardTooClose())
            {
                Despawn(true); // Guard caught the clone
                return;
            }
        }

        private bool IsGuardTooClose()
        {
            if (guardMask == 0) return false;

            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                guardProximityThreshold,
                _guardCheckResults,
                guardMask
            );

            return hitCount > 0;
        }
        #endregion

        #region Movement
        private IEnumerator PatrolRoutine()
        {
            while (true)
            {
                if (movementPattern == CloneMovementPattern.Stationary)
                {
                    yield return null;
                    continue;
                }

                // Move towards current target
                yield return MoveToTarget(_currentTarget);

                // Pause at endpoint
                if (pauseDuration > 0f)
                {
                    _isPaused = true;
                    yield return new WaitForSeconds(pauseDuration);
                    _isPaused = false;
                }

                // Switch target
                SwitchPatrolTarget();
            }
        }

        private IEnumerator MoveToTarget(Vector2 target)
        {
            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                // Move towards target
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                // Update sprite facing direction
                UpdateFacingDirection(target);

                yield return null;
            }

            // Snap to exact position
            transform.position = target;
        }

        private void SwitchPatrolTarget()
        {
            switch (movementPattern)
            {
                case CloneMovementPattern.BackAndForth:
                    _movingToPointB = !_movingToPointB;
                    _currentTarget = _movingToPointB ? _patrolPointB : _patrolPointA;
                    break;

                case CloneMovementPattern.Circular:
                    // Rotate through circular points
                    float angle = Time.time * (moveSpeed / patrolDistance);
                    _currentTarget = _startPosition + new Vector2(
                        Mathf.Cos(angle) * patrolDistance,
                        Mathf.Sin(angle) * patrolDistance
                    );
                    break;
            }
        }

        private void UpdateFacingDirection(Vector2 target)
        {
            if (_spriteRenderer == null) return;

            // Flip sprite based on movement direction
            Vector2 direction = target - (Vector2)transform.position;
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                _spriteRenderer.flipX = direction.x < 0f;
            }
        }
        #endregion

        #region Despawn
        /// <summary>
        /// Despawns the clone
        /// 분신 제거
        /// </summary>
        /// <param name="caughtByGuard">Was the clone caught by a guard?</param>
        public void Despawn(bool caughtByGuard)
        {
            // Stop all coroutines
            StopAllCoroutines();

            // Despawn VFX
            if (despawnVFXPrefab != null)
            {
                SpawnVFX(despawnVFXPrefab, transform.position);
            }

            // Return to pool or destroy
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            Debug.Log($"Clone despawned (caught by guard: {caughtByGuard})");
        }

        /// <summary>
        /// Manually despawn the clone early
        /// 분신을 조기에 수동으로 제거
        /// </summary>
        public void DespawnEarly()
        {
            Despawn(false);
        }
        #endregion

        #region Debug
        void OnDrawGizmos()
        {
            // Draw patrol points
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_patrolPointA, 0.3f);
                Gizmos.DrawWireSphere(_patrolPointB, 0.3f);
                Gizmos.DrawLine(_patrolPointA, _patrolPointB);

                // Draw guard proximity threshold
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, guardProximityThreshold);
            }
            else
            {
                // Editor preview
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position + Vector3.right * patrolDistance, 0.3f);
                Gizmos.DrawWireSphere(transform.position + Vector3.left * patrolDistance, 0.3f);
            }
        }
        #endregion
    }
}
