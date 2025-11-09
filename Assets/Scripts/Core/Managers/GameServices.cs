using UnityEngine;

/// <summary>
/// 게임 핵심 매니저 중앙 참조 - Null 체크 최적화 및 캐싱 강화
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

    #region Cached Properties (⭐ Null 체크 최적화)
    private static bool _managersCached = false;
    
    // Managers
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
            if (Instance._playerStealth == null && Instance._playerController != null)
                Instance._playerStealth = Instance._playerController.GetComponent<PlayerStealth>();
            return Instance._playerStealth;
        }
    }

    public static PlayerAssassination PlayerAssassination
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._playerAssassination == null && Instance._playerController != null)
                Instance._playerAssassination = Instance._playerController.GetComponent<PlayerAssassination>();
            return Instance._playerAssassination;
        }
    }

    public static RhythmPatternChecker RhythmChecker
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._rhythmChecker == null && Instance._playerController != null)
                Instance._rhythmChecker = Instance._playerController.GetComponent<RhythmPatternChecker>();
            return Instance._rhythmChecker;
        }
    }

    public static SkillLoadoutManager SkillLoadout
    {
        get
        {
            if (Instance == null) return null;
            if (Instance._skillLoadout == null && Instance._playerController != null)
                Instance._skillLoadout = Instance._playerController.GetComponent<SkillLoadoutManager>();
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

    // Singleton Managers (DontDestroyOnLoad)
    public static SFXManager SFX => SFXManager.Instance;
    public static DifficultyManager Difficulty => DifficultyManager.Instance;
    public static SaveSystem SaveSystem => SaveSystem.Instance;
    public static GameManager GameManager => GameManager.Instance;
    public static ObjectPoolManager ObjectPool => ObjectPoolManager.Instance;
    public static VFXManager_Pooled VFXPooled => VFXManager_Pooled.Instance;
    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _managersCached = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        AutoFindReferences();
    }

    void AutoFindReferences()
    {
        bool anyMissing = false;

        if (_rhythmManager == null)
        {
            _rhythmManager = FindAnyObjectByType<RhythmSyncManager>();
            anyMissing |= _rhythmManager == null;
        }

        if (_missionManager == null)
        {
            _missionManager = FindAnyObjectByType<MissionManager>();
            anyMissing |= _missionManager == null;
        }

        if (_uiManager == null)
        {
            _uiManager = FindAnyObjectByType<UIManager>();
            anyMissing |= _uiManager == null;
        }

        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<PlayerController>();
            
            if (_playerController != null)
            {
                _playerStealth = _playerController.GetComponent<PlayerStealth>();
                _playerAssassination = _playerController.GetComponent<PlayerAssassination>();
                _rhythmChecker = _playerController.GetComponent<RhythmPatternChecker>();
                _skillLoadout = _playerController.GetComponent<SkillLoadoutManager>();
            }
        }

        if (_minimapController == null)
            _minimapController = FindAnyObjectByType<MinimapController>();

        if (_vfxManager == null)
            _vfxManager = FindAnyObjectByType<VFXManager>();

        if (_tutorialManager == null)
            _tutorialManager = FindAnyObjectByType<TutorialManager>();

        _managersCached = !anyMissing;
        
        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (_rhythmManager == null)
            Debug.LogWarning("[GameServices] RhythmSyncManager 미할당");

        if (_playerController == null)
            Debug.LogWarning("[GameServices] PlayerController 미할당");

        if (_missionManager == null)
            Debug.LogWarning("[GameServices] MissionManager 미할당");
    }

    public static void RefreshReferences()
    {
        if (Instance != null)
        {
            _managersCached = false;
            Instance.AutoFindReferences();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            _managersCached = false;
        }
    }

    #region Helper Methods
    public static bool IsPaused() => GameManager != null && GameManager.isPaused;
    public static int CurrentBeat() => RhythmManager != null ? RhythmManager.currentBeatCount : 0;
    public static float BeatInterval() => RhythmManager != null ? RhythmManager.beatInterval : 0.5f;
    public static Vector3 PlayerPosition() => Player != null ? Player.transform.position : Vector3.zero;
    
    /// <summary>
    /// 모든 필수 매니저가 준비되었는지 확인
    /// </summary>
    public static bool IsReady()
    {
        return Instance != null && 
               RhythmManager != null && 
               Player != null && 
               MissionManager != null;
    }
    #endregion
}