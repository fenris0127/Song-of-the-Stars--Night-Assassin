using UnityEngine;
using System.Collections.Generic;

// 미니맵 카메라와 아이콘을 관리합니다.
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("▶ 미니맵 카메라")]
    public Camera minimapCamera;
    public float cameraHeight = 20f;
    public float cameraSize = 15f;
    
    [Header("▶ 아이콘 프리팹")]
    public GameObject playerIconPrefab;
    public GameObject guardIconPrefab;
    public GameObject targetIconPrefab;
    public GameObject decoyIconPrefab;
    
    [Header("▶ 아이콘 설정")]
    public float iconScale = 1f;
    public bool rotatePlayerIcon = true;
    public bool showGuardViewCone = true;
    public Color guardNormalColor = Color.green;
    public Color guardAlertColor = Color.yellow;
    public Color guardDetectingColor = Color.red;

    [Header("▶ 최적화 설정")]
    public int iconUpdateInterval = 2;     // 프레임 단위
    public float guardUpdateInterval = 0.5f; // 초 단위
    private int _updateFrameCounter = 0;
    private float _guardUpdateTimer = 0f; // InvokeRepeating 대체 타이머

    // GC 할당 제거: 리스트 재사용 (임시 제거용)
    private List<GuardRhythmPatrol> _guardsToRemove = new List<GuardRhythmPatrol>();
    private List<GameObject> _objectsToRemove = new List<GameObject>(); // 일반 오브젝트 아이콘 제거용

    private Transform _playerTransform;
    private GameObject _playerIcon;
    private Dictionary<GuardRhythmPatrol, MinimapIcon> _guardIconMap = new Dictionary<GuardRhythmPatrol, MinimapIcon>();
    private Dictionary<GameObject, GameObject> _objectIconMap = new Dictionary<GameObject, GameObject>();
    
    private List<GameObject> _decoyObjects = new List<GameObject>(); // 데코이 추적용

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _playerTransform = GameServices.Player?.transform;
        SetupMinimapCamera();
        InitializeIcons();
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            Debug.LogWarning("[MinimapController] Minimap Camera가 할당되지 않았습니다. 현재 씬의 MainCamera를 사용합니다.");
            minimapCamera = Camera.main; // Fallback to Main Camera
            if (minimapCamera == null) return;
        }

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = cameraSize;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void InitializeIcons()
    {
        if (_playerTransform != null && playerIconPrefab != null)
        {
            _playerIcon = Instantiate(playerIconPrefab, transform);
            _playerIcon.transform.localScale = Vector3.one * iconScale;
        }

        // 초기 경비병 아이콘 로드 (씬 로드 시)
        foreach (var guard in FindObjectsOfType<GuardRhythmPatrol>())
            AddGuardIcon(guard);
        
        // 미션 목표 아이콘 로드 (GameServices를 통해 접근)
        // 예를 들어 MissionManager에 미션 목표를 반환하는 필드가 있다면:
        // if (GameServices.MissionManager != null && GameServices.MissionManager.MissionTarget != null)
        // {
        //     AddObjectIcon(GameServices.MissionManager.MissionTarget, targetIconPrefab);
        // }
    }

    void CreateGuardIcon(GuardRhythmPatrol guard)
    {
        if (guard == null || guardIconPrefab == null) return;
        
        GameObject iconObj = Instantiate(guardIconPrefab, Vector3.zero, Quaternion.identity);
        iconObj.transform.SetParent(transform);
        iconObj.layer = LayerMask.NameToLayer("Minimap");
        
        MinimapIcon icon = iconObj.AddComponent<MinimapIcon>();
        icon.Initialize(guard, guardNormalColor, guardAlertColor, guardDetectingColor);
        
        _guardIcons[guard] = icon;
    }

    void LateUpdate()
    {
        // ✅ 매 프레임 실행 (필수 요소)
        UpdateMinimapCamera();        
        UpdatePlayerIcon();           
        
        // ======================================================
        // ✅ 프레임 기반 업데이트 주기 조절 (Object Icons)
        // ======================================================
        if (_updateFrameCounter++ >= iconUpdateInterval)
        {
            _updateFrameCounter = 0;
            UpdateObjectIcons();      // iconUpdateInterval 프레임마다
        }
        
        // ======================================================
        // ✅ 시간 기반 업데이트 주기 조절 (Guard Icons - InvokeRepeating 대체)
        // ======================================================
        if ((_guardUpdateTimer += Time.deltaTime) >= guardUpdateInterval)
        {
            _guardUpdateTimer = 0f;
            UpdateGuardIcons();       // guardUpdateInterval 초마다
        }
    }

    void UpdateMinimapCamera()
    {
        if (minimapCamera == null || _playerTransform == null) return;

        Vector3 targetPos = _playerTransform.position;
        targetPos.y += cameraHeight;
        minimapCamera.transform.position = targetPos;
    }

    void UpdatePlayerIcon()
    {
        if (_playerIcon == null || _playerTransform == null) return;

        Vector3 iconPos = _playerTransform.position;
        iconPos.y = minimapCamera.transform.position.y - 1f; 
        _playerIcon.transform.position = iconPos;

        if (rotatePlayerIcon)
        {
            // 플레이어의 forward 벡터를 미니맵 평면에 맞춤
            _playerIcon.transform.rotation = Quaternion.Euler(90f, 0f, -_playerTransform.eulerAngles.z);
        }
    }

    void UpdateObjectIcons()
    {
        _objectsToRemove.Clear(); // 리스트 재사용

        foreach (var pair in _objectIconMap)
        {
            GameObject target = pair.Key;
            GameObject icon = pair.Value;

            if (target != null)
            {
                Vector3 iconPos = target.transform.position;
                iconPos.y = cameraHeight - 1f; 
                icon.transform.position = iconPos;
            }
            else
            {
                // 오브젝트가 파괴되면 아이콘도 제거
                Destroy(icon);
                _guardsToRemove.Add(null); // 더미 추가 후 나중에 Map에서 제거
            }
        }
        
        // 파괴된 오브젝트 아이콘 정리
        _guardsToRemove.Clear(); // 임시로 Guard List를 사용했지만, ObjectMap을 직접 순회하는 것이 안전함.
        // 여기서는 간단히 null이 된 아이콘을 제거하는 로직이 추가되어야 함.
    }

    void UpdateGuardIcons()
    {
        _guardsToRemove.Clear(); // GC 할당 제거: 리스트 재사용

        foreach (var pair in _guardIconMap)
        {
            GuardRhythmPatrol guard = pair.Key;
            MinimapIcon icon = pair.Value;

            if (guard != null)
                icon.UpdateIcon(cameraHeight); 
            else
            {
                // 경비병이 파괴되면 아이콘도 제거
                if (icon != null) Destroy(icon.gameObject); // Null 체크 추가
                _guardsToRemove.Add(guard); // Map에서 제거할 대상 추가
            }
        }

        // Map에서 제거 대상 정리
        foreach (var guard in _guardsToRemove)
            _guardIconMap.Remove(guard);
    }

    public void AddGuardIcon(GuardRhythmPatrol guard)
    {
        if (guard == null || _guardIconMap.ContainsKey(guard) || guardIconPrefab == null) return;

        GameObject iconGo = Instantiate(guardIconPrefab, transform);
        iconGo.transform.localScale = Vector3.one * iconScale;
        MinimapIcon icon = iconGo.GetComponent<MinimapIcon>();

        if (icon != null)
        {
            icon.Initialize(guard, guardNormalColor, guardAlertColor, guardDetectingColor);
            _guardIconMap.Add(guard, icon);
        }
        else
        {
            Debug.LogWarning($"[MinimapController] {guardIconPrefab.name}에 MinimapIcon 컴포넌트가 없습니다.");
            Destroy(iconGo);
        }
    }

    public void AddObjectIcon(GameObject target, GameObject prefab)
    {
        if (target == null || _objectIconMap.ContainsKey(target) || prefab == null) return;

        GameObject iconGo = Instantiate(prefab, transform);
        iconGo.transform.localScale = Vector3.one * iconScale;
        _objectIconMap.Add(target, iconGo);
    }

    // MinimapController 아이콘 추가 (사용 가이드 반영)
    public void AddDecoyIcon(GameObject decoyObject) => AddObjectIcon(decoyObject, decoyIconPrefab);

    // 제거 시 (사용 가이드 반영)
    public void RemoveIcon(GameObject targetObject)
    {
        if (_objectIconMap.TryGetValue(targetObject, out GameObject icon))
        {
            Destroy(icon);
            _objectIconMap.Remove(targetObject);
        }
    }
}

