using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 미니맵 카메라와 아이콘을 관리합니다.
/// </summary>
public class MinimapController : MonoBehaviour
{
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
    
    private Transform _playerTransform;
    private GameObject _playerIcon;
    private Dictionary<GuardRhythmPatrol, MinimapIcon> _guardIcons = new Dictionary<GuardRhythmPatrol, MinimapIcon>();
    private Dictionary<GameObject, GameObject> _objectIcons = new Dictionary<GameObject, GameObject>();

    void Start()
    {
        SetupMinimapCamera();
        InitializeIcons();
        InvokeRepeating(nameof(UpdateGuardIcons), 0f, 0.5f);
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            GameObject camObj = new GameObject("MinimapCamera");
            minimapCamera = camObj.AddComponent<Camera>();
        }
        
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = cameraSize;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
        minimapCamera.depth = 10;
        
        // Render Texture 설정 (선택사항)
        RenderTexture rt = new RenderTexture(256, 256, 16);
        minimapCamera.targetTexture = rt;
    }

    void InitializeIcons()
    {
        // 플레이어 아이콘
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            _playerTransform = player.transform;
            if (playerIconPrefab != null)
            {
                _playerIcon = Instantiate(playerIconPrefab, Vector3.zero, Quaternion.identity);
                _playerIcon.transform.SetParent(transform);
                _playerIcon.layer = LayerMask.NameToLayer("Minimap");
            }
        }
        
        // 경비병 아이콘
        GuardRhythmPatrol[] guards = FindObjectsOfType<GuardRhythmPatrol>();
        foreach (GuardRhythmPatrol guard in guards)
            CreateGuardIcon(guard);
        
        // 미션 목표 아이콘
        MissionTarget target = FindObjectOfType<MissionTarget>();
        if (target != null && targetIconPrefab != null)
        {
            GameObject targetIcon = Instantiate(targetIconPrefab, Vector3.zero, Quaternion.identity);
            targetIcon.transform.SetParent(transform);
            targetIcon.layer = LayerMask.NameToLayer("Minimap");
            _objectIcons[target.gameObject] = targetIcon;
        }
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
        UpdateMinimapCamera();
        UpdatePlayerIcon();
        UpdateObjectIcons();
    }

    void UpdateMinimapCamera()
    {
        if (_playerTransform != null && minimapCamera != null)
        {
            Vector3 newPos = _playerTransform.position;
            newPos.y = cameraHeight;
            minimapCamera.transform.position = newPos;
        }
    }

    void UpdatePlayerIcon()
    {
        if (_playerIcon != null && _playerTransform != null)
        {
            Vector3 iconPos = _playerTransform.position;
            iconPos.y = cameraHeight - 1f;
            _playerIcon.transform.position = iconPos;
            
            if (rotatePlayerIcon)
                _playerIcon.transform.rotation = Quaternion.Euler(90f, 0f, -_playerTransform.eulerAngles.z);
        }
    }

    void UpdateObjectIcons()
    {
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (var pair in _objectIcons)
        {
            if (pair.Key == null)
            {
                toRemove.Add(pair.Key);

                if (pair.Value != null)
                    Destroy(pair.Value);

                continue;
            }
            
            Vector3 iconPos = pair.Key.transform.position;
            iconPos.y = cameraHeight - 1f;
            pair.Value.transform.position = iconPos;
        }
        
        foreach (var key in toRemove)
            _objectIcons.Remove(key);
    }

    void UpdateGuardIcons()
    {
        List<GuardRhythmPatrol> toRemove = new List<GuardRhythmPatrol>();
        
        foreach (var pair in _guardIcons)
        {
            if (pair.Key == null)
            {
                toRemove.Add(pair.Key);

                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);

                continue;
            }
            
            pair.Value.UpdateIcon(cameraHeight);
        }
        
        foreach (var key in toRemove)
            _guardIcons.Remove(key);
    }

    /// <summary>
    /// 데코이 아이콘 추가
    /// </summary>
    public void AddDecoyIcon(GameObject decoy)
    {
        if (decoy == null || decoyIconPrefab == null) return;
        
        GameObject iconObj = Instantiate(decoyIconPrefab, Vector3.zero, Quaternion.identity);
        iconObj.transform.SetParent(transform);
        iconObj.layer = LayerMask.NameToLayer("Minimap");
        
        _objectIcons[decoy] = iconObj;
    }
}

/// <summary>
/// 미니맵 아이콘의 상태를 관리하는 컴포넌트
/// </summary>
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
                _renderer.color = _alertColor;
            else
                _renderer.color = _normalColor;
        }
    }
}