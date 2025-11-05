using UnityEngine;

/// <summary>
/// 물고기자리 잔상 프리팹의 수명을 리듬 비트 기준으로 관리합니다.
/// </summary>
public class DecoyLifetime : MonoBehaviour
{
    private RhythmSyncManager _rhythmManager;
    private int _lifetimeEndBeat;

    public void Initialize(RhythmSyncManager manager, int durationInBeats)
    {
        _rhythmManager = manager;
        if (_rhythmManager != null)
        {
            _lifetimeEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
            _rhythmManager.OnBeatCounted.AddListener(CheckTimeout);
        }
        else
        {
            Debug.LogError("DecoyLifetime: RhythmSyncManager를 찾을 수 없습니다.");
            Destroy(gameObject); 
        }
    }

    private void CheckTimeout(int currentBeat)
    {
        if (currentBeat >= _lifetimeEndBeat)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.RemoveListener(CheckTimeout);

        Destroy(gameObject); 
    }
    
    private void OnDestroy()
    {
        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.RemoveListener(CheckTimeout);
    }
}