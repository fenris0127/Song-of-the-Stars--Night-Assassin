using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 2f;
    
    [Header("Skill Status")]
    public bool isMemoryActive = false;
    private Vector3 _memoryPosition;
    
    private bool _isCharging = false;
    private float _chargeEndTime;
    public float chargeSpeed = 10f;

    public bool isShieldActive = false;
    private int _shieldEndBeat = 0;
    
    public bool isIllusionActive = false; 
    private int _illusionEndBeat = 0; 
    public GameObject illusionPrefab; 

    private RhythmSyncManager _rhythmManager;
    private CharacterController _characterController;
    private PlayerStealth _playerStealth;

    void Start()
    {
        _rhythmManager = FindObjectOfType<RhythmSyncManager>();
        _characterController = GetComponent<CharacterController>();
        _playerStealth = GetComponent<PlayerStealth>();
        if (_rhythmManager != null)
        {
            _rhythmManager.OnBeatCounted.AddListener(CheckShieldTimeout);
            _rhythmManager.OnBeatCounted.AddListener(CheckIllusionTimeout);
        }
    }

    void Update()
    {
        HandleChargeMovement();
    }
    
    // --- 게자리 (퇴각의 기억) ---
    public void SetMemoryPosition()
    {
        _memoryPosition = transform.position;
        isMemoryActive = true;
    }

    public void TeleportToMemory()
    {
        if (isMemoryActive)
        {
            _characterController.enabled = false; 
            transform.position = _memoryPosition;
            _characterController.enabled = true;
            isMemoryActive = false;
        }
    }
    
    // --- 양자리 (맹렬한 돌진) ---
    public void ActivateCharge(float duration)
    {
        _isCharging = true;
        _chargeEndTime = Time.time + duration;
    }
    
    void HandleChargeMovement()
    {
        if (_isCharging)
        {
            _characterController.Move(transform.forward * chargeSpeed * Time.deltaTime);
            CheckChargeCollision();
            
            if (Time.time >= _chargeEndTime)
            {
                _isCharging = false;
            }
        }
    }

    void CheckChargeCollision() { /* 경비병 충돌 로직 구현 */ }

    // --- 황소자리 (견고한 방패) ---
    public void ActivateShield(int durationInBeats)
    {
        isShieldActive = true;
        _shieldEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
    }
    
    void CheckShieldTimeout(int currentBeat)
    {
        if (isShieldActive && currentBeat >= _shieldEndBeat)
        {
            isShieldActive = false;
        }
    }

    // --- 물고기자리 (이중 환영) ---
    public void ActivateIllusion(int durationInBeats)
    {
        isIllusionActive = true;
        _illusionEndBeat = _rhythmManager.currentBeatCount + durationInBeats;
    }
    
    void CheckIllusionTimeout(int currentBeat)
    {
        if (isIllusionActive && currentBeat >= _illusionEndBeat)
        {
            isIllusionActive = false;
            _illusionEndBeat = 0;
        }
    }
    
    public bool ReceiveDamage()
    {
        if (isShieldActive)
        {
            isShieldActive = false;
            return false;
        }
        return true;
    }
}