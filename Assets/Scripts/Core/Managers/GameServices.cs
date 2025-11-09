using UnityEngine;
using System.Collections.Generic;  // For List<T>

/// <summary>
/// 게임의 모든 핵심 매니저와 시스템에 대한 중앙 참조 관리자
/// FindObjectOfType 호출을 제거하여 성능을 개선합니다.
/// 
/// 사용법: GameServices.RhythmManager, GameServices.Player 등
/// </summary>
public class GameServices : MonoBehaviour
{
    public static GameServices Instance { get; private set; }

    #region Manager References
    [Header("▶ Core Managers")]
    [SerializeField] private RhythmSyncManager _rhythmManager;
    [SerializeField] private MissionManager _missionManager;
    [SerializeField] private UIManager _uiManager;

    [Header("▶ Player References")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerStealth _playerStealth;
    [SerializeField] private PlayerAssassination _playerAssassination;
    [SerializeField] private RhythmPatternChecker _rhythmChecker;
    [SerializeField] private SkillLoadoutManager _skillLoadout;

    [Header("▶ Other Systems")]
    [SerializeField] private MinimapController _minimapController;
    [SerializeField] private VFXManager _vfxManager;
    [SerializeField] private TutorialManager _tutorialManager;
    #endregion
    
    private static bool _managersCached = false;

    #region Public Properties (Optimized)
    // RhythmSyncManager의 참조를 지연 로딩 및 캐싱하여 반환합니다.
    // FindObjectOfType 호출을 최소화합니다.
    public static RhythmSyncManager RhythmManager
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._rhythmManager == null && !_managersCached)
                Instance._rhythmManager = FindAnyObjectByType<RhythmSyncManager>();
            return Instance._rhythmManager;
        }
    }
    
    public static MissionManager MissionManager
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._missionManager == null && !_managersCached)
                Instance._missionManager = FindAnyObjectByType<MissionManager>();
            return Instance._missionManager;
        }
    }
    
    public static UIManager UIManager
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._uiManager == null && !_managersCached)
                Instance._uiManager = FindAnyObjectByType<UIManager>();
            return Instance._uiManager;
        }
    }

    // Player
    public static PlayerController Player
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._playerController == null && !_managersCached)
                Instance._playerController = FindAnyObjectByType<PlayerController>();
            return Instance._playerController;
        }
    }
    
    public static PlayerStealth PlayerStealth
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._playerStealth == null && !_managersCached)
                Instance._playerStealth = FindAnyObjectByType<PlayerStealth>();
            return Instance._playerStealth;
        }
    }
    
    public static PlayerAssassination PlayerAssassination
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._playerAssassination == null && !_managersCached)
                Instance._playerAssassination = FindAnyObjectByType<PlayerAssassination>();
            return Instance._playerAssassination;
        }
    }
    
    public static RhythmPatternChecker RhythmChecker
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._rhythmChecker == null && !_managersCached)
                Instance._rhythmChecker = FindAnyObjectByType<RhythmPatternChecker>();
            return Instance._rhythmChecker;
        }
    }
    
    public static SkillLoadoutManager SkillLoadout
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._skillLoadout == null && !_managersCached)
                Instance._skillLoadout = FindAnyObjectByType<SkillLoadoutManager>();
            return Instance._skillLoadout;
        }
    }

    // Other Systems
    public static MinimapController MinimapController
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._minimapController == null && !_managersCached)
                Instance._minimapController = FindAnyObjectByType<MinimapController>();
            return Instance._minimapController;
        }
    }
    
    public static VFXManager VFXManager
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._vfxManager == null && !_managersCached)
                Instance._vfxManager = FindAnyObjectByType<VFXManager>();
            return Instance._vfxManager;
        }
    }
    
    public static TutorialManager TutorialManager
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._tutorialManager == null && !_managersCached)
                Instance._tutorialManager = FindAnyObjectByType<TutorialManager>();
            return Instance._tutorialManager;
        }
    }

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AutoFindReferences();
        }
        else if (Instance != this)
            Destroy(gameObject);
    }

    // Inspector에서 할당되지 않은 참조를 자동으로 찾습니다.
    void AutoFindReferences()
    {
        // 씬에서 찾거나, 이미 할당된 값 유지
        if (_rhythmManager == null) _rhythmManager = FindAnyObjectByType<RhythmSyncManager>();
        if (_missionManager == null) _missionManager = FindAnyObjectByType<MissionManager>();
        if (_uiManager == null) _uiManager = FindAnyObjectByType<UIManager>();
        
        if (_playerController == null) _playerController = FindAnyObjectByType<PlayerController>();
        if (_playerStealth == null) _playerStealth = FindAnyObjectByType<PlayerStealth>();
        if (_playerAssassination == null) _playerAssassination = FindAnyObjectByType<PlayerAssassination>();
        if (_rhythmChecker == null) _rhythmChecker = FindAnyObjectByType<RhythmPatternChecker>();
        if (_skillLoadout == null) _skillLoadout = FindAnyObjectByType<SkillLoadoutManager>();

        if (_minimapController == null) _minimapController = FindAnyObjectByType<MinimapController>();
        if (_vfxManager == null) _vfxManager = FindAnyObjectByType<VFXManager>();
        if (_tutorialManager == null) _tutorialManager = FindAnyObjectByType<TutorialManager>();
        
        _managersCached = true; // 최초 로딩 완료 표시
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (_rhythmManager == null)
            Debug.LogWarning("[GameServices] RhythmSyncManager를 찾을 수 없습니다!");

        if (_playerController == null)
            Debug.LogWarning("[GameServices] PlayerController를 찾을 수 없습니다!");

        if (_missionManager == null)
            Debug.LogWarning("[GameServices] MissionManager를 찾을 수 없습니다!");
    }

    // 씬 전환 후 참조를 다시 찾습니다.
    public static void RefreshReferences()
    {
        _managersCached = false;
        if (Instance != null)
            Instance.AutoFindReferences();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #region Helper Methods
    // 게임이 일시정지 상태인지 확인합니다.
    public static bool IsPaused() => GameManager.Instance != null && GameManager.Instance.isPaused;

    // 현재 비트 카운트를 반환합니다.
    public static int CurrentBeat() => RhythmManager != null ? RhythmManager.currentBeatCount : 0;

    // 비트 간격을 반환합니다.
    public static float BeatInterval() => RhythmManager != null ? RhythmManager.beatInterval : 0.5f;

    // 플레이어 위치를 반환합니다.
    public static Vector3 PlayerPosition() => Player != null ? Player.transform.position : Vector3.zero;

    // Centralized physics helpers to avoid assembly/lookup issues
    private static ContactFilter2D _filter = ContactFilter2D.noFilter;  // Using the static property instead of deprecated method
    private static readonly List<Collider2D> _tempColliders = new List<Collider2D>();
    private static readonly List<RaycastHit2D> _tempRaycastHits = new List<RaycastHit2D>();

    private static ContactFilter2D GetFilter(LayerMask mask)
    {
        _filter.SetLayerMask(mask);
        _filter.useLayerMask = true;
        return _filter;
    }

    public static int OverlapCircleCompat(Vector2 point, float radius, LayerMask mask, Collider2D[] results)
    {
        if (results == null || results.Length == 0) return 0;

        int count = Physics2D.OverlapCircle(point, radius, GetFilter(mask), _tempColliders);
        count = Mathf.Min(count, results.Length);

        for (int i = 0; i < count; i++)
            results[i] = _tempColliders[i];
        for (int i = count; i < results.Length; i++)
            results[i] = null;

        return count;
    }

    public static bool RaycastCompat(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance = Mathf.Infinity, int layerMask = Physics2D.DefaultRaycastLayers)
    {
        if (Physics2D.Raycast(origin, direction, GetFilter(layerMask), _tempRaycastHits, distance) > 0)
        {
            hit = _tempRaycastHits[0];
            return true;
        }
        hit = default;
        return false;
    }

    public static bool HasLineOfSight(Vector2 from, Vector2 to, LayerMask obstacleMask)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        direction.Normalize();

        // Return true if there are NO obstacles (raycast hit count is 0)
        return Physics2D.Raycast(from, direction, GetFilter(obstacleMask), _tempRaycastHits, distance) == 0;
    }
    #endregion
}