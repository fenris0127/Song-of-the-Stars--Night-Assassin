using UnityEngine;

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

    #region Public Properties
    // Managers
    public static RhythmSyncManager RhythmManager => Instance?._rhythmManager;
    public static MissionManager MissionManager => Instance?._missionManager;
    public static UIManager UIManager => Instance?._uiManager;

    // Player
    public static PlayerController Player => Instance?._playerController;
    public static PlayerStealth PlayerStealth => Instance?._playerStealth;
    public static PlayerAssassination PlayerAssassination => Instance?._playerAssassination;
    public static RhythmPatternChecker RhythmChecker => Instance?._rhythmChecker;
    public static SkillLoadoutManager SkillLoadout => Instance?._skillLoadout;

    // Other Systems
    public static MinimapController MinimapController => Instance?._minimapController;
    public static VFXManager VFXManager => Instance?._vfxManager;
    public static TutorialManager TutorialManager => Instance?._tutorialManager;

    // Singleton Managers (외부 DontDestroyOnLoad 싱글톤)
    public static SFXManager SFX => SFXManager.Instance;
    public static DifficultyManager Difficulty => DifficultyManager.Instance;
    public static SaveSystem SaveSystem => SaveSystem.Instance;
    public static GameManager GameManager => GameManager.Instance;
    public static ObjectPoolManager ObjectPool => ObjectPoolManager.Instance;
    #endregion

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        AutoFindReferences();
    }

    /// <summary>
    /// Inspector에서 할당되지 않은 참조를 자동으로 찾습니다.
    /// </summary>
    void AutoFindReferences()
    {
        if (_rhythmManager == null)
            _rhythmManager = FindAnyObjectByType<RhythmSyncManager>();

        if (_missionManager == null)
            _missionManager = FindAnyObjectByType<MissionManager>();

        if (_uiManager == null)
            _uiManager = FindAnyObjectByType<UIManager>();

        if (_playerController == null)
            _playerController = FindAnyObjectByType<PlayerController>();

        if (_playerStealth == null && _playerController != null)
            _playerStealth = _playerController.GetComponent<PlayerStealth>();

        if (_playerAssassination == null && _playerController != null)
            _playerAssassination = _playerController.GetComponent<PlayerAssassination>();

        if (_rhythmChecker == null && _playerController != null)
            _rhythmChecker = _playerController.GetComponent<RhythmPatternChecker>();

        if (_skillLoadout == null && _playerController != null)
            _skillLoadout = _playerController.GetComponent<SkillLoadoutManager>();

        if (_minimapController == null)
            _minimapController = FindAnyObjectByType<MinimapController>();

        if (_vfxManager == null)
            _vfxManager = FindAnyObjectByType<VFXManager>();

        if (_tutorialManager == null)
            _tutorialManager = FindAnyObjectByType<TutorialManager>();

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

    /// <summary>
    /// 씬 전환 후 참조를 다시 찾습니다.
    /// </summary>
    public static void RefreshReferences()
    {
        if (Instance != null)
            Instance.AutoFindReferences();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #region Helper Methods
    /// <summary>
    /// 게임이 일시정지 상태인지 확인합니다.
    /// </summary>
    public static bool IsPaused()
    {
        return GameManager != null && GameManager.isPaused;
    }

    /// <summary>
    /// 현재 비트 카운트를 반환합니다.
    /// </summary>
    public static int CurrentBeat()
    {
        return RhythmManager != null ? RhythmManager.currentBeatCount : 0;
    }

    /// <summary>
    /// 비트 간격을 반환합니다.
    /// </summary>
    public static float BeatInterval()
    {
        return RhythmManager != null ? RhythmManager.beatInterval : 0.5f;
    }

    /// <summary>
    /// 플레이어 위치를 반환합니다.
    /// </summary>
    public static Vector3 PlayerPosition()
    {
        return Player != null ? Player.transform.position : Vector3.zero;
    }
    #endregion
}