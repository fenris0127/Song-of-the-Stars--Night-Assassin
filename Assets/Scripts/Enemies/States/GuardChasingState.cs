using UnityEngine;

public class GuardChasingState : GuardState
{
    private float detectionTime = 0f;
    private Vector2 lastKnownPlayerPosition;

    public GuardChasingState(GuardRhythmPatrol guard) : base(guard) { }

    public override void Enter()
    {
        base.Enter(); // 기본 로깅

        missionManager?.IncreaseAlertLevel(1); // 즉시 경계 수준 상승
        
        // 추격 상태 특화 로깅
        if (player != null)
        {
            Vector3 directionToPlayer = player.transform.position - guardTransform.position;
            Debug.Log($"[{guard.name}] 추격 상태 추가 정보:\n" +
                     $"플레이어 방향: {directionToPlayer.normalized:F2}\n" +
                     $"플레이어까지 거리: {directionToPlayer.magnitude:F2}m\n" +
                     $"경계 레벨: {(missionManager != null ? missionManager.currentAlertLevel.ToString() : "N/A")}\n" +
                     $"플레이어 스텔스 상태: {(playerStealth != null ? playerStealth.isStealthActive.ToString() : "N/A")}");
        }
    }

    public override void Update()
    {
        if (player == null) return;

        RaycastHit2D hit;
        bool canSeePlayer = CheckPlayerInSight(out hit);
        
        if (canSeePlayer)
        {
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                if (!player.GetComponent<PlayerStealth>().isStealthActive && !player.isIllusionActive)
                {
                    lastKnownPlayerPosition = player.transform.position;
                    RotateTowards(lastKnownPlayerPosition);
                    MoveToTarget(lastKnownPlayerPosition, 1.5f); // 추격 시 빠른 이동

                    // 점진적 발각 시스템
                    if (guard.useGradualDetection)
                    {
                        detectionTime += Time.deltaTime;
                        if (detectionTime >= guard.timeToFullDetection)
                            missionManager?.MissionComplete(false);
                    }
                    else
                        missionManager?.MissionComplete(false);
                }
                else
                    guard.ChangeState(new GuardInvestigatingState(guard, lastKnownPlayerPosition)); // 플레이어가 스텔스 상태면 수색으로 전환
            }
        }
        else
        {
            // 시야에서 벗어나면 마지막 위치 수색
            if (lastKnownPlayerPosition != Vector2.zero)
                guard.ChangeState(new GuardInvestigatingState(guard, lastKnownPlayerPosition));
            else
                guard.ChangeState(new GuardPatrollingState(guard));
        }
    }

    // 리듬에 맞춰 경계 레벨 증가
    public override void OnBeat(int currentBeat) => missionManager?.IncreaseAlertLevel(1);
}