using UnityEngine;

public class GuardStunnedState : GuardState
{
    private int recoveryBeat;
    private bool isFlashed;

    public GuardStunnedState(GuardRhythmPatrol guard, int duration, bool isFlashbang = false) : base(guard)
    {
        this.recoveryBeat = rhythmManager.currentBeatCount + duration;
        this.isFlashed = isFlashbang;
    }

    public override void Enter()
    {
        if (isFlashed)
            Debug.Log($"경비병 {guard.name}: 섬광탄에 기절 (회복: {recoveryBeat}비트)");
        else
            Debug.Log($"경비병 {guard.name}: 마비됨 (회복: {recoveryBeat}비트)");
    }

    public override void Update()
    {
        // 기절 상태에서는 움직임이나 시야 체크 없음
    }

    public override void OnBeat(int currentBeat)
    {
        if (currentBeat >= recoveryBeat)
        {
            Debug.Log($"경비병 {guard.name}: 상태 이상에서 회복");
            // 회복 시 순찰 상태로 복귀
            guard.ChangeState(new GuardPatrollingState(guard));
        }
    }

    public override void Exit()
    {
        // 상태 이상 플래그 초기화
        if (isFlashed)
            this.guard.isFlashed = false;
        else
            this.guard.isParalyzed = false;
    }
}