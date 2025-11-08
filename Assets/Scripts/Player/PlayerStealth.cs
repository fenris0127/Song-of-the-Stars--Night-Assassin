using UnityEngine;

/// <summary>
/// 플레이어의 은신 상태 및 관련 효과(예: 이속 감소)를 관리합니다.
/// </summary>
public class PlayerStealth : MonoBehaviour
{
    private PlayerController Player => GameServices.Player;
    private float _originalMoveSpeed;
    private SpriteRenderer _spriteRenderer; // ★ 추가: 투명도 조절용
    
    [Header("▶ 스텔스 설정")]
    public float stealthMoveSpeedMultiplier = 0.5f; 
    public bool isStealthActive = false;
    public float stealthAlpha = 0.5f; // ★ 은신 시 투명도 (50%)
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); // ★ 초기화

        if (Player != null)
            _originalMoveSpeed = Player.moveSpeed;
    }
    
    /// <summary>
    /// 은신 상태를 토글하고, 이동 속도를 변경합니다.
    /// </summary>
    public void ToggleStealth()
    {
        isStealthActive = !isStealthActive;

        if (Player != null)
        {
            if (isStealthActive)
            {
                Player.moveSpeed = _originalMoveSpeed * stealthMoveSpeedMultiplier;
                Debug.Log("스텔스 활성화: 이동 속도 감소");
                
                if (_spriteRenderer != null)
                {
                    Color color = _spriteRenderer.color;
                    color.a = stealthAlpha; 
                    _spriteRenderer.color = color;
                }
            }
            else
            {
                Player.moveSpeed = _originalMoveSpeed;
                Debug.Log("스텔스 비활성화: 이동 속도 복구");
                
                if (_spriteRenderer != null)
                {
                    Color color = _spriteRenderer.color;
                    color.a = 1f; 
                    _spriteRenderer.color = color;
                }
            }
        }
    }
}