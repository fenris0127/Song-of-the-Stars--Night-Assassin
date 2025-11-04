using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Text & Bars")]
    public Text beatCounterText;
    public Text alertLevelText;
    public GameObject skillIconPrefab; 
    public Transform skillIconContainer; 
    
    [Header("Judgment Feedback")]
    public Text judgmentText;
    public Color perfectColor = Color.yellow; 
    public Color otherColor = Color.white; 

    private RhythmSyncManager _rhythmManager;
    private MissionManager _missionManager;
    private RhythmPatternChecker _patternChecker;
    
    private Dictionary<ConstellationSkillData, GameObject> _skillIconMap = new Dictionary<ConstellationSkillData, GameObject>();

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _missionManager = FindObjectOfType<MissionManager>();
        _patternChecker = FindObjectOfType<RhythmPatternChecker>();

        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.AddListener(UpdateBeatUI);

        if (_missionManager != null)
            _missionManager.OnAlertLevelChanged.AddListener(UpdateAlertUI); 
            
        InitializeSkillIcons();
        UpdateAlertUI(); 
        if (judgmentText != null) judgmentText.gameObject.SetActive(false);
    }

    void InitializeSkillIcons()
    {
        if (_patternChecker == null || skillIconPrefab == null || skillIconContainer == null) return;
        
        foreach (var skill in _patternChecker.allSkills)
        {
            GameObject icon = Instantiate(skillIconPrefab, skillIconContainer);
            _skillIconMap.Add(skill, icon);
        }
    }

    public void UpdateBeatUI(int currentBeat)
    {
        if (beatCounterText != null)
            beatCounterText.text = $"Beat: {currentBeat}";
            
        foreach (var pair in _skillIconMap)
        {
            ConstellationSkillData skill = pair.Key;
            GameObject icon = pair.Value;
            
            int remainingBeats = _patternChecker.GetRemainingCooldown(skill);
            
            Image iconImage = icon.GetComponent<Image>();
            Text iconText = icon.GetComponentInChildren<Text>(); 
            
            if (iconImage != null)
            {
                if (remainingBeats > 0)
                {
                    iconImage.color = Color.gray; 
                }
                else
                {
                    iconImage.color = Color.white; 
                }
            }
            if (iconText != null)
            {
                iconText.text = remainingBeats > 0 ? remainingBeats.ToString() : "";
            }
        }
    }

    public void UpdateAlertUI()
    {
        if (_missionManager != null && alertLevelText != null)
            alertLevelText.text = $"ALERT: {_missionManager.currentAlertLevel} / {_missionManager.maxAlertLevel}";
    }

    public void ShowJudgment(string judgment)
    {
        if (judgmentText != null)
        {
            judgmentText.text = judgment;
            judgmentText.color = (judgment == "Perfect") ? perfectColor : otherColor;
            judgmentText.gameObject.SetActive(true);
            
            // 시각적 강조 애니메이션 (LeanTween 없이 임시로 Scale 조정)
            judgmentText.transform.localScale = Vector3.one * 1.5f;
            
            CancelInvoke("HideJudgment");
            Invoke("HideJudgment", 0.8f); 
        }
    }

    private void HideJudgment()
    {
        if (judgmentText != null)
            judgmentText.gameObject.SetActive(false);
    }
}