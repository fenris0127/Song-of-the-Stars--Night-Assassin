using UnityEngine;

public class GuardInvestigatingState : GuardState
{
    protected MissionManager MissionManager => GameServices.MissionManager;

    private Vector2 investigatePosition;
    private float investigationStartTime;
    private const float INVESTIGATION_DURATION = 3f;
    private const float INVESTIGATION_RADIUS = 2f;

    public GuardInvestigatingState(GuardRhythmPatrol guard, Vector2 position) : base(guard)
    {
        this.investigatePosition = position;
    }

    public override void Enter()
    {
        base.Enter(); // 기본 로깅
        
        investigationStartTime = Time.time;
        RotateTowards(investigatePosition);
        
        // 조사 상태 특화 로깅
        Debug.Log($"[{guard.name}] 조사 상태 추가 정보:\n" +
                 $"조사 위치: {investigatePosition:F2}\n" +
                 $"현재 위치에서 거리: {Vector2.Distance(guardRigidbody.position, investigatePosition):F2}m\n" +
                 $"예상 조사 시간: {INVESTIGATION_DURATION:F1}초\n" +
                 $"조사 범위: {INVESTIGATION_RADIUS:F1}m");
    }

    public override void Update()
    {
        // 플레이어 발견 시 추격으로 전환
        RaycastHit2D hit;
        if (CheckPlayerInSight(out hit))
        {
            var Player = hit.collider.GetComponent<PlayerController>();
            if (Player != null && PlayerStealth != null &&
                !PlayerStealth.isStealthActive &&
                !Player.isIllusionActive)
            {
                ChangeState(new GuardChasingState(guard));
                return;
            }
        }

        // 조사 지점으로 이동
        MoveToTarget(investigatePosition, 0.7f); // 조사 중에는 약간 느리게 이동

        // 조사 지점 도착 후 주변 순찰
        if (Vector2.Distance(guardRigidbody.position, investigatePosition) < 0.5f)
        {
            // 제자리에서 천천히 회전
            guardTransform.Rotate(0, 0, 45f * Time.deltaTime);
        }

        // 조사 시간 초과 시 순찰로 복귀
        if (Time.time - investigationStartTime > INVESTIGATION_DURATION)
        {
            Debug.Log($"경비병 {guard.name}: 수상한 점 발견하지 못함");
            ChangeState(new GuardPatrollingState(guard));
        }
    }

    public override void OnBeat(int currentBeat)
    {
        // 리듬에 맞춰 경계 레벨 약간 증가
        MissionManager?.IncreaseAlertLevel(1);
    }

    
}