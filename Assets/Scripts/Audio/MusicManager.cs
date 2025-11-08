using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("오디오 구성요소")]
    public AudioSource bgmAudioSource;
    public AudioClip bgmClip;

    [Header("리듬 동기화")]
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private int _lastCalculatedBeat = -1; // ⭐ 중복 계산 방지

    [Header("동기화 설정")]
    public float syncDelaySeconds = 0f;
    public float firstBeatOffsetSeconds = 0f; // 첫 비트가 시작되는 시간 오프셋
    public float timePerBeat; 
    
    [Header("디버거")]
    public bool showBeatDebug = false;

    private double _dspStartTime;
    private int _lastReportedBeat = -1;

    void Start()
    {
        if (bgmAudioSource == null || bgmClip == null || RhythmManager == null)
        {
            Debug.LogError("MusicManager에 필수 컴포넌트가 없습니다!");
            return;
        }

        bgmAudioSource.clip = bgmClip;
        timePerBeat = 60f / RhythmManager.bpm;
        
        Invoke(nameof(StartMusicAndSync), syncDelaySeconds);
    }

    private void StartMusicAndSync()
    {
        // DSP 시간 기반으로 정확한 시작 시간 기록
        _dspStartTime = AudioSettings.dspTime;
        bgmAudioSource.PlayScheduled(_dspStartTime);
        
        Debug.Log($"음악 시작: DSP Time = {_dspStartTime}, BPM = {RhythmManager.bpm}");
    }

    void Update()
    {
        if (!bgmAudioSource.isPlaying || RhythmManager == null) return;

        double dspTimeElapsed = AudioSettings.dspTime - _dspStartTime;
        double adjustedTime = dspTimeElapsed - firstBeatOffsetSeconds;
        
        int calculatedBeat = adjustedTime > 0 
            ? (int)Mathf.Floor((float)(adjustedTime / timePerBeat)) 
            : -1;
        
        // ⭐ 최적화: 비트가 변경되었을 때만 처리
        if (calculatedBeat == _lastCalculatedBeat) return;
        _lastCalculatedBeat = calculatedBeat;
        
        if (calculatedBeat >= 0 && calculatedBeat > RhythmManager.currentBeatCount)
        {
            int beatDifference = calculatedBeat - RhythmManager.currentBeatCount;
            
            if (beatDifference > 1)
                Debug.LogWarning($"비트 동기화 경고: {beatDifference}개 비트 건너뜀 감지");
            
            RhythmManager.currentBeatCount = calculatedBeat;
        }
        
        if (showBeatDebug && calculatedBeat >= 0)
        {
            double nextBeatTime = (calculatedBeat + 1) * timePerBeat + firstBeatOffsetSeconds;
            double timeToNextBeat = nextBeatTime - adjustedTime;
            
            Debug.Log($"♪ Beat {calculatedBeat} | Next in: {timeToNextBeat:F3}s");
        }
    }
    
    /// <summary>
    /// 런타임에 BPM 변경 (보스전 등 특수 상황)
    /// </summary>
    public void ChangeBPM(float newBPM)
    {
        if (RhythmManager == null) return;
    
        RhythmManager.bpm = newBPM;
        timePerBeat = 60f / newBPM;
        RhythmManager.beatInterval = timePerBeat;
        
        Debug.Log($"BPM 변경: {newBPM}");
    }
    
    /// <summary>
    /// 음악 일시정지/재개
    /// </summary>
    public void TogglePause()
    {
        if (bgmAudioSource.isPlaying)
            bgmAudioSource.Pause();
        else
            bgmAudioSource.UnPause();
    }
}