using UnityEngine;

/// <summary>
/// 경비병의 상태를 나타내는 추상 클래스입니다.
/// 각 상태는 이 클래스를 상속받아 고유한 행동을 구현합니다.
/// </summary>
public abstract class GuardState
{
    protected GuardRhythmPatrol guard;
    protected Rigidbody2D guardRigidbody;
    protected Transform guardTransform;
    protected RhythmSyncManager rhythmManager;
    protected PlayerController playerController;
    protected MissionManager missionManager;

    public GuardState(GuardRhythmPatrol guard)
    {
        this.guard = guard;
        this.guardRigidbody = guard.GetComponent<Rigidbody2D>();
        this.guardTransform = guard.transform;
        
        // ⭐ GameServices 사용
        this.rhythmManager = GameServices.RhythmManager;
        this.playerController = GameServices.Player;
        this.missionManager = guard.missionManager;
        
        if (rhythmManager == null)
            Debug.LogError($"[GuardState] RhythmSyncManager를 찾을 수 없습니다!");
        if (playerController == null)
            Debug.LogError($"[GuardState] PlayerController를 찾을 수 없습니다!");
        if (missionManager == null)
            Debug.LogError($"[GuardState] MissionManager를 찾을 수 없습니다!");
    }

    // 상태 진입 시 호출
    public virtual void Enter() { }

    // 매 프레임 업데이트
    public virtual void Update() { }

    // 리듬 비트마다 호출
    public virtual void OnBeat(int currentBeat) { }

    // 상태 종료 시 호출
    public virtual void Exit() { }

    // 상태 전환 래퍼
    protected void ChangeState(GuardState newState)
    {
        guard.ChangeState(newState);
    }

    // --- 공통 유틸리티 함수 ---
    protected void RotateTowards(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - guardRigidbody.position).normalized;
        if (direction.sqrMagnitude > 0.001f) // ⭐ sqrMagnitude
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            guardTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    protected void MoveToTarget(Vector2 targetPosition, float speedMultiplier = 1f)
    {
        if (guardRigidbody == null) return;

        Vector2 currentPos = guardRigidbody.position;
        Vector2 moveVector = targetPosition - currentPos;
        
        // ⭐ sqrMagnitude 사용
        if (moveVector.sqrMagnitude > 0.0001f)
            guardRigidbody.MovePosition(currentPos + moveVector.normalized * guard.moveSpeed * speedMultiplier * Time.deltaTime);
        else
            guardRigidbody.position = targetPosition;
    }

    protected bool CheckPlayerInSight(out RaycastHit2D hit)
    {
        hit = default;
        if (playerController == null) return false;

        Vector2 guardPos = guardRigidbody.position;
        Vector2 playerPos = playerController.transform.position;
        Vector2 directionToPlayer = (playerPos - guardPos).normalized;
        float distanceToPlayer = Vector2.Distance(guardPos, playerPos);

        Vector2 guardForward = guardTransform.up;

        if (Vector2.Angle(guardForward, directionToPlayer) < guard.fieldOfViewAngle / 2f && 
        distanceToPlayer <= guard.viewDistance)
        {
            hit = Physics2D.Raycast(guardPos, directionToPlayer, guard.viewDistance, guard.ObstacleMask);
            return hit.collider != null && hit.collider.GetComponent<PlayerController>() != null;
        }

        return false;
    }
}