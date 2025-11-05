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
    private PlayerController _playerController; // Free Move 상태 체크용
    private SkillLoadoutManager _loadoutManager;
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

        if (_rhythmManager != null)
        {
            // 비트마다 쿨타임 체크
            _rhythmManager.OnBeatCounted.AddListener(CheckAllCooldowns);
        }
    }

    void Update()
    {
        // PlayerController에서 Free Move 상태가 아닐 때만 리듬 입력 처리
        if (_playerController != null && !_playerController.isFreeMoving)
        {
            HandleRhythmInput();
        }
    }
    
    // --- 입력 및 판정 로직 ---
    void HandleRhythmInput()
    {
        foreach (var pair in _loadoutManager.activeSkills)
        {
            KeyCode key = pair.Key;
            
            if (Input.GetKeyDown(key))
            {
                if (!_loadoutManager.activeSkills.ContainsKey(key)) continue;

                RhythmSyncManager.RhythmJudgment judgment = _rhythmManager.CheckJudgment();
                if (_uiManager != null) _uiManager.ShowJudgment(judgment.ToString());

                // 1. Miss 판정: 현재 입력 시퀀스 초기화 (스킬 실패)
                if (judgment == RhythmSyncManager.RhythmJudgment.Miss)
                {
                    ResetInputSequence();
                    return;
                }
                
                // 2. Perfect 판정: Focus 획득 (Perfect 판정은 100% 성공)
                if (judgment == RhythmSyncManager.RhythmJudgment.Perfect)
                {
                    currentFocus = Mathf.Min(maxFocus, currentFocus + focusPerPerfect);
                }
                else
                {
                    // Great 판정 시 콤보는 Perfect가 아님
                    _isCurrentComboPerfect = false;
                }
                
                // 3. 스킬 시퀀스 체크 및 진행
                if (_currentActiveSkillKey == KeyCode.None || _currentActiveSkillKey == key)
                {
                    // 첫 입력이거나 연속 입력인 경우
                    ContinueSkillSequence(key);
                }
                else
                {
                    // 다른 키 입력 시 이전 시퀀스 버리고 새 시퀀스 시작 (관대하게 처리)
                    StartNewSkillSequence(key);
                }
                
                break;
            }
        }
    }
    
    // --- 스킬 시퀀스 관리 ---
    void StartNewSkillSequence(KeyCode key)
    {
        // 이전 시퀀스의 Perfect 여부를 새 시퀀스 시작 전 초기화
        _isCurrentComboPerfect = true; 
        
        _currentActiveSkillKey = key;
        _inputSequenceCount = 1;

        ConstellationSkillData skill = _loadoutManager.activeSkills[key];
        
        // 입력 횟수가 1회인 스킬은 즉시 발동
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
            // 시퀀스가 비어있으면 새 시퀀스 시작
            StartNewSkillSequence(key);
            return;
        }

        if (_currentActiveSkillKey != key) return; // 다른 키 무시

        _inputSequenceCount++;
        ConstellationSkillData skill = _loadoutManager.activeSkills[key];

        // 총 입력 횟수에 도달하면 발동
        if (_inputSequenceCount >= skill.inputCount)
        {
            ActivateSkill(skill);
            ResetInputSequence();
        }
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
        
        // Focus 코스트 차감
        currentFocus = Mathf.Max(0, currentFocus - focusCostPerSkill);

        // 쿨타임 설정
        int actualCooldown = skill.cooldownBeats;
        
        // Perfect 콤보 보상 (쿨타임 감소)
        if (_isCurrentComboPerfect)
        {
            // Perfect 성공 시 쿨타임 50% 감소 (정수 처리)
            actualCooldown = Mathf.Max(1, actualCooldown / 2); 
            // TODO: Perfect VFX/SFX 재생
        }
        
        SetSkillCooldown(skill, actualCooldown);

        // 2. 개별 스킬 로직 분기
        if (_playerController == null) return;

        switch (skill.category)
        {
            case ConstellationSkillData.SkillCategory.Stealth:
                _playerController.GetComponent<PlayerStealth>()?.ToggleStealth();
                break;
            case ConstellationSkillData.SkillCategory.Lure:
                // Decoy (잔상) 스킬
                _playerController.ActivateIllusion(5); // 5비트 동안 잔상 생성 예시
                break;
            case ConstellationSkillData.SkillCategory.Movement:
                // Charge (돌진) 스킬
                _playerController.ActivateCharge(skill.inputCount * _playerController.moveDistance); 
                break;
            case ConstellationSkillData.SkillCategory.Attack:
                // TBD: 공격 로직 (근접 범위 공격 등)
                break;
        }
        
        ResetInputSequence(); // 스킬 발동 후 초기화
    }

    // --- 쿨타임 관리 로직 ---
    public bool IsSkillOnCooldown(ConstellationSkillData skill)
    {
        return _skillCooldowns.ContainsKey(skill) && _skillCooldowns[skill] > 0;
    }

    public void SetSkillCooldown(ConstellationSkillData skill, int beats)
    {
        _skillCooldowns[skill] = _rhythmManager.currentBeatCount + beats;
    }
    
    public int GetRemainingCooldown(ConstellationSkillData skill)
    {
        if (_skillCooldowns.ContainsKey(skill))
        {
            return _skillCooldowns[skill] - _rhythmManager.currentBeatCount;
        }
        return 0;
    }
    
    public void CheckAllCooldowns(int currentBeat)
    {
        // 쿨타임은 자동으로 0 이하로 내려가므로 별도 처리 불필요
        // UI 업데이트를 위해 Dictionary를 순회할 수는 있음
        
        // Dictionary에서 만료된 쿨타임을 제거하는 로직 (선택적 최적화)
        var keysToRemove = _skillCooldowns.Keys.Where(key => _skillCooldowns[key] <= currentBeat).ToList();
        foreach (var key in keysToRemove)
        {
            _skillCooldowns.Remove(key);
        }
    }
}