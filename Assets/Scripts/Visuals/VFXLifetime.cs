using UnityEngine;

/// <summary>
/// VFX(이펙트)의 수명을 리듬 비트 또는 시간 기반으로 관리합니다.
/// </summary>
public class VFXLifetime : MonoBehaviour
{
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private bool _isTimeBasedLifetime = false;
    
    [Header("▶ 수명 설정")]
    public bool useBeatDuration = true; // true: 비트 기반, false: 시간 기반
    public int durationInBeats = 2;
    public float durationInSeconds = 1f;
    
    private int _endBeat = 0;
    private float _endTime = 0f;
    private bool _isInitialized = false;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_isInitialized) return;
    
        if (useBeatDuration && RhythmManager != null)
        {
            _endBeat = RhythmManager.currentBeatCount + durationInBeats;
            RhythmManager.OnBeatCounted.AddListener(CheckBeatTimeout);
        }
        else
            _endTime = Time.time + durationInSeconds;
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// 외부에서 비트 기반 수명 설정
    /// </summary>
    public void SetBeatDuration(int beats)
    {
        useBeatDuration = true;
        durationInBeats = beats;
        
        if (RhythmManager != null)
        {
            _endBeat = RhythmManager.currentBeatCount + beats;
            RhythmManager.OnBeatCounted.AddListener(CheckBeatTimeout);
        }
    }
    
    /// <summary>
    /// 외부에서 시간 기반 수명 설정
    /// </summary>
    public void SetTimeDuration(float seconds)
    {
        useBeatDuration = false;
        durationInSeconds = seconds;
        _endTime = Time.time + seconds;
        _isTimeBasedLifetime = true; // ⭐ 플래그 설정

    }

    void Update()
    {
        // 시간 기반 수명 체크
        if (_isTimeBasedLifetime && !useBeatDuration && Time.time >= _endTime)
            DestroyVFX();
    }

    void CheckBeatTimeout(int currentBeat)
    {
        if (currentBeat >= _endBeat)
            DestroyVFX();
    }

    void DestroyVFX()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.RemoveListener(CheckBeatTimeout);
            
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.RemoveListener(CheckBeatTimeout);
    }
}