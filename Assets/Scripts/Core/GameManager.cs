using UnityEngine;
using UnityEngine.SceneManagement;

 public enum GameState
{
    MainMenu,
    InGame,
    Paused,
    MissionComplete,
    GameOver
}
/// <summary>
/// 게임 전체 상태를 관리하는 중앙 컨트롤러입니다.
/// 모든 싱글톤 매니저들을 초기화하고 조율합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("▶ 게임 상태")]
    public GameState currentState = GameState.MainMenu;
    public bool isPaused = false;

    [Header("▶ 매니저 프리팹들")]
    public GameObject sfxManagerPrefab;
    public GameObject difficultyManagerPrefab;
    public GameObject saveSystemPrefab;

    [Header("▶ 디버그")]
    public bool showDebugInfo = false;
    public KeyCode pauseKey = KeyCode.Escape;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        HandlePauseInput();

        if (showDebugInfo)
            ShowDebugGUI();
    }

    /// <summary>
    /// 모든 필수 싱글톤 매니저를 초기화합니다.
    /// </summary>
    void InitializeManagers()
    {
        // SFXManager 초기화
        if (SFXManager.Instance == null && sfxManagerPrefab != null)
            Instantiate(sfxManagerPrefab);

        // DifficultyManager 초기화
        if (DifficultyManager.Instance == null && difficultyManagerPrefab != null)
            Instantiate(difficultyManagerPrefab);

        // SaveSystem 초기화
        if (SaveSystem.Instance == null && saveSystemPrefab != null)
            Instantiate(saveSystemPrefab);

        // 저장된 설정 로드
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.LoadGame();
    }

    /// <summary>
    /// 씬이 로드될 때 호출됩니다.
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드 완료: {scene.name}");

        // 메인 메뉴 씬인지 확인
        if (scene.name == "MainMenu")
        {
            currentState = GameState.MainMenu;
            Time.timeScale = 1f;
        }
        else
        {
            currentState = GameState.InGame;

            // 난이도 설정 적용
            if (DifficultyManager.Instance != null)
                DifficultyManager.Instance.ApplyDifficultySettings();
        }
    }

    /// <summary>
    /// 일시정지 처리
    /// </summary>
    void HandlePauseInput()
    {
        if (currentState != GameState.InGame) return;

        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            currentState = GameState.Paused;
            Debug.Log("게임 일시정지");
        }
        else
        {
            Time.timeScale = 1f;
            currentState = GameState.InGame;
            Debug.Log("게임 재개");
        }

        // 일시정지 UI 표시 (UIManager에 구현 필요)
        // UIManager.Instance?.ShowPauseMenu(isPaused);
    }

    /// <summary>
    /// 미션 완료 처리
    /// </summary>
    public void OnMissionComplete(bool success, int score)
    {
        currentState = success ? GameState.MissionComplete : GameState.GameOver;

        if (success && SaveSystem.Instance != null)
        {
            int currentMission = SaveSystem.Instance.currentSave.currentMissionIndex;
            SaveSystem.Instance.CompleteMission(currentMission, score);
            SaveSystem.Instance.currentSave.currentMissionIndex++;
            SaveSystem.Instance.SaveGame();
        }

        Debug.Log($"미션 {(success ? "성공" : "실패")}! 점수: {score}");
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartMission()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// 디버그 정보 표시
    /// </summary>
    void ShowDebugGUI()
    {
        // OnGUI에서 처리하거나 TextMeshPro로 구현 가능
    }

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnApplicationQuit()
    {
        // 게임 종료 시 자동 저장
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            Debug.Log("게임 종료 - 자동 저장 완료");
        }
    }
}