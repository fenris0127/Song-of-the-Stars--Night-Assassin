using UnityEngine;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Expert
}

/// <summary>
/// 난이도 설정을 관리하고 적용합니다.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("▶ 난이도 설정")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("▶ 난이도별 설정")]
    public DifficultySettings easySettings;
    public DifficultySettings normalSettings;
    public DifficultySettings hardSettings;
    public DifficultySettings expertSettings;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 현재 난이도 설정을 가져옵니다.
    /// </summary>
    public DifficultySettings GetCurrentSettings()
    {
        return currentDifficulty switch
        {
            DifficultyLevel.Easy => easySettings,
            DifficultyLevel.Normal => normalSettings,
            DifficultyLevel.Hard => hardSettings,
            DifficultyLevel.Expert => expertSettings,
            _ => normalSettings
        };
    }

    /// <summary>
    /// 난이도를 변경하고 모든 매니저에 적용합니다.
    /// </summary>
    public void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        ApplyDifficultySettings();
    }

    /// <summary>
    /// 씬의 모든 오브젝트에 난이도 설정을 적용합니다.
    /// </summary>
    public void ApplyDifficultySettings()
    {
        DifficultySettings settings = GetCurrentSettings();

        // RhythmSyncManager에 적용
        RhythmSyncManager rhythmManager = FindObjectOfType<RhythmSyncManager>();
        if (rhythmManager != null)
        {
            rhythmManager.beatTolerance = settings.beatTolerance;
            rhythmManager.perfectTolerance = settings.perfectTolerance;
        }

        // MissionManager에 적용
        MissionManager missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
            missionManager.maxAlertLevel = settings.maxAlertLevel;

        // PlayerController에 적용
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.moveSpeed = settings.playerMoveSpeed;

        // PlayerStealth에 적용
        PlayerStealth stealth = FindObjectOfType<PlayerStealth>();
        if (stealth != null)
            stealth.stealthMoveSpeedMultiplier = settings.stealthSpeedMultiplier;

        // RhythmPatternChecker에 적용
        RhythmPatternChecker rhythmChecker = FindObjectOfType<RhythmPatternChecker>();
        if (rhythmChecker != null)
        {
            rhythmChecker.maxFocus = settings.maxFocus;
            rhythmChecker.focusPerPerfect = settings.focusPerPerfect;
        }

        // 모든 경비병에 적용
        GuardRhythmPatrol[] guards = FindObjectsOfType<GuardRhythmPatrol>();
        foreach (GuardRhythmPatrol guard in guards)
        {
            guard.viewDistance = settings.guardViewDistance;
            guard.viewAngle = settings.guardViewAngle;
            guard.moveSpeed = settings.guardMoveSpeed;
            guard.patrolBeatIntervalMax = settings.guardPatrolInterval;
            guard.timeToFullDetection = settings.detectionTime;
        }

        Debug.Log($"난이도 적용: {currentDifficulty} - 판정 허용: {settings.beatTolerance}s");
    }

    /// <summary>
    /// 스킬 쿨타임에 난이도 배수 적용
    /// </summary>
    public int ApplyCooldownMultiplier(int baseCooldown)
    {
        DifficultySettings settings = GetCurrentSettings();
        return Mathf.Max(1, Mathf.RoundToInt(baseCooldown * settings.cooldownMultiplier));
    }
}

[System.Serializable]
    public class DifficultySettings
    {
        [Header("리듬 판정")]
        public float beatTolerance = 0.1f;
        public float perfectTolerance = 0.05f;

        [Header("경비병 설정")]
        public float guardViewDistance = 10f;
        public float guardViewAngle = 100f;
        public float guardMoveSpeed = 6f;
        public int guardPatrolInterval = 4;
        public float detectionTime = 2f;

        [Header("플레이어 설정")]
        public float playerMoveSpeed = 8f;
        public float stealthSpeedMultiplier = 0.5f;
        public float maxFocus = 100f;
        public float focusPerPerfect = 10f;

        [Header("스킬 쿨타임 배수")]
        [Range(0.5f, 2f)]
        public float cooldownMultiplier = 1f;

        [Header("경보 설정")]
        public int maxAlertLevel = 5;
        public int alertIncreaseOnDetection = 2;
    }