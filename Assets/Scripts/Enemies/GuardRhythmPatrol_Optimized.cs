using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 최적화된 리듬 기반 경비병 순찰 및 탐지 시스템
/// 
/// 최적화 포인트:
/// - FindObjectOfType 제거 -> GameServices 사용
/// - 캐싱된 참조 사용
/// - Update에서 불필요한 계산 제거
/// - 상태 패턴 사용으로 조건문 최소화
/// </summary>
[RequireComponent(typeof(ProbabilisticDetection))]
[RequireComponent(typeof(Rigidbody2D))]
public class GuardRhythmPatrol_Optimized : MonoBehaviour
{
    #region 컴포넌트 및 참조 (캐싱)
    private Rigidbody2D _rigidbody;
    private ProbabilisticDetection _detectionSystem;
    private Transform _cachedTransform;
    private Vector2 _cachedPosition;

    // GameServices를 통한 참조 (FindObjectOfType 제거)
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private PlayerController Player => GameServices.Player;
    private MissionManager MissionManager => GameServices.MissionManager;
    
    public LayerMask ObstacleMask => RhythmManager?.obstacleMask ?? 0;
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
    public bool useGradualDetection = true;
    public int detectionAlertIncrease = 2;
    private float _detectionTime = 0f;
    public float timeToFullDetection = 2f;

    // 상태 이상
    private bool _isParalyzed = false;
    private int _paralysisEndBeat = 0;
    private bool _isFlashed = false;
    private int _flashEndBeat = 0;

    // 데코이
    private GameObject _activeDecoy = null;
    private Vector2 _lastDecoyPosition;
    #endregion

    #region 최적화: 프레임 스킵
    private int _frameSkipCounter = 0;
    private const int DETECTION_CHECK_INTERVAL = 3; // 3프레임마다 시야 체크
    #endregion

    // 상태 패턴
    private GuardState _currentState;

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
        // GameServices를 통한 이벤트 구독
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.AddListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.AddListener(CheckStatusTimeouts);
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
        
        // 초기 상태 설정
        ChangeState(new GuardPatrollingState(this));
    }

    void Update()
    {
        // 상태 이상 체크
        if (_isParalyzed || _isFlashed) return;

        // 위치 캐싱 (매 프레임 transform.position 접근 최소화)
        _cachedPosition = _rigidbody.position;

        // 프레임 스킵을 이용한 최적화
        _frameSkipCounter++;
        if (_frameSkipCounter >= DETECTION_CHECK_INTERVAL)
        {
            _frameSkipCounter = 0;
            CheckForDecoy();
        }

        HandleDecoyDistraction();
        MoveToTarget();
        
        // 상태 패턴을 통한 업데이트 (조건문 최소화)
        _currentState?.Update();
    }

    #region 리듬 액션
    void HandleRhythmActions(int currentBeat)
    {
        if (_isParalyzed || _isFlashed) return;

        _currentState?.OnBeat(currentBeat);

        if (_isPatrolling && currentBeat >= _nextMoveBeat)
            PatrolToNextPoint(currentBeat);
    }

    void PatrolToNextPoint(int currentBeat)
    {
        if (patrolPoints.Count == 0) return;

        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
        _targetPosition = patrolPoints[_patrolIndex];
        
        // 랜덤 간격 (한 번만 계산)
        _currentPatrolInterval = patrolBeatIntervalMax > 1 
            ? Random.Range(2, patrolBeatIntervalMax + 1) 
            : patrolBeatIntervalMax;
        
        _nextMoveBeat = currentBeat + _currentPatrolInterval;
        
        RotateTowards(_targetPosition);
    }
    #endregion

    #region 이동 (최적화)
    void MoveToTarget()
    {
        if (_rigidbody == null) return;
        
        // 거리 체크 최적화: sqrMagnitude 사용
        Vector2 moveVector = _targetPosition - _cachedPosition;
        float sqrDistance = moveVector.sqrMagnitude;
        
        if (sqrDistance > 0.0001f) // 0.01f의 제곱
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

    void RotateTowards(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - _cachedPosition).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            _cachedTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    #endregion

    #region 상태 이상 관리
    void CheckStatusTimeouts(int currentBeat)
    {
        if (_isParalyzed && currentBeat >= _paralysisEndBeat)
        {
            _isParalyzed = false;
            _nextMoveBeat = currentBeat + 1;
        }
        
        if (_isFlashed && currentBeat >= _flashEndBeat)
            _isFlashed = false;
    }

    public void ApplyParalysis(int durationInBeats)
    {
        if (RhythmManager == null) return;
        
        _isParalyzed = true;
        _paralysisEndBeat = RhythmManager.currentBeatCount + durationInBeats;
        
        ChangeState(new GuardStunnedState(this, durationInBeats, false));
    }

    public void ApplyFlash(int durationInBeats)
    {
        if (RhythmManager == null) return;
        
        _isFlashed = true;
        _flashEndBeat = RhythmManager.currentBeatCount + durationInBeats;
        
        ChangeState(new GuardStunnedState(this, durationInBeats, true));
    }
    #endregion

    #region 데코이 관리
    public bool CheckForDecoy()
    {
        if (_activeDecoy != null) return false;

        // OverlapCircleAll 대신 NonAlloc 버전 사용 (GC 방지)
        Collider2D[] results = new Collider2D[10];
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            _cachedPosition, 
            viewDistance, 
            results, 
            RhythmManager.guardMask
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (results[i].GetComponent<DecoyLifetime>() != null)
            {
                _activeDecoy = results[i].gameObject;
                _lastDecoyPosition = results[i].transform.position;
                _isPatrolling = false;
                _nextMoveBeat = RhythmManager.currentBeatCount;
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
        
        // Slerp 최적화: 프레임 독립적 보간
        _cachedTransform.rotation = Quaternion.Slerp(
            _cachedTransform.rotation, 
            Quaternion.Euler(0, 0, targetAngle), 
            Time.deltaTime * 5f
        );

        _targetPosition = _lastDecoyPosition;

        if (_activeDecoy == null)
            ChangeState(new GuardInvestigatingState(this, _lastDecoyPosition));
    }

    public Vector2 lastDecoyPosition => _lastDecoyPosition;
    #endregion

    #region 생명주기
    public float GetDetectionProgress()
    {
        if (!useGradualDetection || timeToFullDetection <= 0f) 
            return 0f;
        
        return Mathf.Clamp01(_detectionTime / timeToFullDetection);
    }

    public void Die()
    {
        // 이벤트 구독 해제
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.RemoveListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.RemoveListener(CheckStatusTimeouts);
        }
        
        // 풀로 반환하거나 파괴
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
            RhythmManager.OnBeatCounted.RemoveListener(CheckStatusTimeouts);
        }
    }
    #endregion

    #region 상태 패턴
    public void ChangeState(GuardState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
    #endregion

    #region Public Properties
    public bool isFlashed { get => _isFlashed; set => _isFlashed = value; }
    public bool isParalyzed { get => _isParalyzed; set => _isParalyzed = value; }
    public MissionManager missionManager => MissionManager;
    #endregion
}