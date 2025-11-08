using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 최적화된 씬 로딩 시스템
/// - 비동기 씬 로딩
/// - 로딩 화면 표시
/// - 메모리 정리
/// - 오브젝트 풀 관리
/// </summary>
public class OptimizedSceneLoader : MonoBehaviour
{
    public static OptimizedSceneLoader Instance { get; private set; }

    [Header("▶ 로딩 UI")]
    public GameObject loadingScreen;
    public UnityEngine.UI.Slider progressBar;
    public TMPro.TextMeshProUGUI loadingText;
    public TMPro.TextMeshProUGUI tipText;

    [Header("▶ 로딩 팁")]
    public string[] loadingTips = new string[]
    {
        "리듬에 맞춰 움직이면 경비병을 피할 수 있습니다.",
        "Perfect 판정으로 스킬을 발동하면 쿨타임이 절반이 됩니다.",
        "스텔스 상태에서는 발견되기 어렵지만 이동 속도가 느려집니다.",
        "Focus를 50 소모하면 자유롭게 이동할 수 있습니다.",
        "경비병의 시야각과 거리를 주의하세요."
    };

    [Header("▶ 설정")]
    public float minimumLoadTime = 2f; // 최소 로딩 시간 (로딩 화면 표시 보장)
    public bool unloadUnusedAssets = true;
    public bool garbageCollect = true;

    private bool _isLoading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 씬을 비동기로 로드합니다.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning("이미 씬 로딩 중입니다!");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// 씬 인덱스로 로드합니다.
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        _isLoading = true;
        float startTime = Time.realtimeSinceStartup;

        // 1. 로딩 화면 표시
        ShowLoadingScreen();
        yield return null;

        // 2. 현재 씬 정리
        PrepareSceneTransition();
        yield return null;

        // 3. 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 4. 로딩 진행
        while (!asyncLoad.isDone)
        {
            // 0.9까지만 자동 진행됨
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateLoadingUI(progress);

            // 로딩이 거의 완료되었고 최소 시간이 지났으면 씬 활성화
            if (asyncLoad.progress >= 0.9f)
            {
                float elapsed = Time.realtimeSinceStartup - startTime;
                if (elapsed >= minimumLoadTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        // 5. 새 씬 초기화
        yield return StartCoroutine(InitializeNewScene());

        // 6. 로딩 화면 숨기기
        HideLoadingScreen();

        _isLoading = false;
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // 인덱스 유효성 검사
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"잘못된 씬 인덱스: {sceneIndex}");
            _isLoading = false;
            yield break;
        }

        yield return LoadSceneAsync(SceneManager.GetSceneByBuildIndex(sceneIndex).name);
    }

    #region 씬 전환 준비 및 정리
    void PrepareSceneTransition()
    {
        // 입력 비활성화
        if (InputManager.Instance != null)
            InputManager.Instance.DisableInput();

        // 활성 VFX 정리
        if (VFXManager_Pooled.Instance != null)
            VFXManager_Pooled.Instance.StopAllVFX();

        // 오브젝트 풀 정리
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.DespawnAll();

        // Time scale 정상화
        Time.timeScale = 1f;

        Debug.Log("[SceneLoader] 씬 전환 준비 완료");
    }

    IEnumerator InitializeNewScene()
    {
        // GameServices 참조 새로고침
        if (GameServices.Instance != null)
            GameServices.RefreshReferences();

        yield return null;

        // 난이도 설정 적용
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.ApplyDifficultySettings();

        yield return null;

        // 메모리 정리
        if (unloadUnusedAssets)
        {
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }

        if (garbageCollect)
        {
            System.GC.Collect();
            yield return null;
        }

        // 입력 재활성화
        if (InputManager.Instance != null)
            InputManager.Instance.EnableInput();

        Debug.Log("[SceneLoader] 새 씬 초기화 완료");
    }
    #endregion

    #region UI 관리
    void ShowLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            
            // 랜덤 팁 표시
            if (tipText != null && loadingTips.Length > 0)
            {
                string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
                tipText.text = randomTip;
            }
        }
    }

    void HideLoadingScreen()
    {
        if (loadingScreen != null)
        {
            // 페이드 아웃 애니메이션이 있다면 여기서 실행
            loadingScreen.SetActive(false);
        }
    }

    void UpdateLoadingUI(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (loadingText != null)
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }
    #endregion

    #region 편의 함수
    /// <summary>
    /// 현재 씬을 재시작합니다.
    /// </summary>
    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LoadScene(currentScene.name);
    }

    /// <summary>
    /// 메인 메뉴로 돌아갑니다.
    /// </summary>
    public void LoadMainMenu() => LoadScene("MainMenu");

    /// <summary>
    /// 다음 미션을 로드합니다.
    /// </summary>
    public void LoadNextMission()
    {
        if (SaveSystem.Instance != null)
        {
            int nextMission = SaveSystem.Instance.currentSave.currentMissionIndex;
            LoadScene($"Mission_{nextMission}");
        }
    }
    #endregion
}

/// <summary>
/// 씬 로더 사용 예제
/// </summary>
public class SceneLoaderExample : MonoBehaviour
{
    void Start()
    {
        // 버튼 클릭 시
        OptimizedSceneLoader.Instance.LoadScene("GameScene");
        
        // 미션 완료 시
        OptimizedSceneLoader.Instance.LoadNextMission();
        
        // 재시작 시
        OptimizedSceneLoader.Instance.RestartCurrentScene();
    }
}