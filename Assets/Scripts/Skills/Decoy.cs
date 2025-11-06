using UnityEngine;

/// <summary>
/// 경비병을 유인하는 데코이(미끼) 오브젝트입니다.
/// DecoyLifetime과 함께 사용하여 시간 제한을 가질 수 있습니다.
/// </summary>
public class Decoy : MonoBehaviour
{
    [Header("▶ 데코이 설정")]
    public float attractionRadius = 10f;
    public bool makeSound = true; // 소리를 내서 경비병 유인
    public bool isVisualDecoy = false; // 시각적 미끼 (플레이어 환영)
    
    [Header("▶ 경보 설정")]
    public bool triggersAlert = false; // 발견 시 경보 레벨 증가 여부
    public int alertIncrease = 1;
    
    private MissionManager _missionManager;
    private RhythmSyncManager _rhythmManager;
    private bool _hasBeenInvestigated = false;

    void Start()
    {
        _missionManager = FindObjectOfType<MissionManager>();
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        
        // 생성 시 주변 경비병에게 알림
        if (makeSound)
            AlertNearbyGuards();
    }

    /// <summary>
    /// 데코이 생성 시 주변 경비병을 이 위치로 유인합니다.
    /// </summary>
    void AlertNearbyGuards()
    {
        if (_rhythmManager == null) return;
        
        Collider2D[] nearbyGuards = Physics2D.OverlapCircleAll(
            transform.position, 
            attractionRadius, 
            _rhythmManager.guardMask
        );
        
        Debug.Log($"데코이 생성: {nearbyGuards.Length}명의 경비병 감지");
        
        // 경비병들은 GuardRhythmPatrol의 CheckForDecoy()에서 자동으로 이 오브젝트를 감지합니다.
    }

    /// <summary>
    /// 경비병이 데코이에 도착했을 때 호출됩니다.
    /// </summary>
    public void OnGuardArrived(GuardRhythmPatrol guard)
    {
        if (_hasBeenInvestigated) return;
        
        _hasBeenInvestigated = true;
        
        Debug.Log($"{guard.gameObject.name}이(가) 데코이에 도착했습니다.");
        
        // 경보 레벨 증가 (선택사항)
        if (triggersAlert && _missionManager != null)
            _missionManager.IncreaseAlertLevel(alertIncrease);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        GuardRhythmPatrol guard = other.GetComponent<GuardRhythmPatrol>();
        if (guard != null)
            OnGuardArrived(guard);
    }

    // 디버그용: 유인 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}