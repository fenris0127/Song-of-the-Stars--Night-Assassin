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
    private float _deadzoneSquared; // ✅ Deadzone 제곱 캐시
    #endregion

    #region 내부 상태
    private Vector2 _currentMovementInput = Vector2.zero;
    public Vector2 CurrentMovementInput => _currentMovementInput;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // ✅ 개선: Deadzone 제곱 연산을 Awake에서 한 번만 수행
        _deadzoneSquared = movementDeadzone * movementDeadzone;
    }

    void Update()
    {
        if (!enableInput || GameServices.IsPaused()) return;

        PollInputs();
        ProcessSkillInputs();
        ProcessActionInputs();
    }
    
    void PollInputs()
    {
        float x = 0f, y = 0f;

        if (Input.GetKey(keyMapping.up)) y += 1f;
        if (Input.GetKey(keyMapping.down)) y -= 1f;
        if (Input.GetKey(keyMapping.right)) x += 1f;
        if (Input.GetKey(keyMapping.left)) x -= 1f;

        _currentMovementInput = new Vector2(x, y).normalized;
        
        // 개선: 제곱근 연산 대신 제곱된 값과 비교
        if (_currentMovementInput.sqrMagnitude < _deadzoneSquared)
            _currentMovementInput = Vector2.zero;

        OnMovementInput?.Invoke(_currentMovementInput);
    }
    
    void ProcessSkillInputs()
    {
        if (Input.GetKeyDown(keyMapping.skill1)) OnSkillInput?.Invoke(1);
        if (Input.GetKeyDown(keyMapping.skill2)) OnSkillInput?.Invoke(2);
        if (Input.GetKeyDown(keyMapping.skill3)) OnSkillInput?.Invoke(3);
        if (Input.GetKeyDown(keyMapping.skill4)) OnSkillInput?.Invoke(4);
    }

    void ProcessActionInputs()
    {
        if (Input.GetKeyDown(keyMapping.jump)) OnJumpInput?.Invoke();
        if (Input.GetKeyDown(keyMapping.dash)) OnDashInput?.Invoke();
        if (Input.GetKeyDown(keyMapping.interact)) OnInteractInput?.Invoke();
        if (Input.GetKeyDown(keyMapping.pause)) OnPauseInput?.Invoke();

        if (Input.GetKeyDown(KeyCode.F1)) OnDebugToggle?.Invoke();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

[System.Serializable]
public class InputMapping
{
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;

    public KeyCode skill1 = KeyCode.Alpha1;
    public KeyCode skill2 = KeyCode.Alpha2;
    public KeyCode skill3 = KeyCode.Alpha3;
    public KeyCode skill4 = KeyCode.Alpha4;
    
    public KeyCode jump = KeyCode.Space;
    public KeyCode dash = KeyCode.LeftShift;
    public KeyCode interact = KeyCode.E;
    public KeyCode pause = KeyCode.Escape;
}