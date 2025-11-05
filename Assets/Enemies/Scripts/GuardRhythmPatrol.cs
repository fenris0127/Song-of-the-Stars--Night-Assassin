using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events; // UnityEvent 사용 시

public class GuardRhythmPatrol : MonoBehaviour
{
    #region 컴포넌트 및 참조
    private RhythmSyncManager _rhythmManager;
    private PlayerController _playerController; 
    private MissionManager _missionManager;
    private CharacterController _characterController;
    #endregion
    
    #region 순찰 및 AI 설정
    [Header("▶ 순찰 설정")]
    public List<Vector3> patrolPoints; // 순찰 경로 지점 목록
    public float moveSpeed = 6f;       // 이동 속도
    
    [Header("▶ 리듬 난이도")]
    [Tooltip("경비병의 리듬 엄격도: 4 -> 매 4비트마다 이동, Max가 4라면 2~4 중 랜덤 간격")]
    public int patrolBeatIntervalMax = 4; 

    private int _patrolIndex = 0;
    private int _nextMoveBeat = 0;
    private int _currentPatrolInterval; // 이번 이동에 사용할 비트 간격
    private bool _isPatrolling = true;
    private Vector3 _targetPosition;
    
    [Header("▶ 시야 및 상태")]
    public float viewDistance = 10f;
    public float viewAngle = 100f;
    public bool isFixedGuard = false; // 고정 경비 여부

    // 상태 이상 (예시를 위해 변수 유지)
    private bool _isParalyzed = false;
    private int _paralysisEndBeat = 0;
    private bool _isFlashed = false;
    private int _flashEndBeat = 0;

    // 잔상 감지
    private GameObject _activeDecoy = null; 
    private Vector3 _lastDecoyPosition; 
    #endregion

    // --- Unity Life Cycle ---
    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _missionManager = FindObjectOfType<MissionManager>();
        _characterController = GetComponent<CharacterController>();

        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(HandleRhythmActions);
            // 상태 이상 체크 함수 등록 (예시)
            _rhythmManager.OnBeatCounted.AddListener(CheckParalysisTimeout);
        }
        
        // 초기 순찰 목표 설정
        _targetPosition = transform.position;
        _currentPatrolInterval = patrolBeatIntervalMax; // 초기 간격 설정
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
    
    // --- 리듬 이벤트에 반응 ---
    void HandleRhythmActions(int currentBeat)
    {
        if (_isParalyzed || _isFlashed) return; 
        
        if (isFixedGuard)
        {
            // 고정 경비 행동 (시선 전환 등)
        }
        else if (currentBeat >= _nextMoveBeat)
        {
            if (ShouldEnterSearchMode())
            {
                EnterSearchMode(_lastDecoyPosition); // 수색 모드 진입
            }
            else if (_isPatrolling)
            {
                PatrolToNextPoint(currentBeat); // 순찰 이동
            }
        }
    }
    
    // --- 불규칙 순찰 이동 ---
    void PatrolToNextPoint(int currentBeat)
    {
        if (patrolPoints.Count == 0) return;

        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
        _targetPosition = patrolPoints[_patrolIndex];
        
        // 비트 불규칙 순찰 로직 적용 (2~Max 사이 랜덤 간격)
        if (patrolBeatIntervalMax > 1)
        {
            _currentPatrolInterval = Random.Range(2, patrolBeatIntervalMax + 1);
        }
        else
        {
            _currentPatrolInterval = patrolBeatIntervalMax;
        }
        
        _nextMoveBeat = currentBeat + _currentPatrolInterval;
        
        // 목표 방향으로 회전
        Vector3 direction = (_targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
    
    // --- 잔상 및 수색 로직 ---
    void CheckForDecoy()
    {
        // 이미 잔상에 정신이 팔렸다면 다시 찾지 않음
        if (_activeDecoy != null) return; 

        // 근처에 DecoyLifetime 컴포넌트를 가진 오브젝트가 있는지 확인
        // TODO: Physics.OverlapSphere 대신 실제 시야각을 고려한 검사 필요
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, LayerMask.GetMask("Decoy")); 
        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<DecoyLifetime>() != null)
            {
                _activeDecoy = hit.gameObject;
                _lastDecoyPosition = hit.transform.position;
                _isPatrolling = false; 
                _nextMoveBeat = _rhythmManager.currentBeatCount; // 즉시 반응
                return;
            }
        }
    }

    void HandleDecoyDistraction()
    {
        if (_activeDecoy != null)
        {
            // 잔상 쪽으로 시선을 돌림
            Vector3 directionToDecoy = (_activeDecoy.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToDecoy);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); 

            // 잔상으로 목표 위치 설정
            _targetPosition = _activeDecoy.transform.position;

            // 잔상이 파괴되었는지 체크
            if (_activeDecoy == null)
            {
                // 잔상이 사라지면 마지막 잔상 위치를 목표로 수색 모드 진입
                EnterSearchMode(_lastDecoyPosition); 
            }
        }
    }

    bool ShouldEnterSearchMode()
    {
        return _missionManager.currentAlertLevel >= 3;
    }

    void EnterSearchMode(Vector3 searchLocation)
    {
        _isPatrolling = false;
        _targetPosition = searchLocation;
        _nextMoveBeat = _rhythmManager.currentBeatCount + 2; // 수색은 2비트 간격으로 빠르게
    }
    
    // --- 부드러운 이동 ---
    void MoveToTarget()
    {
        if (_characterController == null) return;
        
        if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
        {
            Vector3 moveVector = _targetPosition - transform.position;
            // CharacterController를 사용하여 이동
            _characterController.Move(moveVector.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
             // 목표에 도달하면 위치 보정
             transform.position = _targetPosition;
             _isPatrolling = true; // 이동 완료 후 다시 순찰 상태로 전환 (또는 정지)
        }
    }
    
    // --- 발각 로직 및 상태 이상 타임아웃 (예시) ---
    void CheckPlayerInSight()
    {
        if (_playerController == null || _missionManager == null) return;

        Vector3 raycastStart = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (_playerController.transform.position - raycastStart).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, _playerController.transform.position);

        // 시야각 체크
        if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2f && distanceToPlayer <= viewDistance)
        {
            // 레이캐스트를 통한 장애물 체크
            if (Physics.Raycast(raycastStart, directionToPlayer, out RaycastHit hit, viewDistance))
            {
                if (hit.collider.GetComponent<PlayerController>() != null)
                {
                    // 플레이어 발각 조건
                    if (!_playerController.GetComponent<PlayerStealth>().isStealthActive && !_playerController.isIllusionActive)
                    {
                        _missionManager.MissionComplete(false); // 미션 실패
                    }
                    else
                    {
                        // 플레이어 발견, 경보 레벨 상승
                        _missionManager.IncreaseAlertLevel(1);
                    }
                }
            }
        }
    }

    public void CheckParalysisTimeout(int currentBeat) 
    { 
        if (_isParalyzed && currentBeat >= _paralysisEndBeat) 
        { 
            _isParalyzed = false; 
            // 마비 해제 후 1비트 쉬도록 설정
            _nextMoveBeat = currentBeat + 1; 
        }
    }

    // 오브젝트 파괴 시 이벤트 리스너 해제 (필수)
    private void OnDestroy()
    {
        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.RemoveListener(HandleRhythmActions);
            _rhythmManager.OnBeatCounted.RemoveListener(CheckParalysisTimeout);
        }
    }
}