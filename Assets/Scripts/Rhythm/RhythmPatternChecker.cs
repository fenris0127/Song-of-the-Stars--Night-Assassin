using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Physics2DCompat is in global namespace

/// <summary>
/// 플레이어의 스킬 입력 판정, 쿨타임 관리, Focus 시스템 (최적화 통합 버전)
/// </summary>
public class RhythmPatternChecker : MonoBehaviour
{
    #region 컴포넌트 및 참조 (최적화: 캐싱)
    // ⭐ 최적화: GameServices 사용 (FindObjectOfType 제거)
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private UIManager UI => GameServices.UIManager;
    private SkillLoadoutManager LoadoutManager => GameServices.SkillLoadout;
    private PlayerAssassination PlayerAssassination => GameServices.PlayerAssassination;
    private PlayerController Player => GameServices.Player;
    #endregion
    
    #region Focus 시스템
    [Header("▶ Focus 시스템")]
    public float maxFocus = 100f;
    public float focusPerPerfect = 10f;
    public float focusCostPerSkill = 5f;
    public float currentFocus = 0f;
    #endregion

    // 외부 UI/다른 시스템과의 호환성을 위한 프로퍼티
    public float CurrentFocus => currentFocus;
    public float MaxFocus => maxFocus;

    #region 스킬 상태 및 쿨타임
    private Dictionary<ConstellationSkillData, int> _skillCooldowns = new Dictionary<ConstellationSkillData, int>();
    private KeyCode _currentActiveSkillKey = KeyCode.None;
    private int _inputSequenceCount = 0;
    private bool _isCurrentComboPerfect = true;
    
