using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 리듬 기반 경비병 순찰 및 탐지 시스템 (최적화 + 상태 시스템 통합)
/// </summary>
[RequireComponent(typeof(ProbabilisticDetection))]
[RequireComponent(typeof(Rigidbody2D))]
public class GuardRhythmPatrol : MonoBehaviour
{
    #region 컴포넌트 및 참조
    private Rigidbody2D _rigidbody;
    private ProbabilisticDetection _detectionSystem;
    private Transform _cachedTransform;
    private Vector2 _cachedPosition;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private PlayerController Player => GameServices.Player;
    private MissionManager MissionManager => GameServices.MissionManager;
    
    public LayerMask ObstacleMask => RhythmManager?.obstacleMask ?? 0;
    public Transform PlayerTransform => Player?.transform;
    public MissionManager missionManager => MissionManager;
    #endregion
    
    #region 순찰 및 AI 설정
    [Header("▶ 순찰 설정")]
    public List<Vector2> patrolPoints = new List<Vector2>();
    public float moveSpeed = 6f;
    public int patrolBeatIntervalMax = 4;

    private int _patrolIndex = 0;
    private int _nextMoveBeat = 0;
    private int _currentPatrolInterval;
    private bool _isPatrolling = true;
    private Vector2 _targetPosition;
    
    [Header("▶ 시야 및 상태")]
    public float viewDistance = 10f;
    public float fieldOfViewAngle = 100f;
    public float decoyDetectionRange = 15f;
    public bool isFixedGuard = false;
    
    [Header("▶ 탐지 시스템")]
    [SerializeField] private ProbabilisticDetection detectionSystem;
    [SerializeField] private DetectionProbabilityData detectionData;
    
    [Header("▶ 발각 시스템")]
    public bool useGradualDetection = true;
    public int detectionAlertIncrease = 2;
    private float _detectionTime = 0f;
    public float timeToFullDetection = 2f;

    private bool _isParalyzed = false;
    private int _paralysisEndBeat = 0;
    private bool _isFlashed = false;
    private int _flashEndBeat = 0;
    
    private GameObject _activeDecoy = null;
    private Vector2 _lastDecoyPosition;
    #endregion

    #region 최적화
    private int _frameSkipCounter = 0;
    private const int DETECTION_CHECK_INTERVAL = 3;
    private Collider2D[] _decoyCheckResults = new Collider2D[10];
    #endregion

    // ⭐ 상태 시스템
    private GuardState _currentState;
    private bool _stateSystemEnabled = true;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _detectionSystem = GetComponent<ProbabilisticDetection>();
        _cachedTransform = transform;
        
        if (_rigidbody == null)
        {
            Debug.LogError("GuardRhythmPatrol: Rigidbody2D 필요!");
            enabled = false;
            return;
        }
        
