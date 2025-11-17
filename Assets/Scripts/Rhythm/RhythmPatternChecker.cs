using UnityEngine;

/// <summary>
/// 리듬 판정 및 스킬 관리 (완전 최적화 버전)
/// </summary>
public class RhythmPatternChecker : MonoBehaviour
{
    #region 컴포넌트 참조
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private UIManager UI => GameServices.UIManager;
    private SkillLoadoutManager LoadoutManager => GameServices.SkillLoadout;
    private PlayerAssassination PlayerAssassination => GameServices.PlayerAssassination;
    private PlayerController Player => GameServices.Player;
    #endregion
    
    #region Focus 시스템
    [Header("▶ Focus 시스템")]
    [Tooltip("Focus values can be overridden by Difficulty settings")]
    public float maxFocus = 100f;
    public float focusPerPerfect = 10f;
    public float focusCostPerSkill = 15f; // Increased from 5 for better balance
    public float focusDecayPerMiss = 15f;
    public float currentFocus = 0f;

    [Header("▶ Perfect Combo Bonuses")]
    [Tooltip("Cooldown multiplier for perfect combo (0.67 = 33% reduction)")]
    [Range(0.5f, 1f)]
    public float perfectComboCooldownMultiplier = 0.67f; // Changed from 0.5 (50% reduction)
    #endregion

    #region 스킬 상태
    private int[] _skillCooldownBeats = new int[4];
    private ConstellationSkillData[] _skillDataCache = new ConstellationSkillData[4];
    private bool _skillDataCached = false;
    
    private KeyCode _currentActiveSkillKey = KeyCode.None;
    private int _inputSequenceCount = 0;
    private bool _isCurrentComboPerfect = true;
    
