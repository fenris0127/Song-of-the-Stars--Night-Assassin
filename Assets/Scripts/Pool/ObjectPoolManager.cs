using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 오브젝트의 생성/파괴를 최적화하는 오브젝트 풀링 시스템
/// VFX, 투사체, 데코이 등의 빈번한 생성/파괴로 인한 GC 부하를 줄입니다.
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
    }

    [Header("▶ 풀 설정")]
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    private Dictionary<string, Pool> _poolSettings;
    private Dictionary<string, Transform> _poolParents;
    private Dictionary<string, int> _poolSizes;

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

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"ObjectPoolManager: '{pool.tag}' 풀의 프리팹이 null입니다!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            // 풀 부모 오브젝트 생성 (Hierarchy 정리용)
            GameObject poolParent = new GameObject($"Pool_{pool.tag}");
            poolParent.transform.SetParent(transform);
            _poolParents[pool.tag] = poolParent.transform;

            // 초기 오브젝트 생성
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewObject(pool.prefab, poolParent.transform);
                objectPool.Enqueue(obj);
            }

            _poolDictionary.Add(pool.tag, objectPool);
            _poolSettings.Add(pool.tag, pool);
            _poolSizes[pool.tag] = pool.initialSize;
        }

        Debug.Log($"ObjectPoolManager: {pools.Count}개의 풀 초기화 완료");
    }

    GameObject CreateNewObject(GameObject prefab, Transform parent)
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        
        // 풀링된 오브젝트임을 표시
        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        if (pooledComponent == null)
            pooledComponent = obj.AddComponent<PooledObject>();
        
        return obj;
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다.
    /// </summary>
    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"ObjectPoolManager: '{tag}' 태그를 가진 풀이 존재하지 않습니다!");
            return null;
        }

        GameObject obj = null;
        Queue<GameObject> pool = _poolDictionary[tag];
        Pool settings = _poolSettings[tag];

        // 풀에서 사용 가능한 오브젝트 찾기
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            
            // 이미 파괴된 오브젝트인 경우 새로 생성
            if (obj == null)
            {
                obj = CreateNewObject(settings.prefab, _poolParents[tag]);
                _poolSizes[tag]++;
            }
        }
        else if (settings.expandable && _poolSizes[tag] < settings.maxSize)
        {
            // 풀 확장
            obj = CreateNewObject(settings.prefab, _poolParents[tag]);
            _poolSizes[tag]++;
        }
        else
        {
            Debug.LogWarning($"ObjectPoolManager: '{tag}' 풀이 가득 찼습니다! (Max: {settings.maxSize})");
            return null;
        }

        // 오브젝트 활성화 및 설정
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // PooledObject 컴포넌트에 정보 전달
        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        if (pooledComponent != null)
            pooledComponent.OnSpawn();

        return obj;
    }

    /// <summary>
    /// 오버로드: Transform 없이 Spawn
    /// </summary>
    public GameObject Spawn(string tag)
    {
        return Spawn(tag, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void Despawn(GameObject obj, string tag)
    {
        if (obj == null) return;

        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"ObjectPoolManager: '{tag}' 태그를 가진 풀이 존재하지 않습니다!");
            Destroy(obj);
            return;
        }

        // PooledObject 콜백 호출
        PooledObject pooledComponent = obj.GetComponent<PooledObject>();
        if (pooledComponent != null)
            pooledComponent.OnDespawn();

        obj.SetActive(false);
        obj.transform.SetParent(_poolParents[tag]);
        _poolDictionary[tag].Enqueue(obj);
    }

    /// <summary>
    /// 특정 풀의 모든 오브젝트를 비활성화합니다.
    /// </summary>
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

    /// <summary>
    /// 모든 풀의 오브젝트를 비활성화합니다.
    /// </summary>
    public void DespawnAll()
    {
        foreach (string tag in _poolDictionary.Keys)
            DespawnAll(tag);
    }

    /// <summary>
    /// 특정 풀의 통계 정보를 반환합니다.
    /// </summary>
    public string GetPoolStats(string tag)
    {
        if (!_poolDictionary.ContainsKey(tag))
            return $"'{tag}' 풀이 존재하지 않습니다.";

        Queue<GameObject> pool = _poolDictionary[tag];
        int totalSize = _poolSizes[tag];
        int availableCount = pool.Count;
        int activeCount = totalSize - availableCount;

        return $"[{tag}] Total: {totalSize}, Active: {activeCount}, Available: {availableCount}";
    }

    /// <summary>
    /// 모든 풀의 통계를 로그로 출력합니다.
    /// </summary>
    public void LogAllPoolStats()
    {
        Debug.Log("=== Object Pool Statistics ===");
        foreach (string tag in _poolDictionary.Keys)
            Debug.Log(GetPoolStats(tag));
    }
}

/// <summary>
/// 풀링된 오브젝트에 부착되는 컴포넌트
/// Spawn/Despawn 시 자동으로 초기화/정리 작업을 수행합니다.
/// </summary>
public class PooledObject : MonoBehaviour
{
    private ParticleSystem[] _particleSystems;
    private TrailRenderer[] _trailRenderers;
    private Rigidbody2D _rigidbody;

    void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _trailRenderers = GetComponentsInChildren<TrailRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 풀에서 꺼내질 때 호출됩니다.
    /// </summary>
    public void OnSpawn()
    {
        // 파티클 시스템 재시작
        foreach (ParticleSystem ps in _particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        // 트레일 초기화
        foreach (TrailRenderer trail in _trailRenderers)
            trail.Clear();

        // Rigidbody 초기화
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// 풀로 반환될 때 호출됩니다.
    /// </summary>
    public void OnDespawn()
    {
        // 파티클 시스템 정지
        foreach (ParticleSystem ps in _particleSystems)
            ps.Stop();

        // Rigidbody 정지
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
    }
}