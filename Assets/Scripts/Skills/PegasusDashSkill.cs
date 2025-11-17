using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Skills
{
    /// <summary>
    /// Pegasus Dash skill - Instant teleport dash
    /// Pegasus Dash 스킬 - 순간 텔레포트 대시
    ///
    /// Skill Details:
    /// - Category: Movement
    /// - Cooldown: 10 beats
    /// - Focus Cost: 20
    /// - Input Pattern: 1-2 (two beats)
    /// - Range: 5 meters
    ///
    /// Effect: Instantly teleports player in facing direction.
    /// Can pass through thin obstacles. Used for quick escapes and shortcuts.
    /// </summary>
    public class PegasusDashSkill : MonoBehaviour
    {
        [Header("▶ Dash Settings")]
        [Tooltip("Maximum dash distance in units")]
        public float dashDistance = 5f;

        [Tooltip("Can dash pass through obstacles?")]
        public bool canPassThroughObstacles = true;

        [Tooltip("Maximum obstacle thickness that can be passed through")]
        public float maxObstacleThickness = 1f;

        [Header("▶ Collision Settings")]
        [Tooltip("Layer mask for solid obstacles (cannot teleport into)")]
        public LayerMask solidObstacleMask;

        [Tooltip("Radius for destination collision check")]
        public float destinationCheckRadius = 0.5f;

        [Header("▶ VFX & SFX")]
        [Tooltip("VFX at starting position")]
        public GameObject startVFXPrefab;

        [Tooltip("VFX at destination position")]
        public GameObject endVFXPrefab;

        [Tooltip("Trail VFX between start and end")]
        public GameObject trailVFXPrefab;

        [Tooltip("Sound effect when dashing")]
        public AudioClip dashSFX;

        [Tooltip("Sound effect when dash fails (blocked)")]
        public AudioClip blockedSFX;

        [Header("▶ Camera")]
        [Tooltip("Should camera follow dash instantly?")]
        public bool instantCameraFollow = true;

        #region Private State
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private PlayerController _playerController;
        private Collider2D _playerCollider;
        #endregion

        #region Initialization
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _playerController = GetComponent<PlayerController>();
            _playerCollider = GetComponent<Collider2D>();
        }
        #endregion

        #region Skill Activation
        /// <summary>
        /// Activates Pegasus Dash
        /// Pegasus Dash 활성화
        /// </summary>
        public void ActivateSkill()
        {
            // Get dash direction
            Vector2 dashDirection = GetDashDirection();

            // Calculate destination
            Vector2 startPosition = transform.position;
            Vector2 targetPosition = startPosition + dashDirection * dashDistance;

            // Find valid destination (check for obstacles)
            Vector2 validDestination = FindValidDestination(startPosition, targetPosition, dashDirection);

            // Check if we can dash to destination
            if (Vector2.Distance(startPosition, validDestination) < 0.1f)
            {
                // Dash blocked or invalid
                OnDashBlocked(startPosition);
                return;
            }

            // Execute dash
            ExecuteDash(startPosition, validDestination);
        }

        private Vector2 GetDashDirection()
        {
            // Get facing direction from sprite renderer
            if (_spriteRenderer != null)
            {
                return _spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }

            // Fallback: right
            return Vector2.right;
        }

        private Vector2 FindValidDestination(Vector2 start, Vector2 target, Vector2 direction)
        {
            // Raycast to find obstacles
            RaycastHit2D hit = Physics2D.Raycast(
                start,
                direction,
                dashDistance,
                solidObstacleMask
            );

            if (hit.collider != null)
            {
                if (canPassThroughObstacles)
                {
                    // Check if obstacle is thin enough to pass through
                    float obstacleThickness = GetObstacleThickness(hit, direction);

                    if (obstacleThickness <= maxObstacleThickness)
                    {
                        // Can pass through - use full target position
                        return target;
                    }
                    else
                    {
                        // Too thick - stop before obstacle
                        return hit.point - direction * destinationCheckRadius;
                    }
                }
                else
                {
                    // Cannot pass through - stop before obstacle
                    return hit.point - direction * destinationCheckRadius;
                }
            }

            // No obstacle - check if destination is valid (not inside solid object)
            if (IsDestinationValid(target))
            {
                return target;
            }
            else
            {
                // Destination blocked - return start position (dash fails)
                return start;
            }
        }

        private float GetObstacleThickness(RaycastHit2D hit, Vector2 direction)
        {
            // Cast ray from hit point in same direction to find exit point
            Vector2 entryPoint = hit.point;
            RaycastHit2D exitHit = Physics2D.Raycast(
                entryPoint + direction * 0.1f, // Offset slightly to avoid same collider
                direction,
                maxObstacleThickness * 2f,
                solidObstacleMask
            );

            if (exitHit.collider != null && exitHit.collider == hit.collider)
            {
                // Found exit point on same collider
                return Vector2.Distance(entryPoint, exitHit.point);
            }

            // Assume thin obstacle or complex geometry
            return 0.5f;
        }

        private bool IsDestinationValid(Vector2 destination)
        {
            // Check if destination overlaps with solid obstacles
            Collider2D overlap = Physics2D.OverlapCircle(
                destination,
                destinationCheckRadius,
                solidObstacleMask
            );

            return overlap == null;
        }

        private void ExecuteDash(Vector2 startPosition, Vector2 endPosition)
        {
            // Play start VFX
            if (startVFXPrefab != null)
            {
                PlayVFX(startVFXPrefab, startPosition);
            }

            // Spawn trail VFX
            if (trailVFXPrefab != null)
            {
                SpawnTrailVFX(startPosition, endPosition);
            }

            // Teleport player
            transform.position = endPosition;

            // Update rigidbody if present
            if (_rigidbody != null)
            {
                _rigidbody.position = endPosition;
                _rigidbody.velocity = Vector2.zero; // Stop any momentum
            }

            // Play end VFX
            if (endVFXPrefab != null)
            {
                PlayVFX(endVFXPrefab, endPosition);
            }

            // Play dash SFX
            if (dashSFX != null)
            {
                PlaySFX(dashSFX);
            }

            // Camera follow (if enabled)
            if (instantCameraFollow)
            {
                // TODO: Force camera to follow player instantly
                // This would integrate with your camera controller
            }

            Debug.Log($"Pegasus Dash: {startPosition} → {endPosition} (distance: {Vector2.Distance(startPosition, endPosition):F2}m)");
        }

        private void OnDashBlocked(Vector2 position)
        {
            // Play blocked SFX
            if (blockedSFX != null)
            {
                PlaySFX(blockedSFX);
            }

            // Play blocked VFX at current position
            if (startVFXPrefab != null)
            {
                PlayVFX(startVFXPrefab, position);
            }

            Debug.Log("Pegasus Dash blocked - destination invalid");
        }

        private void SpawnTrailVFX(Vector2 start, Vector2 end)
        {
            if (trailVFXPrefab == null) return;

            // Calculate trail position and rotation
            Vector2 midPoint = (start + end) / 2f;
            Vector2 direction = (end - start).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Spawn trail
            GameObject trail = Instantiate(trailVFXPrefab, midPoint, Quaternion.Euler(0f, 0f, angle));

            // Scale trail based on distance
            float distance = Vector2.Distance(start, end);
            trail.transform.localScale = new Vector3(distance, 1f, 1f);

            // Auto-destroy after duration
            Destroy(trail, 1f);
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

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Test Pegasus Dash")]
        private void TestDash()
        {
            if (Application.isPlaying)
            {
                ActivateSkill();
            }
        }
        #endif

        void OnDrawGizmos()
        {
            // Draw dash range preview
            Vector2 dashDirection = Vector2.right;
            if (_spriteRenderer != null && Application.isPlaying)
            {
                dashDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }

            Vector2 startPos = transform.position;
            Vector2 endPos = startPos + dashDirection * dashDistance;

            // Draw dash line
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, endPos);

            // Draw destination circle
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(endPos, destinationCheckRadius);

            // Draw obstacle pass-through indicator
            if (canPassThroughObstacles)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(startPos + dashDirection * (dashDistance * 0.5f), 0.3f);
            }
        }
        #endregion
    }
}