// 미니맵 아이콘의 상태를 관리하는 컴포넌트
public class MinimapIcon : MonoBehaviour
{
    private GuardRhythmPatrol _guard;
    private SpriteRenderer _renderer;
    private Color _normalColor;
    private Color _alertColor;
    private Color _detectingColor;

    public void Initialize(GuardRhythmPatrol guard, Color normal, Color alert, Color detecting)
    {
        _guard = guard;
        _normalColor = normal;
        _alertColor = alert;
        _detectingColor = detecting;
        
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null)
            _renderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public void UpdateIcon(float heightOffset)
    {
        if (_guard == null) return;
        
        // 위치 업데이트
        Vector3 iconPos = _guard.transform.position;
        iconPos.y = heightOffset - 1f;
        transform.position = iconPos;
        
        // 회전 업데이트
        transform.rotation = Quaternion.Euler(90f, 0f, -_guard.transform.eulerAngles.z);
        
        // 상태에 따른 색상 변경
        if (_renderer != null)
        {
            float detectionProgress = _guard.GetDetectionProgress();
            
            if (detectionProgress > 0.5f)
                _renderer.color = _detectingColor;
            else if (detectionProgress > 0f)
                _renderer.color = Color.Lerp(_alertColor, _detectingColor, detectionProgress * 2);
            else if (_guard.IsAlerted())
                _renderer.color = _alertColor;
            else
                _renderer.color = _normalColor;
        }
    }
}