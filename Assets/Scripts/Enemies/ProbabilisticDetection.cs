using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어의 스텔스 상태와 경비병의 시야를 고려한 확률적 탐지 시스템
/// </summary>
public class ProbabilisticDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private DetectionProbabilityData detectionData;
    [SerializeField] private float detectionCheckInterval = 0.5f;
    
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
        detectionTimer -= Time.deltaTime;
        
        if (detectionTimer <= 0f)
        {
            detectionTimer = detectionCheckInterval;
            TryDetectPlayer();
        }
    }

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
    private bool IsInFieldOfView(Vector2 directionToPlayer, float distance)
    {
        if (distance > detectionData.maxViewDistance) return false;

        float angle = Vector2.Angle(transform.right, directionToPlayer);
        return angle <= guardPatrol.fieldOfViewAngle * 0.5f;
    }

    /// <summary>
    /// 현재 상황에 기반한 탐지 확률을 계산합니다.
    /// </summary>
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