using UnityEngine;
using SongOfTheStars.Data;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Expert
}

/// <summary>
/// Manages difficulty settings and applies them to game systems
/// 난이도 설정을 관리하고 게임 시스템에 적용합니다.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    // Event fired when difficulty changes
    public static event System.Action<DifficultySettings> OnDifficultyChanged;

    [Header("▶ Current Difficulty")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("▶ Difficulty Presets (ScriptableObjects)")]
    [Tooltip("Assign ScriptableObject presets for each difficulty")]
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
    /// Changes difficulty and applies settings to all game systems
    /// 난이도를 변경하고 모든 게임 시스템에 적용합니다.
    /// </summary>
    public void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        ApplyDifficultySettings();
    }

    /// <summary>
    /// Applies difficulty settings to scene objects and fires event
    /// 씬의 모든 오브젝트에 난이도 설정을 적용하고 이벤트를 발생시킵니다.
    /// </summary>
    public void ApplyDifficultySettings()
    {
        DifficultySettings settings = GetCurrentSettings();

        if (settings == null)
        {
            Debug.LogError($"DifficultySettings for {currentDifficulty} is null! Please assign in Inspector.");
            return;
        }

        // Apply to core managers via GameServices
        ApplyToManagers(settings);

        // Apply to scene objects (guards, etc.)
        ApplyToSceneObjects(settings);

        // Fire event for any listeners
        OnDifficultyChanged?.Invoke(settings);

        Debug.Log($"Difficulty applied: {currentDifficulty} | Beat tolerance: {settings.beatTolerance}s | Perfect: {settings.perfectTolerance}s");
    }

    /// <summary>
    /// Applies settings to manager singletons
    /// </summary>
    private void ApplyToManagers(DifficultySettings settings)
    {
        // Rhythm System
        if (GameServices.RhythmManager != null)
        {
            GameServices.RhythmManager.beatTolerance = settings.beatTolerance;
            GameServices.RhythmManager.perfectTolerance = settings.perfectTolerance;
        }

        // Mission System
        if (GameServices.MissionManager != null)
            GameServices.MissionManager.maxAlertLevel = settings.maxAlertLevel;

        // Player Movement
        if (GameServices.Player != null)
            GameServices.Player.moveSpeed = settings.playerMoveSpeed;

        // Player Stealth
        if (GameServices.PlayerStealth != null)
            GameServices.PlayerStealth.stealthMoveSpeedMultiplier = settings.stealthSpeedMultiplier;

        // Focus System
        if (GameServices.RhythmChecker != null)
        {
            GameServices.RhythmChecker.maxFocus = settings.maxFocus;
            GameServices.RhythmChecker.focusPerPerfect = settings.focusPerPerfect;
        }
    }

    /// <summary>
    /// Applies settings to scene objects (guards, etc.)
    /// Note: Uses FindObjectsOfType which is expensive, but only called on difficulty change
    /// </summary>
    private void ApplyToSceneObjects(DifficultySettings settings)
    {
        // Apply to all guards in scene
        GuardRhythmPatrol[] guards = FindObjectsOfType<GuardRhythmPatrol>();
        foreach (GuardRhythmPatrol guard in guards)
        {
            if (guard != null)
            {
                guard.viewDistance = settings.guardViewDistance;
                guard.fieldOfViewAngle = settings.guardViewAngle;
                guard.moveSpeed = settings.guardMoveSpeed;
                guard.patrolBeatIntervalMax = settings.guardPatrolInterval;
                guard.timeToFullDetection = settings.detectionTime;
            }
        }

        if (guards.Length > 0)
        {
            Debug.Log($"Applied difficulty to {guards.Length} guards");
        }
    }

    /// <summary>
    /// Applies difficulty cooldown multiplier to base cooldown
    /// 스킬 쿨타임에 난이도 배수 적용
    /// </summary>
    public int ApplyCooldownMultiplier(int baseCooldown)
    {
        DifficultySettings settings = GetCurrentSettings();
        if (settings == null) return baseCooldown;

        return Mathf.Max(1, Mathf.RoundToInt(baseCooldown * settings.cooldownMultiplier));
    }
}