    // ⭐ 최적화: 입력 키 배열 캐싱 (GC 방지)
    private static readonly KeyCode[] SKILL_KEYS = new KeyCode[]
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4
    };
    
    // ⭐ 최적화: NonAlloc용 결과 배열
    private Collider2D[] _guardCheckResults = new Collider2D[20];
    #endregion

    void Start()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.AddListener(CheckAllCooldowns);
        else
            Debug.LogError("RhythmPatternChecker: RhythmSyncManager를 찾을 수 없습니다!");
    }

    void Update()
    {
        if (Player != null && !Player.isFreeMoving)
            HandleRhythmInput();
    }

    void HandleRhythmInput()
    {
        if (LoadoutManager == null) return;

        // ⭐ 최적화: 미리 정의된 키 배열 사용 (foreach 대신 for)
        for (int i = 0; i < SKILL_KEYS.Length; i++)
        {
            KeyCode key = SKILL_KEYS[i];

            if (Input.GetKeyDown(key))
            {
                ProcessSkillInput(key);
                break; // 한 프레임에 하나의 입력만 처리
            }
        }
    }

    public void DecreaseFocus(float amount)
    {
        // Focus는 0 이하로 내려가지 않도록 제한
        currentFocus = Mathf.Max(0f, currentFocus - amount);
        UI?.UpdateFocusBar();
    }
    
    // ⭐ 추가: Focus 증가 메서드 (캡슐화)
    public void IncreaseFocus(float amount)
    {
        // Focus는 maxFocus를 초과하지 않도록 제한
        currentFocus = Mathf.Min(maxFocus, currentFocus + amount);
        UI?.UpdateFocusBar();
    }

    void ProcessSkillInput(KeyCode key)
    {
        if (!LoadoutManager.activeSkills.ContainsKey(key))
            return;

        RhythmJudgment judgment = RhythmManager.CheckJudgment();
        
        // UI 표시
        if (UI != null)
            UI.DisplayJudgment(judgment.ToString());

        // 사운드 재생
        PlayJudgmentSound(judgment);

        if (judgment == RhythmJudgment.Miss)
        {
            ResetInputSequence();
            return;
        }

        // Focus 업데이트
        if (judgment == RhythmJudgment.Perfect)
            currentFocus = Mathf.Min(maxFocus, currentFocus + focusPerPerfect);
        else
            _isCurrentComboPerfect = false;

        // 스킬 시퀀스 처리
        if (_currentActiveSkillKey == KeyCode.None || _currentActiveSkillKey == key)
            ContinueSkillSequence(key);
        else
            StartNewSkillSequence(key);
    }

    void PlayJudgmentSound(RhythmJudgment judgment)
    {
        if (SFXManager.Instance == null) return;

        switch (judgment)
        {
            case RhythmJudgment.Perfect:
                SFXManager.Instance.PlayPerfectSound();
                break;
            case RhythmJudgment.Great:
                SFXManager.Instance.PlayGreatSound();
                break;
            case RhythmJudgment.Miss:
                SFXManager.Instance.PlayMissSound();
                break;
        }
    }
    
    public void SetSkillCooldown(ConstellationSkillData skill, int beats)
    {
        if (RhythmManager == null) return;

        int adjustedBeats = beats;
        if (DifficultyManager.Instance != null)
            adjustedBeats = DifficultyManager.Instance.ApplyCooldownMultiplier(beats);
        
        _skillCooldowns[skill] = RhythmManager.currentBeatCount + adjustedBeats;
    }
    
    void StartNewSkillSequence(KeyCode key)
    {
        _isCurrentComboPerfect = true;
        
        _currentActiveSkillKey = key;
        _inputSequenceCount = 1;

        ConstellationSkillData skill = LoadoutManager.activeSkills[key];
        
        if (skill.inputCount == 1)
        {
            ActivateSkill(skill);
            ResetInputSequence();
        }
    }

    void ContinueSkillSequence(KeyCode key)
    {
        if (_currentActiveSkillKey == KeyCode.None)
        {
            StartNewSkillSequence(key);
            return;
        }

        if (_currentActiveSkillKey != key) return;

        _inputSequenceCount++;
        ConstellationSkillData skill = LoadoutManager.activeSkills[key];

        if (_inputSequenceCount >= skill.inputCount)
        {
            ActivateSkill(skill);
            ResetInputSequence();
        }
    }
    
    void ApplyFlashToClosestGuard(int durationInBeats)
    {
        if (RhythmManager == null) return;
        
        float effectRange = 10f;
        Vector2 playerPos = transform.position;

        // Use compatibility helper with the reusable buffer to avoid allocations
        GuardRhythmPatrol closestGuard = null;
        float minSqrDistance = float.MaxValue;

    int hitCount = GameServices.OverlapCircleCompat(playerPos, effectRange, RhythmManager.guardMask, _guardCheckResults);
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
        {
            closestGuard.ApplyFlash(durationInBeats);
            Debug.Log($"{closestGuard.gameObject.name}에게 섬광 효과 적용!");
        }
        else
        {
            Debug.Log("주변에 섬광탄을 맞출 경비병이 없습니다.");
        }
    }

    void ResetInputSequence()
    {
        _currentActiveSkillKey = KeyCode.None;
        _inputSequenceCount = 0;
        _isCurrentComboPerfect = true;
    }
    
    void ActivateSkill(ConstellationSkillData skill)
    {
        if (IsSkillOnCooldown(skill))
        {
            Debug.Log($"{skill.skillName}이(가) 쿨타임 중입니다!");
            return;
        }
        
        currentFocus = Mathf.Max(0, currentFocus - focusCostPerSkill);

        // Perfect 콤보 시 쿨타임 절반
        int actualCooldown = _isCurrentComboPerfect 
            ? Mathf.Max(1, skill.cooldownBeats / 2)
            : skill.cooldownBeats;
        
        SetSkillCooldown(skill, actualCooldown);

        if (Player == null) return;

        switch (skill.category)
        {
            case SkillCategory.Stealth:
                GameServices.PlayerStealth?.ToggleStealth();
                break;
            case SkillCategory.Lure:
                // Lure (Decoy) activation
                // Canonical flow: let PlayerController instantiate and own the decoy lifecycle.
                // Use skill.effectDurationBeats when present; fall back to 5 if unspecified.
                int duration = skill != null ? Mathf.Max(1, skill.effectDurationBeats) : 5;
                // Pass the skill's effect prefab so designers can specify custom decoy prefabs per-skill.
                Player.ActivateIllusion(skill != null ? skill.skillEffectPrefab : null, duration);
                break;
            case SkillCategory.Movement:
                Player.ActivateCharge(skill.inputCount * Player.moveDistance);
                break;
            case SkillCategory.Attack:
                // 입력 시퀀스 수에 따라 다른 공격 스킬 발동
                if (_inputSequenceCount == 1)
                {
                    // 1회 입력: 근접 암살 시도
                    var target = PlayerAssassination.FindGuardInAssassinationRange();
                    if (target != null)
                    {
                        PlayerAssassination.ExecuteAssassinationStrike(target);
                        Debug.Log("근접 암살 시도!");
                    }
                }
                else if (_inputSequenceCount == 2)
                {
                    // 2회 입력: 원거리 암살 시도
                    var target = PlayerAssassination.FindGuardInRangedRange();
                    if (target != null)
                    {
                        PlayerAssassination.ExecuteRangedAssassination(target);
                        Debug.Log("원거리 암살 시도!");
                    }
                }
                else
                {
                    // 3회 입력: 섬광탄 효과
                    ApplyFlashToClosestGuard(3);
                }
                break;
        }
        
        // Instantiate skill effect prefab if present (e.g., projectile, VFX). NOTE: for Lure
        // we avoid instantiating the prefab here because PlayerController owns decoy instantiation
        // in the canonical flow.
        if (skill.skillEffectPrefab != null && Player != null && skill.category != SkillCategory.Lure)
        {
            GameObject spawned = Instantiate(skill.skillEffectPrefab, Player.transform.position, Quaternion.identity);
        }

        Debug.Log($"스킬 발동: {skill.skillName} (쿨타임: {GetRemainingCooldown(skill)} 비트)");
        
        ResetInputSequence();
    }
    
    public bool IsSkillOnCooldown(ConstellationSkillData skill)
    {
        if (RhythmManager == null) return false;
        return _skillCooldowns.ContainsKey(skill) && 
               _skillCooldowns[skill] > RhythmManager.currentBeatCount;
    }
    
    public int GetRemainingCooldown(ConstellationSkillData skill)
    {
        if (RhythmManager == null) return 0;
        if (!_skillCooldowns.ContainsKey(skill))
            return 0;
            
        int remaining = _skillCooldowns[skill] - RhythmManager.currentBeatCount;
        return Mathf.Max(0, remaining);
    }
    
    public void CheckAllCooldowns(int currentBeat)
    {
        // ⭐ 최적화: LINQ Where 대신 for 루프 사용 (GC 방지)
        List<ConstellationSkillData> keysToRemove = null;
        
        foreach (var pair in _skillCooldowns)
        {
            if (pair.Value <= currentBeat)
            {
                if (keysToRemove == null)
                    keysToRemove = new List<ConstellationSkillData>(4);
                    
                keysToRemove.Add(pair.Key);
            }
        }
        
        if (keysToRemove != null)
        {
            foreach (var key in keysToRemove)
                _skillCooldowns.Remove(key);
        }
    }

    void OnDestroy()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.RemoveListener(CheckAllCooldowns);
    }
}