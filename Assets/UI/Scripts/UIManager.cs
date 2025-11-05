using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // ★ UI 관련 컴포넌트 사용을 위해 추가
using TMPro; // ★ TextMeshPro 사용 시 추가

/// <summary>
/// 게임 UI (스킬 아이콘, Focus 바, 경보 레벨 등)를 관리합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    private RhythmPatternChecker _rhythmChecker;
    private SkillLoadoutManager _loadoutManager; 
    
    [Header("▶ UI 오브젝트")]
    public GameObject skillIconPrefab;
    public Transform skillIconContainer;
    public TextMeshProUGUI judgmentText; // ★ 판정 텍스트 표시용 (TextMeshProG UI 컴포넌트 필요)
    public float judgmentDisplayDuration = 0.5f; // ★ 판정 텍스트 표시 시간
    
    public Dictionary<ConstellationSkillData, GameObject> _skillIconMap = 
        new Dictionary<ConstellationSkillData, GameObject>(); 

    // --- Unity Life Cycle ---
    void Start()
    {
        _rhythmChecker = FindObjectOfType<RhythmPatternChecker>();
        _loadoutManager = FindObjectOfType<SkillLoadoutManager>(); 
        
        InitializeSkillIcons(); 
        
        // ★ TODO 구현: 판정 텍스트 초기화
        if (judgmentText != null)
        {
            judgmentText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateSkillCooldowns();
    }
    
    // --- 스킬 UI 초기화 ---
    void InitializeSkillIcons()
    {
        if (_loadoutManager == null || skillIconPrefab == null || skillIconContainer == null) return;
        
        foreach (var pair in _loadoutManager.activeSkills)
        {
            KeyCode key = pair.Key; // ★ 키 정보
            ConstellationSkillData skill = pair.Value;
            
            GameObject iconObject = Instantiate(skillIconPrefab, skillIconContainer);
            
            // ★ TODO 구현: 생성된 아이콘에 SkillData.icon과 KeyCode를 표시
            SkillIconUI iconUI = iconObject.GetComponent<SkillIconUI>(); // SkillIconUI라는 보조 스크립트가 필요합니다 (아래 참조).
            if (iconUI != null)
            {
                iconUI.Setup(skill.icon, key.ToString().Replace("Alpha", ""), skill.skillName);
            }

            _skillIconMap.Add(skill, iconObject); 
        }
    }
    
    // --- 쿨타임 업데이트 로직 ---
    void UpdateSkillCooldowns()
    {
        if (_rhythmChecker == null || _loadoutManager == null) return;

        foreach (var skillPair in _loadoutManager.activeSkills)
        {
            ConstellationSkillData skill = skillPair.Value;

            if (_skillIconMap.ContainsKey(skill))
            {
                GameObject iconObject = _skillIconMap[skill];
                int remainingBeats = _rhythmChecker.GetRemainingCooldown(skill);
                
                SkillIconUI iconUI = iconObject.GetComponent<SkillIconUI>();
                if (iconUI != null)
                {
                    iconUI.UpdateCooldown(remainingBeats);
                }
            }
        }
    }
    
    // --- 판정 결과 표시 함수 ---
    public void ShowJudgment(string judgmentTextString)
    {
        // ★ TODO 구현: 화면 중앙에 Perfect, Great 등의 텍스트를 잠시 띄우는 로직 구현
        if (judgmentText != null)
        {
            judgmentText.text = judgmentTextString;
            judgmentText.gameObject.SetActive(true);
            
            // 지정된 시간 후 텍스트 비활성화
            CancelInvoke(nameof(HideJudgmentText));
            Invoke(nameof(HideJudgmentText), judgmentDisplayDuration);
        }
    }

    private void HideJudgmentText()
    {
        if (judgmentText != null)
        {
            judgmentText.gameObject.SetActive(false);
        }
    }
}