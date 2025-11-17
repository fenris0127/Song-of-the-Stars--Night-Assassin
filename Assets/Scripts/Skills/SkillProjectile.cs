using UnityEngine;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Generic projectile for skill-based ranged attacks
    /// 스킬 기반 원거리 공격용 범용 투사체
    /// </summary>
    public class SkillProjectile : MonoBehaviour
    {
        [Header("▶ Projectile Settings")]
        [Tooltip("Speed of projectile in units per second")]
        public float speed = 30f;

        [Tooltip("Maximum travel distance before self-destruct")]
        public float maxDistance = 15f;

        [Tooltip("Does this projectile pierce through targets?")]
        public bool pierce = false;

        [Header("▶ Effects")]
        [Tooltip("VFX to spawn on impact")]
        public GameObject impactVFXPrefab;

        [Tooltip("VFX to spawn on projectile destroy")]
        public GameObject destroyVFXPrefab;

        [Header("▶ Damage")]
        [Tooltip("Damage dealt to targets")]
        public float damage = 100f; // Instant kill for guards

        [Tooltip("Layer mask for obstacles that block projectile")]
        public LayerMask obstacleMask;

        [Tooltip("Layer mask for valid targets")]
        public LayerMask targetMask;

        #region Private State
        private Vector2 _direction;
        private Vector3 _startPosition;
        private float _traveledDistance;
        private bool _hasHitTarget;
        private Rigidbody2D _rigidbody;
        #endregion

        #region Initialization
        /// <summary>
        /// Launches the projectile in specified direction
        /// 지정된 방향으로 투사체 발사
        /// </summary>
        public void Launch(Vector2 direction)
        {
            _direction = direction.normalized;
            _startPosition = transform.position;
            _traveledDistance = 0f;
            _hasHitTarget = false;

            // Setup rigidbody for physics
            _rigidbody = GetComponent<Rigidbody2D>();
            if (_rigidbody != null)
            {
                _rigidbody.velocity = _direction * speed;
            }

            // Rotate to face direction
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
        #endregion

        #region Update
        void Update()
        {
            // Manual movement if no rigidbody
            if (_rigidbody == null)
            {
                float moveDistance = speed * Time.deltaTime;
                transform.Translate(Vector3.right * moveDistance, Space.Self);
            }

            // Track distance traveled
            _traveledDistance = Vector3.Distance(_startPosition, transform.position);

            // Destroy if exceeded max distance
            if (_traveledDistance >= maxDistance)
            {
                DestroyProjectile(false);
            }
        }

        void FixedUpdate()
        {
            // Raycast ahead to detect hits
            if (_rigidbody != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    _direction,
                    speed * Time.fixedDeltaTime,
                    obstacleMask | targetMask
                );

                if (hit.collider != null)
                {
                    HandleHit(hit);
                }
            }
        }
        #endregion

        #region Collision Handling
        void OnTriggerEnter2D(Collider2D other)
        {
            // Backup collision detection via triggers
            if (_hasHitTarget && !pierce) return;

            // Check if target
            if (((1 << other.gameObject.layer) & targetMask) != 0)
            {
                RaycastHit2D hit = new RaycastHit2D
                {
                    collider = other,
                    point = transform.position,
                    normal = -_direction
                };
                HandleHit(hit);
            }
            // Check if obstacle
            else if (((1 << other.gameObject.layer) & obstacleMask) != 0)
            {
                DestroyProjectile(false);
            }
        }

        private void HandleHit(RaycastHit2D hit)
        {
            // Check if hit a target
            bool isTarget = ((1 << hit.collider.gameObject.layer) & targetMask) != 0;
            bool isObstacle = ((1 << hit.collider.gameObject.layer) & obstacleMask) != 0;

            if (isTarget)
            {
                // Deal damage to target
                ApplyDamageToTarget(hit.collider.gameObject);

                // Spawn impact VFX
                if (impactVFXPrefab != null)
                {
                    SpawnImpactVFX(hit.point);
                }

                // Mark as hit
                _hasHitTarget = true;

                // Destroy if not piercing
                if (!pierce)
                {
                    DestroyProjectile(true);
                }
            }
            else if (isObstacle)
            {
                // Hit obstacle, destroy
                DestroyProjectile(false);
            }
        }

        private void ApplyDamageToTarget(GameObject target)
        {
            // Check for guard
            GuardRhythmPatrol guard = target.GetComponent<GuardRhythmPatrol>();
            if (guard != null)
            {
                // Instant elimination (damage >= guard health)
                guard.TakeDamage(damage);
                Debug.Log($"SkillProjectile hit guard: {target.name}");
                return;
            }

            // Check for other damageable components
            // (Future: IDamageable interface)
        }

        private void SpawnImpactVFX(Vector2 position)
        {
            if (impactVFXPrefab == null) return;

            // Use VFXManager if available
            VFXManager vfxManager = FindObjectOfType<VFXManager>();
            if (vfxManager != null)
            {
                vfxManager.PlayVFXAt(impactVFXPrefab, position);
            }
            else
            {
                Instantiate(impactVFXPrefab, position, Quaternion.identity);
            }
        }
        #endregion

        #region Destruction
        private void DestroyProjectile(bool hitTarget)
        {
            // Spawn destroy VFX if not hit target (obstacle hit or max distance)
            if (!hitTarget && destroyVFXPrefab != null)
            {
                SpawnImpactVFX(transform.position);
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
        }
        #endregion

        #region Debug
        void OnDrawGizmos()
        {
            // Draw max distance circle in editor
            Gizmos.color = Color.yellow;
            if (Application.isPlaying && _startPosition != Vector3.zero)
            {
                Gizmos.DrawWireSphere(_startPosition, maxDistance);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, maxDistance);
            }

            // Draw direction arrow
            if (Application.isPlaying && _direction != Vector2.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)_direction * 2f);
            }
        }
        #endregion
    }
}
