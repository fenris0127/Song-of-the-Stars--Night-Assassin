using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SongOfTheStars.Tutorial
{
    /// <summary>
    /// UI display for tutorial prompts and progress
    /// 튜토리얼 프롬프트 및 진행 상황 UI 표시
    ///
    /// Works with TutorialController to show step-by-step guidance
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("▶ UI Panels")]
        public GameObject tutorialPanel;
        public GameObject completionPanel;

        [Header("▶ Text Elements")]
        public Text titleText;
        public Text descriptionText;
        public Text progressText;
        public Text stepCountText;

        [Header("▶ Progress Display")]
        public Slider progressSlider;
        public GameObject progressContainer;

        [Header("▶ Buttons")]
        public Button acknowledgeButton;
        public GameObject buttonContainer;

        [Header("▶ Animation")]
        public Animator panelAnimator;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 0.3f;

        [Header("▶ Styling")]
        public Color titleColor = Color.white;
        public Color descriptionColor = new Color(0.9f, 0.9f, 0.9f);
        public Color progressColor = Color.cyan;
        public int titleFontSize = 28;
        public int descriptionFontSize = 18;

        private CanvasGroup _panelCanvasGroup;
        private TutorialController _controller;
        private bool _isShowing = false;

        void Awake()
        {
            _controller = FindObjectOfType<TutorialController>();

            // Setup canvas group for fading
            if (tutorialPanel != null && tutorialPanel.GetComponent<CanvasGroup>() == null)
            {
                _panelCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }
            else if (tutorialPanel != null)
            {
                _panelCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            }

            // Setup acknowledge button
            if (acknowledgeButton != null)
            {
                acknowledgeButton.onClick.AddListener(OnAcknowledgeClicked);
            }

            // Start hidden
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }
        }

        public void ShowStep(TutorialStep step)
        {
            if (tutorialPanel == null) return;

            // Update text content
            if (titleText != null)
            {
                titleText.text = step.title;
                titleText.color = titleColor;
                titleText.fontSize = titleFontSize;
            }

            if (descriptionText != null)
            {
                descriptionText.text = step.description;
                descriptionText.color = descriptionColor;
                descriptionText.fontSize = descriptionFontSize;
            }

            // Update step counter
            if (stepCountText != null && _controller != null)
            {
                int current = _controller.GetCurrentStepIndex() + 1;
                int total = _controller.GetTotalSteps();
                stepCountText.text = $"Step {current}/{total}";
            }

            // Setup progress display
            bool showProgress = step.targetCount > 1;
            if (progressContainer != null)
            {
                progressContainer.SetActive(showProgress);
            }

            if (showProgress)
            {
                UpdateProgress(step.currentProgress, step.targetCount);
            }

            // Setup button visibility
            bool showButton = step.requireAcknowledgment;
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(showButton);
            }
            else if (acknowledgeButton != null)
            {
                acknowledgeButton.gameObject.SetActive(showButton);
            }

            // Show panel
            tutorialPanel.SetActive(true);
            StartCoroutine(FadeIn());
        }

        public void UpdateProgress(int current, int target)
        {
            if (progressText != null)
            {
                progressText.text = $"{current}/{target}";
                progressText.color = progressColor;
            }

            if (progressSlider != null)
            {
                progressSlider.maxValue = target;
                progressSlider.value = current;
            }
        }

        public void ShowStepComplete()
        {
            StartCoroutine(FlashCompletion());
        }

        private IEnumerator FlashCompletion()
        {
            // Flash the panel to indicate completion
            if (titleText != null)
            {
                Color originalColor = titleText.color;
                titleText.color = Color.green;

                yield return new WaitForSeconds(0.5f);

                titleText.color = originalColor;
            }
        }

        public void HideStep()
        {
            StartCoroutine(FadeOutAndHide());
        }

        public void ShowTutorialComplete()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }
        }

        private void OnAcknowledgeClicked()
        {
            if (_controller != null)
            {
                _controller.AcknowledgeCurrentStep();
            }
        }

        #region Animation
        private IEnumerator FadeIn()
        {
            if (_panelCanvasGroup == null) yield break;

            _isShowing = true;

            float elapsed = 0f;
            _panelCanvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            _panelCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndHide()
        {
            if (_panelCanvasGroup == null) yield break;

            _isShowing = false;

            float elapsed = 0f;
            _panelCanvasGroup.alpha = 1f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                _panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            _panelCanvasGroup.alpha = 0f;

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }
        #endregion
    }
}
