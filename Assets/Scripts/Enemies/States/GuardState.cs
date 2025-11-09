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
    protected RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    protected PlayerController Player => GameServices.Player;
    protected MissionManager MissionManager => GameServices.MissionManager;
    protected PlayerStealth PlayerStealth => GameServices.PlayerStealth;

    // 로깅을 위한 상태 이름
    protected virtual string StateName => GetType().Name;
    protected float stateStartTime;
    protected Vector3 stateStartPosition;

    public GuardState(GuardRhythmPatrol guard)
    {
        this.guard = guard;
        this.guardRigidbody = guard.GetComponent<Rigidbody2D>();
        this.guardTransform = guard.transform;
        
        // ⭐ GameServices 사용
        if (RhythmManager == null)
            Debug.LogError($"[GuardState] RhythmSyncManager를 찾을 수 없습니다!");
        if (Player == null)
            Debug.LogError($"[GuardState] PlayerController를 찾을 수 없습니다!");
        if (MissionManager == null)
            Debug.LogError($"[GuardState] MissionManager를 찾을 수 없습니다!");
        if (PlayerStealth == null)
            Debug.LogError($"[GuardState] PlayerStealth를 찾을 수 없습니다!");
    }

    // 상태 진입 시 호출
    public virtual void Enter()
    {
        stateStartTime = Time.time;
        stateStartPosition = guardTransform.position;
        
        string rhythmInfo = RhythmManager != null ? $"비트: {RhythmManager.currentBeatCount}" : "리듬 매니저 없음";
        Debug.Log($"[{guard.name}] {StateName} 상태 진입 @ {Time.time:F2}초\n" +
                 $"위치: {stateStartPosition:F2}\n" +
                 $"{rhythmInfo}\n" +
                 $"플레이어와의 거리: {(Player != null ? Vector3.Distance(stateStartPosition, Player.transform.position).ToString("F2") : "N/A")}");
    }

    // 매 프레임 업데이트
    public virtual void Update() { }

    // 리듬 비트마다 호출
    public virtual void OnBeat(int currentBeat) { }

    // 상태 종료 시 호출
    public virtual void Exit()
    {
        float stateDuration = Time.time - stateStartTime;
        Vector3 totalMovement = guardTransform.position - stateStartPosition;
        
        Debug.Log($"[{guard.name}] {StateName} 상태 종료 @ {Time.time:F2}초\n" +
                 $"상태 지속 시간: {stateDuration:F2}초\n" +
                 $"이동 거리: {totalMovement.magnitude:F2}m\n" +
                 $"평균 이동 속도: {(totalMovement.magnitude / stateDuration):F2}m/s");
    }

    // 상태 전환 래퍼
    protected void ChangeState(GuardState newState) => guard.ChangeState(newState);

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
        if (Player == null || PlayerStealth == null) return false;

        Vector2 guardPos = guardRigidbody.position;
        Vector2 playerPos = Player.transform.position;
        Vector2 directionToPlayer = (playerPos - guardPos).normalized;
        float distanceToPlayer = Vector2.Distance(guardPos, playerPos);

        Vector2 guardForward = guardTransform.up;

        if (Vector2.Angle(guardForward, directionToPlayer) < guard.fieldOfViewAngle / 2f && 
        distanceToPlayer <= guard.viewDistance)
        {
            if (GameServices.RaycastCompat(guardPos, directionToPlayer, out hit, guard.viewDistance, guard.ObstacleMask))
            {
                return hit.collider.GetComponent<PlayerController>() != null;
            }
        }

        return false;
    }
}