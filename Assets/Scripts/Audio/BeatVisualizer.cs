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
    
    private RhythmSyncManager _rhythmManager;
    private Color _originalColor;
    private float _flashTimer = 0f;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        
        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.AddListener(OnBeatFlash);
        
        if (beatIndicator != null)
            _originalColor = beatIndicator.color;
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
        if (_rhythmManager == null || movingIndicator == null) return;
        
        float beatInterval = _rhythmManager.beatInterval;
        float timeSinceLastBeat = Time.time - (_rhythmManager.currentBeatCount * beatInterval);
        float progress = timeSinceLastBeat / beatInterval;
        
        // 판정 영역 내에서 좌우로 이동하는 인디케이터
        float xPosition = Mathf.Lerp(-indicatorWidth / 2f, indicatorWidth / 2f, progress);
        movingIndicator.anchoredPosition = new Vector2(xPosition, movingIndicator.anchoredPosition.y);
        
        // Perfect 판정 영역 표시
        if (judgmentZone != null)
        {
            float perfectZoneWidth = (_rhythmManager.perfectTolerance / beatInterval) * indicatorWidth;
            judgmentZone.sizeDelta = new Vector2(perfectZoneWidth * 2f, judgmentZone.sizeDelta.y);
        }
    }
    
    void OnDestroy()
    {
        if (_rhythmManager != null)
            _rhythmManager.OnBeatCounted.RemoveListener(OnBeatFlash);
    }
}