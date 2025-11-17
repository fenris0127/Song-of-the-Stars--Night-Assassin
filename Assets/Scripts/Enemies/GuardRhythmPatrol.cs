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
    #endregion
    
    #region 시야 및 상태
    [Header("▶ 시야 및 상태")]
    public float fieldOfViewAngle = 90f;
    public float viewDistance = 10f;
    public bool isStunned = false;
    public bool isFlashed = false;
    public Vector2 lastDecoyPosition = Vector2.zero;

    [Header("▶ 탐지 설정 (Chasing State)")]
    public bool useGradualDetection = true;
    public float timeToFullDetection = 2f;
    
    private float _detectionProgress = 0f;
    private bool _isAlerted = false; // 경비병의 최종 경계 상태 (MinimapController에서 사용)

    // ⭐ 상태 시스템
    private GuardState _currentState; // ✅ 선언된 필드를 활용
    public bool _stateSystemEnabled = true; // Inspector에서 설정 가능
    #endregion
    
    #region 상태 효과
    private int _paralysisEndBeat = 0;
    private int _flashEndBeat = 0;
    #endregion

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _detectionSystem = GetComponent<ProbabilisticDetection>();
        _cachedTransform = transform;
    }

    void Start()
    {
        if (RhythmManager != null)
        {
            RhythmManager.OnBeatCounted.AddListener(HandleRhythmActions);
            RhythmManager.OnBeatCounted.AddListener(CheckParalysisTimeout);
            RhythmManager.OnBeatCounted.AddListener(CheckFlashTimeout);
        }

        if (_stateSystemEnabled)
        {
            // ✅ 상태 시스템 통합: Start에서 초기 상태 설정
            ChangeState(new GuardPatrollingState(this)); 
        }
    }

    void Update()
    {
        // 리듬에 동기화되지 않는 로직 (예: 발각 진행도 업데이트, 감지 시각화)
        // ProbabilisticDetection의 현재 발각 진행도 가져오기
        _detectionProgress = _detectionSystem.CalculateDetection();
        
        // 상태 시스템 업데이트 (일부 비트 비동기 로직 포함)
        _currentState?.Update();

        // 탐지 진행도 UI 업데이트 (MinimapController 또는 UIManager 호출)
        if (GameServices.UIManager != null)
            GameServices.UIManager.UpdateDetectionProgress(_detectionProgress);
            
        _isAlerted = _detectionProgress > 0f;
    }

    private void HandleRhythmActions(int beat)
    {
        if (GameServices.IsPaused()) return;

        // 상태 시스템에게 리듬 전달
        _currentState?.OnBeat(beat);
    }
    
    public float GetDetectionProgress() => _detectionProgress;
    public bool IsAlerted() => _isAlerted;
    
    private void CheckParalysisTimeout(int beat)
    {
        if (isStunned && beat >= _paralysisEndBeat)
        {
            isStunned = false; // ⭐ isStunned를 false로 설정

            // 상태 전환 로직 추가 (예: ChangeState(new GuardSearchingState(this)))
            if (_currentState is GuardStunnedState) // 현재 기절 상태라면 해제
                 ChangeState(new GuardPatrollingState(this)); // 예시: 순찰 상태로 복귀
        }
    }

    private void CheckFlashTimeout(int beat)
    {
        if (isFlashed && beat >= _flashEndBeat)
        {
            isFlashed = false; // ⭐ isFlashed를 false로 설정
            // 상태 전환 로직 추가
            if (_currentState is GuardStunnedState) // 현재 기절 상태라면 해제
            {
                // isStunned가 true인데 isFlashed만 false가 되는 경우를 고려하거나,
                // 여기서는 isFlashed만 해제하고 isStunned는 Paralyze/Flash에 의해 개별적으로 관리되도록 합니다.
                // (복잡해지므로, 이 경우에는 GuardStunnedState에서 관리하는 것이 더 깔끔합니다.)
                // 이 코드는 현재 isFlashed만 해제하고 다른 상태로의 전환은 하지 않습니다.
            }
        }
    }
    
    /// <summary>
