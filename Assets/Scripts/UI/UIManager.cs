using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 UI (스킬 아이콘, Focus 바, 경보 레벨 등)를 관리합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ⭐ GameServices 사용 (불필요한 private 필드 제거)
    private RhythmPatternChecker RhythmChecker => GameServices.RhythmChecker;
    private SkillLoadoutManager LoadoutManager => GameServices.SkillLoadout;
    private MissionManager MissionManager => GameServices.MissionManager;

    private bool _needsUpdate = true;

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

    void Start()
    {
        InitializeSkillIcons();

        if (judgmentText != null)
            judgmentText.gameObject.SetActive(false);

        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.AddListener(OnBeat);

        if (MissionManager != null)
            MissionManager.OnAlertLevelChanged.AddListener(UpdateAlertUI);
    }

    void Update()
    {
        if (!_needsUpdate) return;
    
        UpdateSkillCooldowns();
        UpdateFocusUI();
        _needsUpdate = false;
    }

    void InitializeSkillIcons()
    {
        if (LoadoutManager == null || skillIconPrefab == null || skillIconContainer == null)
        {
            Debug.LogWarning("UIManager: 스킬 아이콘 초기화에 필요한 컴포넌트가 누락되었습니다.");
            return;
        }

        foreach (var pair in LoadoutManager.activeSkills)
        {
            KeyCode key = pair.Key;
            ConstellationSkillData skill = pair.Value;

            GameObject iconObject = Instantiate(skillIconPrefab, skillIconContainer);
            SkillIconUI iconUI = iconObject.GetComponent<SkillIconUI>();
            
            if (iconUI != null)
            {
                string keyDisplay = key.ToString().Replace("Alpha", "");
                iconUI.Setup(skill.icon, keyDisplay, skill.skillName);
                _skillIconMap.Add(skill, iconUI);
            }
            else
                Debug.LogError($"UIManager: SkillIconUI 컴포넌트를 찾을 수 없습니다! ({skill.skillName})");
        }
    }

    void UpdateSkillCooldowns()
    {
        if (RhythmChecker == null || LoadoutManager == null) return;

        foreach (var skillPair in LoadoutManager.activeSkills)
        {
            ConstellationSkillData skill = skillPair.Value;

            if (_skillIconMap.TryGetValue(skill, out SkillIconUI iconUI))
            {
                int remainingBeats = RhythmChecker.GetRemainingCooldown(skill);
                iconUI.UpdateCooldown(remainingBeats);
            }
        }
    }

    void UpdateFocusUI()
    {
        if (RhythmChecker == null) return;

        if (focusBarFill != null)
        {
            float fillAmount = RhythmChecker.currentFocus / RhythmChecker.maxFocus;
            focusBarFill.fillAmount = fillAmount;
        }

        if (focusText != null)
            focusText.text = $"{Mathf.RoundToInt(RhythmChecker.currentFocus)} / {Mathf.RoundToInt(RhythmChecker.maxFocus)}";
    }

    void UpdateAlertUI()
    {
        if (MissionManager == null) return;

        if (alertBarFill != null)
        {
            float fillAmount = (float)MissionManager.currentAlertLevel / MissionManager.maxAlertLevel;
            alertBarFill.fillAmount = fillAmount;

            // 경보 레벨에 따른 색상 변경
            if (fillAmount >= 0.8f)
                alertBarFill.color = Color.red;
            else if (fillAmount >= 0.5f)
                alertBarFill.color = Color.yellow;
            else
                alertBarFill.color = Color.green;
        }

        if (alertLevelText != null)
            alertLevelText.text = $"경보: {MissionManager.currentAlertLevel}/{MissionManager.maxAlertLevel}";

        // 경보 레벨이 높으면 경고 표시
        if (detectionWarning != null)
            detectionWarning.SetActive(MissionManager.currentAlertLevel >= MissionManager.maxAlertLevel - 1);
    }

    public void ShowJudgment(string judgmentTextString)
    {
        if (judgmentText != null)
        {
            judgmentText.text = judgmentTextString;
            judgmentText.gameObject.SetActive(true);

            // 색상 설정
            switch (judgmentTextString)
            {
                case "Perfect":
                    judgmentText.color = Color.cyan;
                    break;
                case "Great":
                    judgmentText.color = Color.yellow;
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
            detectionWarning.SetActive(progress > 0.3f);
    }

    void OnBeat(int beat) => _needsUpdate = true;

    void OnDestroy()
    {
        if (GameServices.RhythmManager != null)
            GameServices.RhythmManager.OnBeatCounted.RemoveListener(OnBeat);

        if (MissionManager != null)
            MissionManager.OnAlertLevelChanged.RemoveListener(UpdateAlertUI);
    }
}