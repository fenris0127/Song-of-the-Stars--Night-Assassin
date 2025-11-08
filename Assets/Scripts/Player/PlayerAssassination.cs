using UnityEngine;

public class PlayerAssassination : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationRange = 1.5f; 
    public float maxRange = 15f;
    public LayerMask guardMask;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    // private RhythmSyncManager _rhythmManager;

    private Collider2D[] _assassinationResults = new Collider2D[10];


    // void Start()
    // {
    //     _rhythmManager = FindObjectOfType<RhythmSyncManager>();
    // }

    /// <summary>
    /// 근접 암살 범위 내의 경비병을 찾습니다 (2D)
    /// </summary>
    public GuardRhythmPatrol FindGuardInAssassinationRange()
    {
        // ⭐ 최적화: OverlapCircleNonAlloc 사용
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, assassinationRange, _assassinationResults, guardMask);

        if (hitCount > 0)
        {
            GuardRhythmPatrol closestGuard = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                GuardRhythmPatrol guard = _assassinationResults[i].GetComponent<GuardRhythmPatrol>();
                if (guard != null)
                {
                    // ⭐ sqrMagnitude 사용
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

        RaycastHit2D hit = Physics2D.Raycast(playerPos, playerForward, maxRange, guardMask);

        if (hit.collider != null)
        {
            RaycastHit2D obstacleCheck = Physics2D.Raycast(playerPos, playerForward, hit.distance, RhythmManager.obstacleMask);

            if (obstacleCheck.collider == null)
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