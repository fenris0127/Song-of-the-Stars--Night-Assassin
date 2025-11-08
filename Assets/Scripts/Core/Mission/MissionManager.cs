using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; 

public class MissionManager : MonoBehaviour
{
    [Header("▶ 이벤트")]
    public UnityEvent OnAlertLevelChanged;
    
    [Header("▶ 경보 설정")]
    public int maxAlertLevel = 5;
    public int currentAlertLevel = 0;
    
    [Header("▶ 게임 상태")]
    public bool isMissionActive = true;
    
    public void IncreaseAlertLevel(int amount)
    {
        if (!isMissionActive) return;

        int previousLevel = currentAlertLevel;
        currentAlertLevel = Mathf.Min(maxAlertLevel, currentAlertLevel + amount);
        
        if (currentAlertLevel != previousLevel)
            OnAlertLevelChanged.Invoke(); 
        
        if (currentAlertLevel >= maxAlertLevel)
            MissionComplete(false); 
    }
    
    public void DecreaseAlertLevel(int amount)
    {
        if (!isMissionActive) return;

        int previousLevel = currentAlertLevel;
        currentAlertLevel = Mathf.Max(0, currentAlertLevel - amount);
        
        if (currentAlertLevel != previousLevel)
            OnAlertLevelChanged.Invoke(); 
    }

    public void MissionComplete(bool success) 
    {
        if (!isMissionActive) return;
        isMissionActive = false;

        if (success)
            Debug.Log("⭐ 미션 성공!");
        else
            Debug.Log("💀 미션 실패!");

        Invoke(nameof(ReturnToMainMenu), 5f);
    }
    
    private void ReturnToMainMenu()
    {
        Debug.Log("5초 후 메인 메뉴로 돌아갑니다.");
        SceneManager.LoadScene("MainMenu");
    }
}