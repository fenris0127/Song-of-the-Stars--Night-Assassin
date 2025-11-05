using UnityEngine;

/// <summary>
/// 물고기자리 잔상 프리팹의 수명을 리듬 비트 기준으로 관리합니다.
/// </summary>
public class DecoyLifetime : MonoBehaviour
{
    private RhythmSyncManager _rhythmManager;
    private int _lifetimeEndBeat;

    /// <summary>
    /// 잔상 오브젝트를 초기화하고 리듬 이벤트에 등록합니다.
    /// </summary>
    /// <param name="manager">RhythmSyncManager 인스턴스</param>
    /// <param name="durationInBeats">잔상이 유지될 비트 수</param>
    public void Initialize(RhythmSyncManager manager, int durationInBeats)
    {
        _rhythmManager = manager;
        if (_rhythmManager != null)
        {
            // 현재 비트 수에 지속 시간을 더하여 종료 비트 계산
            _lifetimeEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
            
            // 비트 이벤트 리스너 등록
            _rhythmManager.OnBeatCounted.AddListener(CheckTimeout);
        }
        else
        {
            Debug.LogError("DecoyLifetime: RhythmSyncManager를 찾을 수 없습니다. 잔상 수명 관리가 불가능합니다.");
            Destroy(gameObject); 
        }
    }

    /// <summary>
    /// 매 비트마다 호출되어 수명이 다했는지 확인합니다.
    /// </summary>
    /// <param name="currentBeat">현재 비트 수</param>
    private void CheckTimeout(int currentBeat)
    {
        if (currentBeat >= _lifetimeEndBeat)
        {
            Deactivate();
        }
    }

    /// <summary>
    /// 잔상 오브젝트를 정리하고 파괴합니다.
    /// </summary>
    private void Deactivate()
    {
        if (_rhythmManager != null)
        {
            // 이벤트 리스너 해제
            _rhythmManager.OnBeatCounted.RemoveListener(CheckTimeout);
        }
        // 잔상 오브젝트 파괴
        Destroy(gameObject); 
    }
    
    // 오브젝트가 외부 요인으로 파괴될 때 이벤트 리스너 해제 안전 장치
    private void OnDestroy()
    {
        if (_rhythmManager != null)
        {
            // 중복 해제를 방지하기 위해 CheckTimeout 함수만 해제
            _rhythmManager.OnBeatCounted.RemoveListener(CheckTimeout);
        }
    }
}