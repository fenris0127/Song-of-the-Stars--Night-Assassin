using UnityEngine;

public class GuardInvestigatingState : GuardState
{
    protected MissionManager MissionManager => guard.missionManager;

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
        investigationStartTime = Time.time;
        RotateTowards(investigatePosition);
        Debug.Log($"경비병 {guard.name}: 수상한 소리 조사 시작 @ {investigatePosition}");
    }

    public override void Update()
    {
        // 플레이어 발견 시 추격으로 전환
        RaycastHit2D hit;
        if (CheckPlayerInSight(out hit))
        {
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null && !player.GetComponent<PlayerStealth>().isStealthActive && !player.isIllusionActive)
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

    // GuardRhythmPatrol의 missionManager 접근용
    
}