using UnityEngine;

public class PlayerAssassination : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationRange = 1.5f; 
    public float maxRange = 15f;
    public LayerMask guardMask;

    private RhythmSyncManager _rhythmManager;
    
    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
    }

    /// <summary>
    /// 근접 암살 범위 내의 경비병을 찾습니다 (2D)
    /// </summary>
    public GuardRhythmPatrol FindGuardInAssassinationRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, assassinationRange, guardMask);

        if (hits.Length > 0)
        {
            // 가장 가까운 경비병 찾기
            GuardRhythmPatrol closestGuard = null;
            float minDistance = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                GuardRhythmPatrol guard = hit.GetComponent<GuardRhythmPatrol>();
                if (guard != null)
                {
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
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
        if (_rhythmManager == null) return null;

        Vector2 playerPos = transform.position;
        Vector2 playerForward = transform.up; // 플레이어가 바라보는 방향

        // Raycast로 플레이어 정면의 경비병 탐지
        RaycastHit2D hit = Physics2D.Raycast(playerPos, playerForward, maxRange, guardMask);

        if (hit.collider != null)
        {
            // 장애물에 가로막히지 않았는지 확인
            RaycastHit2D obstacleCheck = Physics2D.Raycast(playerPos, playerForward, hit.distance, _rhythmManager.obstacleMask);

            if (obstacleCheck.collider == null)
            {
                return hit.collider.GetComponent<GuardRhythmPatrol>();
            }
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