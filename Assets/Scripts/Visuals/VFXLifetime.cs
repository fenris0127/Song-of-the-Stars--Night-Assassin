using UnityEngine;

/// <summary>
/// VFX의 수명을 리듬 비트 또는 시간 기반으로 관리 (간소화)
/// </summary>
public class VFXLifetime : MonoBehaviour
{
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    
    [Header("▶ 수명 설정")]
    public bool useBeatDuration = true;
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
    
        if (useBeatDuration)
        {
            if (RhythmManager != null)
            {
                _endBeat = RhythmManager.currentBeatCount + durationInBeats;
                RhythmManager.OnBeatCounted.AddListener(CheckBeatTimeout);
            }
            else
            {
                // 폴백: 리듬 매니저가 없으면 시간 기반으로 전환
                Debug.LogWarning("VFXLifetime: RhythmManager가 없어 시간 기반으로 전환합니다.");
                useBeatDuration = false;
                _endTime = Time.time + durationInSeconds;
            }
        }
        else
            _endTime = Time.time + durationInSeconds;
        
        _isInitialized = true;
    }
    
    public void SetBeatDuration(int beats)
    {
        useBeatDuration = true;
        durationInBeats = beats;
        
        if (RhythmManager != null)
        {
            _endBeat = RhythmManager.currentBeatCount + beats;
            RhythmManager.OnBeatCounted.AddListener(CheckBeatTimeout);
        }
        else
        {
            useBeatDuration = false;
            SetTimeDuration(beats * 0.5f); // 폴백
        }
        
        _isInitialized = true;
    }
    
    public void SetTimeDuration(float seconds)
    {
        useBeatDuration = false;
        durationInSeconds = seconds;
        _endTime = Time.time + seconds;
        _isInitialized = true;
    }

    void Update()
    {
        // 시간 기반 수명만 Update에서 체크
        if (!useBeatDuration && _isInitialized && Time.time >= _endTime)
            DestroyVFX();
    }

    void CheckBeatTimeout(int currentBeat)
    {
        if (currentBeat >= _endBeat)
            DestroyVFX();
    }

    void DestroyVFX()
    {
        if (RhythmManager != null && useBeatDuration)
            RhythmManager.OnBeatCounted.RemoveListener(CheckBeatTimeout);
            
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (RhythmManager != null && useBeatDuration)
            RhythmManager.OnBeatCounted.RemoveListener(CheckBeatTimeout);
    }
}