using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [System.Serializable]
    public class AlertEvent : UnityEvent {}
    public AlertEvent OnAlertLevelChanged;
    
    public int currentAlertLevel = 0;
    public int maxAlertLevel = 5;

    public void IncreaseAlertLevel(int amount)
    {
        int previousLevel = currentAlertLevel;
        currentAlertLevel = Mathf.Min(maxAlertLevel, currentAlertLevel + amount);
        
        if (currentAlertLevel != previousLevel)
        {
            OnAlertLevelChanged.Invoke(); 
        }
    }

    public void DecreaseAlertLevel(int amount)
    {
        int previousLevel = currentAlertLevel;
        currentAlertLevel = Mathf.Max(0, currentAlertLevel - amount);
        
        if (currentAlertLevel != previousLevel)
        {
            OnAlertLevelChanged.Invoke(); 
        }
    }
}