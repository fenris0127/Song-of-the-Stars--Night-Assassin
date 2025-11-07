using UnityEngine;

public class GuardChasingState : GuardState
{
    private float detectionTime = 0f;
    private Vector2 lastKnownPlayerPosition;

    public GuardChasingState(GuardRhythmPatrol guard) : base(guard) { }

    public override void Enter()
    {
        Debug.Log($"경비병 {guardTransform.name}: 추격 상태 시작");
        missionManager?.IncreaseAlertLevel(1); // 즉시 경계 수준 상승
    }

    public override void Update()
    {
        if (playerController == null) return;

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
                        {
                            missionManager?.MissionComplete(false);
                        }
                    }
                    else
                    {
                        missionManager?.MissionComplete(false);
                    }
                }
                else
                {
                    // 플레이어가 스텔스 상태면 수색으로 전환
                    guard.ChangeState(new GuardInvestigatingState(guard, lastKnownPlayerPosition));
                }
            }
        }
        else
        {
            // 시야에서 벗어나면 마지막 위치 수색
            if (lastKnownPlayerPosition != Vector2.zero)
            {
                guard.ChangeState(new GuardInvestigatingState(guard, lastKnownPlayerPosition));
            }
            else
            {
                guard.ChangeState(new GuardPatrollingState(guard));
            }
        }
    }

    public override void OnBeat(int currentBeat)
    {
        // 리듬에 맞춰 경계 레벨 증가
        missionManager?.IncreaseAlertLevel(1);
    }
}