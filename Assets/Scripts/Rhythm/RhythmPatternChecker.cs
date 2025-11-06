using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어의 스킬 입력 판정, 쿨타임 관리, Focus 시스템을 담당합니다.
/// </summary>
public class RhythmPatternChecker : MonoBehaviour
{
    #region 컴포넌트 및 참조
    private RhythmSyncManager _rhythmManager;
    private UIManager _uiManager; 
    private PlayerController _playerController;
    private SkillLoadoutManager _loadoutManager; 
    private PlayerAssassination _playerAssassination;
    #endregion
    
    #region Focus 시스템
    [Header("▶ Focus 시스템")]
    public float maxFocus = 100f;
    public float focusPerPerfect = 10f;
    public float focusCostPerSkill = 5f;
    public float currentFocus = 0f;
    #endregion

    #region 스킬 상태 및 쿨타임
    private Dictionary<ConstellationSkillData, int> _skillCooldowns = new Dictionary<ConstellationSkillData, int>();
    private KeyCode _currentActiveSkillKey = KeyCode.None;
    private int _inputSequenceCount = 0; 
    private bool _isCurrentComboPerfect = true; 
    #endregion

    // --- Unity Life Cycle ---
    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _uiManager = FindObjectOfType<UIManager>();
        _playerController = GetComponent<PlayerController>();
        _loadoutManager = GetComponent<SkillLoadoutManager>();
        _playerAssassination = GetComponent<PlayerAssassination>();

        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckAllCooldowns);
        }
    }

    void Update()
    {
        if (_playerController != null && !_playerController.isFreeMoving)
        {
            HandleRhythmInput();
        }
    }

    // --- 입력 및 판정 로직 ---
    void HandleRhythmInput()
    {
        if (_loadoutManager == null) return;

        // 스킬 입력 (1, 2, 3, 4 키)
        foreach (var pair in _loadoutManager.activeSkills)
        {
            KeyCode key = pair.Key;

            if (Input.GetKeyDown(key))
            {
                RhythmJudgment judgment = _rhythmManager.CheckJudgment();
                if (_uiManager != null)
                    _uiManager.ShowJudgment(judgment.ToString());

                if (SFXManager.Instance != null)
                {
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

                if (judgment == RhythmJudgment.Miss)
                {
                    ResetInputSequence();
                    return;
                }

                if (judgment == RhythmJudgment.Perfect)
                    currentFocus = Mathf.Min(maxFocus, currentFocus + focusPerPerfect);
                else
                    _isCurrentComboPerfect = false;

                if (_currentActiveSkillKey == KeyCode.None || _currentActiveSkillKey == key)
                    ContinueSkillSequence(key);
                else
                    StartNewSkillSequence(key);

                break;
            }
        }
    }
    
    public void SetSkillCooldown(ConstellationSkillData skill, int beats)
    {
        if (_rhythmManager == null) return;

        // 난이도 배수 적용
        int adjustedBeats = beats;
        if (DifficultyManager.Instance != null)
        {
            adjustedBeats = DifficultyManager.Instance.ApplyCooldownMultiplier(beats);
        }
        
        _skillCooldowns[skill] = _rhythmManager.currentBeatCount + adjustedBeats;
    }
    
    // --- 스킬 시퀀스 관리 ---
    void StartNewSkillSequence(KeyCode key)
    {
        _isCurrentComboPerfect = true; 
        
        _currentActiveSkillKey = key;
        _inputSequenceCount = 1;

        ConstellationSkillData skill = _loadoutManager.activeSkills[key];
        
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
        ConstellationSkillData skill = _loadoutManager.activeSkills[key];

        if (_inputSequenceCount >= skill.inputCount)
        {
            ActivateSkill(skill);
            ResetInputSequence();
        }
    }
    
    /// <summary>
    /// 플레이어 주변의 가장 가까운 경비병을 찾아 섬광 효과를 적용합니다.
    /// </summary>
    /// <param name="durationInBeats">섬광 효과 지속 비트</param>
    void ApplyFlashToClosestGuard(int durationInBeats)
    {
        if (_rhythmManager == null) return;
        
        float effectRange = 10f; // 섬광탄의 유효 범위 (미터)
        Vector2 playerPos = transform.position;

        // 경비병 레이어 마스크를 사용하여 주변 경비병 탐색
        Collider2D[] hitGuards = Physics2D.OverlapCircleAll(playerPos, effectRange, _rhythmManager.guardMask);
        
        GuardRhythmPatrol closestGuard = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hit in hitGuards)
        {
            GuardRhythmPatrol guard = hit.GetComponent<GuardRhythmPatrol>();
            if (guard != null)
            {
                float distance = Vector2.Distance(playerPos, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestGuard = guard;
                }
            }
        }
        
        if (closestGuard != null)
        {
            // GuardRhythmPatrol 스크립트의 ApplyFlash 함수 호출
            closestGuard.ApplyFlash(durationInBeats);
        }
        else
            Debug.Log("주변에 섬광탄을 맞출 경비병이 없습니다.");
    }

    void ResetInputSequence()
    {
        _currentActiveSkillKey = KeyCode.None;
        _inputSequenceCount = 0;
        _isCurrentComboPerfect = true;
    }
    
    // --- 스킬 발동 로직 ---
    void ActivateSkill(ConstellationSkillData skill)
    {
        if (IsSkillOnCooldown(skill)) return;
        
        currentFocus = Mathf.Max(0, currentFocus - focusCostPerSkill);

        int actualCooldown = skill.cooldownBeats;
        
        if (_isCurrentComboPerfect)
        {
            actualCooldown = Mathf.Max(1, actualCooldown / 2); 
        }
        
        SetSkillCooldown(skill, actualCooldown);

        if (_playerController == null) return;

        switch (skill.category)
        {
            case SkillCategory.Stealth:
                _playerController.GetComponent<PlayerStealth>()?.ToggleStealth();
                break;
            case SkillCategory.Lure:
                _playerController.ActivateIllusion(5);
                break;
            case SkillCategory.Movement:
                _playerController.ActivateCharge(skill.inputCount * _playerController.moveDistance); 
                break;
            case SkillCategory.Attack:
                ApplyFlashToClosestGuard(3);
                break;
        }
        
        ResetInputSequence();
    }
    
    // --- 쿨타임 관리 로직 ---
    public bool IsSkillOnCooldown(ConstellationSkillData skill) => _skillCooldowns.ContainsKey(skill) && _skillCooldowns[skill] > 0;

    public void SetSkillCooldown(ConstellationSkillData skill, int beats)
    {
        if (_rhythmManager == null) return;

        _skillCooldowns[skill] = _rhythmManager.currentBeatCount + beats;
    }
    
    public int GetRemainingCooldown(ConstellationSkillData skill)
    {
        if (_rhythmManager == null) return 0;
        if (_skillCooldowns.ContainsKey(skill))
            return _skillCooldowns[skill] - _rhythmManager.currentBeatCount;
            
        return 0;
    }
    
    public void CheckAllCooldowns(int currentBeat)
    {
        var keysToRemove = _skillCooldowns.Keys.Where(key => _skillCooldowns[key] <= currentBeat).ToList();
        foreach (var key in keysToRemove)
            _skillCooldowns.Remove(key);
    }
}