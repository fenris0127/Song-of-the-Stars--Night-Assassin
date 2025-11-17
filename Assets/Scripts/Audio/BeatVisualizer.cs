using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enhanced rhythm beat visualizer with pulse effects and particle feedback
/// 펄스 효과와 파티클 피드백이 있는 향상된 리듬 비트 시각화
/// </summary>
public class BeatVisualizer : MonoBehaviour
{
    [Header("▶ Beat Flash")]
    [Tooltip("Image that flashes on each beat")]
    public Image beatIndicator;
    public Color beatColor = Color.white;
    public float flashDuration = 0.1f;

    [Header("▶ Judgment Zone")]
    [Tooltip("Visual representation of Perfect judgment zone")]
    public RectTransform judgmentZone;

    [Tooltip("Moving indicator showing current timing")]
    public RectTransform movingIndicator;
    public float indicatorWidth = 800f;

    [Header("▶ Screen Pulse Effect")]
    [Tooltip("Enable screen pulse on beats")]
    public bool enableScreenPulse = true;

    [Tooltip("Panel that pulses with the beat (optional, can be background)")]
    public Image screenPulsePanel;

    [Tooltip("Pulse intensity multiplier")]
    [Range(0.1f, 1f)]
    public float pulseIntensity = 0.3f;

    [Tooltip("Pulse fade speed")]
    [Range(1f, 10f)]
    public float pulseFadeSpeed = 5f;

    [Header("▶ Particle Effects")]
    [Tooltip("Particle system for Perfect timing")]
    public ParticleSystem perfectParticles;

    [Tooltip("Particle system for Great timing")]
    public ParticleSystem greatParticles;

    [Tooltip("Particle system for beat pulse")]
    public ParticleSystem beatParticles;

    [Header("▶ Judgment Colors")]
    public Color perfectPulseColor = new Color(0f, 1f, 0f, 0.3f); // Green
    public Color greatPulseColor = new Color(1f, 1f, 0f, 0.2f);   // Yellow
    public Color missPulseColor = new Color(1f, 0f, 0f, 0.15f);   // Red

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private RhythmPatternChecker RhythmChecker => GameServices.RhythmChecker;

    private float _halfIndicatorWidth;
    private float _perfectZoneWidthCache;

    private Color _originalColor;
    private float _flashTimer = 0f;

    // Screen pulse state
    private float _currentPulseAlpha = 0f;
    private Color _currentPulseColor = Color.clear;
    private Color _targetPulseColor = Color.clear;

    void Start()
    {
        if (RhythmManager != null)
        RhythmManager.OnBeatCounted.AddListener(OnBeatFlash);
    
        if (beatIndicator != null)
            _originalColor = beatIndicator.color;
        
        // ⭐ 사전 계산
        _halfIndicatorWidth = indicatorWidth / 2f;
    }

    void Update()
    {
        UpdateFlash();
        UpdateJudgmentIndicator();
        UpdateScreenPulse();
    }

    void OnBeatFlash(int beat)
    {
        if (beatIndicator != null)
        {
            beatIndicator.color = beatColor;
            _flashTimer = flashDuration;
        }

        // Trigger screen pulse on beat
        if (enableScreenPulse)
        {
            TriggerScreenPulse(beatColor, pulseIntensity * 0.5f); // Subtle pulse for regular beats
        }

        // Trigger beat particles if available
        if (beatParticles != null)
        {
            beatParticles.Play();
        }
    }

