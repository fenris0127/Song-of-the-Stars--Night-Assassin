using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 리듬 비트를 시각적으로 표시하는 디버그 도구입니다.
/// </summary>
public class BeatVisualizer : MonoBehaviour
{
    [Header("시각화 설정")]
    public Image beatIndicator; // 비트마다 깜빡일 이미지
    public Color beatColor = Color.white;
    public float flashDuration = 0.1f;
    
    [Header("판정 영역 표시")]
    public RectTransform judgmentZone; // Perfect 판정 영역 표시
    public RectTransform movingIndicator; // 현재 타이밍을 나타내는 바
    public float indicatorWidth = 800f;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    // private RhythmSyncManager _rhythmManager;
    
    private float _halfIndicatorWidth;
    private float _perfectZoneWidthCache;

    private Color _originalColor;
    private float _flashTimer = 0f;

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
    }

    void OnBeatFlash(int beat)
    {
        if (beatIndicator != null)
        {
            beatIndicator.color = beatColor;
            _flashTimer = flashDuration;
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
    
    void OnDestroy()
    {
        if (RhythmManager != null)
        RhythmManager.OnBeatCounted.RemoveListener(OnBeatFlash);
    }
}