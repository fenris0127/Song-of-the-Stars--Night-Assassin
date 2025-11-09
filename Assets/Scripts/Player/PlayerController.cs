using UnityEngine;

/// <summary>
/// 플레이어의 움직임과 스킬 사용을 제어하는 메인 컴포넌트입니다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    #region 컴포넌트 및 설정
    [Header("▶ 이동 설정")]
    public float moveDistance = 2f;
    public float moveSpeed = 8f;

    [Header("▶ 스킬 설정")]
    public GameObject illusionPrefab;
    private Rigidbody2D _rigidbody;
    private RhythmPatternChecker _rhythmChecker;
    
    [Header("▶ Decoupling (optional)")]
    public IntGameEvent beatGameEvent;
    public Vector2GameEvent movementCommandEvent;
    #endregion

    #region 데코이 관리
    private GameObject _currentDecoy;
    private bool _isDecoyActive;
    private int _decoyEndBeat;

    /// <summary>
    /// 현재 활성화된 데코이의 오브젝트
    /// </summary>
    public GameObject DecoyObject => _currentDecoy;

    /// <summary>
    /// 데코이의 현재 위치
    /// </summary>
    public Vector2 DecoyPosition => _currentDecoy != null ? _currentDecoy.transform.position : Vector2.zero;

    /// <summary>
    /// 데코이가 현재 활성화되어 있는지 여부
    /// </summary>
    public bool isDecoyActive => _isDecoyActive && _currentDecoy != null;
    #endregion
    
    #region 상태 플래그
    private bool _isMoving = false;
    private Vector2 _targetPosition;
    private Vector2 _queuedDirection = Vector2.zero;
    private bool _isCharging = false;
    private float _chargeDistanceRemaining = 0f;

    public bool isFreeMoving = false;
    private float _freeMoveEndTime = 0f;
    private const float FREE_MOVE_FOCUS_COST = 50f;
    private const float FREE_MOVE_DURATION = 1f;

    private bool _subscribedToBeatEvent = false;
    private bool _subscribedToMovementEvent = false;
    #endregion

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rhythmChecker = GameServices.RhythmChecker;
    }

    void Start()
    {
        _targetPosition = new Vector2(transform.position.x, transform.position.y);
        
        // 리듬 이벤트 구독
        if (beatGameEvent != null)
        {
            beatGameEvent.OnEvent.AddListener(CheckDecoyTimeout);
            beatGameEvent.OnEvent.AddListener(ExecuteQueuedMovement);
            _subscribedToBeatEvent = true;
        }
        else if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.AddListener(CheckDecoyTimeout);
            RhythmManager.OnBeatCounted.AddListener(ExecuteQueuedMovement);
            _subscribedToBeatEvent = true;
        }

        // 입력 이벤트 구독
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMovementInput += EnqueueMovementDirection;
            _subscribedToMovementEvent = true;
        }
    }

    void Update()
    {
        if (_isCharging)
        {
            HandleChargeMovement();
        }

        if (isFreeMoving)
        {
            HandleFreeMovement();
        }
    }
    
    // ✅ 버그 수정: OnDestroy 이벤트 구독 해제 및 정리
    void OnDestroy()
    {
        if (_subscribedToBeatEvent)
        {
            // beatGameEvent 구독 해제
            if (beatGameEvent != null)
            {
                beatGameEvent.OnEvent.RemoveListener(CheckDecoyTimeout);
                beatGameEvent.OnEvent.RemoveListener(ExecuteQueuedMovement);
            }
            // RhythmManager 구독 해제
            else if (RhythmManager != null)
            {
                RhythmManager.OnBeatCounted.RemoveListener(CheckDecoyTimeout);
                RhythmManager.OnBeatCounted.RemoveListener(ExecuteQueuedMovement);
            }
        }
        
        // 정리: 활성화된 데코이 제거
        DeactivateDecoy();

        // 이동 입력 구독 해제
        if (_subscribedToMovementEvent && InputManager.Instance != null)
            InputManager.Instance.OnMovementInput -= EnqueueMovementDirection;
            
        if (isFreeMoving) isFreeMoving = false;
    }

    private void EnqueueMovementDirection(Vector2 direction)
    {
        if (!_isMoving && !_isCharging)
        {
            _queuedDirection = direction;
        }
    }

    private void ExecuteQueuedMovement(int beat)
    {
        if (GameServices.IsPaused() || _isCharging || isFreeMoving) return;
        
        if (_queuedDirection != Vector2.zero)
        {
            Move(_queuedDirection);
            _queuedDirection = Vector2.zero;
        }
    }

    public void Move(Vector2 direction)
    {
        if (_isMoving || _isCharging) return;

        // 이동 방향 설정 및 회전
        transform.up = direction;
        
        Vector2 startPosition = _rigidbody.position;
        _targetPosition = startPosition + direction * moveDistance;
        _isMoving = true;
        
        // 이동 시작
        _rigidbody.MovePosition(_targetPosition);
        _isMoving = false; // 간단한 버전에서는 즉시 완료 처리
    }

    /// <summary>
    /// 데코이를 활성화하고 이전 데코이가 있다면 비활성화합니다.
    /// </summary>
    public void ActivateIllusion(int durationBeats)
    {
        // Backwards-compatible: use the inspector-assigned illusionPrefab
        ActivateIllusion(illusionPrefab, durationBeats);
    }

    /// <summary>
    /// Canonical activation that allows callers to provide a specific prefab.
    /// If prefab is null, falls back to the inspector `illusionPrefab`.
    /// </summary>
    public void ActivateIllusion(GameObject prefab, int durationBeats)
    {
        if (RhythmManager == null) return;

        GameObject toSpawn = prefab != null ? prefab : illusionPrefab;
        if (toSpawn == null) return;

        // 이전 데코이가 있다면 제거
        DeactivateDecoy();

        // 새 데코이 생성 및 초기화
        _currentDecoy = Instantiate(toSpawn, transform.position, transform.rotation);
        DecoyLifetime lifetimeScript = _currentDecoy.AddComponent<DecoyLifetime>();
        lifetimeScript.Initialize(RhythmManager, durationBeats);

        // 데코이 상태 추적
        _isDecoyActive = true;
        _decoyEndBeat = RhythmManager.currentBeatCount + durationBeats;

        Debug.Log($"데코이 생성({toSpawn.name}): {durationBeats}비트 동안 유지됨");
    }

    /// <summary>
    /// 기존 코드 호환성용: 외부에서 생성된 데코이(GameObject)를 플레이어에 등록합니다.
    /// Decoy 프리팹이 자체적으로 수명을 관리(DecoyLifetime)하는 경우 해당 로직에 맡깁니다.
    /// </summary>
    public void ActivateDecoy(GameObject decoyObject)
    {
        if (decoyObject == null) return;

        // 이전 데코이 제거(중복 등록 방지)
        if (_currentDecoy != null && _currentDecoy != decoyObject)
            DeactivateDecoy();

        _currentDecoy = decoyObject;
        _isDecoyActive = true;

        Debug.Log("외부 데코이 등록됨: " + decoyObject.name);
    }

    /// <summary>
    /// 활성화된 데코이를 제거합니다.
    /// </summary>
    public void DeactivateDecoy()
    {
        if (_currentDecoy != null)
        {
            Destroy(_currentDecoy);
            _currentDecoy = null;
        }
        _isDecoyActive = false;
        Debug.Log("데코이 비활성화");
    }

    /// <summary>
    /// 현재 비트에서 데코이 타임아웃을 체크합니다.
    /// </summary>
    private void CheckDecoyTimeout(int beat)
    {
        if (_isDecoyActive && beat >= _decoyEndBeat)
        {
            DeactivateDecoy();
        }
    }
    
    // ⭐ Free Move (임시로 리듬 동기화를 해제하고 자유 이동)
    public void StartFreeMove()
    {
        if (isFreeMoving) return;
        
        isFreeMoving = true;
        _freeMoveEndTime = Time.time + FREE_MOVE_DURATION;
        // Focus 코스트 지불
        _rhythmChecker.DecreaseFocus(FREE_MOVE_FOCUS_COST);
    }
    
    private void HandleFreeMovement()
    {
        if (Time.time > _freeMoveEndTime)
        {
            isFreeMoving = false;
            return;
        }
        
        // 이동 로직 (Update에서 Time.deltaTime 사용)
        Vector2 moveDirection = InputManager.Instance.CurrentMovementInput; // InputManager에서 직접 접근 필요
        if (moveDirection != Vector2.zero)
        {
            _rigidbody.MovePosition(_rigidbody.position + moveDirection * moveSpeed * Time.deltaTime);
        }
    }
    
    public void StartCharge(float distance)
    {
        if (_isCharging) return;
        
        _isCharging = true;
        _chargeDistanceRemaining = distance;
    }

    // 호환성용 wrapper: 다른 스크립트에서 호출하던 ActivateCharge를 그대로 지원
    public void ActivateCharge(float distance) => StartCharge(distance);
    
    private void HandleChargeMovement()
    {
        if (GameServices.IsPaused()) return;
        
        float step = moveSpeed * Time.deltaTime;
        float actualStep = Mathf.Min(step, _chargeDistanceRemaining);
        
        if (actualStep > 0f)
        {
            Vector2 forward2D = new Vector2(transform.up.x, transform.up.y);
            _rigidbody.MovePosition(_rigidbody.position + forward2D * actualStep);
            _chargeDistanceRemaining -= actualStep;
        }

        if (_chargeDistanceRemaining <= 0f)
        {
            _isCharging = false;
            _targetPosition = new Vector2(transform.position.x, transform.position.y);
            _isMoving = false;
        }
    }
}