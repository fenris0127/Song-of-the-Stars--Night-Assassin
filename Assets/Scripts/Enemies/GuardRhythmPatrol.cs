using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GuardRhythmPatrol : MonoBehaviour
{
    #region 컴포넌트 및 참조
    private RhythmSyncManager _rhythmManager;
    private PlayerController _playerController; 
    private MissionManager _missionManager;
    private Rigidbody2D _rigidbody; 
    #endregion
    
    #region 순찰 및 AI 설정
    [Header("▶ 순찰 설정")]
    public List<Vector2> patrolPoints; 
    public float moveSpeed = 6f;       
    public int patrolBeatIntervalMax = 4; 

    private int _patrolIndex = 0;
    private int _nextMoveBeat = 0;
    private int _currentPatrolInterval; 
    private bool _isPatrolling = true;
    private Vector2 _targetPosition; 
    
    [Header("▶ 시야 및 상태")]
    public float viewDistance = 10f;
    public float viewAngle = 100f;
    public float decoyDetectionRange = 15f; // 데코이 전용 감지 범위 (시야보다 넓음)
    public bool isFixedGuard = false;

    [Header("▶ 발각 시스템")]
    public bool useGradualDetection = true; // true: 단계적 경보, false: 즉시 실패
    public int detectionAlertIncrease = 2; // 발각 시 경보 증가량
    private float _detectionTime = 0f; // 플레이어를 본 시간
    public float timeToFullDetection = 2f; // 완전 발각까지 걸리는 시간 (초)

    private bool _isParalyzed = false;
    private int _paralysisEndBeat = 0;
    private bool _isFlashed = false;
    private int _flashEndBeat = 0;   
    
    private bool _isInSearchMode = false; 
    private int _searchEndBeat = 0;
    private const int SEARCH_DURATION_BEATS = 8;

    private GameObject _activeDecoy = null; 
    private Vector2 _lastDecoyPosition; 
    #endregion

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _missionManager = FindObjectOfType<MissionManager>();
        _rigidbody = GetComponent<Rigidbody2D>(); 

        if (_rigidbody == null)
        {
            Debug.LogError("GuardRhythmPatrol: Rigidbody2D 컴포넌트가 필요합니다!");
            return;
        }
        _rigidbody.freezeRotation = true; 
        
        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(HandleRhythmActions);
            _rhythmManager.OnBeatCounted.AddListener(CheckParalysisTimeout);
            _rhythmManager.OnBeatCounted.AddListener(CheckFlashTimeout); 
        }
        
        _targetPosition = new Vector2(transform.position.x, transform.position.y);
        _currentPatrolInterval = patrolBeatIntervalMax; 
        _nextMoveBeat = _rhythmManager.currentBeatCount + _currentPatrolInterval;
    }

    void Update()
    {
        if (_isParalyzed || _isFlashed) return; 

        CheckForDecoy(); 
        HandleDecoyDistraction(); 
             
        MoveToTarget(); 
        CheckPlayerInSight(); 
    }
    
    // --- 리듬 액션 핸들러 ---
    void HandleRhythmActions(int currentBeat)
    {
        if (_isParalyzed || _isFlashed) return; 

        if (_isPatrolling && currentBeat >= _nextMoveBeat)
            PatrolToNextPoint(currentBeat);
    }

    void PatrolToNextPoint(int currentBeat)
    {
        if (patrolPoints.Count == 0) return;

        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
        _targetPosition = patrolPoints[_patrolIndex];
        
        if (patrolBeatIntervalMax > 1)
            _currentPatrolInterval = Random.Range(2, patrolBeatIntervalMax + 1);
        else
            _currentPatrolInterval = patrolBeatIntervalMax;
        
        _nextMoveBeat = currentBeat + _currentPatrolInterval;
        
        Vector2 current2DPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (_targetPosition - current2DPos).normalized;

        if (direction != Vector2.zero)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    // --- 이동 ---
    void MoveToTarget()
    {
        if (_rigidbody == null) return;
        
        if (Vector2.Distance(_rigidbody.position, _targetPosition) > 0.01f)
        {
            Vector2 moveVector = _targetPosition - _rigidbody.position;
            _rigidbody.MovePosition(_rigidbody.position + moveVector.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
             _rigidbody.position = _targetPosition;
             _isPatrolling = true;
        }
    }
    
    // --- 발각 로직 (2D 시야 체크) ---
    void CheckPlayerInSight()
    {
        if (_playerController == null || _missionManager == null || _isFlashed) return; 

        Vector2 current2DPos = _rigidbody.position;
        Vector2 player2DPos = new Vector2(_playerController.transform.position.x, _playerController.transform.position.y);
        
        Vector2 directionToPlayer = (player2DPos - current2DPos).normalized;
        float distanceToPlayer = Vector2.Distance(current2DPos, player2DPos);
        
        Vector2 guardForward = transform.up; 

        if (Vector2.Angle(guardForward, directionToPlayer) < viewAngle / 2f && distanceToPlayer <= viewDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(current2DPos, directionToPlayer, viewDistance, _rhythmManager.obstacleMask);
            
            if (hit.collider != null && hit.collider.GetComponent<PlayerController>() != null)
            {
                if (!_playerController.GetComponent<PlayerStealth>().isStealthActive && !_playerController.isIllusionActive)
                    _missionManager.MissionComplete(false);
                else
                    _missionManager.IncreaseAlertLevel(1);
            }
        }
    }

    // --- 상태 이상 적용 함수 ---
    public void ApplyParalysis(int durationInBeats) 
    {
        if (_rhythmManager == null) return;
        
        _isParalyzed = true;
        _paralysisEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
        
        Debug.Log($"경비병 {gameObject.name} 마비 적용! {_paralysisEndBeat} 비트까지 정지.");
    }
    
    public void CheckParalysisTimeout(int currentBeat) 
    { 
        if (_isParalyzed && currentBeat >= _paralysisEndBeat) 
        { 
            _isParalyzed = false; 
            _nextMoveBeat = currentBeat + 1; 
        }
    }
    
    public void ApplyFlash(int durationInBeats) 
    {
        if (_rhythmManager == null) return;
        
        _isFlashed = true;
        _flashEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
        
        Debug.Log($"경비병 {gameObject.name} 섬광탄 적용! {_flashEndBeat} 비트까지 시야 차단.");
    }
    
    public void CheckFlashTimeout(int currentBeat) 
    { 
        if (_isFlashed && currentBeat >= _flashEndBeat) 
            _isFlashed = false; 
    }
    
    // --- 잔상 및 수색 로직 (Decoy, SearchMode 등) ---
    void CheckForDecoy()
    {
        if (_activeDecoy != null) return; 

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewDistance, _rhythmManager.guardMask); 
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<DecoyLifetime>() != null)
            {
                _activeDecoy = hit.gameObject;
                _lastDecoyPosition = new Vector2(hit.transform.position.x, hit.transform.position.y);
                _isPatrolling = false; 
                _nextMoveBeat = _rhythmManager.currentBeatCount; 
                return;
            }
        }
    }

    void HandleDecoyDistraction()
    {
        if (_activeDecoy != null)
        {
            Vector2 directionToDecoy = (_lastDecoyPosition - _rigidbody.position).normalized;
            float targetAngle = Vector2.SignedAngle(Vector2.up, directionToDecoy);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), Time.deltaTime * 5f); 

            _targetPosition = _lastDecoyPosition;

            if (_activeDecoy == null)
                EnterSearchMode(_lastDecoyPosition); 
        }
    }

    void EnterSearchMode(Vector2 searchLocation)
    {
        _isPatrolling = false;
        _targetPosition = searchLocation;
        _nextMoveBeat = _rhythmManager.currentBeatCount + 2;
    }
    
    // --- 생명 주기 및 제거 ---

    /// <summary>
    /// UI를 위한 발각 진행도 반환 (0~1)
    /// </summary>
    public float GetDetectionProgress()
    {
        if (!useGradualDetection || timeToFullDetection <= 0f) return 0f;
        
        return Mathf.Clamp01(_detectionTime / timeToFullDetection);
    }
    
    /// <summary>
    /// 경비병을 씬에서 제거하고 필요한 정리 작업을 수행합니다.
    /// </summary>
    public void Die() // ★ Die 메서드 추가
    {
        // 리듬 매니저의 이벤트 리스너 제거
        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.RemoveListener(HandleRhythmActions);
            _rhythmManager.OnBeatCounted.RemoveListener(CheckParalysisTimeout);
            _rhythmManager.OnBeatCounted.RemoveListener(CheckFlashTimeout); 
        }
        
        Debug.Log($"경비병 {gameObject.name}이/가 제거되었습니다.");
        
        // 경비병 오브젝트 파괴
        Destroy(gameObject); 
    }
}