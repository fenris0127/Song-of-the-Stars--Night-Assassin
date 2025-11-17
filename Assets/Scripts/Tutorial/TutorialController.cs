using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SongOfTheStars.Core;
using SongOfTheStars.Rhythm;

namespace SongOfTheStars.Tutorial
{
    /// <summary>
    /// Manages tutorial flow with step-by-step guidance
    /// 튜토리얼 흐름을 단계별 안내와 함께 관리
    ///
    /// Integrates with EnhancedMissionManager for tutorial mission
    /// Works with TutorialUI for visual prompts
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        #region Singleton
        public static TutorialController Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        [Header("▶ Tutorial Settings")]
        [Tooltip("Enable tutorial prompts and hand-holding")]
        public bool enableTutorial = true;

        [Tooltip("Wait for user acknowledgment before continuing?")]
        public bool waitForAcknowledgment = true;

        [Tooltip("Show beat visualization always (even when not required)")]
        public bool alwaysShowBeatVisual = true;

        [Header("▶ Tutorial Steps")]
        public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

        [Header("▶ UI References")]
        public TutorialUI tutorialUI;
        public GameObject beatVisualizationOverlay;

        [Header("▶ Audio")]
        public AudioClip stepCompleteSFX;
        public AudioClip tutorialCompleteSFX;

        #region Private State
        private int _currentStepIndex = 0;
        private bool _isTutorialActive = false;
        private bool _waitingForAcknowledgment = false;
        private RhythmSyncManager _rhythmManager;
        private EnhancedMissionManager _missionManager;
        private int _perfectInputCount = 0;
        #endregion

        #region Initialization
        void Start()
        {
            _rhythmManager = FindObjectOfType<RhythmSyncManager>();
            _missionManager = FindObjectOfType<EnhancedMissionManager>();

            if (enableTutorial)
            {
                SetupTutorialSteps();
                StartTutorial();
            }

            // Always show beat visual in tutorial
            if (alwaysShowBeatVisual && beatVisualizationOverlay != null)
            {
                beatVisualizationOverlay.SetActive(true);
            }
        }

        private void SetupTutorialSteps()
        {
            // If no steps defined, create default tutorial flow
            if (tutorialSteps.Count == 0)
            {
                tutorialSteps = CreateDefaultTutorialSteps();
            }
        }

