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
    public float timePerBeat; 

    private float _dspStartTime;

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
        
        Invoke("StartMusicAndSync", syncDelaySeconds);
    }

    private void StartMusicAndSync()
    {
        bgmAudioSource.Play();
        _dspStartTime = (float)AudioSettings.dspTime; 
    }

    void Update()
    {
        if (!bgmAudioSource.isPlaying || _rhythmManager == null) return;

        double dspTimeElapsed = AudioSettings.dspTime - _dspStartTime;
        
        // 현재 비트 인덱스 계산 (0부터 시작)
        int calculatedBeat = (int)Mathf.Floor((float)(dspTimeElapsed / timePerBeat));
        
        // MusicManager가 RhythmSyncManager의 비트 카운트를 이끌도록 함
        if (calculatedBeat >= _rhythmManager.currentBeatCount)
        {
            _rhythmManager.currentBeatCount = calculatedBeat;
            _rhythmManager.NotifyBeatElapsed(); 
        }
    }
}