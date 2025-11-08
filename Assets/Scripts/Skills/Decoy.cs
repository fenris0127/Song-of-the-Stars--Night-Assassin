using UnityEngine;

/// <summary>
/// 경비병을 유인하는 데코이(미끼) 오브젝트입니다.
/// DecoyLifetime과 함께 사용하여 시간 제한을 가질 수 있습니다.
/// </summary>
public class Decoy : MonoBehaviour
{
    private MissionManager MissionManager => GameServices.MissionManager;
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private Collider2D[] _nearbyGuardsResults = new Collider2D[20];


    [Header("▶ 데코이 설정")]
    public float attractionRadius = 10f;
    public bool makeSound = true; // 소리를 내서 경비병 유인
    public bool isVisualDecoy = false; // 시각적 미끼 (플레이어 환영)
    
    [Header("▶ 경보 설정")]
    public bool triggersAlert = false; // 발견 시 경보 레벨 증가 여부
    public int alertIncrease = 1;
    
    // private MissionManager _missionManager;
    // private RhythmSyncManager _rhythmManager;
    private bool _hasBeenInvestigated = false;

    void Start()
    {
        if (makeSound)
            AlertNearbyGuards();
    }

    /// <summary>
    /// 데코이 생성 시 주변 경비병을 이 위치로 유인합니다.
    /// </summary>
    void AlertNearbyGuards()
    {
        if (RhythmManager == null) return;
    
        // ⭐ NonAlloc 사용
        int guardCount = Physics2D.OverlapCircleNonAlloc(transform.position, attractionRadius, _nearbyGuardsResults, RhythmManager.guardMask);
        
        Debug.Log($"데코이 생성: {guardCount}명의 경비병 감지");

    }

    /// <summary>
    /// 경비병이 데코이에 도착했을 때 호출됩니다.
    /// </summary>
    public void OnGuardArrived(GuardRhythmPatrol guard)
    {
        if (_hasBeenInvestigated) return;
    
        _hasBeenInvestigated = true;
        
        Debug.Log($"{guard.gameObject.name}이(가) 데코이에 도착했습니다.");
        
        if (triggersAlert && MissionManager != null)
            MissionManager.IncreaseAlertLevel(alertIncrease);
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