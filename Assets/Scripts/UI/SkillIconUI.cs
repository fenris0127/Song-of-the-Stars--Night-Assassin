using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

/// <summary>
/// 개별 스킬 아이콘 프리팹에 부착되어 쿨타임 및 키 정보를 표시합니다.
/// </summary>
public class SkillIconUI : MonoBehaviour
{
    // Unity Editor에서 연결해야 하는 컴포넌트
    public Image skillImage;
    public Image cooldownOverlay; // 쿨타임 시각화를 위한 오버레이 이미지
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI cooldownText;

    private int _currentRemainingBeats = 0;

    public void Setup(Sprite icon, string key, string skillName)
    {
        if (skillImage != null)
        {
            skillImage.sprite = icon;
        }
        if (keyText != null)
        {
            // KeyCode.Alpha1을 '1'로 표시
            keyText.text = key; 
        }
        gameObject.name = "SkillIcon_" + skillName;
        
        UpdateCooldown(0); 
    }

    /// <summary>
    /// 남은 비트 수를 받아 UI를 업데이트합니다.
    /// </summary>
    public void UpdateCooldown(int remainingBeats)
    {
        _currentRemainingBeats = remainingBeats;
        
        // 쿨타임 오버레이 표시 로직
        if (cooldownOverlay != null)
        {
            // 쿨타임이 0보다 클 경우 오버레이 활성화 (Fill Amount는 1로 설정하여 완전 덮음)
            cooldownOverlay.fillAmount = remainingBeats > 0 ? 1f : 0f;
            cooldownOverlay.gameObject.SetActive(remainingBeats > 0); 
        }

        // 쿨타임 텍스트 표시 로직
        if (cooldownText != null)
        {
            cooldownText.text = remainingBeats > 0 ? remainingBeats.ToString() : "";
        }
    }
}