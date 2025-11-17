using UnityEngine;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Orion's Arrow - Ranged assassination skill
    /// 오리온의 화살 - 원거리 암살 스킬
    ///
    /// Fires a star projectile that eliminates a single guard at range
    /// </summary>
    public class OrionsArrowSkill : MonoBehaviour
    {
        [Header("▶ Projectile Settings")]
        [Tooltip("Projectile prefab to spawn")]
        public GameObject projectilePrefab;

        [Tooltip("Spawn offset from player position")]
        public Vector2 spawnOffset = new Vector2(0.5f, 0f);

        [Header("▶ Targeting")]
        [Tooltip("Auto-target nearest guard in range")]
        public bool autoTarget = true;

        [Tooltip("Maximum auto-target range")]
        public float autoTargetRange = 15f;

        [Tooltip("Layer mask for valid targets (guards)")]
        public LayerMask targetMask;

        [Tooltip("Layer mask for obstacles")]
        public LayerMask obstacleMask;

        [Header("▶ Audio")]
        [Tooltip("Sound effect on arrow fire")]
        public AudioClip fireSFX;

        #region Private State
        private PlayerController _player;
        private Transform _playerTransform;
        #endregion

        #region Initialization
        void Awake()
        {
            _player = GetComponent<PlayerController>();
            _playerTransform = transform;
        }
        #endregion

        #region Skill Execution
        /// <summary>
        /// Activates Orion's Arrow skill
        /// 오리온의 화살 스킬 활성화
        /// </summary>
        public void ActivateSkill()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("OrionsArrowSkill: Projectile prefab not assigned!");
                return;
            }

            // Determine target direction
            Vector2 targetDirection;
            if (autoTarget)
            {
                GameObject target = FindNearestGuard();
                if (target != null)
                {
                    // Fire towards target
                    targetDirection = (target.transform.position - _playerTransform.position).normalized;
                }
                else
                {
                    // No target found, fire in facing direction
                    targetDirection = GetPlayerFacingDirection();
                    Debug.Log("OrionsArrow: No target found, firing in facing direction");
                }
            }
            else
            {
                // Manual aiming (use mouse or facing direction)
                targetDirection = GetPlayerFacingDirection();
            }

            // Spawn and launch projectile
            FireProjectile(targetDirection);

            // Play SFX
            if (fireSFX != null && GameServices.SFX != null)
            {
                GameServices.SFX.PlaySFX(fireSFX);
            }

            Debug.Log($"OrionsArrow fired in direction: {targetDirection}");
        }

        /// <summary>
        /// Finds the nearest guard within range and line of sight
        /// </summary>
        private GameObject FindNearestGuard()
        {
            Collider2D[] guards = Physics2D.OverlapCircleAll(
                _playerTransform.position,
                autoTargetRange,
                targetMask
            );

            if (guards.Length == 0) return null;

            GameObject nearestGuard = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D guardCollider in guards)
            {
                Vector2 directionToGuard = guardCollider.transform.position - _playerTransform.position;
                float distance = directionToGuard.magnitude;

                // Check line of sight
                RaycastHit2D hit = Physics2D.Raycast(
                    _playerTransform.position,
                    directionToGuard.normalized,
                    distance,
                    obstacleMask
                );

                // If no obstacle hit, guard is visible
                if (hit.collider == null)
                {
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestGuard = guardCollider.gameObject;
                    }
                }
            }

            return nearestGuard;
        }

        /// <summary>
        /// Gets the player's current facing direction
        /// </summary>
        private Vector2 GetPlayerFacingDirection()
        {
            // Use player's last movement direction or default to right
            if (_player != null)
            {
                // Try to get last movement direction from InputManager
                InputManager inputManager = GameServices.Input;
                if (inputManager != null && inputManager.CurrentMovementInput != Vector2.zero)
                {
                    return inputManager.CurrentMovementInput.normalized;
                }
            }

            // Default: right direction
            return Vector2.right;
        }

        /// <summary>
        /// Spawns and fires the projectile
        /// </summary>
        private void FireProjectile(Vector2 direction)
        {
            // Calculate spawn position (slightly in front of player)
            Vector2 spawnPosition = (Vector2)_playerTransform.position + spawnOffset;

            // Instantiate projectile
            GameObject projectileObj;
            if (ObjectPoolManager.Instance != null)
            {
                projectileObj = ObjectPoolManager.Instance.GetPooledObject(projectilePrefab);
                if (projectileObj != null)
                {
                    projectileObj.transform.position = spawnPosition;
                    projectileObj.SetActive(true);
                }
                else
                {
                    projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                }
            }
            else
            {
                projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            }

            // Launch projectile
            SkillProjectile projectile = projectileObj.GetComponent<SkillProjectile>();
            if (projectile != null)
            {
                // Setup projectile masks if not already set
                if (projectile.targetMask == 0)
                    projectile.targetMask = targetMask;
                if (projectile.obstacleMask == 0)
                    projectile.obstacleMask = obstacleMask;

                projectile.Launch(direction);
            }
            else
            {
                Debug.LogError("OrionsArrowSkill: Projectile prefab missing SkillProjectile component!");
            }
        }
        #endregion

        #region Debug
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw auto-target range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, autoTargetRange);

            // Draw line to nearest guard
            if (autoTarget)
            {
                GameObject nearest = FindNearestGuard();
                if (nearest != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, nearest.transform.position);
                }
            }
        }
        #endregion
    }
}