    void UpdateFlash()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            
            if (_flashTimer <= 0f && beatIndicator != null)
                beatIndicator.color = _originalColor;
        }
    }

    void UpdateJudgmentIndicator()
    {
        if (RhythmManager == null || movingIndicator == null) return;
    
        float beatInterval = RhythmManager.beatInterval;
        float timeSinceLastBeat = Time.time - (RhythmManager.currentBeatCount * beatInterval);
        float progress = timeSinceLastBeat / beatInterval;
        
        // ⭐ 최적화: Lerp 대신 직접 계산
        float xPosition = (progress * indicatorWidth) - _halfIndicatorWidth;
        movingIndicator.anchoredPosition = new Vector2(xPosition, movingIndicator.anchoredPosition.y);
        
        if (judgmentZone != null)
        {
            if (_perfectZoneWidthCache == 0f)
            {
                _perfectZoneWidthCache = (RhythmManager.perfectTolerance / beatInterval) * indicatorWidth * 2f;
            }
            judgmentZone.sizeDelta = new Vector2(_perfectZoneWidthCache, judgmentZone.sizeDelta.y);
        }
    }

    #region Screen Pulse Effect
    /// <summary>
    /// Updates the screen pulse fade
    /// 화면 펄스 페이드 업데이트
    /// </summary>
    void UpdateScreenPulse()
    {
        if (!enableScreenPulse || screenPulsePanel == null) return;

        // Fade out current pulse
        if (_currentPulseAlpha > 0f)
        {
            _currentPulseAlpha -= Time.deltaTime * pulseFadeSpeed;
            _currentPulseAlpha = Mathf.Max(0f, _currentPulseAlpha);

            // Apply alpha to current color
            _currentPulseColor.a = _currentPulseAlpha;
            screenPulsePanel.color = _currentPulseColor;
        }
    }

    /// <summary>
    /// Triggers a screen pulse with specified color and intensity
    /// 지정된 색상과 강도로 화면 펄스 트리거
    /// </summary>
    /// <param name="color">Pulse color</param>
    /// <param name="intensity">Pulse intensity (0-1)</param>
    public void TriggerScreenPulse(Color color, float intensity)
    {
        if (!enableScreenPulse || screenPulsePanel == null) return;

        _currentPulseColor = color;
        _currentPulseAlpha = Mathf.Clamp01(intensity);
        _currentPulseColor.a = _currentPulseAlpha;
        screenPulsePanel.color = _currentPulseColor;
    }
    #endregion

    #region Judgment Feedback
    /// <summary>
    /// Triggers visual feedback for judgment quality
    /// 판정 품질에 대한 시각적 피드백 트리거
    /// </summary>
    /// <param name="judgmentQuality">Perfect, Great, or Miss</param>
    public void OnJudgmentFeedback(string judgmentQuality)
    {
        switch (judgmentQuality.ToLower())
        {
            case "perfect":
                TriggerPerfectFeedback();
                break;
            case "great":
                TriggerGreatFeedback();
                break;
            case "miss":
                TriggerMissFeedback();
                break;
        }
    }

    private void TriggerPerfectFeedback()
    {
        // Screen pulse for Perfect
        if (enableScreenPulse)
        {
            TriggerScreenPulse(perfectPulseColor, pulseIntensity);
        }

        // Particle effect for Perfect
        if (perfectParticles != null)
        {
            perfectParticles.Play();
        }
    }

    private void TriggerGreatFeedback()
    {
        // Screen pulse for Great
        if (enableScreenPulse)
        {
            TriggerScreenPulse(greatPulseColor, pulseIntensity * 0.7f);
        }

        // Particle effect for Great
        if (greatParticles != null)
        {
            greatParticles.Play();
        }
    }

    private void TriggerMissFeedback()
    {
        // Screen pulse for Miss (red warning)
        if (enableScreenPulse)
        {
            TriggerScreenPulse(missPulseColor, pulseIntensity * 0.5f);
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Manually trigger beat visualizer (for testing)
    /// 비트 비주얼라이저 수동 트리거 (테스트용)
    /// </summary>
    [ContextMenu("Test Beat Pulse")]
    public void TestBeatPulse()
    {
        if (Application.isPlaying)
        {
            OnBeatFlash(0);
        }
    }

    [ContextMenu("Test Perfect Judgment")]
    public void TestPerfectJudgment()
    {
        if (Application.isPlaying)
        {
            OnJudgmentFeedback("perfect");
        }
    }

    [ContextMenu("Test Great Judgment")]
    public void TestGreatJudgment()
    {
        if (Application.isPlaying)
        {
            OnJudgmentFeedback("great");
        }
    }

    [ContextMenu("Test Miss Judgment")]
    public void TestMissJudgment()
    {
        if (Application.isPlaying)
        {
            OnJudgmentFeedback("miss");
        }
    }
    #endregion

    void OnDestroy()
    {
        if (RhythmManager != null)
            RhythmManager.OnBeatCounted.RemoveListener(OnBeatFlash);
    }
}