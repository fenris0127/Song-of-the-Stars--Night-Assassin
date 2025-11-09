using UnityEngine;
using System;

public enum InputType
{
    Movement,
    Skill1,
    Skill2,
    Skill3,
    Skill4,
    Jump,
    Dash,
    Interact
}

/// <summary>
/// 최적화된 입력 관리 시스템
/// 매 프레임 Input 클래스 호출을 최소화하고 입력 이벤트를 중앙 집중화합니다.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("▶ 키 매핑 (향후 확장용)")]
    public InputMapping keyMapping = new InputMapping();

    #region 입력 이벤트
    // 이동 입력
    public event Action<Vector2> OnMovementInput;

    // 스킬 입력 (1, 2, 3, 4)
    public event Action<int> OnSkillInput; // 1-4 숫자

    // 특수 액션
    public event Action OnJumpInput;
    public event Action OnDashInput;
    public event Action OnInteractInput;
    public event Action OnPauseInput;

    // 디버그 키
    public event Action OnDebugToggle;
    #endregion

    #region 입력 설정
    [Header("▶ 입력 설정")]
    public bool enableInput = true;
    public float inputBufferTime = 0.1f;

    [Header("▶ 데드존")]
    public float movementDeadzone = 0.1f;
    private float _deadzoneSquared;
    #endregion

    #region 입력 상태 캐싱
    // 현재 프레임 입력 상태
    private Vector2 _currentMovementInput;
    private bool[] _skillPressed = new bool[4];
    private bool _jumpPressed;
    private bool _dashPressed;
    private bool _interactPressed;
    private bool _pausePressed;

    // 이전 프레임 상태 (Edge 감지용)
    private bool[] _previousSkillState = new bool[4];
    private bool _previousJumpState;
    private bool _previousDashState;
    private bool _previousInteractState;
    private bool _previousPauseState;
    #endregion

    #region 입력 버퍼
    private struct BufferedInput
    {
        public InputType type;
        public float timestamp;
        public int skillIndex; // 스킬 입력의 경우
    }

    private BufferedInput? _bufferedInput = null;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        _deadzoneSquared = movementDeadzone * movementDeadzone;
    }

    void Update()
    {
        if (!enableInput) return;

        PollInputs();
        ProcessInputs();
        UpdateInputBuffer();
    }

    #region 입력 폴링 (한 곳에서만 Input 클래스 접근)
    void PollInputs()
    {
        // 이동 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _currentMovementInput = new Vector2(h, v);

        // 데드존 적용
        if (_currentMovementInput.sqrMagnitude < _deadzoneSquared)
            _currentMovementInput = Vector2.zero;

        // 스킬 입력 (1, 2, 3, 4)
        _skillPressed[0] = Input.GetKey(KeyCode.Alpha1);
        _skillPressed[1] = Input.GetKey(KeyCode.Alpha2);
        _skillPressed[2] = Input.GetKey(KeyCode.Alpha3);
        _skillPressed[3] = Input.GetKey(KeyCode.Alpha4);

        // 액션 입력
        _jumpPressed = Input.GetKey(KeyCode.Space);
        _dashPressed = Input.GetKey(KeyCode.LeftShift);
        _interactPressed = Input.GetKey(KeyCode.E);
        _pausePressed = Input.GetKey(KeyCode.Escape);

        // 디버그 키
        if (Input.GetKeyDown(KeyCode.F3))
            OnDebugToggle?.Invoke();
    }
    #endregion

    #region 입력 처리 및 이벤트 발생
    void ProcessInputs()
    {
        // 이동 입력 (매 프레임 발생)
        if (_currentMovementInput.sqrMagnitude > 0)
            OnMovementInput?.Invoke(_currentMovementInput);

        // 스킬 입력 (키다운 감지)
        for (int i = 0; i < 4; i++)
        {
            if (_skillPressed[i] && !_previousSkillState[i])
            {
                OnSkillInput?.Invoke(i + 1); // 1-4로 전달
                BufferInput(InputType.Skill1 + i, i);
            }
            _previousSkillState[i] = _skillPressed[i];
        }

        // 점프 (키다운)
        if (_jumpPressed && !_previousJumpState)
        {
            OnJumpInput?.Invoke();
            BufferInput(InputType.Jump);
        }
        _previousJumpState = _jumpPressed;

        // 대쉬 (키다운)
        if (_dashPressed && !_previousDashState)
        {
            OnDashInput?.Invoke();
            BufferInput(InputType.Dash);
        }
        _previousDashState = _dashPressed;

        // 상호작용 (키다운)
        if (_interactPressed && !_previousInteractState)
            OnInteractInput?.Invoke();
        _previousInteractState = _interactPressed;

        // 일시정지 (키다운)
        if (_pausePressed && !_previousPauseState)
            OnPauseInput?.Invoke();
        _previousPauseState = _pausePressed;
    }
    #endregion

    #region 입력 버퍼 관리
    void BufferInput(InputType type, int skillIndex = 0)
    {
        _bufferedInput = new BufferedInput
        {
            type = type,
            timestamp = Time.time,
            skillIndex = skillIndex
        };
    }

    void UpdateInputBuffer()
    {
        if (_bufferedInput.HasValue)
        {
            float elapsed = Time.time - _bufferedInput.Value.timestamp;
            if (elapsed > inputBufferTime)
                _bufferedInput = null;
        }
    }

    /// <summary>
    /// 버퍼된 입력을 소비합니다.
    /// </summary>
    public bool ConsumeBufferedInput(InputType type, out int skillIndex)
    {
        skillIndex = 0;

        if (_bufferedInput.HasValue && _bufferedInput.Value.type == type)
        {
            skillIndex = _bufferedInput.Value.skillIndex;
            _bufferedInput = null;
            return true;
        }

        return false;
    }
    #endregion

    #region 유틸리티 함수
    /// <summary>
    /// 입력을 일시적으로 비활성화합니다.
    /// </summary>
    public void DisableInput(float duration = 0f)
    {
        enableInput = false;

        if (duration > 0f)
            Invoke(nameof(EnableInput), duration);
    }

    /// <summary>
    /// 입력을 다시 활성화합니다.
    /// </summary>
    public void EnableInput() => enableInput = true;

    /// <summary>
    /// 모든 입력 상태를 초기화합니다.
    /// </summary>
    public void ResetInputStates()
    {
        _currentMovementInput = Vector2.zero;

        for (int i = 0; i < 4; i++)
        {
            _skillPressed[i] = false;
            _previousSkillState[i] = false;
        }

        _jumpPressed = false;
        _dashPressed = false;
        _interactPressed = false;
        _pausePressed = false;

        _previousJumpState = false;
        _previousDashState = false;
        _previousInteractState = false;
        _previousPauseState = false;

        _bufferedInput = null;
    }

    /// <summary>
    /// 현재 이동 입력을 반환합니다.
    /// </summary>
    public Vector2 GetMovementInput() => _currentMovementInput;

    /// <summary>
    /// 특정 스킬 키가 눌려있는지 확인합니다.
    /// </summary>
    public bool IsSkillHeld(int skillIndex)
    {
        if (skillIndex < 1 || skillIndex > 4) return false;
        return _skillPressed[skillIndex - 1];
    }
    #endregion
}
    

