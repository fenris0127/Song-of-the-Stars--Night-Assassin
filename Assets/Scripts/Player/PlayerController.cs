using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    #region 컴포넌트 및 설정
    [Header("▶ 이동 설정")]
    public float moveDistance = 2f;
    public float moveSpeed = 8f;

    [Header("▶ 스킬 설정")]
    public GameObject illusionPrefab;
    private Rigidbody2D _rigidbody;
    private RhythmSyncManager _rhythmManager;
    private RhythmPatternChecker _rhythmChecker;
    #endregion

    #region 상태 플래그
    private bool _isMoving = false;
    private Vector2 _targetPosition;
    private Vector2 _queuedDirection = Vector2.zero;
    private bool _isCharging = false;
    private float _chargeDistanceRemaining = 0f;

    public bool isIllusionActive = false;
    private int _illusionEndBeat = 0;

    // Focus 시스템
    public bool isFreeMoving = false;
    private float _freeMoveEndTime = 0f;
    private const float FREE_MOVE_FOCUS_COST = 50f;
    private const int FREE_MOVE_DURATION_BEATS = 4;
    #endregion

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _rhythmChecker = GetComponent<RhythmPatternChecker>();

        if (_rigidbody == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D 컴포넌트가 필요합니다!");
            return;
        }

        _rigidbody.freezeRotation = true;

        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckIllusionTimeout);
            _rhythmManager.OnBeatCounted.AddListener(ExecuteQueuedMovement);
        }

        _targetPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        HandleChargeMovement();

        if (!isFreeMoving)
            HandleRhythmMovementInput(); // WASD 이동 처리
        else
        {
            HandleFreeMoveInput();
            CheckFreeMoveTimeout();
        }

        MoveToTarget();

        if (Input.GetKeyDown(KeyCode.Space))
            AttemptActivateFreeMove();
    }

    // --- 리듬 이동 로직 ---
    void HandleRhythmMovementInput()
    {
        if (_isCharging || _queuedDirection != Vector2.zero || isFreeMoving) return;

        // WASD 이동 입력 (스킬 입력과 분리됨)
        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S)) direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A)) direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D)) direction = Vector2.right;

        if (direction != Vector2.zero) _queuedDirection = direction;
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
                float angle = Vector2.SignedAngle(Vector2.up, _queuedDirection);
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
        if (_rigidbody == null || _rhythmManager == null) return false;

        float checkRadius = 0.4f;
        Collider2D hit = Physics2D.OverlapCircle(destination, checkRadius, _rhythmManager.obstacleMask);

        return hit != null;
    }

    // --- Focus 기반 Free Move 로직 ---
    public void AttemptActivateFreeMove()
    {
        if (isFreeMoving) return;

        if (_rhythmChecker.currentFocus >= FREE_MOVE_FOCUS_COST)
        {
            _rhythmChecker.currentFocus -= FREE_MOVE_FOCUS_COST;
            isFreeMoving = true;

            _freeMoveEndTime = Time.time + (_rhythmManager.beatInterval * FREE_MOVE_DURATION_BEATS);

            _targetPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }

    void HandleFreeMoveInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 inputDir = new Vector2(h, v).normalized;

        if (inputDir.magnitude > 0)
        {
            _rigidbody.MovePosition(_rigidbody.position + inputDir * moveSpeed * Time.deltaTime * 0.5f);

            float angle = Vector2.SignedAngle(Vector2.up, inputDir);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void CheckFreeMoveTimeout()
    {
        if (isFreeMoving && Time.time >= _freeMoveEndTime)
        {
            isFreeMoving = false;
            _targetPosition = new Vector2(transform.position.x, transform.position.y);
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

            isIllusionActive = true;
            _illusionEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
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
}
