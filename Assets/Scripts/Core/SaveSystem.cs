using UnityEngine;
using System.IO;

/// <summary>
/// 게임 진행 상황, 설정, 스킬 언락 등을 저장/로드합니다.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string _saveFilePath;
    private const string SAVE_FILE_NAME = "savegame.json";

    public SaveData currentSave = new SaveData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 현재 게임 상태를 저장합니다.
    /// </summary>
    public void SaveGame()
    {
        try
        {
            // SFXManager 볼륨 저장
            if (SFXManager.Instance != null)
            {
                currentSave.masterVolume = SFXManager.Instance.masterVolume;
                currentSave.sfxVolume = SFXManager.Instance.sfxVolume;
                currentSave.uiVolume = SFXManager.Instance.uiVolume;
            }

            // DifficultyManager 난이도 저장
            if (DifficultyManager.Instance != null)
                currentSave.difficultyLevel = (int)DifficultyManager.Instance.currentDifficulty;

            string json = JsonUtility.ToJson(currentSave, true);
            File.WriteAllText(_saveFilePath, json);

            Debug.Log($"게임 저장 완료: {_saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 저장된 게임을 로드합니다.
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
            {
                Debug.Log("저장 파일이 없습니다. 새 게임을 시작합니다.");
                return false;
            }

            string json = File.ReadAllText(_saveFilePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);

            // 로드한 설정 적용
            ApplyLoadedSettings();

            Debug.Log($"게임 로드 완료: {_saveFilePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 실패: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 로드한 설정을 매니저들에 적용합니다.
    /// </summary>
    void ApplyLoadedSettings()
    {
        // SFXManager 볼륨 적용
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.masterVolume = currentSave.masterVolume;
            SFXManager.Instance.sfxVolume = currentSave.sfxVolume;
            SFXManager.Instance.uiVolume = currentSave.uiVolume;
        }

        // DifficultyManager 난이도 적용
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.SetDifficulty((DifficultyManager.DifficultyLevel)currentSave.difficultyLevel);
    }

    /// <summary>
    /// 저장 파일을 삭제합니다.
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
                Debug.Log("저장 파일 삭제 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"삭제 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 저장 파일이 존재하는지 확인합니다.
    /// </summary>
    public bool HasSaveFile() => File.Exists(_saveFilePath);

    // === 편의 함수들 ===

    public void UnlockSkill(string skillName)
    {
        if (System.Array.IndexOf(currentSave.unlockedSkills, skillName) == -1)
        {
            int newLength = currentSave.unlockedSkills.Length + 1;
            string[] newArray = new string[newLength];
            currentSave.unlockedSkills.CopyTo(newArray, 0);
            newArray[newLength - 1] = skillName;
            currentSave.unlockedSkills = newArray;
        }
    }

    public bool IsSkillUnlocked(string skillName) =>
        System.Array.IndexOf(currentSave.unlockedSkills, skillName) != -1;

    public void CompleteMission(int missionIndex, int score)
    {
        if (System.Array.IndexOf(currentSave.completedMissions, missionIndex) == -1)
        {
            int newLength = currentSave.completedMissions.Length + 1;
            int[] newArray = new int[newLength];
            currentSave.completedMissions.CopyTo(newArray, 0);
            newArray[newLength - 1] = missionIndex;
            currentSave.completedMissions = newArray;
        }

        currentSave.totalScore += score;
        SaveGame();
    }
}

[System.Serializable]
    public class SaveData
    {
        // 게임 진행
        public int currentMissionIndex = 0;
        public int[] completedMissions = new int[0];
        public int totalScore = 0;
        
        // 플레이어 설정
        public float masterVolume = 1f;
        public float sfxVolume = 0.8f;
        public float uiVolume = 0.6f;
        public int difficultyLevel = 1; // 0=Easy, 1=Normal, 2=Hard, 3=Expert
        
        // 스킬 언락 (ScriptableObject 이름 저장)
        public string[] unlockedSkills = new string[0];
        
        // 장착 스킬
        public string equippedStealthSkill = "";
        public string equippedLureSkill = "";
        public string equippedMovementSkill = "";
        public string equippedAttackSkill = "";
        
        // 통계
        public int totalPlayTime = 0; // 초 단위
        public int perfectCount = 0;
        public int greatCount = 0;
        public int missCount = 0;
        public int guardsDefeated = 0;
    }