[Serializable]
public class InputMapping
{
    public KeyCode moveUp = KeyCode.W;
    public KeyCode moveDown = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    
    public KeyCode skill1 = KeyCode.Alpha1;
    public KeyCode skill2 = KeyCode.Alpha2;
    public KeyCode skill3 = KeyCode.Alpha3;
    public KeyCode skill4 = KeyCode.Alpha4;
    
    public KeyCode jump = KeyCode.Space;
    public KeyCode dash = KeyCode.LeftShift;
    public KeyCode interact = KeyCode.E;
    public KeyCode pause = KeyCode.Escape;
}

/// <summary>
/// InputManager 사용 예제 스크립트
/// </summary>
public class InputManagerExample : MonoBehaviour
{
    void Start()
    {
        // 이벤트 구독
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMovementInput += HandleMovement;
            InputManager.Instance.OnSkillInput += HandleSkill;
            InputManager.Instance.OnJumpInput += HandleJump;
            InputManager.Instance.OnPauseInput += HandlePause;
        }
    }

    void HandleMovement(Vector2 direction)
    {
        // 이동 처리
        Debug.Log($"Movement: {direction}");
    }

    void HandleSkill(int skillIndex)
    {
        // 스킬 처리
        Debug.Log($"Skill {skillIndex} pressed");
    }

    void HandleJump()
    {
        Debug.Log("Jump!");
    }

    void HandlePause()
    {
        Debug.Log("Pause");
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMovementInput -= HandleMovement;
            InputManager.Instance.OnSkillInput -= HandleSkill;
            InputManager.Instance.OnJumpInput -= HandleJump;
            InputManager.Instance.OnPauseInput -= HandlePause;
        }
    }
}