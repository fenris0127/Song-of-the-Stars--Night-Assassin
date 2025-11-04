using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RhythmPatternChecker : MonoBehaviour
{
    [Header("Component References")]
    private RhythmSyncManager _rhythmManager;
    private PlayerController _playerController;
    private PlayerAssassination _assassinationManager;
    private MissionManager _missionManager;
    private VFXManager _vfxManager;
    private UIManager _uiManager; 
    
    [Header("Skill Assets")]
    public List<ConstellationSkillData> allSkills;
    public GameObject capricornTrapPrefab;
    public GameObject perfectVFXPrefab;
    public AudioSource audioSource;
    public AudioClip perfectSFX;

    private List<KeyCode> _currentInputPattern = new List<KeyCode>();
    private Dictionary<ConstellationSkillData, int> _skillCooldowns = new Dictionary<ConstellationSkillData, int>();
    private bool _isCurrentComboPerfect = true;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _playerController = GetComponent<PlayerController>();
        _assassinationManager = GetComponent<PlayerAssassination>();
        _missionManager = FindObjectOfType<MissionManager>();
        _vfxManager = FindObjectOfType<VFXManager>();
        _uiManager = FindObjectOfType<UIManager>();

        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckCooldowns);
        }
    }

    void Update()
    {
        HandleRhythmInput();
    }

    void HandleRhythmInput()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keycode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keycode))
                {
                    if (keycode == KeyCode.W || keycode == KeyCode.A || keycode == KeyCode.S || keycode == KeyCode.D || keycode == KeyCode.Space)
                    {
                        RhythmSyncManager.RhythmJudgment judgment = CheckRhythmJudgment();
                        
                        if (judgment != RhythmSyncManager.RhythmJudgment.None)
                        {
                            if (_uiManager != null)
                                _uiManager.ShowJudgment(judgment.ToString());
                        }

                        if (judgment == RhythmSyncManager.RhythmJudgment.Miss)
                        {
                            _currentInputPattern.Clear();
                            _isCurrentComboPerfect = false; 
                            return; 
                        }
                        
                        _currentInputPattern.Add(keycode);
                        _isCurrentComboPerfect = _isCurrentComboPerfect && (judgment == RhythmSyncManager.RhythmJudgment.Perfect);
                        CheckForSkillMatch();
                        break;
                    }
                }
            }
        }
    }
    
    RhythmSyncManager.RhythmJudgment CheckRhythmJudgment()
    {
        // 리듬 싱크 매니저의 BPM과 현재 DSP 시간을 이용해 정확도를 계산해야 합니다. 
        // 여기서는 간단화를 위해 BeatInterval 주변의 입력만 Perfect로 간주합니다.
        float timeSinceLastBeat = (float)(AudioSettings.dspTime % _rhythmManager.beatInterval);
        
        if (timeSinceLastBeat < 0.1f || timeSinceLastBeat > _rhythmManager.beatInterval - 0.1f)
            return RhythmSyncManager.RhythmJudgment.Perfect;
        
        return RhythmSyncManager.RhythmJudgment.Miss; 
    }

    void CheckForSkillMatch()
    {
        foreach (var skill in allSkills)
        {
            if (IsSkillOnCooldown(skill)) continue;

            if (_currentInputPattern.Count >= skill.rhythmPattern.Count)
            {
                bool match = true;
                for (int i = 0; i < skill.rhythmPattern.Count; i++)
                {
                    if (_currentInputPattern[_currentInputPattern.Count - skill.rhythmPattern.Count + i] != skill.rhythmPattern[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    ActivateSkill(skill);
                    _currentInputPattern.Clear();
                    _isCurrentComboPerfect = true; 
                    return;
                }
            }
        }
        if (_currentInputPattern.Count > 10)
        {
            _currentInputPattern.RemoveAt(0); 
        }
    }

    void ActivateSkill(ConstellationSkillData skill)
    {
        if (_vfxManager != null && skill.skillEffectPrefab != null)
        {
            _vfxManager.PlayVFXAt(skill.skillEffectPrefab, transform.position);
        }
        
        if (_isCurrentComboPerfect)
        {
            if (_vfxManager != null && perfectVFXPrefab != null)
            {
                _vfxManager.PlayVFXAt(perfectVFXPrefab, transform.position); 
            }
            if (audioSource != null && perfectSFX != null)
            {
                audioSource.PlayOneShot(perfectSFX);
            }
        }

        // --- 12개 별자리 스킬 로직 ---
        if (skill.constellationName == "쌍둥이자리") 
        {
            PlayerStealth stealth = _playerController.GetComponent<PlayerStealth>();
            if (stealth != null) stealth.ToggleStealth();
        }
        else if (skill.constellationName == "전갈자리") 
        {
            GuardRhythmPatrol target = _assassinationManager.FindGuardInAssassinationRange();
            if (target != null) { _assassinationManager.ExecuteAssassinationStrike(target); }
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "천칭자리") 
        {
            GuardRhythmPatrol target = _assassinationManager.FindGuardInAssassinationRange();
            int duration = 5; if (_isCurrentComboPerfect) duration += 3;
            if (target != null) target.ApplyParalysis(duration);
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "게자리") 
        {
            if (!_playerController.isMemoryActive) { _playerController.SetMemoryPosition(); }
            else { _playerController.TeleportToMemory(); SetSkillCooldown(skill); }
        }
        else if (skill.constellationName == "양자리") 
        {
            float duration = 3f; if (_isCurrentComboPerfect) duration += 1.5f;
            _playerController.ActivateCharge(duration);
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "황소자리") 
        {
            int duration = 6; if (_isCurrentComboPerfect) duration += 4;
            _playerController.ActivateShield(duration);
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "처녀자리") 
        {
            int amount = 3; if (_isCurrentComboPerfect) amount += 2; 
            _missionManager.DecreaseAlertLevel(amount);
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "염소자리") 
        {
            if (capricornTrapPrefab != null)
            {
                GameObject trap = Instantiate(capricornTrapPrefab, transform.position, Quaternion.identity);
                CapricornTrap trapComponent = trap.GetComponent<CapricornTrap>();

                if (_isCurrentComboPerfect) { trapComponent.durationBeats += 5; trapComponent.stunDurationBeats += 2; }
            }
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "물병자리") 
        {
            const float REDUCTION_AMOUNT = 5f; int duration = 6; if (_isCurrentComboPerfect) duration += 3;
            Collider[] hitGuards = Physics.OverlapSphere(transform.position, 10f, _rhythmManager.guardMask);
            foreach (Collider guardCollider in hitGuards)
            {
                GuardRhythmPatrol guard = guardCollider.GetComponent<GuardRhythmPatrol>();
                if (guard != null) guard.ApplyJamming(REDUCTION_AMOUNT, duration);
            }
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "물고기자리") 
        {
            int duration = 6; if (_isCurrentComboPerfect) duration += 4;
            _playerController.ActivateIllusion(duration);
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "사자자리") 
        {
            const float REDUCTION_AMOUNT = 85f; int duration = 4; if (_isCurrentComboPerfect) duration += 2; 
            Collider[] hitGuards = Physics.OverlapSphere(transform.position, 12f, _rhythmManager.guardMask);
            foreach (Collider guardCollider in hitGuards)
            {
                GuardRhythmPatrol guard = guardCollider.GetComponent<GuardRhythmPatrol>();
                if (guard != null) guard.ApplyFlash(REDUCTION_AMOUNT, duration);
            }
            SetSkillCooldown(skill);
        }
        else if (skill.constellationName == "궁수자리") 
        {
            RaycastHit hit; Vector3 raycastStart = transform.position + Vector3.up * 1f; 
            if (Physics.Raycast(raycastStart, transform.forward, out hit, _assassinationManager.maxRange, _rhythmManager.guardMask))
            {
                GuardRhythmPatrol targetGuard = hit.collider.GetComponent<GuardRhythmPatrol>();
                if (targetGuard != null)
                {
                    _assassinationManager.ExecuteRangedAssassination(targetGuard);
                    if (_isCurrentComboPerfect)
                    {
                         int currentBeat = _rhythmManager.currentBeatCount;
                         int newCooldownEnd = _skillCooldowns[skill] - 12; 
                         _skillCooldowns[skill] = Mathf.Max(currentBeat, newCooldownEnd); 
                    }
                }
            }
            SetSkillCooldown(skill);
        }
    }

    // --- 쿨타임 관리 로직 ---
    bool IsSkillOnCooldown(ConstellationSkillData skill)
    {
        if (_skillCooldowns.ContainsKey(skill) && _skillCooldowns[skill] > _rhythmManager.currentBeatCount) { return true; }
        return false;
    }

    void SetSkillCooldown(ConstellationSkillData skill)
    {
        if (skill.cooldownBeats > 0) { _skillCooldowns[skill] = _rhythmManager.currentBeatCount + skill.cooldownBeats; }
    }
    
    void CheckCooldowns(int currentBeat) {} // 쿨타임 확인은 IsSkillOnCooldown에서만 사용

    // UIManager 연동을 위한 PUBLIC 함수
    public int GetRemainingCooldown(ConstellationSkillData skill)
    {
        if (_skillCooldowns.ContainsKey(skill))
        {
            return Mathf.Max(0, _skillCooldowns[skill] - _rhythmManager.currentBeatCount);
        }
        return 0;
    }
}