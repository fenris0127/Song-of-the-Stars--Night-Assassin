using UnityEngine;

public class CapricornTrap : MonoBehaviour
{
    // private RhythmSyncManager _rhythmManager;
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private bool _hasTriggered = false;

    public int durationBeats = 10;
    public int stunDurationBeats = 5; 

    private int _trapEndBeat = 0;

    void Start()
    {
        if (RhythmManager == null) return;

        _trapEndBeat = RhythmManager.currentBeatCount + durationBeats;
        RhythmManager.OnBeatCounted.AddListener(CheckTrapTimeout);

    }

    private void CheckTrapTimeout(int currentBeat)
    {
        if (currentBeat >= _trapEndBeat)
            DeactivateTrap();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return; // ⭐ 중복 방지
    
        GuardRhythmPatrol guard = other.GetComponent<GuardRhythmPatrol>();

        if (guard != null)
        {
            _hasTriggered = true;
            guard.ApplyParalysis(stunDurationBeats);
            DeactivateTrap();
        }
    }

    private void DeactivateTrap()
    {
        if (RhythmManager != null)
        RhythmManager.OnBeatCounted.RemoveListener(CheckTrapTimeout);
        
        // ⭐ 오브젝트 풀링 지원
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.Despawn(gameObject, "Trap");
        else
            Destroy(gameObject);
    }
}