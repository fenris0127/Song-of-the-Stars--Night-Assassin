using UnityEngine;

public class PlayerAssassination : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationRange = 1.5f; 
    public float maxRange = 15f;
    public LayerMask guardMask;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    private Collider2D[] _assassinationResults = new Collider2D[10];


    /// <summary>
    /// 근접 암살 범위 내의 경비병을 찾습니다 (2D)
    /// </summary>
    public GuardRhythmPatrol FindGuardInAssassinationRange()
    {
        // OverlapCircleNonAlloc이 deprecated되어 OverlapCircleAll로 대체
    int hitCount = GameServices.OverlapCircleCompat(transform.position, assassinationRange, guardMask, _assassinationResults);

        if (hitCount > 0)
        {
            GuardRhythmPatrol closestGuard = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                GuardRhythmPatrol guard = _assassinationResults[i].GetComponent<GuardRhythmPatrol>();
                if (guard != null)
                {
                    float sqrDist = (_assassinationResults[i].transform.position - transform.position).sqrMagnitude;
                    if (sqrDist < minSqrDistance)
                    {
                        minSqrDistance = sqrDist;
                        closestGuard = guard;
                    }
                }
            }

            return closestGuard;
        }

        return null;
    }

    /// <summary>
    /// 원거리 암살 범위 내의 경비병을 찾습니다 (2D Raycast)
    /// </summary>
    public GuardRhythmPatrol FindGuardInRangedRange()
    {
        if (RhythmManager == null) return null;

        Vector2 playerPos = transform.position;
        Vector2 playerForward = transform.up;

        RaycastHit2D hit;
        if (GameServices.RaycastCompat(playerPos, playerForward, out hit, maxRange, guardMask) && hit.collider != null)
        {
            // Use HasLineOfSight to check for obstacles between us and the guard
            if (GameServices.HasLineOfSight(playerPos, hit.point, RhythmManager.obstacleMask))
                return hit.collider.GetComponent<GuardRhythmPatrol>();
        }

        return null;
    }
    
    /// <summary>
    /// 근접 암살 실행
    /// </summary>
    public void ExecuteAssassinationStrike(GuardRhythmPatrol target)
    {
        if (target != null)
        {
            Debug.Log($"근접 암살 성공: {target.gameObject.name}");
            target.Die();
        }
    }

    /// <summary>
    /// 원거리 암살 실행 (투척 무기 등)
    /// </summary>
    public void ExecuteRangedAssassination(GuardRhythmPatrol target)
    {
        if (target != null)
        {
            Debug.Log($"원거리 암살 성공: {target.gameObject.name}");
            target.Die();
        }
    }
}