        _rigidbody.freezeRotation = true;
    }

    void Start()
    {
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.AddListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.AddListener(CheckParalysisTimeout);
            RhythmManager.OnBeatCounted.AddListener(CheckFlashTimeout);
        }
        else
        {
            Debug.LogError("GuardRhythmPatrol: RhythmSyncManager를 찾을 수 없습니다!");
            enabled = false;
            return;
        }
        
        _targetPosition = _rigidbody.position;
        _currentPatrolInterval = patrolBeatIntervalMax;
        _nextMoveBeat = RhythmManager.currentBeatCount + _currentPatrolInterval;

        // ⭐ 상태 시스템 초기화
        if (_stateSystemEnabled)
            ChangeState(new GuardPatrollingState(this));
    }

    void Update()
    {
        _cachedPosition = _rigidbody.position;

        // ⭐ 상태 시스템 사용
        if (_stateSystemEnabled && _currentState != null)
        {
            _currentState.Update();
            return;
        }

        // 레거시 시스템 (상태 시스템 미사용 시)
        if (_isParalyzed || _isFlashed) return;

        _frameSkipCounter++;
        if (_frameSkipCounter >= DETECTION_CHECK_INTERVAL)
        {
            _frameSkipCounter = 0;
            CheckForDecoy();
        }

        HandleDecoyDistraction();
        MoveToTarget();
        CheckPlayerInSight();
    }
    
    void HandleRhythmActions(int currentBeat)
    {
        // ⭐ 상태 시스템 사용
        if (_stateSystemEnabled && _currentState != null)
        {
            _currentState.OnBeat(currentBeat);
            return;
        }

        // 레거시 시스템
        if (_isParalyzed || _isFlashed) return;

        if (_isPatrolling && currentBeat >= _nextMoveBeat)
            PatrolToNextPoint(currentBeat);
    }

    void PatrolToNextPoint(int currentBeat)
    {
        if (patrolPoints.Count == 0) return;

        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
        _targetPosition = patrolPoints[_patrolIndex];
        
        _currentPatrolInterval = patrolBeatIntervalMax > 1 
            ? Random.Range(2, patrolBeatIntervalMax + 1)
            : patrolBeatIntervalMax;
        
        _nextMoveBeat = currentBeat + _currentPatrolInterval;
        
        Vector2 direction = (_targetPosition - _cachedPosition).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            _cachedTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    void MoveToTarget()
    {
        if (_rigidbody == null) return;
        
        Vector2 moveVector = _targetPosition - _cachedPosition;
        float sqrDistance = moveVector.sqrMagnitude;
        
        if (sqrDistance > 0.0001f)
        {
            Vector2 normalizedMove = moveVector.normalized;
            _rigidbody.MovePosition(_cachedPosition + normalizedMove * moveSpeed * Time.deltaTime);
        }
        else
        {
            _rigidbody.position = _targetPosition;
            _isPatrolling = true;
        }
    }
    
    void CheckPlayerInSight()
    {
        if (Player == null || MissionManager == null || _isFlashed) return;

        Vector2 player2DPos = Player.transform.position;
        Vector2 directionToPlayer = (player2DPos - _cachedPosition).normalized;
        
        float sqrDistanceToPlayer = (player2DPos - _cachedPosition).sqrMagnitude;
        float sqrViewDistance = viewDistance * viewDistance;
        
        Vector2 guardForward = _cachedTransform.up;

        if (Vector2.Angle(guardForward, directionToPlayer) < fieldOfViewAngle / 2f && 
            sqrDistanceToPlayer <= sqrViewDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(_cachedPosition, directionToPlayer, viewDistance, ObstacleMask);
            
            if (hit.collider != null && hit.collider.GetComponent<PlayerController>() != null)
            {
                PlayerStealth stealth = Player.GetComponent<PlayerStealth>();
                if (stealth != null && !stealth.isStealthActive && !Player.isIllusionActive)
                    MissionManager.MissionComplete(false);
                else
                    MissionManager.IncreaseAlertLevel(1);
            }
        }
    }

    public void ApplyParalysis(int durationInBeats)
    {
        if (RhythmManager == null) return;
        
        _isParalyzed = true;
        _paralysisEndBeat = RhythmManager.currentBeatCount + durationInBeats;
        
        // ⭐ 상태 시스템 사용
        if (_stateSystemEnabled)
            ChangeState(new GuardStunnedState(this, durationInBeats, false));
        
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
        if (RhythmManager == null) return;
        
        _isFlashed = true;
        _flashEndBeat = RhythmManager.currentBeatCount + durationInBeats;
        
        // ⭐ 상태 시스템 사용
        if (_stateSystemEnabled)
            ChangeState(new GuardStunnedState(this, durationInBeats, true));
        
        Debug.Log($"경비병 {gameObject.name} 섬광탄 적용! {_flashEndBeat} 비트까지 시야 차단.");
    }
    
    public void CheckFlashTimeout(int currentBeat)
    {
        if (_isFlashed && currentBeat >= _flashEndBeat)
            _isFlashed = false;
    }
    
    public Vector2 lastDecoyPosition => _lastDecoyPosition;
    
    public bool CheckForDecoy()
    {
        if (_activeDecoy != null) return false;
        
        int hitCount = Physics2D.OverlapCircleNonAlloc(_cachedPosition, viewDistance, _decoyCheckResults, RhythmManager.guardMask);
        
        for (int i = 0; i < hitCount; i++)
        {
            if (_decoyCheckResults[i].GetComponent<DecoyLifetime>() != null)
            {
                _activeDecoy = _decoyCheckResults[i].gameObject;
                _lastDecoyPosition = _decoyCheckResults[i].transform.position;
                _isPatrolling = false;
                _nextMoveBeat = RhythmManager.currentBeatCount;

                // ⭐ 상태 시스템 사용
                if (_stateSystemEnabled)
                    ChangeState(new GuardInvestigatingState(this, _lastDecoyPosition));

                return true;
            }
        }
        
        return false;
    }

    void HandleDecoyDistraction()
    {
        if (_activeDecoy == null) return;

        Vector2 directionToDecoy = (_lastDecoyPosition - _cachedPosition).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, directionToDecoy);
        
        _cachedTransform.rotation = Quaternion.Slerp(
            _cachedTransform.rotation, 
            Quaternion.Euler(0, 0, targetAngle), 
            Time.deltaTime * 5f
        );

        _targetPosition = _lastDecoyPosition;

        if (_activeDecoy == null)
            EnterSearchMode(_lastDecoyPosition);
    }

    void EnterSearchMode(Vector2 searchLocation)
    {
        _isPatrolling = false;
        _targetPosition = searchLocation;
        _nextMoveBeat = RhythmManager.currentBeatCount + 2;
    }
    
    public float GetDetectionProgress()
    {
        if (!useGradualDetection || timeToFullDetection <= 0f) 
            return 0f;
        
        return Mathf.Clamp01(_detectionTime / timeToFullDetection);
    }
    
    public void Die()
    {
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.RemoveListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.RemoveListener(CheckParalysisTimeout);
            RhythmManager.OnBeatCounted.RemoveListener(CheckFlashTimeout);
        }
        
        Debug.Log($"경비병 {gameObject.name}이/가 제거되었습니다.");
        
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.Despawn(gameObject, "Guard");
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.RemoveListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.RemoveListener(CheckParalysisTimeout);
            RhythmManager.OnBeatCounted.RemoveListener(CheckFlashTimeout);
        }
    }

    // ⭐ 상태 시스템 메서드
    public void ChangeState(GuardState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    /// <summary>
    /// 상태 시스템 활성화/비활성화 (디버깅용)
    /// </summary>
    public void SetStateSystemEnabled(bool enabled)
    {
        _stateSystemEnabled = enabled;
        
        if (enabled && _currentState == null)
            ChangeState(new GuardPatrollingState(this));
    }

    public bool isFlashed { get => _isFlashed; set => _isFlashed = value; }
    public bool isParalyzed { get => _isParalyzed; set => _isParalyzed = value; }
}