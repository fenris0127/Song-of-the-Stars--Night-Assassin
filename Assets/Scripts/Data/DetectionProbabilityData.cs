using UnityEngine;

    /// <summary>
    /// 경비병의 시야각과 거리에 따른 탐지 확률을 정의하는 ScriptableObject
    /// </summary>
[CreateAssetMenu(fileName = "DetectionProbabilityData", menuName = "Song of the Stars/Detection Probability Data")]
public class DetectionProbabilityData : ScriptableObject
{
    [Header("기본 탐지 설정")]
    [Tooltip("FOV 내부에서의 기본 탐지 확률 (0-1)")]
    [Range(0f, 1f)]
    public float baseFOVDetectionProbability = 0.6f;

    [Header("거리 기반 탐지")]
    [Tooltip("최대 시야 거리")]
    public float maxViewDistance = 10f;
    
    [Tooltip("거리에 따른 탐지 확률 감소 커브")]
    public AnimationCurve distanceFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("이동 속도 영향")]
    [Tooltip("이동 속도가 탐지 확률에 미치는 영향 승수")]
    [Range(1f, 3f)]
    public float movementSpeedMultiplier = 1.5f;
    
    [Tooltip("최대 이동 속도 (이 속도일 때 최대 영향)")]
    public float maxMovementSpeed = 5f;

    [Header("스텔스 영향")]
    [Tooltip("스텔스 알파값이 탐지 확률에 미치는 영향 커브")]
    public AnimationCurve stealthAlphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("경보 레벨 영향")]
    [Tooltip("경보 레벨당 탐지 확률 증가")]
    [Range(0f, 0.5f)]
    public float alertLevelProbabilityIncrease = 0.1f;
}