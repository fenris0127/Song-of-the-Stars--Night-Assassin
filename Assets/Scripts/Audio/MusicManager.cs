using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Components")]
    public AudioSource bgmAudioSource;
    public AudioClip bgmClip;
    
    [Header("Rhythm Sync Reference")]
    private RhythmSyncManager _rhythmManager;

    [Header("Synchronization Settings")]
    public float syncDelaySeconds = 0f;
    public float firstBeatOffsetSeconds = 0f; // 첫 비트가 시작되는 시간 오프셋
    public float timePerBeat; 
    
    [Header("Debug")]
    public bool showBeatDebug = false;

    private double _dspStartTime;
    private int _lastReportedBeat = -1;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        
        if (bgmAudioSource == null || bgmClip == null || _rhythmManager == null)
        {
            Debug.LogError("MusicManager에 필수 컴포넌트가 없습니다!");
            return;
        }

        bgmAudioSource.clip = bgmClip;
        timePerBeat = 60f / _rhythmManager.bpm;
        
        Invoke(nameof(StartMusicAndSync), syncDelaySeconds);
    }

    private void StartMusicAndSync()
    {
        // DSP 시간 기반으로 정확한 시작 시간 기록
        _dspStartTime = AudioSettings.dspTime;
        bgmAudioSource.PlayScheduled(_dspStartTime);
        
        Debug.Log($"음악 시작: DSP Time = {_dspStartTime}, BPM = {_rhythmManager.bpm}");
    }

    void Update()
    {
        if (!bgmAudioSource.isPlaying || _rhythmManager == null) return;

        // DSP 시간 기반 정확한 경과 시간 계산
        double dspTimeElapsed = AudioSettings.dspTime - _dspStartTime;
        
        // 첫 비트 오프셋 적용
        double adjustedTime = dspTimeElapsed - firstBeatOffsetSeconds;
        
        // 현재 비트 인덱스 계산 (0부터 시작)
        int calculatedBeat = adjustedTime > 0 ? (int)Mathf.Floor((float)(adjustedTime / timePerBeat)) : -1;
        
        // MusicManager가 RhythmSyncManager의 비트 카운트를 이끌도록 함
        if (calculatedBeat >= 0 && calculatedBeat > _rhythmManager.currentBeatCount)
        {
            // 비트가 건너뛰어지지 않도록 보정
            int beatDifference = calculatedBeat - _rhythmManager.currentBeatCount;
            
            if (beatDifference > 1)
                Debug.LogWarning($"비트 동기화 경고: {beatDifference}개 비트 건너뜀 감지");
            
            _rhythmManager.currentBeatCount = calculatedBeat;
        }
        
        // 디버그 표시
        if (showBeatDebug && calculatedBeat != _lastReportedBeat && calculatedBeat >= 0)
        {
            _lastReportedBeat = calculatedBeat;
            double nextBeatTime = (calculatedBeat + 1) * timePerBeat + firstBeatOffsetSeconds;
            double timeToNextBeat = nextBeatTime - adjustedTime;
            
            Debug.Log($"♪ Beat {calculatedBeat} | Next in: {timeToNextBeat:F3}s | DSP: {dspTimeElapsed:F3}s");
        }
    }
    
    /// <summary>
    /// 런타임에 BPM 변경 (보스전 등 특수 상황)
    /// </summary>
    public void ChangeBPM(float newBPM)
    {
        if (_rhythmManager == null) return;
        
        _rhythmManager.bpm = newBPM;
        timePerBeat = 60f / newBPM;
        _rhythmManager.beatInterval = timePerBeat;
        
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