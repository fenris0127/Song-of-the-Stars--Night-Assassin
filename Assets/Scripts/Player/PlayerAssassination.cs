using UnityEngine;

public class PlayerAssassination : MonoBehaviour
{
    [Header("Assassination Settings")]
    public float assassinationRange = 1.5f; 
    public float maxRange = 15f;
    public LayerMask guardMask;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    private Collider2D[] _assassinationResults = new Collider2D[10];

    // 근접 암살 범위 내의 경비병을 찾습니다 (2D)
    public GuardRhythmPatrol FindGuardInAssassinationRange()
    {
        // OverlapCircleNonAlloc이 deprecated되어 OverlapCircleAll로 대체
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position, 
            assassinationRange, 
            _assassinationResults, 
            guardMask
        );

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

    /// 원거리 암살 범위 내의 경비병을 찾습니다 (2D Raycast)
    public GuardRhythmPatrol FindGuardInRangedRange()
    {
        if (RhythmManager == null) return null;

        // Vector3 -> Vector2 암시적 변환
        Vector2 playerPos = transform.position;
        Vector2 playerForward = transform.up;

        // ⭐ 수정: GameServices.RaycastCompat 대신 Physics2D.Raycast 직접 사용
        RaycastHit2D hit = Physics2D.Raycast(playerPos, playerForward, maxRange, guardMask | RhythmManager.obstacleMask);

        if (hit.collider != null)
        {
            GuardRhythmPatrol guard = hit.collider.GetComponent<GuardRhythmPatrol>();

            // 1. Raycast가 경비병을 먼저 맞췄는지 확인
            if (guard != null)
                return guard;

            // 2. 경비병 마스크나 장애물 마스크에 해당하는 무언가를 맞췄다면
            // 이 로직은 복잡한 HasLineOfSight 호출 없이도 작동합니다.
            // Physics2D.Raycast를 경비병과 장애물 마스크를 모두 포함하여 호출했으므로,
            // 만약 경비병이 아닌 장애물(ObstacleMask)이 먼저 감지되었다면 guard는 null일 것입니다.
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