using UnityEngine;

public enum EffectType
{
    Flash,      // 섬광 (시야 차단)
    Paralysis,  // 마비 (이동 정지)
    Assassination, // 암살 (즉사)
    Alert       // 경보 (소음)
}

/// <summary>
/// 투사체나 스킬 이펙트가 충돌 시 적용할 효과를 관리합니다.
/// (예: 섬광탄, 함정, 투척 암살 무기 등)
/// </summary>
public class CollisionEffect : MonoBehaviour
{
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;
    private Collider2D[] _areaEffectResults = new Collider2D[30];


    [Header("▶ 충돌 효과 타입")]
    public EffectType effectType = EffectType.Flash;

    [Header("▶ 효과 설정")]
    public int durationInBeats = 3;
    public float effectRadius = 5f;
    public LayerMask targetMask;

    [Header("▶ VFX")]
    public GameObject impactVFXPrefab;

    private RhythmSyncManager _rhythmManager;
    private bool _hasTriggered = false;

    void Start()
    {
        // _rhythmManager = FindObjectOfType<RhythmSyncManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;

        // 경비병과 충돌 시
        GuardRhythmPatrol guard = other.GetComponent<GuardRhythmPatrol>();
        if (guard != null)
        {
            ApplyEffectToGuard(guard);

            _hasTriggered = true;

            // 충돌 VFX 생성
            if (impactVFXPrefab != null)
            {
                GameObject vfx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
                VFXLifetime lifetime = vfx.GetComponent<VFXLifetime>();
                if (lifetime != null)
                    lifetime.SetBeatDuration(2);
                else
                    Destroy(vfx, 2f);
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 범위 효과 (수류탄처럼 폭발)
    /// </summary>
    public void TriggerAreaEffect()
    {
        if (_hasTriggered || RhythmManager == null) return;

        _hasTriggered = true;

        // Use compatibility helper to centralize overlap queries and avoid allocations
    int hitCount = GameServices.OverlapCircleCompat(transform.position, effectRadius, targetMask, _areaEffectResults);

        for (int i = 0; i < hitCount; i++)
        {
            GuardRhythmPatrol guard = _areaEffectResults[i].GetComponent<GuardRhythmPatrol>();
            if (guard != null)
                ApplyEffectToGuard(guard);
        }

        // VFX 생성
        if (impactVFXPrefab != null)
        {
            if (VFXManager_Pooled.Instance != null)
                VFXManager_Pooled.Instance.PlayVFX("VFX_Explosion", transform.position);
            else
            {
                GameObject vfx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }
        }

        Destroy(gameObject);
    }

    void ApplyEffectToGuard(GuardRhythmPatrol guard)
    {
        switch (effectType)
        {
            case EffectType.Flash:
                guard.ApplyFlash(durationInBeats);
                Debug.Log($"{guard.gameObject.name}에게 섬광 효과 적용 ({durationInBeats} 비트)");
                break;
            case EffectType.Paralysis:
                guard.ApplyParalysis(durationInBeats);
                Debug.Log($"{guard.gameObject.name}에게 마비 효과 적용 ({durationInBeats} 비트)");
                break;
            case EffectType.Assassination:
                guard.Die();
                Debug.Log($"{guard.gameObject.name} 암살 성공");
                break;
            case EffectType.Alert:
                if (GameServices.MissionManager != null)
                {
                    GameServices.MissionManager.IncreaseAlertLevel(2);
                    Debug.Log("소음으로 인한 경보 레벨 증가!");
                }
                break;
        }
    }

    // 디버그용: 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}