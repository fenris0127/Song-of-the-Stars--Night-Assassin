using UnityEngine;

public class CapricornTrap : MonoBehaviour
{
    private RhythmSyncManager _rhythmManager;

    public int durationBeats = 10;
    public int stunDurationBeats = 5; 

    private int _trapEndBeat = 0;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        if (_rhythmManager == null) return;

        _trapEndBeat = _rhythmManager.currentBeatCount + durationBeats;
        _rhythmManager.OnBeatCounted.AddListener(CheckTrapTimeout);
    }

    private void CheckTrapTimeout(int currentBeat)
    {
        if (currentBeat >= _trapEndBeat)
            DeactivateTrap();
    }

    void OnTriggerEnter(Collider other)
    {
        GuardRhythmPatrol guard = other.GetComponent<GuardRhythmPatrol>();

        if (guard != null)
        {
            guard.ApplyParalysis(stunDurationBeats); 
            DeactivateTrap(); 
        }
    }

    private void DeactivateTrap()
    {
        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.RemoveListener(CheckTrapTimeout);
            
        Destroy(gameObject);
    }
}