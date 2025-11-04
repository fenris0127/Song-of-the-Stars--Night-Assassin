using System;
using UnityEngine;

public class GuardRhythmPatrol : MonoBehaviour
{
    [Header("Detection Settings")]
    public float viewDistance = 10f; 
    public float viewAngle = 100f; 
    private float _originalViewDistance;
    private float _originalViewAngle;
    
    [Header("Status")]
    public bool _isPlayerDetected = false;
    private bool _isParalyzed = false;
    private int _paralysisEndBeat = 0;
    
    private bool _isJamming = false; 
    private int _jammingEndBeat = 0;
    
    private bool _isFlashed = false; 
    private int _flashEndBeat = 0;
    
    private Decoy _activeDecoy = null; 

    private RhythmSyncManager _rhythmManager;
    private MissionManager _missionManager;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _missionManager = FindObjectOfType<MissionManager>();
        _originalViewDistance = viewDistance;
        _originalViewAngle = viewAngle;

        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckParalysisTimeout);
            _rhythmManager.OnBeatCounted.AddListener(CheckJammingTimeout);
            _rhythmManager.OnBeatCounted.AddListener(CheckFlashTimeout);
        }
    }

    void Update()
    {
        if (!_isParalyzed && !_isFlashed)
        {
             CheckForDecoy(); 
             HandleDecoyDistraction(); 
             AdjustBehaviorByAlertLevel(); 
             // 순찰 로직 실행
             CheckPlayerInSight();
        }
    }
    
    void CheckPlayerInSight() { /* 플레이어 감지 로직 구현 */ }
    
    // --- 물고기자리 (Decoy) 반응 ---
    void CheckForDecoy()
    {
        if (_activeDecoy != null) return; 

        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance * 0.8f, _rhythmManager.obstacleMask); 
        foreach (Collider hit in hits)
        {
            Decoy decoy = hit.GetComponent<Decoy>();
            if (decoy != null)
            {
                _activeDecoy = decoy;
                // 잔상 지속 시간만큼 조사하도록 타이머 설정 로직 추가
                return;
            }
        }
    }

    void HandleDecoyDistraction()
    {
        if (_activeDecoy != null)
        {
            Vector3 directionToDecoy = (_activeDecoy.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToDecoy);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); 
        }
    }
    
    // --- 경보 레벨에 따른 행동 변화 ---
    void AdjustBehaviorByAlertLevel()
    {
        if (_missionManager == null) return;
        
        float alertFactor = 1f;

        if (_missionManager.currentAlertLevel >= 4) { alertFactor = 1.75f; }
        else if (_missionManager.currentAlertLevel >= 2) { alertFactor = 1.25f; }
        else { alertFactor = 1f; }

        viewDistance = _originalViewDistance * alertFactor;
    }
    
    // --- 상태 이상 적용 및 해제 ---
    public void ApplyParalysis(int durationInBeats)
    {
        _isParalyzed = true;
        _paralysisEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
    }
    
    void CheckParalysisTimeout(int currentBeat)
    {
        if (_isParalyzed && currentBeat >= _paralysisEndBeat) { _isParalyzed = false; }
    }
    
    public void ApplyJamming(float reductionAmount, int durationInBeats)
    {
        viewDistance = Mathf.Max(1f, _originalViewDistance - reductionAmount);
        _isJamming = true;
        _jammingEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
    }
    
    void CheckJammingTimeout(int currentBeat)
    {
        if (_isJamming && currentBeat >= _jammingEndBeat) { viewDistance = _originalViewDistance; _isJamming = false; }
    }
    
    public void ApplyFlash(float reductionAmount, int durationInBeats)
    {
        viewAngle = Mathf.Max(5f, _originalViewAngle - reductionAmount);
        _isFlashed = true;
        _flashEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
    }

    void CheckFlashTimeout(int currentBeat)
    {
        if (_isFlashed && currentBeat >= _flashEndBeat) { viewAngle = _originalViewAngle; _isFlashed = false; }
    }
    
    public void Die() { Destroy(gameObject); }
}