    private static readonly KeyCode[] SKILL_KEYS = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4
    };
    
    private Collider2D[] _guardCheckResults = new Collider2D[20];
    #endregion

    void Start()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.AddListener(CheckAllCooldowns);
        else
            Debug.LogError("RhythmPatternChecker: RhythmSyncManager 없음!");
            
        CacheSkillData();
    }

    void CacheSkillData()
    {
        if (_skillDataCached || LoadoutManager == null) return;

        for (int i = 0; i < 4; i++)
        {
            KeyCode key = SKILL_KEYS[i];
            if (LoadoutManager.activeSkills.TryGetValue(key, out ConstellationSkillData skill))
                _skillDataCache[i] = skill;
        }
        
        _skillDataCached = true;
    }

    void Update()
    {
        if (Player != null && !Player.isFreeMoving)
            HandleRhythmInput();
    }

    void HandleRhythmInput()
    {
        if (!_skillDataCached) 
            CacheSkillData();

        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(SKILL_KEYS[i]))
            {
                ProcessSkillInput(SKILL_KEYS[i], i);
                break;
            }
        }
    }

    void ProcessSkillInput(KeyCode key, int skillIndex)
    {
        if (_skillDataCache[skillIndex] == null) return;

        RhythmJudgment judgment = RhythmManager.CheckJudgment();
        
        if (UI != null)
            UI.ShowJudgment(judgment);

        PlayJudgmentSound(judgment);

        if (judgment == RhythmJudgment.Miss)
        {
            DecreaseFocus(focusDecayPerMiss);
            ResetInputSequence();
            return;
        }

        if (judgment == RhythmJudgment.Perfect)
            IncreaseFocus(focusPerPerfect);
        else
            _isCurrentComboPerfect = false;

        if (_currentActiveSkillKey == KeyCode.None || _currentActiveSkillKey == key)
            ContinueSkillSequence(key, skillIndex);
        else
            StartNewSkillSequence(key, skillIndex);
    }

    void PlayJudgmentSound(RhythmJudgment judgment)
    {
        var sfx = SFXManager.Instance;
        if (sfx == null) return;

        switch (judgment)
        {
            case RhythmJudgment.Perfect:
                sfx.PlayPerfectSound();
                break;
            case RhythmJudgment.Great:
                sfx.PlayGreatSound();
                break;
            case RhythmJudgment.Miss:
                sfx.PlayMissSound();
                break;
        }
    }
    
    public void SetSkillCooldown(int skillIndex, int beats)
    {
        if (RhythmManager == null || skillIndex < 0 || skillIndex >= 4) return;

        int adjustedBeats = beats;
        var difficulty = DifficultyManager.Instance;
        if (difficulty != null)
            adjustedBeats = difficulty.ApplyCooldownMultiplier(beats);
        
        _skillCooldownBeats[skillIndex] = RhythmManager.currentBeatCount + adjustedBeats;
    }
    
    void StartNewSkillSequence(KeyCode key, int skillIndex)
    {
        _isCurrentComboPerfect = true;
        _currentActiveSkillKey = key;
        _inputSequenceCount = 1;

        ConstellationSkillData skill = _skillDataCache[skillIndex];
        
        if (skill.inputCount == 1)
        {
            ActivateSkill(skill, skillIndex);
            ResetInputSequence();
        }
    }

    void ContinueSkillSequence(KeyCode key, int skillIndex)
    {
        if (_currentActiveSkillKey == KeyCode.None)
        {
            StartNewSkillSequence(key, skillIndex);
            return;
        }

        if (_currentActiveSkillKey != key) return;

        _inputSequenceCount++;
        ConstellationSkillData skill = _skillDataCache[skillIndex];

        if (_inputSequenceCount >= skill.inputCount)
        {
            ActivateSkill(skill, skillIndex);
            ResetInputSequence();
        }
    }
    
    void ApplyFlashToClosestGuard(int durationInBeats)
    {
        if (RhythmManager == null) return;
        
        float effectRange = 10f;
        Vector2 playerPos = transform.position;

        int hitCount = Physics2D.OverlapCircleNonAlloc(
            playerPos, effectRange, _guardCheckResults, RhythmManager.guardMask);
        
        GuardRhythmPatrol closestGuard = null;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            GuardRhythmPatrol guard = _guardCheckResults[i].GetComponent<GuardRhythmPatrol>();
            if (guard != null)
            {
                float sqrDist = (_guardCheckResults[i].transform.position - (Vector3)playerPos).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closestGuard = guard;
                }
            }
        }
        
        if (closestGuard != null)
            closestGuard.ApplyFlash(durationInBeats);
    }

    void ResetInputSequence()
    {
        _currentActiveSkillKey = KeyCode.None;
        _inputSequenceCount = 0;
        _isCurrentComboPerfect = true;
    }
    
    void ActivateSkill(ConstellationSkillData skill, int skillIndex)
    {
        if (IsSkillOnCooldown(skillIndex))
        {
            Debug.Log($"{skill.skillName} 쿨타임 중!");
            return;
        }
        
        if (!TryConsumeFocus(focusCostPerSkill))
        {
            Debug.Log("Focus 부족!");
            return;
        }

        // Apply perfect combo cooldown bonus (33% reduction instead of 50%)
        int actualCooldown = _isCurrentComboPerfect
            ? Mathf.Max(1, Mathf.RoundToInt(skill.cooldownBeats * perfectComboCooldownMultiplier))
            : skill.cooldownBeats;
        
        SetSkillCooldown(skillIndex, actualCooldown);

        if (Player == null) return;

        switch (skill.category)
        {
            case SkillCategory.Stealth:
                ExecuteStealthSkill(skill);
                break;
            case SkillCategory.Lure:
                ExecuteLureSkill(skill);
                break;
            case SkillCategory.Movement:
                ExecuteMovementSkill(skill);
                break;
            case SkillCategory.Attack:
                ExecuteAttackSkill(skill);
                break;
        }
        
        ResetInputSequence();
    }

    void ExecuteAttackSkill(ConstellationSkillData skill)
    {
        // Check for specific skill implementations
        if (skill != null && (skill.skillName == "Orion's Arrow" || skill.skillName.Contains("Orion")))
        {
            // Orion's Arrow - Ranged projectile attack
            var orionArrow = Player?.GetComponent<SongOfTheStars.Skills.OrionsArrowSkill>();
            if (orionArrow != null)
            {
                orionArrow.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("OrionsArrowSkill component not found on Player!");
            }
        }

        // Legacy/default attack behavior based on input sequence
        if (_inputSequenceCount == 1)
        {
            var target = PlayerAssassination?.FindGuardInAssassinationRange();
            if (target != null)
                PlayerAssassination.ExecuteAssassinationStrike(target);
        }
        else if (_inputSequenceCount == 2)
        {
            var target = PlayerAssassination?.FindGuardInRangedRange();
            if (target != null)
                PlayerAssassination.ExecuteRangedAssassination(target);
        }
        else
        {
            ApplyFlashToClosestGuard(3);
        }
    }

    void ExecuteLureSkill(ConstellationSkillData skill)
    {
        // Check for specific skill implementations
        if (skill != null && (skill.skillName == "Gemini Clone" || skill.skillName.Contains("Gemini")))
        {
            // Gemini Clone - Moving decoy
            var geminiClone = Player?.GetComponent<SongOfTheStars.Skills.GeminiCloneSkill>();
            if (geminiClone != null)
            {
                geminiClone.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("GeminiCloneSkill component not found on Player!");
            }
        }

        // Legacy/default lure behavior
        if (Player != null)
        {
            Player.ActivateIllusion(5);
        }
    }

    void ExecuteStealthSkill(ConstellationSkillData skill)
    {
        // Check for specific skill implementations
        if (skill != null && (skill.skillName == "Shadow Blend" || skill.skillName.Contains("Shadow")))
        {
            // Shadow Blend - Stationary invisibility
            var shadowBlend = Player?.GetComponent<SongOfTheStars.Skills.ShadowBlendSkill>();
            if (shadowBlend != null)
            {
                shadowBlend.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("ShadowBlendSkill component not found on Player!");
            }
        }
        else if (skill != null && (skill.skillName == "Andromeda's Veil" || skill.skillName.Contains("Andromeda")))
        {
            // Andromeda's Veil - Full invisibility with movement (to be implemented)
            var andromedaVeil = Player?.GetComponent<SongOfTheStars.Skills.AndromedaVeilSkill>();
            if (andromedaVeil != null)
            {
                andromedaVeil.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("AndromedaVeilSkill component not found on Player!");
            }
        }

        // Legacy/default stealth behavior
        GameServices.PlayerStealth?.ToggleStealth();
    }

    void ExecuteMovementSkill(ConstellationSkillData skill)
    {
        // Check for specific skill implementations
        if (skill != null && (skill.skillName == "Pegasus Dash" || skill.skillName.Contains("Pegasus")))
        {
            // Pegasus Dash - Teleport dash
            var pegasusDash = Player?.GetComponent<SongOfTheStars.Skills.PegasusDashSkill>();
            if (pegasusDash != null)
            {
                pegasusDash.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("PegasusDashSkill component not found on Player!");
            }
        }
        else if (skill != null && (skill.skillName == "Aquarius Flow" || skill.skillName.Contains("Aquarius")))
        {
            // Aquarius Flow - Speed boost
            var aquariusFlow = Player?.GetComponent<SongOfTheStars.Skills.AquariusFlowSkill>();
            if (aquariusFlow != null)
            {
                aquariusFlow.ActivateSkill();
                return;
            }
            else
            {
                Debug.LogWarning("AquariusFlowSkill component not found on Player!");
            }
        }

        // Legacy/default movement behavior (charge)
        if (Player != null)
        {
            Player.ActivateCharge(skill.inputCount * Player.moveDistance);
        }
    }

    public bool IsSkillOnCooldown(int skillIndex)
    {
        if (RhythmManager == null || skillIndex < 0 || skillIndex >= 4) 
            return false;
            
        return _skillCooldownBeats[skillIndex] > RhythmManager.currentBeatCount;
    }
    
    public int GetRemainingCooldown(ConstellationSkillData skill)
    {
        if (RhythmManager == null || !_skillDataCached) return 0;

        for (int i = 0; i < 4; i++)
        {
            if (_skillDataCache[i] == skill)
            {
                int remaining = _skillCooldownBeats[i] - RhythmManager.currentBeatCount;
                return Mathf.Max(0, remaining);
            }
        }
        
        return 0;
    }
    
    public void CheckAllCooldowns(int currentBeat)
    {
        for (int i = 0; i < 4; i++)
        {
            if (_skillCooldownBeats[i] <= currentBeat)
                _skillCooldownBeats[i] = 0;
        }
    }

    void OnDestroy()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.RemoveListener(CheckAllCooldowns);
    }
    
    #region Focus 관리
    public void IncreaseFocus(float amount)
    {
        currentFocus = Mathf.Min(maxFocus, currentFocus + amount);
    }
    
    public void DecreaseFocus(float amount)
    {
        currentFocus = Mathf.Max(0f, currentFocus - amount);
    }
    
    public bool TryConsumeFocus(float amount)
    {
        if (currentFocus >= amount)
        {
            currentFocus -= amount;
            return true;
        }
        return false;
    }
    #endregion
}