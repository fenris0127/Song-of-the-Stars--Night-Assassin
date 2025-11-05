using UnityEngine;
using System.Linq; // List<Vector3>를 Vector3로 변환할 때 필요할 수 있음

public class PlayerController : MonoBehaviour
{
    #region 컴포넌트 및 설정
    [Header("▶ 이동 설정")]
    public float moveDistance = 2f; 
    public float moveSpeed = 8f; 
    
    [Header("▶ 스킬 설정")]
    public GameObject illusionPrefab; // 물고기자리 잔상 Prefab

    private RhythmSyncManager _rhythmManager;
    private CharacterController _characterController;
    private RhythmPatternChecker _rhythmChecker;
    #endregion

    #region 상태 플래그
    private bool _isMoving = false; 
    private Vector3 _targetPosition; 
    private Vector3 _queuedDirection = Vector3.zero; 
    private bool _isCharging = false;
    private float _chargeDistanceRemaining = 0f;
    
    // 물고기자리 (Illusion) 상태 관리
    public bool isIllusionActive = false;
    private int _illusionEndBeat = 0; 
    
    // Focus 시스템
    public bool isFreeMoving = false;
    private float _freeMoveEndTime = 0f;
    private const float FREE_MOVE_FOCUS_COST = 50f;
    private const int FREE_MOVE_DURATION_BEATS = 4;
    #endregion

