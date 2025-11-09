using UnityEngine;
using UnityEngine.Events;

public enum RhythmJudgment { Perfect, Great, Miss }

/// <summary>
/// 리듬 동기화 및 판정 (판정 구간 사전 계산)
/// </summary>
public class RhythmSyncManager : MonoBehaviour
{
    #region 설정
    [Header("▶ 리듬 설정")]
    public float bpm = 120f;
    public float beatInterval;
    public float beatTolerance = 0.1f;
    public float perfectTolerance = 0.05f;
    
    [Header("▶ 레이어 마스크")]
    public LayerMask obstacleMask;
    public LayerMask guardMask;
    
    [Header("▶ 리듬 상태")]
    public int currentBeatCount = 0;
    private float _timeSinceLastBeat;
    
    public UnityEvent<int> OnBeatCounted;
    #endregion
    
    #region 판정 최적화
    // ⭐ 판정 구간 사전 계산
    private float _perfectWindowStart;
    private float _perfectWindowEnd;
    private float _greatWindowStart;
    private float _greatWindowEnd;
    private bool _judgmentCacheValid = false;
    #endregion
    
    void Awake()
    {
        beatInterval = 60f / bpm;
        
        if (OnBeatCounted == null)
            OnBeatCounted = new UnityEvent<int>();
            
        UpdateJudgmentWindows();
    }

    void Update()
    {
        _timeSinceLastBeat += Time.deltaTime;

        if (_timeSinceLastBeat >= beatInterval)
        {
            _timeSinceLastBeat -= beatInterval;
            currentBeatCount++;
            
            // ⭐ 비트 전환 시 판정 구간 재계산
            if (!_judgmentCacheValid)
                UpdateJudgmentWindows();
            
            OnBeatCounted.Invoke(currentBeatCount);
        }
    }
    
    /// <summary>
    /// ⭐ 판정 구간 사전 계산 (매 프레임 계산 제거)
    /// </summary>
    void UpdateJudgmentWindows()
    {
        _perfectWindowStart = beatInterval - perfectTolerance;
        _perfectWindowEnd = perfectTolerance;
        _greatWindowStart = beatInterval - beatTolerance;
        _greatWindowEnd = beatTolerance;
        _judgmentCacheValid = true;
    }
    
    /// <summary>
    /// ⭐ 캐시된 구간으로 판정 (분기 최소화)
    /// </summary>
    public RhythmJudgment CheckJudgment() 
    {
        float t = _timeSinceLastBeat;

        // Perfect 체크 (비트 시작 또는 끝)
        if (t <= _perfectWindowEnd || t >= _perfectWindowStart)
            return RhythmJudgment.Perfect;

        // Great 체크
        if (t <= _greatWindowEnd || t >= _greatWindowStart)
            return RhythmJudgment.Great;

        return RhythmJudgment.Miss;
    }
    
    public float GetBeatProgress() => _timeSinceLastBeat / beatInterval;
    public float GetTimeToNextBeat() => beatInterval - _timeSinceLastBeat;
    
    /// <summary>
    /// BPM 변경 시 호출 (판정 구간 재계산 트리거)
    /// </summary>
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        beatInterval = 60f / bpm;
        _judgmentCacheValid = false;
        UpdateJudgmentWindows();
    }
}