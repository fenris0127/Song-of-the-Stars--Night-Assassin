using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임의 성능을 실시간으로 모니터링하고 표시합니다.
/// FPS, 메모리 사용량, GC 호출, 오브젝트 풀 상태 등을 추적합니다.
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    public static PerformanceMonitor Instance { get; private set; }

    [Header("▶ 설정")]
    public bool showMonitor = true;
    public KeyCode toggleKey = KeyCode.F3;
    public bool detailedMode = false;
    
    [Header("▶ UI 설정")]
    public int fontSize = 14;
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public Color textColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    [Header("▶ 성능 임계값")]
    public float targetFPS = 60f;
    public float warningFPS = 45f;
    public float criticalFPS = 30f;
    public float warningMemoryMB = 500f;
    public float criticalMemoryMB = 800f;

    // FPS 추적
    private float _deltaTime = 0f;
    private float _fps = 0f;
    private float _minFPS = float.MaxValue;
    private float _maxFPS = 0f;
    private Queue<float> _fpsHistory = new Queue<float>();
    private const int FPS_HISTORY_SIZE = 60;

    // 메모리 추적
    private float _lastMemoryCheck = 0f;
    private const float MEMORY_CHECK_INTERVAL = 1f;
    private long _totalMemoryMB = 0;
    private long _allocatedMemoryMB = 0;
    private int _gcCount = 0;
    private int _lastGCCount = 0;

    // 프레임 타이밍
    private float _updateTime = 0f;
    private float _fixedUpdateTime = 0f;
    private float _lateUpdateTime = 0f;

    // 렌더링 통계
    private int _drawCalls = 0;
    private int _triangles = 0;
    private int _vertices = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showMonitor = !showMonitor;

        if (Input.GetKeyDown(KeyCode.F4))
            detailedMode = !detailedMode;

        UpdateFPS();
        UpdateMemoryStats();
        
        float startTime = Time.realtimeSinceStartup;
        _updateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
    }

    void FixedUpdate()
    {
        float startTime = Time.realtimeSinceStartup;
        _fixedUpdateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
    }

    void LateUpdate()
    {
        float startTime = Time.realtimeSinceStartup;
        _lateUpdateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
    }

    void UpdateFPS()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _fps = 1.0f / _deltaTime;

        // FPS 히스토리 업데이트
        _fpsHistory.Enqueue(_fps);
        if (_fpsHistory.Count > FPS_HISTORY_SIZE)
            _fpsHistory.Dequeue();

        // Min/Max FPS 업데이트
        if (_fps < _minFPS && _fps > 0) _minFPS = _fps;
        if (_fps > _maxFPS) _maxFPS = _fps;
    }

    void UpdateMemoryStats()
    {
        if (Time.time - _lastMemoryCheck >= MEMORY_CHECK_INTERVAL)
        {
            _lastMemoryCheck = Time.time;

            _totalMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
            _allocatedMemoryMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);

            _gcCount = System.GC.CollectionCount(0);
            
            // 렌더링 통계 (Unity 2020.2+)
            #if UNITY_2020_2_OR_NEWER
            UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
            #endif
        }
    }

    #if UNITY_2020_2_OR_NEWER
    void OnEndFrameRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera[] cameras)
    {
        // 렌더링 통계는 프로파일러 API를 통해 얻을 수 있습니다
        UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;
    }
    #endif

    void OnGUI()
    {
        if (!showMonitor) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        style.padding = new RectOffset(10, 10, 10, 10);

        int width = detailedMode ? 400 : 300;
        int height = detailedMode ? 500 : 200;

        Rect bgRect = new Rect(10, 10, width, height);
        DrawColoredRect(bgRect, backgroundColor);

        GUILayout.BeginArea(new Rect(20, 20, width - 20, height - 20));
        
        DrawBasicStats(style);

        if (detailedMode)
        {
            GUILayout.Space(10);
            DrawDetailedStats(style);
        }

        GUILayout.EndArea();
    }

    void DrawBasicStats(GUIStyle style)
    {
        // FPS
        Color fpsColor = GetFPSColor(_fps);
        style.normal.textColor = fpsColor;
        GUILayout.Label($"FPS: {_fps:F1}", style);
        
        style.normal.textColor = textColor;
        GUILayout.Label($"Min: {_minFPS:F1} | Max: {_maxFPS:F1}", style);

        // 메모리
        Color memoryColor = GetMemoryColor(_allocatedMemoryMB);
        style.normal.textColor = memoryColor;
        GUILayout.Label($"Memory: {_allocatedMemoryMB} MB", style);
        
        // GC
        style.normal.textColor = textColor;
        int gcDelta = _gcCount - _lastGCCount;
        if (gcDelta > 0)
        {
            style.normal.textColor = warningColor;
            GUILayout.Label($"GC: {_gcCount} (+{gcDelta} this second!)", style);
            _lastGCCount = _gcCount;
        }
        else
        {
            style.normal.textColor = textColor;
            GUILayout.Label($"GC: {_gcCount}", style);
        }

        // 단축키 안내
        style.normal.textColor = Color.gray;
        style.fontSize = fontSize - 2;
        GUILayout.Label($"[{toggleKey}] Toggle | [F4] Detailed", style);
        style.fontSize = fontSize;
    }

    void DrawDetailedStats(GUIStyle style)
    {
        style.normal.textColor = Color.cyan;
        GUILayout.Label("=== Detailed Stats ===", style);
        
        style.normal.textColor = textColor;
        
        // 프레임 타이밍
        GUILayout.Label($"Update: {_updateTime:F2}ms", style);
        GUILayout.Label($"FixedUpdate: {_fixedUpdateTime:F2}ms", style);
        GUILayout.Label($"LateUpdate: {_lateUpdateTime:F2}ms", style);
        
        GUILayout.Space(5);
        
        // 시스템 정보
        GUILayout.Label($"Time Scale: {Time.timeScale:F2}", style);
        GUILayout.Label($"Unity: {Application.unityVersion}", style);
        
        // 오브젝트 풀 통계
        if (ObjectPoolManager.Instance != null)
        {
            GUILayout.Space(5);
            style.normal.textColor = Color.green;
            GUILayout.Label("=== Object Pools ===", style);
            style.normal.textColor = textColor;
            
            // 여기에 풀 통계를 표시할 수 있습니다
            // ObjectPoolManager에서 GetAllPoolStats() 같은 메서드가 필요합니다
        }

        // 게임 상태
        if (GameServices.Instance != null)
        {
            GUILayout.Space(5);
            style.normal.textColor = Color.yellow;
            GUILayout.Label("=== Game State ===", style);
            style.normal.textColor = textColor;
            
            if (GameServices.RhythmManager != null)
                GUILayout.Label($"Beat: {GameServices.RhythmManager.currentBeatCount}", style);
            
            if (GameServices.Player != null)
                GUILayout.Label($"Player Pos: {GameServices.Player.transform.position}", style);
        }
    }

    Color GetFPSColor(float fps)
    {
        if (fps < criticalFPS) return criticalColor;
        if (fps < warningFPS) return warningColor;
        return Color.green;
    }

    Color GetMemoryColor(long memoryMB)
    {
        if (memoryMB > criticalMemoryMB) return criticalColor;
        if (memoryMB > warningMemoryMB) return warningColor;
        return textColor;
    }

    void DrawColoredRect(Rect rect, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.DrawTexture(rect, texture);
        Destroy(texture);
    }

    /// <summary>
    /// FPS 통계를 리셋합니다.
    /// </summary>
    public void ResetStats()
    {
        _minFPS = float.MaxValue;
        _maxFPS = 0f;
        _fpsHistory.Clear();
    }
}