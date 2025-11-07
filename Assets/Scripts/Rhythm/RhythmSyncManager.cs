using UnityEngine;
using UnityEngine.Events;

public enum RhythmJudgment { Perfect, Great, Miss }

/// <summary>
/// 리듬 동기화 및 판정을 관리하는 핵심 매니저
/// </summary>
public class RhythmSyncManager : MonoBehaviour
{
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
    private float _timeSinceLastBeat;
    
    public UnityEvent<int> OnBeatCounted;
    #endregion
    
    void Awake()
    {
        beatInterval = 60f / bpm;
        
        // UnityEvent 초기화
        if (OnBeatCounted == null)
            OnBeatCounted = new UnityEvent<int>();
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
    
    /// <summary>
    /// 입력 타이밍에 대한 판정을 반환합니다.
    /// </summary>
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
    
    /// <summary>
    /// 현재 비트 진행도 (0~1)
    /// </summary>
    public float GetBeatProgress()
    {
        return _timeSinceLastBeat / beatInterval;
    }
    
    /// <summary>
    /// 다음 비트까지 남은 시간 (초)
    /// </summary>
    public float GetTimeToNextBeat()
    {
        return beatInterval - _timeSinceLastBeat;
    }
}