        private List<TutorialStep> CreateDefaultTutorialSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    stepID = "welcome",
                    title = "Welcome, Night Assassin",
                    description = "This training will teach you the fundamentals of rhythm-based stealth.\n\nFeel the rhythm of the stars. Each beat is an opportunity.",
                    type = TutorialStepType.Dialog,
                    requireAcknowledgment = true
                },
                new TutorialStep
                {
                    stepID = "movement",
                    title = "Movement",
                    description = "Use WASD or Arrow Keys to move.\n\nMove to the marked location on the beat for best results.",
                    type = TutorialStepType.Movement,
                    targetTag = "Tutorial_Waypoint_1",
                    highlightTarget = true
                },
                new TutorialStep
                {
                    stepID = "rhythm_basics",
                    title = "Rhythm Timing",
                    description = "Watch the beat visualization.\n\nActions performed ON THE BEAT grant Focus energy.\n\nTry to hit 5 Perfect inputs.",
                    type = TutorialStepType.RhythmPractice,
                    targetCount = 5
                },
                new TutorialStep
                {
                    stepID = "focus_explanation",
                    title = "Focus Energy",
                    description = "Perfect timing generates Focus (blue bar).\n\nFocus powers your constellation skills.\n\nMaintain your combo for cooldown reduction!",
                    type = TutorialStepType.Dialog,
                    requireAcknowledgment = true
                },
                new TutorialStep
                {
                    stepID = "first_skill",
                    title = "Constellation Skills",
                    description = "Press 1 ON THE BEAT to use Capricorn Trap.\n\nSkills cost Focus and have cooldowns.\n\nPerfect timing reduces cooldowns!",
                    type = TutorialStepType.SkillUse,
                    skillSlot = 0,
                    targetCount = 1
                },
                new TutorialStep
                {
                    stepID = "elimination",
                    title = "Elimination",
                    description = "Approach the practice dummy when it's trapped.\n\nPress SPACE to eliminate.\n\nStealth is your greatest weapon.",
                    type = TutorialStepType.Elimination,
                    targetTag = "TutorialDummy",
                    targetCount = 1
                },
                new TutorialStep
                {
                    stepID = "complete",
                    title = "Tutorial Complete!",
                    description = "You've mastered the basics!\n\n• Move on the beat\n• Perfect timing = Focus\n• Use skills wisely\n• Stay in the shadows\n\nYou're ready for your first mission.",
                    type = TutorialStepType.Dialog,
                    requireAcknowledgment = true
                }
            };
        }
        #endregion

        #region Tutorial Flow
        public void StartTutorial()
        {
            if (!enableTutorial) return;

            _isTutorialActive = true;
            _currentStepIndex = 0;

            Debug.Log("Tutorial started");

            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            if (_currentStepIndex >= tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }

            TutorialStep step = tutorialSteps[_currentStepIndex];

            Debug.Log($"Tutorial Step {_currentStepIndex + 1}/{tutorialSteps.Count}: {step.title}");

            // Show UI
            if (tutorialUI != null)
            {
                tutorialUI.ShowStep(step);
            }

            // Highlight target if needed
            if (step.highlightTarget && !string.IsNullOrEmpty(step.targetTag))
            {
                HighlightTarget(step.targetTag);
            }

            // Handle step-specific setup
            HandleStepSetup(step);

            // If requires acknowledgment, wait for input
            if (step.requireAcknowledgment)
            {
                _waitingForAcknowledgment = true;
            }
        }

        private void HandleStepSetup(TutorialStep step)
        {
            switch (step.type)
            {
                case TutorialStepType.RhythmPractice:
                    _perfectInputCount = 0;
                    break;

                case TutorialStepType.SkillUse:
                    // Ensure player has focus
                    var focusManager = FindObjectOfType<FocusManager>();
                    if (focusManager != null)
                    {
                        // Give enough focus to use skill
                        focusManager.GenerateFocus(30f);
                    }
                    break;
            }
        }

        public void AcknowledgeCurrentStep()
        {
            if (!_waitingForAcknowledgment) return;

            _waitingForAcknowledgment = false;

            // Auto-advance dialog-only steps
            TutorialStep currentStep = tutorialSteps[_currentStepIndex];
            if (currentStep.type == TutorialStepType.Dialog)
            {
                AdvanceToNextStep();
            }
        }

        public void NotifyStepProgress(string stepID, int amount = 1)
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep currentStep = tutorialSteps[_currentStepIndex];

            if (currentStep.stepID != stepID) return;

            currentStep.currentProgress += amount;

            // Update UI
            if (tutorialUI != null)
            {
                tutorialUI.UpdateProgress(currentStep.currentProgress, currentStep.targetCount);
            }

            // Check completion
            if (currentStep.currentProgress >= currentStep.targetCount)
            {
                OnStepCompleted(currentStep);
            }
        }

        private void OnStepCompleted(TutorialStep step)
        {
            Debug.Log($"Tutorial step completed: {step.title}");

            // Play SFX
            if (stepCompleteSFX != null)
            {
                AudioSource.PlayClipAtPoint(stepCompleteSFX, Camera.main.transform.position, 0.5f);
            }

            // Show completion feedback
            if (tutorialUI != null)
            {
                tutorialUI.ShowStepComplete();
            }

            // Advance after delay
            StartCoroutine(AdvanceAfterDelay(1.5f));
        }

        private IEnumerator AdvanceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceToNextStep();
        }

        private void AdvanceToNextStep()
        {
            _currentStepIndex++;
            ShowCurrentStep();
        }

        private void CompleteTutorial()
        {
            _isTutorialActive = false;

            Debug.Log("Tutorial completed!");

            // Play completion SFX
            if (tutorialCompleteSFX != null)
            {
                AudioSource.PlayClipAtPoint(tutorialCompleteSFX, Camera.main.transform.position);
            }

            // Show completion screen
            if (tutorialUI != null)
            {
                tutorialUI.ShowTutorialComplete();
            }

            // Notify mission manager
            if (_missionManager != null)
            {
                _missionManager.CompleteMission();
            }
        }
        #endregion

        #region Event Handlers
        public void OnRhythmInput(string judgment)
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep currentStep = tutorialSteps[_currentStepIndex];

            if (currentStep.type == TutorialStepType.RhythmPractice)
            {
                if (judgment == "Perfect")
                {
                    NotifyStepProgress(currentStep.stepID, 1);
                }
            }
        }

        public void OnSkillUsed(int skillSlot)
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep currentStep = tutorialSteps[_currentStepIndex];

            if (currentStep.type == TutorialStepType.SkillUse)
            {
                if (currentStep.skillSlot == skillSlot)
                {
                    NotifyStepProgress(currentStep.stepID, 1);
                }
            }
        }

        public void OnTargetReached(string targetTag)
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep currentStep = tutorialSteps[_currentStepIndex];

            if (currentStep.type == TutorialStepType.Movement)
            {
                if (currentStep.targetTag == targetTag)
                {
                    NotifyStepProgress(currentStep.stepID, 1);
                }
            }
        }

        public void OnEnemyEliminated(string enemyTag)
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex >= tutorialSteps.Count) return;

            TutorialStep currentStep = tutorialSteps[_currentStepIndex];

            if (currentStep.type == TutorialStepType.Elimination)
            {
                if (currentStep.targetTag == enemyTag)
                {
                    NotifyStepProgress(currentStep.stepID, 1);
                }
            }
        }
        #endregion

        #region Visual Helpers
        private void HighlightTarget(string targetTag)
        {
            GameObject target = GameObject.FindGameObjectWithTag(targetTag);
            if (target != null)
            {
                // Add highlight effect
                var highlighter = target.GetComponent<TutorialHighlight>();
                if (highlighter == null)
                {
                    highlighter = target.AddComponent<TutorialHighlight>();
                }
                highlighter.StartHighlight();
            }
        }
        #endregion

        #region Public API
        public bool IsTutorialActive() => _isTutorialActive;
        public int GetCurrentStepIndex() => _currentStepIndex;
        public int GetTotalSteps() => tutorialSteps.Count;
        public TutorialStep GetCurrentStep() => (_currentStepIndex < tutorialSteps.Count) ? tutorialSteps[_currentStepIndex] : null;
        #endregion
    }

    #region Data Structures
    [System.Serializable]
    public class TutorialStep
    {
        public string stepID;
        public string title;
        [TextArea(3, 6)]
        public string description;
        public TutorialStepType type;

        [Header("Targeting")]
        public string targetTag = "";
        public int targetCount = 1;
        public int currentProgress = 0;
        public bool highlightTarget = false;

        [Header("Skill-specific")]
        public int skillSlot = -1; // Which skill slot (0-3)

        [Header("Dialog-specific")]
        public bool requireAcknowledgment = false;
    }

    public enum TutorialStepType
    {
        Dialog,          // Just show text, wait for acknowledgment
        Movement,        // Reach a target location
        RhythmPractice,  // Hit X perfect inputs
        SkillUse,        // Use a specific skill
        Elimination,     // Eliminate target enemy
        Stealth,         // Stay undetected for duration
        Custom           // Custom condition (script-driven)
    }
    #endregion
}
