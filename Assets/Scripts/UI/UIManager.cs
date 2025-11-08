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
    private MissionManager _missionManager;

    private RhythmPatternChecker RhythmChecker => GameServices.RhythmChecker;
    private SkillLoadoutManager LoadoutManager => GameServices.SkillLoadout;
    private MissionManager MissionManager => GameServices.MissionManager;

    // ⭐ 최적화: 비트 이벤트 기반 업데이트
    private bool _needsUpdate = true;

    [Header("▶ UI 오브젝트")]
    public GameObject skillIconPrefab;
    public Transform skillIconContainer;
    public TextMeshProUGUI judgmentText; // ★ 판정 텍스트 표시용 (TextMeshProG UI 컴포넌트 필요)
    public float judgmentDisplayDuration = 0.5f; // ★ 판정 텍스트 표시 시간

    [Header("▶ 경보 레벨 UI")]
    public Image alertBarFill;
    public TextMeshProUGUI alertLevelText;

    [Header("▶ 발각 진행도 UI")]
    public GameObject detectionWarning; // "!" 경고 아이콘
    public Image detectionProgressBar; // 발각 진행도 바

    [Header("▶ Focus UI")]
    public Image focusBarFill;

    public Dictionary<ConstellationSkillData, GameObject> _skillIconMap =
        new Dictionary<ConstellationSkillData, GameObject>();

    void Start()
    {
        InitializeSkillIcons();

        if (judgmentText != null)
            judgmentText.gameObject.SetActive(false);

        // ⭐ 비트 이벤트에 구독
        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.AddListener(OnBeat);
    }

    void Update()
    {
        if (!_needsUpdate) return;
    
        UpdateSkillCooldowns();
        _needsUpdate = false;
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
        if (RhythmChecker == null || LoadoutManager == null) return;

        foreach (var skillPair in LoadoutManager.activeSkills)
        {
            ConstellationSkillData skill = skillPair.Value;

            if (_skillIconMap.TryGetValue(skill, out GameObject iconObject))
            {
                int remainingBeats = RhythmChecker.GetRemainingCooldown(skill);
                
                SkillIconUI iconUI = iconObject.GetComponent<SkillIconUI>();
                if (iconUI != null)
                    iconUI.UpdateCooldown(remainingBeats);
            }
        }
    }

    // --- 판정 결과 표시 함수 ---
    public void ShowJudgment(string judgmentTextString)
    {
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

    void OnBeat(int beat) => _needsUpdate = true;

    void OnDestroy()
    {
        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.RemoveListener(OnBeat);
    }
}