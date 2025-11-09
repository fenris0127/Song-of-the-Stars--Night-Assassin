using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 UI (스킬 아이콘, Focus 바, 경보 레벨 등)를 관리합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    // ⭐ GameServices 사용 (불필요한 private 필드 제거 및 보고서 수정 반영)
    private RhythmPatternChecker RhythmChecker => GameServices.RhythmChecker;
    private SkillLoadoutManager LoadoutManager => GameServices.SkillLoadout;
    private MissionManager MissionManager => GameServices.MissionManager;

    private bool _needsUpdate = true;
    private List<SkillIconUI> _skillIcons = new List<SkillIconUI>();

    [Header("▶ UI 오브젝트")]
    public GameObject skillIconPrefab;
    public Transform skillIconContainer;
    public TextMeshProUGUI judgmentText;
    public float judgmentDisplayDuration = 0.5f;

    [Header("▶ 경보 레벨 UI")]
    public Image alertBarFill;
    public TextMeshProUGUI alertLevelText;

    [Header("▶ 발각 진행도 UI")]
    public GameObject detectionWarning;
    public Image detectionProgressBar;

    [Header("▶ Focus UI")]
    public Image focusBarFill;
    public TextMeshProUGUI focusText;

    private Dictionary<ConstellationSkillData, SkillIconUI> _skillIconMap =
        new Dictionary<ConstellationSkillData, SkillIconUI>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeSkillIcons();

        if (judgmentText != null)
            judgmentText.gameObject.SetActive(false);

        // 리듬 매니저의 이벤트 구독
        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.AddListener(OnBeat);

        // 미션 매니저의 이벤트 구독
        if (MissionManager != null)
            MissionManager.OnAlertLevelChanged.AddListener(UpdateAlertLevel);
        
        UpdateUI();
    }
    
    void Update()
    {
        // 리듬에 동기화되지 않는 UI는 Update에서 업데이트 (주로 Focus Bar)
        UpdateFocusBar();

        if (_needsUpdate)
        {
            UpdateSkillIcons();
            _needsUpdate = false;
        }
    }
    
    private void InitializeSkillIcons()
    {
        if (LoadoutManager == null || skillIconContainer == null || skillIconPrefab == null) return;

        int index = 0;
        foreach (var skillData in LoadoutManager.GetCurrentSkills())
        {
            GameObject iconGo = Instantiate(skillIconPrefab, skillIconContainer);
            SkillIconUI iconUI = iconGo.GetComponent<SkillIconUI>();

            if (iconUI != null)
            {
                iconUI.Initialize(skillData, index + 1); // 1-based index
                _skillIconMap.Add(skillData, iconUI);
                _skillIcons.Add(iconUI);
            }
            index++;
        }
    }

    private void UpdateUI()
    {
        UpdateAlertLevel(MissionManager != null ? MissionManager.CurrentAlertLevel : 0f);
        UpdateSkillIcons();
    }
    
    private void UpdateSkillIcons()
    {
        if (LoadoutManager == null) return;
        
        foreach (var iconUI in _skillIcons)
        {
            iconUI.UpdateCooldown(LoadoutManager);
        }
    }

    public void UpdateFocusBar()
    {
        if (RhythmChecker != null && focusBarFill != null && focusText != null)
        {
            float currentFocus = RhythmChecker.CurrentFocus;
            float maxFocus = RhythmChecker.MaxFocus;
            
            focusBarFill.fillAmount = currentFocus / maxFocus;
            focusText.text = $"{Mathf.FloorToInt(currentFocus)} / {Mathf.FloorToInt(maxFocus)}";
        }
    }

    public void UpdateAlertLevel(float alertLevel)
    {
        if (alertBarFill != null)
        {
            alertBarFill.fillAmount = alertLevel / 100f;
        }

        if (alertLevelText != null)
        {
            alertLevelText.text = $"ALERT: {Mathf.FloorToInt(alertLevel)}%";
        }
    }

    /// <summary>
    /// 판정 텍스트 표시
    /// </summary>
    public void DisplayJudgment(string judgmentTextString)
    {
        if (judgmentText != null)
        {
            judgmentText.gameObject.SetActive(true);
            judgmentText.text = judgmentTextString;

            // 색상 설정 (네온 스타일 반영)
            switch (judgmentTextString)
            {
                case "Perfect":
                    judgmentText.color = Color.yellow; // 노란색 강조 (Perfect 판정)
                    break;
                case "Great":
                    judgmentText.color = Color.cyan;
                    break;
                case "Miss":
                    judgmentText.color = Color.red;
                    break;
            }

            CancelInvoke(nameof(HideJudgmentText));
            Invoke(nameof(HideJudgmentText), judgmentDisplayDuration);
        }
    }

    private void HideJudgmentText()
    {
        if (judgmentText != null)
            judgmentText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 발각 진행도 UI 업데이트 (경비병이 플레이어를 발견했을 때)
    /// </summary>
    public void UpdateDetectionProgress(float progress)
    {
        if (detectionProgressBar != null)
        {
            detectionProgressBar.fillAmount = progress;
            detectionProgressBar.gameObject.SetActive(progress > 0f);
        }

        if (detectionWarning != null)
            detectionWarning.SetActive(progress > 0.3f); // 붉은색 경고등 (경보 레벨)
    }

    void OnBeat(int beat) => _needsUpdate = true;

    void OnDestroy()
    {
        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.RemoveListener(OnBeat);

        if (MissionManager != null)
            MissionManager.OnAlertLevelChanged.RemoveListener(UpdateAlertLevel);
        
        if (Instance == this)
            Instance = null;
    }
}