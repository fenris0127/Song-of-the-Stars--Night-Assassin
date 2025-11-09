using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어의 스텔스 상태와 경비병의 시야를 고려한 확률적 탐지 시스템
/// </summary>
public class ProbabilisticDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    private float _currentProgress = 0f;
    [SerializeField] private DetectionProbabilityData detectionData;
    [SerializeField] private float detectionCheckInterval = 0.5f;

    public float progressIncreaseMultiplier = 1.0f; // 시야에 있을 때 기본 증가율
    public float progressDecreaseRate = 0.5f;      // 시야에서 벗어났을 때 감소 속도 (초당)
    
    [Header("References")]
    [SerializeField] private GuardRhythmPatrol guardPatrol;
    [SerializeField] private Transform playerTransform;
    
    private PlayerStealth playerStealth;
    private Rigidbody2D playerRigidbody;
    private float detectionTimer;
    private int currentAlertLevel;

    // 탐지 시도 이벤트 (현재 탐지 확률)
    public UnityEvent<float> onDetectionAttempt = new UnityEvent<float>();
    // 탐지 성공 이벤트
    public UnityEvent onDetectionSuccess = new UnityEvent();

    private void Start()
    {
        if (playerTransform != null)
        {
            playerStealth = playerTransform.GetComponent<PlayerStealth>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("Player Transform이 설정되지 않았습니다!");
        }

        detectionTimer = detectionCheckInterval;
    }

    private void Update()
    {
        // 1. 발각 진행도 감소 (모든 프레임에서 점진적 감소)
        _currentProgress -= progressDecreaseRate * Time.deltaTime;

        // 2. 플레이어 시야 체크 (GuardRhythmPatrol의 시야각/거리를 이용)
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        // IsInFieldOfView는 ProbabilisticDetection.cs에 정의되어 있어야 합니다.
        if (IsInFieldOfView(directionToPlayer, distance)) 
        {
            // 3. 탐지 확률 계산
            float probability = CalculateDetectionProbability(distance);

            // 4. 발각 진행도 증가
            // 확률에 비례하여 증가율 조정 (예: 확률이 높으면 더 빨리 참)
            _currentProgress += probability * progressIncreaseMultiplier * Time.deltaTime;
        }

        // 5. 진행도 클램프
        _currentProgress = Mathf.Clamp01(_currentProgress);
        
        // 6. 탐지 성공 체크
        if (_currentProgress >= 1f)
             onDetectionSuccess?.Invoke();
    }

    // 현재 발각 진행도를 반환합니다. (0.0~1.0)
    public float GetDetectionProgress() => _currentProgress;
    
    /// <summary>
    /// 기존 코드에서 호출하던 CalculateDetection() 호환 래퍼
    /// </summary>
    public float CalculateDetection() => GetDetectionProgress();

    /// <summary>
    /// 현재 상태에 기반하여 플레이어 탐지를 시도합니다.
    /// </summary>
    private void TryDetectPlayer()
    {
        if (playerTransform == null || playerStealth == null || guardPatrol == null) return;

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // FOV 체크
        if (!IsInFieldOfView(directionToPlayer, distanceToPlayer)) return;

        float detectionProbability = CalculateDetectionProbability(distanceToPlayer);
        
        // 탐지 시도 이벤트 발생
        onDetectionAttempt.Invoke(detectionProbability);

        // 탐지 판정
        if (Random.value < detectionProbability)
        {
            Debug.Log($"[ProbabilisticDetection] 플레이어 탐지 성공! (확률: {detectionProbability:P2})");
            onDetectionSuccess.Invoke();
        }
    }

    /// <summary>
    /// 플레이어가 경비병의 시야각 내에 있는지 확인합니다.
    /// </summary>
    private bool IsInFieldOfView(Vector2 directionToPlayer, float distanceToPlayer)
    {
        if (guardPatrol == null || guardPatrol.PlayerTransform == null) return false;

        Vector2 guardForward = guardPatrol.transform.up;
        float angleToPlayer = Vector2.Angle(guardForward, directionToPlayer);

        // 거리 및 각도 체크 (GuardRhythmPatrol의 값 사용)
        return distanceToPlayer <= guardPatrol.viewDistance &&
               angleToPlayer < guardPatrol.fieldOfViewAngle / 2f;
    }

    /// 현재 상황에 기반한 탐지 확률을 계산합니다.
    private float CalculateDetectionProbability(float distance)
    {
        // 1. 기본 FOV 탐지 확률
        float probability = detectionData.baseFOVDetectionProbability;

        // 2. 거리에 따른 감소
        float distanceFactor = detectionData.distanceFalloffCurve.Evaluate(distance / detectionData.maxViewDistance);
        probability *= distanceFactor;

        // 3. 플레이어 이동 속도 영향
        if (playerRigidbody != null)
        {
            float speedFactor = Mathf.Clamp01(playerRigidbody.linearVelocity.magnitude / detectionData.maxMovementSpeed);
            probability *= 1f + (speedFactor * (detectionData.movementSpeedMultiplier - 1f));
        }

        // 4. 스텔스 알파값 영향
        float stealthFactor = detectionData.stealthAlphaCurve.Evaluate(playerStealth.stealthAlpha);
        probability *= stealthFactor;

        // 5. 경보 레벨 영향
        probability += currentAlertLevel * detectionData.alertLevelProbabilityIncrease;

        return Mathf.Clamp01(probability);
    }

    /// <summary>
    /// 현재 경보 레벨을 업데이트합니다.
    /// </summary>
    public void UpdateAlertLevel(int newLevel)
    {
        currentAlertLevel = newLevel;
        Debug.Log($"[ProbabilisticDetection] 경보 레벨 변경: {currentAlertLevel}");
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerTransform == null) return;

        // 현재 탐지 확률을 시각화
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (IsInFieldOfView(directionToPlayer, distance))
        {
            float probability = CalculateDetectionProbability(distance);
            Gizmos.color = Color.Lerp(Color.green, Color.red, probability);
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    #endif
}