/// 현재 활성화된 데코이가 시야 내에 있는지 확인합니다.
/// </summary>
public bool CheckForDecoy()
{
    // ⭐ PlayerController에 isDecoyActive, DecoyPosition, DecoyObject 속성이 있다고 가정
    if (Player == null || !Player.isDecoyActive) 
        return false;

    Vector2 decoyPos = Player.DecoyPosition; 
    Vector2 guardPosition2D = _cachedTransform.position;

    // 2. 데코이가 현재 경비병의 시야 거리 내에 있는지 확인
    float distanceToDecoy = Vector2.Distance(guardPosition2D, decoyPos);
    if (distanceToDecoy > viewDistance)
        return false;

    // 3. 데코이가 시야각 내에 있는지 확인
    Vector2 directionToDecoy = (decoyPos - guardPosition2D).normalized;
    Vector2 guardForward = _cachedTransform.up; 
    
    if (Vector2.Angle(guardForward, directionToDecoy) < fieldOfViewAngle / 2f)
    {
        // 4. 장애물 레이캐스트 체크 (Physics2D.Raycast는 정적 메서드)
        RaycastHit2D hit = Physics2D.Raycast(_cachedTransform.position, directionToDecoy, distanceToDecoy, ObstacleMask);
        
        // hit.collider가 없거나, hit된 오브젝트가 데코이 본인이라면 감지 성공
        if (hit.collider == null || (Player.DecoyObject != null && hit.collider.gameObject == Player.DecoyObject)) 
        {
            // 데코이 감지 성공! 조사할 위치를 저장합니다.
            lastDecoyPosition = decoyPos;
            
            // Player.DeactivateDecoy(); // 필요하다면 데코이 비활성화 로직 추가
            
            return true;
        }
    }

    return false;
}

    public void Paralyze(int durationBeats)
    {
        // 1. GuardRhythmPatrol의 isStunned 플래그를 true로 설정 (다른 컴포넌트에서 이 플래그를 참조할 수 있도록)
        isStunned = true;
        isFlashed = false; // 마비는 섬광이 아니므로 false
        _paralysisEndBeat = RhythmManager.currentBeatCount + durationBeats;

        // 2. GuardStunnedState로 전환. durationBeats를 전달하여 상태 내부에서 관리하도록 함.
        ChangeState(new GuardStunnedState(this, durationBeats, false)); 
    }
    
    public void Flash(int durationBeats)
    {
        // 1. GuardRhythmPatrol의 isFlashed와 isStunned 플래그를 true로 설정
        isFlashed = true;
        isStunned = true;
        _flashEndBeat = RhythmManager.currentBeatCount + durationBeats;

        // 2. GuardStunnedState로 전환. durationBeats와 isFlashed=true를 전달.
        ChangeState(new GuardStunnedState(this, durationBeats, true));
    }
    
    // ------------------------------------------------------------------
    // Compatibility aliases used by other scripts
    // ------------------------------------------------------------------
    public void ApplyFlash(int durationBeats) => Flash(durationBeats);
    public void ApplyParalysis(int durationBeats) => Paralyze(durationBeats);
    
    /// <summary>
    /// Takes damage from skills or attacks
    /// 스킬이나 공격으로부터 피해를 받습니다
    /// </summary>
    public void TakeDamage(float damage)
    {
        // Guard has no health system - any damage kills instantly
        // This matches the instant-kill assassination design
        Debug.Log($"Guard {gameObject.name} took {damage} damage - eliminated");
        Die();
    }

    // ⭐ 플레이어 암살 시 호출되는 메서드
    public void Die()
    {
        // 경비병 제거 로직 (보고서에 명시된 내용 반영)
        
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
    /// 상태 시스템 활성화/비활성화 (디버깅용) (보고서 버그 수정 반영)
    /// </summary>
    public void SetStateSystemEnabled(bool enabled)
    {
        _stateSystemEnabled = enabled;
        
        if (enabled && _currentState == null)
            ChangeState(new GuardPatrollingState(this));
        else if (!enabled)
        {
            _currentState?.Exit();
            _currentState = null;
        }
    }
}