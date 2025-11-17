using UnityEngine;

namespace SongOfTheStars.Utilities
{
    /// <summary>
    /// Shared utility for vision and line-of-sight calculations
    /// 시야 및 가시선 계산을 위한 공유 유틸리티
    ///
    /// Consolidates duplicate vision code across GuardState, ProbabilisticDetection, etc.
    /// </summary>
    public static class VisionUtility
    {
        /// <summary>
        /// Checks if target is within field of view and line of sight
        /// 타겟이 시야각과 가시선 내에 있는지 확인
        /// </summary>
        /// <param name="observer">The observing transform</param>
        /// <param name="targetPosition">Position to check</param>
        /// <param name="viewDistance">Maximum view distance</param>
        /// <param name="viewAngle">Field of view angle in degrees</param>
        /// <param name="obstacleMask">LayerMask for obstacles that block vision</param>
        /// <returns>True if target is visible</returns>
        public static bool IsInFieldOfView(
            Transform observer,
            Vector2 targetPosition,
            float viewDistance,
            float viewAngle,
            LayerMask obstacleMask)
        {
            if (observer == null)
            {
                Debug.LogWarning("VisionUtility: Observer transform is null");
                return false;
            }

            Vector2 observerPos = observer.position;
            Vector2 directionToTarget = targetPosition - observerPos;
            float distanceToTarget = directionToTarget.magnitude;

            // Check distance first (cheapest check)
            if (distanceToTarget > viewDistance)
                return false;

            // Check angle
            float angleToTarget = Vector2.Angle(observer.right, directionToTarget);
            if (angleToTarget > viewAngle / 2f)
                return false;

            // Check line of sight (most expensive check, do last)
            return IsLineOfSightClear(observerPos, targetPosition, distanceToTarget, obstacleMask);
        }

        /// <summary>
        /// Checks if there are obstacles between two points
        /// 두 지점 사이에 장애물이 있는지 확인
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="distance">Distance between points (pre-calculated for optimization)</param>
        /// <param name="obstacleMask">LayerMask for obstacles</param>
        /// <returns>True if line of sight is clear</returns>
        public static bool IsLineOfSightClear(
            Vector2 from,
            Vector2 to,
            float distance,
            LayerMask obstacleMask)
        {
            Vector2 direction = (to - from).normalized;
            RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, obstacleMask);

            // No hit means clear line of sight
            return hit.collider == null;
        }

        /// <summary>
        /// Overload that calculates distance automatically
        /// </summary>
        public static bool IsLineOfSightClear(
            Vector2 from,
            Vector2 to,
            LayerMask obstacleMask)
        {
            float distance = Vector2.Distance(from, to);
            return IsLineOfSightClear(from, to, distance, obstacleMask);
        }

        /// <summary>
        /// Gets the angle from observer to target
        /// 관찰자에서 타겟까지의 각도를 가져옵니다
        /// </summary>
        /// <param name="observer">Observer transform</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Angle in degrees</returns>
        public static float GetAngleToTarget(Transform observer, Vector2 targetPosition)
        {
            if (observer == null) return 0f;

            Vector2 observerPos = observer.position;
            Vector2 directionToTarget = targetPosition - observerPos;
            return Vector2.Angle(observer.right, directionToTarget);
        }

        /// <summary>
        /// Checks if target is within a cone-shaped vision area
        /// 타겟이 원뿔 모양의 시야 영역 내에 있는지 확인
        /// </summary>
        /// <param name="observer">Observer transform</param>
        /// <param name="targetPosition">Position to check</param>
        /// <param name="viewDistance">Maximum view distance</param>
        /// <param name="viewAngle">Field of view angle in degrees</param>
        /// <returns>True if within vision cone (ignores obstacles)</returns>
        public static bool IsWithinVisionCone(
            Transform observer,
            Vector2 targetPosition,
            float viewDistance,
            float viewAngle)
        {
            if (observer == null) return false;

            Vector2 observerPos = observer.position;
            Vector2 directionToTarget = targetPosition - observerPos;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget > viewDistance)
                return false;

            float angleToTarget = Vector2.Angle(observer.right, directionToTarget);
            return angleToTarget <= viewAngle / 2f;
        }

        /// <summary>
        /// Debug visualization of vision cone
        /// 시야각 디버그 시각화
        /// </summary>
        /// <param name="observer">Observer transform</param>
        /// <param name="viewDistance">View distance</param>
        /// <param name="viewAngle">Field of view angle</param>
        /// <param name="color">Gizmo color</param>
        /// <param name="segments">Number of segments for cone drawing (default: 20)</param>
        public static void DrawVisionConeGizmo(
            Transform observer,
            float viewDistance,
            float viewAngle,
            Color color,
            int segments = 20)
        {
            if (observer == null) return;

            Gizmos.color = color;
            Vector3 forward = observer.right;
            Vector3 position = observer.position;

            // Draw view distance circle arc
            float halfAngle = viewAngle / 2f;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward * viewDistance;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward * viewDistance;

            // Draw boundary lines
            Gizmos.DrawLine(position, position + leftBoundary);
            Gizmos.DrawLine(position, position + rightBoundary);

            // Draw arc
            Vector3 previousPoint = position + leftBoundary;
            for (int i = 1; i <= segments; i++)
            {
                float angle = -halfAngle + (viewAngle * i / segments);
                Vector3 direction = Quaternion.Euler(0, 0, angle) * forward;
                Vector3 point = position + direction * viewDistance;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }

        /// <summary>
        /// Calculates visibility factor based on distance (0 to 1)
        /// 거리 기반 가시성 계산 (0~1)
        /// </summary>
        /// <param name="distance">Current distance to target</param>
        /// <param name="maxDistance">Maximum detection distance</param>
        /// <returns>Visibility factor (1 = very close, 0 = at max distance)</returns>
        public static float GetDistanceVisibilityFactor(float distance, float maxDistance)
        {
            if (distance >= maxDistance) return 0f;
            if (distance <= 0f) return 1f;

            // Linear falloff
            return 1f - (distance / maxDistance);
        }

        /// <summary>
        /// Calculates visibility factor based on angle (0 to 1)
        /// 각도 기반 가시성 계산 (0~1)
        /// </summary>
        /// <param name="angle">Angle to target in degrees</param>
        /// <param name="maxAngle">Maximum field of view angle</param>
        /// <returns>Visibility factor (1 = center of vision, 0 = edge)</returns>
        public static float GetAngleVisibilityFactor(float angle, float maxAngle)
        {
            float halfAngle = maxAngle / 2f;
            if (angle >= halfAngle) return 0f;
            if (angle <= 0f) return 1f;

            // Linear falloff from center
            return 1f - (angle / halfAngle);
        }
    }
}
