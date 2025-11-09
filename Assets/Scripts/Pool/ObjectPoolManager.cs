using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 향상된 오브젝트 풀링 시스템
/// - 프리워밍 지원
/// - 자동 확장 제한
/// - 통계 추적
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
        public bool expandable = true;
        public bool prewarm = true; // ⭐ 사전 로딩
    }

    [Header("▶ 풀 설정")]
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    private Dictionary<string, Pool> _poolSettings;
    private Dictionary<string, Transform> _poolParents;
    private Dictionary<string, int> _poolSizes;
    
    // ⭐ 통계 추적
    private Dictionary<string, PoolStats> _poolStats;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
            Destroy(gameObject);
    }

    void InitializePools()
    {
        _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        _poolSettings = new Dictionary<string, Pool>();
        _poolParents = new Dictionary<string, Transform>();
        _poolSizes = new Dictionary<string, int>();
        _poolStats = new Dictionary<string, PoolStats>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"[ObjectPool] '{pool.tag}' 프리팹 누락");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            GameObject poolParent = new GameObject($"Pool_{pool.tag}");
            poolParent.transform.SetParent(transform);
            _poolParents[pool.tag] = poolParent.transform;

            // ⭐ 프리워밍
            if (pool.prewarm)
            {
                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(pool.prefab, poolParent.transform);
                    objectPool.Enqueue(obj);
                }
            }

            _poolDictionary.Add(pool.tag, objectPool);
            _poolSettings.Add(pool.tag, pool);
            _poolSizes[pool.tag] = pool.prewarm ? pool.initialSize : 0;
            _poolStats[pool.tag] = new PoolStats();
        }
    }

    GameObject CreateNewObject(GameObject prefab, Transform parent)
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        
        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        if (pooledComponent == null)
            pooledComponent = obj.AddComponent<PooledObject>();
        
        return obj;
    }

    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"[ObjectPool] '{tag}' 풀 없음");
            return null;
        }

        GameObject obj = null;
        Queue<GameObject> pool = _poolDictionary[tag];
        Pool settings = _poolSettings[tag];

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            
            if (obj == null)
            {
                obj = CreateNewObject(settings.prefab, _poolParents[tag]);
                _poolSizes[tag]++;
            }
        }
        else if (settings.expandable && _poolSizes[tag] < settings.maxSize)
        {
            obj = CreateNewObject(settings.prefab, _poolParents[tag]);
            _poolSizes[tag]++;
            _poolStats[tag].expansions++;
        }
        else
        {
            Debug.LogWarning($"[ObjectPool] '{tag}' 풀 가득참 (Max: {settings.maxSize})");
            _poolStats[tag].failedSpawns++;
            return null;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        pooledComponent?.OnSpawn();

        _poolStats[tag].totalSpawns++;
        return obj;
    }

    public GameObject Spawn(string tag) => Spawn(tag, Vector3.zero, Quaternion.identity);

    public void Despawn(GameObject obj, string tag)
    {
        if (obj == null) return;

        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"[ObjectPool] '{tag}' 풀 없음");
            Destroy(obj);
            return;
        }

        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        pooledComponent?.OnDespawn();

        obj.SetActive(false);
        obj.transform.SetParent(_poolParents[tag]);
        _poolDictionary[tag].Enqueue(obj);
        
        _poolStats[tag].totalDespawns++;
    }

    public void DespawnAll(string tag)
    {
        if (!_poolParents.ContainsKey(tag)) return;

        Transform parent = _poolParents[tag];
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject obj = parent.GetChild(i).gameObject;
            if (obj.activeInHierarchy)
                Despawn(obj, tag);
        }
    }

    public void DespawnAll()
    {
        foreach (string tag in _poolDictionary.Keys)
            DespawnAll(tag);
    }

    /// <summary>
    /// ⭐ 향상된 통계
    /// </summary>
    public string GetPoolStats(string tag)
    {
        if (!_poolDictionary.ContainsKey(tag))
            return $"'{tag}' 풀 없음";

        Queue<GameObject> pool = _poolDictionary[tag];
        int totalSize = _poolSizes[tag];
        int availableCount = pool.Count;
        int activeCount = totalSize - availableCount;
        PoolStats stats = _poolStats[tag];

        return $"[{tag}] Total:{totalSize} Active:{activeCount} Available:{availableCount}\n" +
               $"  Spawns:{stats.totalSpawns} Despawns:{stats.totalDespawns} " +
               $"Expansions:{stats.expansions} Failed:{stats.failedSpawns}";
    }

    public void LogAllPoolStats()
    {
        Debug.Log("=== Object Pool Statistics ===");
        foreach (string tag in _poolDictionary.Keys)
            Debug.Log(GetPoolStats(tag));
    }

    /// <summary>
    /// 런타임 풀 생성
    /// </summary>
    public void CreatePool(string tag, GameObject prefab, int size, int maxSize = 50)
    {
        if (_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] '{tag}' 풀 이미 존재");
            return;
        }

        Pool newPool = new Pool
        {
            tag = tag,
            prefab = prefab,
            initialSize = size,
            maxSize = maxSize,
            expandable = true,
            prewarm = false
        };

        pools.Add(newPool);
        
        Queue<GameObject> objectPool = new Queue<GameObject>();
        GameObject poolParent = new GameObject($"Pool_{tag}");
        poolParent.transform.SetParent(transform);
        
        _poolDictionary[tag] = objectPool;
        _poolSettings[tag] = newPool;
        _poolParents[tag] = poolParent.transform;
        _poolSizes[tag] = 0;
        _poolStats[tag] = new PoolStats();
    }
}

public class PooledObject : MonoBehaviour
{
    private ParticleSystem[] _particleSystems;
    private TrailRenderer[] _trailRenderers;
    private Rigidbody2D _rigidbody;
    private bool _initialized = false;

    void Initialize()
    {
        if (_initialized) return;
        
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _trailRenderers = GetComponentsInChildren<TrailRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _initialized = true;
    }

    public void OnSpawn()
    {
        if (!_initialized) Initialize();

        foreach (ParticleSystem ps in _particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        foreach (TrailRenderer trail in _trailRenderers)
            trail.Clear();

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }

    public void OnDespawn()
    {
        if (!_initialized) return;

        foreach (ParticleSystem ps in _particleSystems)
            ps.Stop();

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }
}

[System.Serializable]
public class PoolStats
{
    public int totalSpawns;
    public int totalDespawns;
    public int expansions;
    public int failedSpawns;
}