    // --- Unity Life Cycle ---
    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _characterController = GetComponent<CharacterController>();
        _rhythmChecker = GetComponent<RhythmPatternChecker>();
        
        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckIllusionTimeout);
            // 비트 이벤트가 발생할 때 대기 중인 이동을 실행
            _rhythmManager.OnBeatCounted.AddListener(ExecuteQueuedMovement); 
        }
        
        _targetPosition = transform.position;
    }

    void Update()
    {
        // 1. 돌진 스킬 처리 (Time.deltaTime 기반)
        HandleChargeMovement();
        
        // 2. Free Move 상태 체크 및 입력 처리
        if (!isFreeMoving)
        {
            HandleRhythmMovementInput(); // 리듬 이동 입력 감지 (큐 저장)
        }
        else
        {
            HandleFreeMoveInput();
            CheckFreeMoveTimeout();
        }

        // 3. 목표 위치로 부드럽게 이동
        MoveToTarget();
        
        // 4. Focus 스킬 발동 시도 (Spacebar)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttemptActivateFreeMove();
        }
    }
    
    // --- 리듬 이동 로직 ---
    void HandleRhythmMovementInput()
    {
        // 돌진 중이거나, 이미 다음 비트에 실행할 방향이 대기 중이거나, Free Move 중이면 입력 무시
        if (_isCharging || _queuedDirection != Vector3.zero || isFreeMoving) return; 

        // 쿼터 뷰/탑다운에 맞는 4방향 입력 처리 (WASD)
        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = new Vector3(1, 0, 1).normalized; // 쿼터뷰: 위쪽 대각선
        else if (Input.GetKeyDown(KeyCode.S)) direction = new Vector3(-1, 0, -1).normalized; // 쿼터뷰: 아래쪽 대각선
        else if (Input.GetKeyDown(KeyCode.A)) direction = new Vector3(-1, 0, 1).normalized; // 쿼터뷰: 왼쪽 대각선
        else if (Input.GetKeyDown(KeyCode.D)) direction = new Vector3(1, 0, -1).normalized; // 쿼터뷰: 오른쪽 대각선
        
        if (direction != Vector3.zero)
        {
            _queuedDirection = direction; // 다음 비트까지 방향을 대기열에 저장
            // TODO: 입력이 수락되었음을 나타내는 시각적 피드백 제공 (예: 플레이어 테두리 하이라이트)
        }
    }

    /// <summary>
    /// RhythmSyncManager의 비트 이벤트에 의해 호출되어 실제 이동을 실행합니다.
    /// </summary>
    void ExecuteQueuedMovement(int currentBeat)
    {
        // 이동 큐에 방향이 있고, 현재 이동 중이 아니며, Free Move 중이 아닐 때만 실행
        if (_queuedDirection != Vector3.zero && !_isMoving && !isFreeMoving)
        {
            // 1. 목표 위치 계산
            Vector3 intendedPosition = transform.position + _queuedDirection * moveDistance;
            
            // 2. 장애물 체크
            if (CheckForObstacle(intendedPosition))
            {
                Debug.Log("이동 경로에 장애물 감지: 이동 취소");
                _queuedDirection = Vector3.zero;
                return;
            }

            // 3. 이동 실행
            _targetPosition = intendedPosition;
            _isMoving = true;
            transform.forward = _queuedDirection;

            // 4. 입력 성공 후 방향 큐 초기화
            _queuedDirection = Vector3.zero; 
        }
    }

    void MoveToTarget()
    {
        // Free Move 중이거나 목표에 이미 도달했다면 리턴
        if (!_isMoving || isFreeMoving) return;

        Vector3 moveVector = _targetPosition - transform.position;
        float step = moveSpeed * Time.deltaTime; 
        
        if (moveVector.magnitude < step)
        {
            _characterController.Move(moveVector); 
            _isMoving = false;
        }
        else
        {
            _characterController.Move(moveVector.normalized * step);
        }
    }

    bool CheckForObstacle(Vector3 destination)
    {
        if (_characterController == null || _rhythmManager == null) return false;
        
        float checkRadius = _characterController.radius * 0.9f;
        
        // OverlapSphere 대신, CharacterController를 위한 CapsuleCast를 사용하는 것이 더 정확할 수 있습니다.
        // 여기서는 간단히 CheckSphere를 사용합니다.
        if (Physics.CheckSphere(destination, checkRadius, _rhythmManager.obstacleMask))
        {
            return true;
        }
        return false;
    }
    
    // --- Focus 기반 Free Move 로직 ---
    public void AttemptActivateFreeMove()
    {
        if (isFreeMoving) return;
        
        if (_rhythmChecker.currentFocus >= FREE_MOVE_FOCUS_COST)
        {
            _rhythmChecker.currentFocus -= FREE_MOVE_FOCUS_COST; 
            isFreeMoving = true;
            
            // 4비트 시간 동안 자유 이동
            _freeMoveEndTime = Time.time + (_rhythmManager.beatInterval * FREE_MOVE_DURATION_BEATS); 
            
            // 이동 목표를 현재 위치로 재설정하여 리듬 이동 충돌 방지
            _targetPosition = transform.position;
        }
    }
    
    void HandleFreeMoveInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // 쿼터뷰 방향에 맞게 입력 벡터 변환 (간단화를 위해 3D 일반 이동으로 처리)
        Vector3 inputDir = new Vector3(h, 0, v);
        
        if (inputDir.magnitude > 0)
        {
            _characterController.Move(inputDir.normalized * moveSpeed * Time.deltaTime * 0.5f); // 일반 이동보다 느리게
            transform.forward = inputDir.normalized;
        }
    }
    
    void CheckFreeMoveTimeout()
    {
        if (isFreeMoving && Time.time >= _freeMoveEndTime)
        {
            isFreeMoving = false;
            // Free Move 종료 시 리듬 이동 준비
            _targetPosition = transform.position; 
        }
    }

    // --- 스킬 관련 함수 ---
    public void ActivateIllusion(int durationInBeats)
    {
        if (illusionPrefab != null)
        {
            GameObject decoyInstance = Instantiate(illusionPrefab, transform.position, transform.rotation);
            DecoyLifetime lifetimeScript = decoyInstance.AddComponent<DecoyLifetime>();
            lifetimeScript.Initialize(_rhythmManager, durationInBeats);
            
            // 잔상 활성화 상태 관리 (경비병이 잔상에 반응하는 동안 플레이어는 안전함)
            isIllusionActive = true;
            _illusionEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
        }
    }

    public void CheckIllusionTimeout(int currentBeat)
    {
        if (isIllusionActive && currentBeat >= _illusionEndBeat)
        {
            isIllusionActive = false;
        }
    }

    public void ActivateCharge(float distance)
    {
        _isCharging = true;
        _chargeDistanceRemaining = distance;
    }
    
    void HandleChargeMovement()
    {
        if (!_isCharging) return;
        
        float step = moveSpeed * 2f * Time.deltaTime; // 돌진은 빠르게
        
        // 아직 남아있는 돌진 거리
        if (_chargeDistanceRemaining > 0f)
        {
            float actualStep = Mathf.Min(step, _chargeDistanceRemaining);
            _characterController.Move(transform.forward * actualStep);
            _chargeDistanceRemaining -= actualStep;
        }
        
        if (_chargeDistanceRemaining <= 0f)
        {
            _isCharging = false;
            _targetPosition = transform.position; // 돌진 끝난 위치로 목표 재설정
            _isMoving = false; // 혹시 모를 리듬 이동 충돌 방지
        }
    }
}