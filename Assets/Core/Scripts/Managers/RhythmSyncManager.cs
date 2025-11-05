using UnityEngine;
using UnityEngine.Events;

public enum RhythmJudgment { Perfect, Great, Miss }

public class RhythmSyncManager : MonoBehaviour
{
    // === 판정 열거형 ===

    #region 설정 및 이벤트
    [Header("▶ 리듬 설정")]
    public float bpm = 120f;
    public float beatInterval; // 60 / BPM
    public float beatTolerance = 0.1f; // 판정 허용 시간 (초)
    public float perfectTolerance = 0.05f; // Perfect 판정 허용 시간 (초)
    
    [Header("▶ 레이어 마스크 (2D)")]
    public LayerMask obstacleMask; 
    public LayerMask guardMask;    
    
    [Header("▶ 리듬 상태")]
    public int currentBeatCount = 0;
    private float _songStartTime;
    private float _timeSinceLastBeat;
    
    public UnityEvent<int> OnBeatCounted; 
    #endregion
    
    void Start()
    {
        beatInterval = 60f / bpm; 
        _songStartTime = Time.time;
        _timeSinceLastBeat = 0f;
    }

    void Update()
    {
        _timeSinceLastBeat += Time.deltaTime;

        if (_timeSinceLastBeat >= beatInterval)
        {
            _timeSinceLastBeat -= beatInterval;
            currentBeatCount++;
            
            OnBeatCounted.Invoke(currentBeatCount); 
        }
    }
    
    // --- 핵심 기능: 입력 판정 ---
    public RhythmJudgment CheckJudgment() 
    {
        float timeFromBeatStart = _timeSinceLastBeat; 
        float timeToNextBeat = beatInterval - timeFromBeatStart; 

        float deviation = Mathf.Min(timeFromBeatStart, timeToNextBeat);

        if (deviation <= perfectTolerance)
            return RhythmJudgment.Perfect;
        else if (deviation <= beatTolerance)
            return RhythmJudgment.Great;
        else
            return RhythmJudgment.Miss;
    }
}