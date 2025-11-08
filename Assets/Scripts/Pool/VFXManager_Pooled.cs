using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오브젝트 풀링을 활용한 최적화된 VFX 관리자
/// Instantiate/Destroy 호출을 최소화하여 GC 부하를 줄입니다.
/// </summary>
public class VFXManager_Pooled : MonoBehaviour
{
    public static VFXManager_Pooled Instance { get; private set; }

    [Header("▶ VFX 프리팹 (풀링용)")]
    public VFXConfig[] vfxConfigs;

    private Dictionary<string, Queue<GameObject>> _vfxPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, VFXConfig> _vfxConfigMap = new Dictionary<string, VFXConfig>();
    private Dictionary<GameObject, float> _activeVFX = new Dictionary<GameObject, float>();
    private Transform _poolContainer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools();
        }
        else
            Destroy(gameObject);
    }

    void InitializePools()
    {
        _poolContainer = new GameObject("VFX_Pool").transform;
        _poolContainer.SetParent(transform);

        foreach (VFXConfig config in vfxConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"VFXManager: '{config.effectName}' VFX 프리팹이 없습니다!");
                continue;
            }

            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < config.poolSize; i++)
            {
                GameObject vfx = Instantiate(config.prefab, _poolContainer);
                vfx.SetActive(false);
                pool.Enqueue(vfx);
            }

            _vfxPools[config.effectName] = pool;
            _vfxConfigMap[config.effectName] = config;
        }

        Debug.Log($"VFXManager: {vfxConfigs.Length}개의 VFX 풀 초기화 완료");
    }

    void Update()
    {
        UpdateActiveVFX();
    }

    void UpdateActiveVFX()
    {
        if (_activeVFX.Count == 0) return;

        List<GameObject> toReturn = null;
        float currentTime = Time.time;

        foreach (var pair in _activeVFX)
        {
            if (currentTime >= pair.Value)
            {
                if (toReturn == null)
                    toReturn = new List<GameObject>();

                toReturn.Add(pair.Key);
            }
        }

        if (toReturn != null)
        {
            foreach (GameObject vfx in toReturn)
                ReturnVFXToPool(vfx);
        }
    }

    /// <summary>
    /// VFX를 재생합니다 (오브젝트 풀 사용)
    /// </summary>
    public GameObject PlayVFX(string effectName, Vector3 position, Quaternion rotation, float? customDuration = null)
    {
        if (!_vfxPools.ContainsKey(effectName))
        {
            Debug.LogError($"VFXManager: '{effectName}' VFX를 찾을 수 없습니다!");
            return null;
        }

        Queue<GameObject> pool = _vfxPools[effectName];
        VFXConfig config = _vfxConfigMap[effectName];

        GameObject vfx = null;

        // 풀에서 가져오기
        if (pool.Count > 0)
        {
            vfx = pool.Dequeue();

            // 파괴된 경우 새로 생성
            if (vfx == null)
            {
                vfx = Instantiate(config.prefab, _poolContainer);
            }
        }
        else
        {
            // 풀이 비어있으면 새로 생성 (확장)
            vfx = Instantiate(config.prefab, _poolContainer);
            Debug.LogWarning($"VFXManager: '{effectName}' 풀이 가득 차서 확장되었습니다.");
        }

        // VFX 활성화 및 설정
        vfx.transform.position = position;
        vfx.transform.rotation = rotation;
        vfx.SetActive(true);

        // 파티클 시스템 재생
        ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        // 지속 시간 설정
        float duration = customDuration ?? config.defaultDuration;
        _activeVFX[vfx] = Time.time + duration;

        return vfx;
    }

    /// <summary>
    /// 오버로드: 위치만 지정
    /// </summary>
    public GameObject PlayVFX(string effectName, Vector3 position)
    {
        return PlayVFX(effectName, position, Quaternion.identity);
    }

    /// <summary>
    /// 비트 기반 VFX 재생
    /// </summary>
    public GameObject PlayVFXOnBeat(string effectName, Vector3 position, int? customBeats = null)
    {
        VFXConfig config = _vfxConfigMap.ContainsKey(effectName)
            ? _vfxConfigMap[effectName]
            : null;

        if (config == null || GameServices.RhythmManager == null)
            return PlayVFX(effectName, position);

        int beats = customBeats ?? config.defaultBeats;
        float duration = GameServices.BeatInterval() * beats;

        return PlayVFX(effectName, position, Quaternion.identity, duration);
    }

    /// <summary>
    /// VFX를 풀로 반환
    /// </summary>
    void ReturnVFXToPool(GameObject vfx)
    {
        if (vfx == null) return;

        // activeVFX에서 제거
        _activeVFX.Remove(vfx);

        // 파티클 정지
        ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
            ps.Stop();

        vfx.SetActive(false);
        vfx.transform.SetParent(_poolContainer);

        // 해당하는 풀 찾기 (이름 기반)
        foreach (var pair in _vfxConfigMap)
        {
            if (vfx.name.Contains(pair.Value.prefab.name))
            {
                _vfxPools[pair.Key].Enqueue(vfx);
                return;
            }
        }

        // 풀을 찾지 못한 경우 파괴
        Debug.LogWarning($"VFXManager: '{vfx.name}'에 해당하는 풀을 찾을 수 없어 파괴합니다.");
        Destroy(vfx);
    }

    /// <summary>
    /// 모든 활성 VFX를 즉시 중단
    /// </summary>
    public void StopAllVFX()
    {
        List<GameObject> allActive = new List<GameObject>(_activeVFX.Keys);
        foreach (GameObject vfx in allActive)
            ReturnVFXToPool(vfx);
    }

    /// <summary>
    /// 특정 VFX를 즉시 중단
    /// </summary>
    public void StopVFX(GameObject vfx)
    {
        if (vfx != null && _activeVFX.ContainsKey(vfx))
            ReturnVFXToPool(vfx);
    }

    /// <summary>
    /// 풀 통계 반환
    /// </summary>
    public string GetPoolStats()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== VFX Pool Stats ===");

        foreach (var pair in _vfxPools)
        {
            int available = pair.Value.Count;
            int active = CountActiveVFX(pair.Key);
            sb.AppendLine($"[{pair.Key}] Active: {active}, Available: {available}");
        }

        return sb.ToString();
    }

    int CountActiveVFX(string effectName)
    {
        int count = 0;
        VFXConfig config = _vfxConfigMap[effectName];

        foreach (GameObject vfx in _activeVFX.Keys)
        {
            if (vfx.name.Contains(config.prefab.name))
                count++;
        }

        return count;
    }
}

[System.Serializable]
public class VFXConfig
{
    public string effectName;
    public GameObject prefab;
    public int poolSize = 5;
    public float defaultDuration = 2f;
    public bool useBeats = false;
    public int defaultBeats = 2;
}