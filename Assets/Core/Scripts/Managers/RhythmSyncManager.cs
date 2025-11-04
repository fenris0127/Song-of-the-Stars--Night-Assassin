using UnityEngine;
using UnityEngine.Events;

public class RhythmSyncManager : MonoBehaviour
{
    [Header("Rhythm Settings")]
    public float bpm = 120f;
    public float beatInterval; // 60 / BPM
    
    [Header("Rhythm State")]
    public int currentBeatCount = 0;

    [Header("Masks")]
    public LayerMask guardMask;
    public LayerMask obstacleMask;

    [System.Serializable]
    public class OnBeatEvent : UnityEvent<int> {}
    public OnBeatEvent OnBeatCounted;

    public enum RhythmJudgment { None, Perfect, Good, Miss, Bad }

    void Awake()
    {
        beatInterval = 60f / bpm;
    }

    // MusicManager에 의해 정확한 박자 시점에 호출됨
    public void NotifyBeatElapsed()
    {
        currentBeatCount++;
        OnBeatCounted.Invoke(currentBeatCount);
    }
}