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

    [Header("Compatibility")]
    [Tooltip("If true, this prefab will automatically register itself with the Player on Start. Set to false when using canonical PlayerController instantiation to avoid double-registration.")]
    public bool autoRegisterWithPlayer = false;
    
    [Header("▶ 경보 설정")]
    public bool triggersAlert = false; // 발견 시 경보 레벨 증가 여부
    public int alertIncrease = 1;
    
    private bool _hasBeenInvestigated = false;

    void Start()
    {
        if (makeSound)
            AlertNearbyGuards();

        // prefabs if you need the old behavior.
        if (autoRegisterWithPlayer && GameServices.Player != null)
            GameServices.Player.ActivateDecoy(gameObject);
    }

    // 데코이 생성 시 주변 경비병을 이 위치로 유인합니다.
    void AlertNearbyGuards()
    {
        if (RhythmManager == null) return;
    
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position, 
            attractionRadius, 
            _nearbyGuardsResults, 
            RhythmManager.guardMask
        );

        Debug.Log($"데코이 생성: {hitCount}명의 경비병 감지");

        for (int i = 0; i < hitCount; i++)
        {
            GuardRhythmPatrol guard = _nearbyGuardsResults[i].GetComponent<GuardRhythmPatrol>();
            if (guard != null)
                guard.ChangeState(new GuardInvestigatingState(guard, transform.position));
        }
    }

    // 경비병이 데코이에 도착했을 때 호출됩니다.
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

    void OnDestroy()
    {
        // Ensure player decoy state is cleared when this decoy is destroyed
        if (GameServices.Player != null && GameServices.Player.DecoyObject == gameObject)
            GameServices.Player.DeactivateDecoy();
    }

    // 디버그용: 유인 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}