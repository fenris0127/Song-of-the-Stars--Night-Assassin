using UnityEngine;

public class GuardPatrollingState : GuardState
{
    private int nextMoveBeat;
    private int patrolIndex = 0;
    private int currentPatrolInterval;
    private Vector2 targetPosition;

    public GuardPatrollingState(GuardRhythmPatrol guard) : base(guard)
    {
        currentPatrolInterval = guard.patrolBeatIntervalMax;
           nextMoveBeat = rhythmManager.currentBeatCount + currentPatrolInterval;
    }

    public override void Enter()
    {
        if (guard.patrolPoints.Count > 0)
            targetPosition = guard.patrolPoints[patrolIndex];
        else
            targetPosition = guardRigidbody.position;

        Debug.Log($"경비병 {guard.name}: 순찰 상태 시작");
    }

    public override void Update()
    {
        // 플레이어 감지 체크
        RaycastHit2D hit;
        if (CheckPlayerInSight(out hit))
        {
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null && !player.GetComponent<PlayerStealth>().isStealthActive && !player.isIllusionActive)
            {
                guard.ChangeState(new GuardChasingState(guard));
                return;
            }
        }

        // 데코이 감지 체크
        if (CheckForDecoy())
        {
            ChangeState(new GuardInvestigatingState(guard, guard.lastDecoyPosition));
            return;
        }

        MoveToTarget(targetPosition);
    }

    public override void OnBeat(int currentBeat)
    {
        if (currentBeat >= nextMoveBeat)
            MoveToNextPatrolPoint(currentBeat);
    }

    private void MoveToNextPatrolPoint(int currentBeat)
    {
        if (guard.patrolPoints.Count == 0) return;

        patrolIndex = (patrolIndex + 1) % guard.patrolPoints.Count;
        targetPosition = guard.patrolPoints[patrolIndex];
        
        if (guard.patrolBeatIntervalMax > 1)
            currentPatrolInterval = Random.Range(2, guard.patrolBeatIntervalMax + 1);
        else
            currentPatrolInterval = guard.patrolBeatIntervalMax;
        
        nextMoveBeat = currentBeat + currentPatrolInterval;
        
        RotateTowards(targetPosition);
    }

    // GuardRhythmPatrol의 데코이 감지 래퍼
    protected bool CheckForDecoy() => guard.CheckForDecoy();
}