using UnityEngine;

/// <summary>
/// 미션 성공 조건이 되는 목표물입니다.
/// </summary>
public class MissionTarget : MonoBehaviour
{
    private MissionManager _missionManager;

    void Start()
    {
        _missionManager = FindObjectOfType<MissionManager>();
        
        if (_missionManager == null)
            Debug.LogError("MissionTarget: MissionManager를 찾을 수 없습니다!");
    }

    /// <summary>
    /// 플레이어와 접촉 시 미션 성공을 처리합니다.
    /// </summary>
    /// <param name="other">접촉한 Collider2D</param>
    void OnTriggerEnter2D(Collider2D other) 
    {
        // 플레이어 오브젝트인지 확인
        if (other.GetComponent<PlayerController>() != null)
        {
            if (_missionManager != null && _missionManager.isMissionActive)
                _missionManager.MissionComplete(true); // 미션 성공
        }
    }
}