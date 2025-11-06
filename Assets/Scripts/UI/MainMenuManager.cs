using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 메인 메뉴 UI와 기능을 관리합니다.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("▶ 메뉴 패널들")]
    public GameObject mainMenuPanel;
    public GameObject missionSelectPanel;
    public GameObject skillLoadoutPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    
    [Header("▶ 버튼들")]
    public Button newGameButton;
    public Button continueButton;
    public Button missionSelectButton;
    public Button skillLoadoutButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("▶ 설정 UI")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiVolumeSlider;
    public TMP_Dropdown difficultyDropdown;
    
    [Header("▶ 미션 선택 UI")]
    public Transform missionButtonContainer;
    public GameObject missionButtonPrefab;
    
    [Header("▶ 스킬 로드아웃 UI")]
    public Transform skillSlotContainer;
    public GameObject skillSlotPrefab;

    void Start()
    {
        InitializeButtons();
        LoadSettings();
        CheckContinueButton();
        ShowPanel(mainMenuPanel);
    }

    void InitializeButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGame);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);

        if (missionSelectButton != null)
            missionSelectButton.onClick.AddListener(() => ShowPanel(missionSelectPanel));

        if (skillLoadoutButton != null)
            skillLoadoutButton.onClick.AddListener(() => ShowPanel(skillLoadoutPanel));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));

        if (creditsButton != null)
            creditsButton.onClick.AddListener(() => ShowPanel(creditsPanel));

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        // 설정 슬라이더
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (uiVolumeSlider != null)
            uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
        
        // 난이도 드롭다운
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
    }

    void LoadSettings()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();

            if (masterVolumeSlider != null)
                masterVolumeSlider.value = SaveSystem.Instance.currentSave.masterVolume;
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = SaveSystem.Instance.currentSave.sfxVolume;

            if (uiVolumeSlider != null)
                uiVolumeSlider.value = SaveSystem.Instance.currentSave.uiVolume;

            if (difficultyDropdown != null)
                difficultyDropdown.value = SaveSystem.Instance.currentSave.difficultyLevel;
        }
    }

    void CheckContinueButton()
    {
        if (continueButton != null && SaveSystem.Instance != null)
            continueButton.interactable = SaveSystem.Instance.HasSaveFile();
    }

    void ShowPanel(GameObject panel)
    {
        mainMenuPanel?.SetActive(panel == mainMenuPanel);
        missionSelectPanel?.SetActive(panel == missionSelectPanel);
        skillLoadoutPanel?.SetActive(panel == skillLoadoutPanel);
        settingsPanel?.SetActive(panel == settingsPanel);
        creditsPanel?.SetActive(panel == creditsPanel);
        
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }

    void OnNewGame()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.currentSave = new SaveSystem.SaveData();

        LoadMission("Tutorial");
    }

    void OnContinue()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
            int missionIndex = SaveSystem.Instance.currentSave.currentMissionIndex;
            LoadMission($"Mission_{missionIndex}");
        }
    }

    void LoadMission(string sceneName)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
        
        SceneManager.LoadScene(sceneName);
    }

    // === 설정 콜백 함수들 ===
    
    void OnMasterVolumeChanged(float value)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.masterVolume = value;
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.currentSave.masterVolume = value;
            SaveSystem.Instance.SaveGame();
        }
    }

    void OnSFXVolumeChanged(float value)
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.sfxVolume = value;
            SFXManager.Instance.PlayButtonClick();
        }
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.currentSave.sfxVolume = value;
            SaveSystem.Instance.SaveGame();
        }
    }

    void OnUIVolumeChanged(float value)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.uiVolume = value;
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.currentSave.uiVolume = value;
            SaveSystem.Instance.SaveGame();
        }
    }

    void OnDifficultyChanged(int index)
    {
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.SetDifficulty((DifficultyManager.DifficultyLevel)index);
        
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.currentSave.difficultyLevel = index;
            SaveSystem.Instance.SaveGame();
        }
        
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
    }

    public void OnBackToMainMenu() => ShowPanel(mainMenuPanel);

    void OnQuit()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayButtonClick();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}