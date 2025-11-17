using UnityEngine;
using System.Collections.Generic;

namespace SongOfTheStars.Procedural
{
    /// <summary>
    /// Procedurally generates guard patrol routes and positions
    /// 가드 순찰 경로와 위치를 프로시저럴하게 생성
    ///
    /// Attach to scene and call GeneratePatrols() to auto-create guards
    /// </summary>
    public class ProceduralGuardPatrolGenerator : MonoBehaviour
    {
        [Header("▶ Generation Settings")]
        [Tooltip("Number of guards to spawn")]
        [Range(1, 20)]
        public int guardCount = 8;

        [Tooltip("Minimum distance between guards")]
        public float minGuardSpacing = 5f;

        [Tooltip("Area bounds for guard placement")]
        public Bounds spawnBounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 1f));

        [Header("▶ Patrol Settings")]
        [Tooltip("Percentage of guards that patrol (vs stationary)")]
        [Range(0f, 1f)]
        public float patrolPercentage = 0.7f;

        [Tooltip("Min/max waypoints per patrol")]
        public Vector2Int waypointRange = new Vector2Int(2, 5);

        [Tooltip("Max patrol route length")]
        public float maxPatrolDistance = 15f;

        [Header("▶ Guard Prefab")]
        public GameObject guardPrefab;

        [Header("▶ Difficulty Scaling")]
        public bool scaleWithDifficulty = true;
        public int difficultyMultiplier = 1; // 1=Easy, 2=Normal, 3=Hard

        [Header("▶ Debug")]
        public bool showGizmos = true;
        public Color patrolLineColor = Color.yellow;
        public Color stationaryColor = Color.red;

        private List<GuardPatrolData> _generatedPatrols = new List<GuardPatrolData>();
        private List<GameObject> _spawnedGuards = new List<GameObject>();

        [ContextMenu("Generate Guard Patrols")]
        public void GeneratePatrols()
        {
            ClearExistingGuards();

            int actualGuardCount = scaleWithDifficulty
                ? guardCount * difficultyMultiplier
                : guardCount;

            _generatedPatrols.Clear();

            // Generate spawn positions with spacing
            List<Vector2> spawnPositions = GenerateSpawnPositions(actualGuardCount);

            // Create guards with patrol routes
            for (int i = 0; i < spawnPositions.Count; i++)
            {
                bool shouldPatrol = Random.value < patrolPercentage;

                GuardPatrolData patrolData = new GuardPatrolData
                {
                    guardID = $"Guard_{i:D3}",
                    spawnPosition = spawnPositions[i],
                    isPatrolling = shouldPatrol
                };

                if (shouldPatrol)
                {
                    patrolData.waypoints = GeneratePatrolRoute(spawnPositions[i]);
                    patrolData.patrolType = (PatrolType)Random.Range(0, 3);
                }
                else
                {
                    patrolData.waypoints = new List<Vector2> { spawnPositions[i] };
                    patrolData.patrolType = PatrolType.Stationary;
                }

                // Random facing direction for stationary guards
                patrolData.initialFacingDirection = Random.insideUnitCircle.normalized;

                // Vision settings based on difficulty
                patrolData.visionRange = Random.Range(6f, 10f) * (1f + difficultyMultiplier * 0.2f);
                patrolData.visionAngle = Random.Range(75f, 120f);

                _generatedPatrols.Add(patrolData);

                // Spawn guard if prefab exists
                if (guardPrefab != null)
                {
                    SpawnGuard(patrolData);
                }
            }

            Debug.Log($"✅ Generated {_generatedPatrols.Count} guard patrols " +
                      $"({Mathf.RoundToInt(patrolPercentage * 100)}% patrolling)");
        }

        [ContextMenu("Clear All Guards")]
        public void ClearExistingGuards()
        {
            foreach (GameObject guard in _spawnedGuards)
            {
                if (guard != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(guard);
#else
                    Destroy(guard);
#endif
                }
            }
            _spawnedGuards.Clear();
        }

        private List<Vector2> GenerateSpawnPositions(int count)
        {
            List<Vector2> positions = new List<Vector2>();
            int maxAttempts = count * 20; // Prevent infinite loops
            int attempts = 0;

            while (positions.Count < count && attempts < maxAttempts)
            {
                attempts++;

                // Random position within bounds
                Vector2 candidate = new Vector2(
                    Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                    Random.Range(spawnBounds.min.y, spawnBounds.max.y)
                );

                // Check minimum spacing
                bool validPosition = true;
                foreach (Vector2 existing in positions)
                {
                    if (Vector2.Distance(candidate, existing) < minGuardSpacing)
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    positions.Add(candidate);
                }
            }

            if (positions.Count < count)
            {
                Debug.LogWarning($"Could only generate {positions.Count}/{count} positions. " +
                                 "Try increasing spawn bounds or reducing min spacing.");
            }

            return positions;
        }

        private List<Vector2> GeneratePatrolRoute(Vector2 startPosition)
        {
            List<Vector2> waypoints = new List<Vector2> { startPosition };

            int waypointCount = Random.Range(waypointRange.x, waypointRange.y + 1);
            Vector2 currentPos = startPosition;

            for (int i = 1; i < waypointCount; i++)
            {
                // Generate next waypoint within max distance
                Vector2 randomOffset = Random.insideUnitCircle * Random.Range(3f, maxPatrolDistance);
                Vector2 nextPos = currentPos + randomOffset;

                // Clamp to spawn bounds
                nextPos.x = Mathf.Clamp(nextPos.x, spawnBounds.min.x, spawnBounds.max.x);
                nextPos.y = Mathf.Clamp(nextPos.y, spawnBounds.min.y, spawnBounds.max.y);

                waypoints.Add(nextPos);
                currentPos = nextPos;
            }

            return waypoints;
        }

        private void SpawnGuard(GuardPatrolData data)
        {
            GameObject guard = Instantiate(guardPrefab, data.spawnPosition, Quaternion.identity, transform);
            guard.name = data.guardID;

            // Configure guard component (if exists)
            var guardComponent = guard.GetComponent<GuardRhythmPatrol>();
            if (guardComponent != null)
            {
                // Set waypoints
                // guardComponent.waypoints = data.waypoints; // Uncomment if GuardRhythmPatrol has waypoints field

                // Set vision settings
                // guardComponent.viewDistance = data.visionRange;
                // guardComponent.viewAngle = data.visionAngle;
            }

            _spawnedGuards.Add(guard);
        }

        public List<GuardPatrolData> GetGeneratedPatrols()
        {
            return new List<GuardPatrolData>(_generatedPatrols);
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw spawn bounds
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);

            // Draw patrols
            foreach (GuardPatrolData patrol in _generatedPatrols)
            {
                if (patrol.isPatrolling)
                {
                    Gizmos.color = patrolLineColor;

                    // Draw patrol route
                    for (int i = 0; i < patrol.waypoints.Count - 1; i++)
                    {
                        Gizmos.DrawLine(patrol.waypoints[i], patrol.waypoints[i + 1]);
                    }

                    // Draw loop back to start
                    if (patrol.waypoints.Count > 1)
                    {
                        Gizmos.DrawLine(patrol.waypoints[patrol.waypoints.Count - 1], patrol.waypoints[0]);
                    }

                    // Draw waypoint markers
                    foreach (Vector2 waypoint in patrol.waypoints)
                    {
                        Gizmos.DrawWireSphere(waypoint, 0.3f);
                    }
                }
                else
                {
                    // Stationary guard
                    Gizmos.color = stationaryColor;
                    Gizmos.DrawWireSphere(patrol.spawnPosition, 0.5f);

                    // Draw facing direction
                    Vector2 facingEnd = patrol.spawnPosition + patrol.initialFacingDirection * 1.5f;
                    Gizmos.DrawLine(patrol.spawnPosition, facingEnd);
                }
            }
        }
    }

    [System.Serializable]
    public class GuardPatrolData
    {
        public string guardID;
        public Vector2 spawnPosition;
        public bool isPatrolling;
        public List<Vector2> waypoints = new List<Vector2>();
        public PatrolType patrolType;
        public Vector2 initialFacingDirection;
        public float visionRange;
        public float visionAngle;
    }

    public enum PatrolType
    {
        Stationary,
        Loop,           // A → B → C → A
        PingPong,       // A → B → C → B → A
        Random          // Random waypoint each time
    }
}
