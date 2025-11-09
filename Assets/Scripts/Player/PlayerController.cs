using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    private static readonly Vector2 UP = Vector2.up;
    private static readonly Vector2 DOWN = Vector2.down;
    private static readonly Vector2 LEFT = Vector2.left;
    private static readonly Vector2 RIGHT = Vector2.right;

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

    #region 상태 플래그
    private bool _isMoving = false;
    private Vector2 _targetPosition;
    private Vector2 _queuedDirection = Vector2.zero;
    private bool _isCharging = false;
    private float _chargeDistanceRemaining = 0f;

    public bool isIllusionActive = false;
    private int _illusionEndBeat = 0;

    public bool isFreeMoving = false;
    private float _freeMoveEndTime = 0f;
    private const float FREE_MOVE_FOCUS_COST = 50f;
    private const int FREE_MOVE_DURATION_BEATS = 4;
    
    // ⭐ 이벤트 구독 추적용
    private bool _subscribedToBeatEvent = false;
    private bool _subscribedToMovementEvent = false;
    #endregion

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rhythmChecker = GetComponent<RhythmPatternChecker>();

        if (_rigidbody == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D 컴포넌트가 필요합니다!");
            return;
        }

        _rigidbody.freezeRotation = true;

        // 이벤트 구독
        if (beatGameEvent != null)
        {
            beatGameEvent.OnEvent.AddListener(CheckIllusionTimeout);
            beatGameEvent.OnEvent.AddListener(ExecuteQueuedMovement);
            _subscribedToBeatEvent = true;
        }
        else if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.AddListener(CheckIllusionTimeout);
            RhythmManager.OnBeatCounted.AddListener(ExecuteQueuedMovement);
            _subscribedToBeatEvent = true;
        }

        if (movementCommandEvent != null)
        {
            movementCommandEvent.OnEvent.AddListener(EnqueueMovementDirection);
            _subscribedToMovementEvent = true;
        }

        _targetPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        HandleChargeMovement();

        if (!isFreeMoving)
            HandleRhythmMovementInput();
        else
        {
            HandleFreeMoveInput();
            CheckFreeMoveTimeout();
        }

        MoveToTarget();

        if (Input.GetKeyDown(KeyCode.Space))
            AttemptActivateFreeMove();
    }

    void HandleRhythmMovementInput()
    {
        if (_isCharging || _queuedDirection != Vector2.zero || isFreeMoving) return;

        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = UP;
        else if (Input.GetKeyDown(KeyCode.S)) direction = DOWN;
        else if (Input.GetKeyDown(KeyCode.A)) direction = LEFT;
        else if (Input.GetKeyDown(KeyCode.D)) direction = RIGHT;

        if (direction != Vector2.zero) 
            _queuedDirection = direction;
    }

    private void EnqueueMovementDirection(Vector2 direction)
    {
        if (_isCharging || _queuedDirection != Vector2.zero || isFreeMoving) return;
        if (direction == Vector2.zero) return;
        _queuedDirection = direction;
    }

    void ExecuteQueuedMovement(int currentBeat)
    {
        if (_queuedDirection != Vector2.zero && !_isMoving && !isFreeMoving)
        {
            Vector2 current2DPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 intendedPosition = current2DPos + _queuedDirection * moveDistance;

            if (CheckForObstacle(intendedPosition))
            {
                Debug.Log("이동 경로에 장애물 감지: 이동 취소");
                _queuedDirection = Vector2.zero;
                return;
            }

            _targetPosition = intendedPosition;
            _isMoving = true;

            if (_queuedDirection != Vector2.zero)
            {
                float angle = Vector2.SignedAngle(UP, _queuedDirection);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            _queuedDirection = Vector2.zero;
        }
    }

    void MoveToTarget()
    {
        if (!_isMoving || isFreeMoving) return;

        Vector2 current2DPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 moveVector = _targetPosition - current2DPos;
        float step = moveSpeed * Time.deltaTime;

        if (moveVector.magnitude < step)
        {
            _rigidbody.MovePosition(_targetPosition);
            _isMoving = false;
        }
        else
        {
            Vector2 newPosition = current2DPos + moveVector.normalized * step;
            _rigidbody.MovePosition(newPosition);
        }
    }

    bool CheckForObstacle(Vector2 destination)
    {
        if (_rigidbody == null || RhythmManager == null) return false;

        float checkRadius = 0.4f;
        Collider2D hit = Physics2D.OverlapCircle(destination, checkRadius, RhythmManager.obstacleMask);

        return hit != null;
    }

    public void AttemptActivateFreeMove()
    {
        if (isFreeMoving) return;

        if (_rhythmChecker != null && _rhythmChecker.currentFocus >= FREE_MOVE_FOCUS_COST)
        {
            _rhythmChecker.currentFocus -= FREE_MOVE_FOCUS_COST;
            isFreeMoving = true;

            if (RhythmManager != null)
                _freeMoveEndTime = Time.time + (RhythmManager.beatInterval * FREE_MOVE_DURATION_BEATS);
            else
                _freeMoveEndTime = Time.time + 2f; // 폴백

            _targetPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }

    void HandleFreeMoveInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
            return;

        Vector2 inputDir = new Vector2(h, v).normalized;
        Vector2 intendedPosition = _rigidbody.position + inputDir * moveSpeed * Time.deltaTime * 0.5f;
        
        if (!CheckForObstacle(intendedPosition))
            _rigidbody.MovePosition(intendedPosition);

        float angle = Vector2.SignedAngle(UP, inputDir);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void CheckFreeMoveTimeout()
    {
        if (isFreeMoving && Time.time >= _freeMoveEndTime)
        {
            isFreeMoving = false;
            _targetPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }

    public void ActivateIllusion(int durationInBeats)
    {
        if (illusionPrefab != null && RhythmManager != null)
        {
            GameObject decoyInstance = Instantiate(illusionPrefab, transform.position, transform.rotation);
            DecoyLifetime lifetimeScript = decoyInstance.AddComponent<DecoyLifetime>();
            lifetimeScript.Initialize(RhythmManager, durationInBeats);

            isIllusionActive = true;
            _illusionEndBeat = RhythmManager.currentBeatCount + durationInBeats;
        }
    }

    public void CheckIllusionTimeout(int currentBeat)
    {
        if (isIllusionActive && currentBeat >= _illusionEndBeat)
            isIllusionActive = false;
    }

    public void ActivateCharge(float distance)
    {
        _isCharging = true;
        _chargeDistanceRemaining = distance;
    }

    void HandleChargeMovement()
    {
        if (!_isCharging) return;

        float step = moveSpeed * 2f * Time.deltaTime;

        if (_chargeDistanceRemaining > 0f)
        {
            float actualStep = Mathf.Min(step, _chargeDistanceRemaining);
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

    // ⭐ 수정: 이벤트 구독 해제
    void OnDestroy()
    {
        // if (_subscribedToBeatEvent)
        // {
        //     if (beatGameEvent != null)
        //     {
        //         beatGameEvent.OnEvent.RemoveListener(CheckIllusionTimeout);
        //         beatGameEvent.OnEvent.RemoveListener(ExecuteQueuedMovement);
        //     }
        //     else if (RhythmManager != null)
        //     {
        //         RhythmManager.OnBeatCounted.RemoveListener(CheckIllusionTimeout);
        //         RhythmManager.OnBeatCounted.RemoveListener(ExecuteQueuedMovement);
        //     }
        // }

        if (_subscribedToBeatEvent)
        {
            if (beatGameEvent != null)
                beatGameEvent.OnEvent.RemoveListener(CheckIllusionTimeout);
            else if (RhythmManager != null)
                RhythmManager.OnBeatCounted.RemoveListener(CheckIllusionTimeout);
        }

        if (_subscribedToMovementEvent && movementCommandEvent != null)
            movementCommandEvent.OnEvent.RemoveListener(EnqueueMovementDirection);
    }
}