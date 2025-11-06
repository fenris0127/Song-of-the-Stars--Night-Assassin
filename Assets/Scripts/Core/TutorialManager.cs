using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum TutorialTrigger
{
    Manual,          // 버튼 클릭으로만 진행
    OnPerfectInput,  // Perfect 판정 시
    OnSkillUse,      // 스킬 사용 시
    OnGuardDetected, // 경비병 발견 시
    OnMissionStart   // 미션 시작 시
}


/// <summary>
/// 튜토리얼 단계를 관리하고 가이드를 표시합니다.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("▶ UI 요소")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialTitleText;
    public TextMeshProUGUI tutorialDescriptionText;
    public Image tutorialImage;
    public Button nextButton;
    public Button skipButton;

    [Header("▶ 튜토리얼 단계")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    private int _currentStepIndex = 0;
    private bool _isTutorialActive = false;

    [Header("▶ 인게임 가이드")]
    public GameObject beatIndicatorHighlight;
    public GameObject skillIconHighlight;

    private RhythmSyncManager _rhythmManager;
    private PlayerController _playerController;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _playerController = FindObjectOfType<PlayerController>();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);

        // 첫 실행인지 확인
        if (SaveSystem.Instance != null && !SaveSystem.Instance.currentSave.completedMissions.Length.Equals(0))
            tutorialPanel?.SetActive(false); // 이미 게임을 플레이한 적이 있으면 튜토리얼 스킵
        else
            StartTutorial();
    }

    /// <summary>
    /// 튜토리얼을 시작합니다.
    /// </summary>
    public void StartTutorial()
    {
        _isTutorialActive = true;
        _currentStepIndex = 0;
        ShowCurrentStep();
    }

    /// <summary>
    /// 현재 단계를 표시합니다.
    /// </summary>
    void ShowCurrentStep()
    {
        if (_currentStepIndex >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[_currentStepIndex];

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (tutorialTitleText != null)
            tutorialTitleText.text = step.title;

        if (tutorialDescriptionText != null)
            tutorialDescriptionText.text = step.description;

        if (tutorialImage != null && step.illustrationImage != null)
        {
            tutorialImage.sprite = step.illustrationImage;
            tutorialImage.gameObject.SetActive(true);
        }
        else if (tutorialImage != null)
            tutorialImage.gameObject.SetActive(false);

        // 게임 일시정지
        if (step.pauseGame)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        // 인게임 가이드 표시
        if (beatIndicatorHighlight != null)
            beatIndicatorHighlight.SetActive(step.showBeatIndicator);
        if (skillIconHighlight != null)
            skillIconHighlight.SetActive(step.showSkillHighlight);
    }

    /// <summary>
    /// 다음 단계로 진행합니다.
    /// </summary>
    public void NextStep()
    {
        _currentStepIndex++;
        ShowCurrentStep();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// 튜토리얼을 건너뜁니다.
    /// </summary>
    public void SkipTutorial()
    {
        EndTutorial();

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// 튜토리얼을 종료합니다.
    /// </summary>
    void EndTutorial()
    {
        _isTutorialActive = false;
        Time.timeScale = 1f;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (beatIndicatorHighlight != null)
            beatIndicatorHighlight.SetActive(false);

        if (skillIconHighlight != null)
            skillIconHighlight.SetActive(false);

        Debug.Log("튜토리얼 완료");
    }

    /// <summary>
    /// 특정 트리거로 다음 단계로 진행합니다.
    /// </summary>
    public void TriggerStep(TutorialTrigger trigger)
    {
        if (!_isTutorialActive) return;
        if (_currentStepIndex >= tutorialSteps.Count) return;

        TutorialStep currentStep = tutorialSteps[_currentStepIndex];
        if (currentStep.trigger == trigger)
            NextStep();
    }

    // 외부에서 호출할 수 있는 트리거 함수들
    public void OnPerfectInput() => TriggerStep(TutorialTrigger.OnPerfectInput);
    public void OnSkillUsed() => TriggerStep(TutorialTrigger.OnSkillUse);
    public void OnGuardDetected() => TriggerStep(TutorialTrigger.OnGuardDetected);
}

[System.Serializable]
public class TutorialStep
{
    public string title;
    [TextArea(3, 6)]
    public string description;
    public Sprite illustrationImage;
    public TutorialTrigger trigger = TutorialTrigger.Manual;
    public bool pauseGame = true;
    public bool showBeatIndicator = false;
    public bool showSkillHighlight = false;
}
