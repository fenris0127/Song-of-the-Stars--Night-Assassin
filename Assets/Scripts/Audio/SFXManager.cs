using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 전역 사운드 이펙트를 관리합니다.
/// </summary>
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    
    [Header("▶ AudioSource 풀")]
    public int audioSourcePoolSize = 10;
    private int _currentPoolIndex = 0;
    private List<AudioSource> _audioSourcePool = new List<AudioSource>();
    
    [Header("▶ 스킬 사운드")]
    public AudioClip stealthActivateSound;
    public AudioClip stealthDeactivateSound;
    public AudioClip illusionCastSound;
    public AudioClip chargeSound;
    public AudioClip assassinationSound;
    public AudioClip flashbangSound;
    
    [Header("▶ 판정 사운드")]
    public AudioClip perfectSound;
    public AudioClip greatSound;
    public AudioClip missSound;
    
    [Header("▶ 게임 이벤트 사운드")]
    public AudioClip alertIncreaseSound;
    public AudioClip detectionWarningSound;
    public AudioClip missionSuccessSound;
    public AudioClip missionFailSound;
    
    [Header("▶ 경비병 사운드")]
    public AudioClip guardDeathSound;
    public AudioClip guardAlertSound;
    public AudioClip guardSearchSound;
    
    [Header("▶ UI 사운드")]
    public AudioClip buttonClickSound;
    public AudioClip skillCooldownReadySound;
    
    [Header("▶ 볼륨 설정")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;
    private float _cachedSFXVolume;
    private float _cachedUIVolume;


    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSourcePool();
        }
        else
            Destroy(gameObject);
    }

    void InitializeAudioSourcePool()
    {
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _audioSourcePool.Add(source);
        }
    }

    /// <summary>
    /// 사운드 이펙트 재생 (기본)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = masterVolume * sfxVolume * volumeMultiplier;
        source.Play();
    }
    
    /// <summary>
    /// 3D 위치 기반 사운드 재생
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        
        AudioSource.PlayClipAtPoint(clip, position, masterVolume * sfxVolume * volumeMultiplier);
    }
    
    /// <summary>
    /// UI 사운드 재생
    /// </summary>
    public void PlayUISFX(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = masterVolume * uiVolume;
        source.Play();
    }

    AudioSource GetAvailableAudioSource()
    {
        // 먼저 사용 가능한 소스 찾기
        for (int i = 0; i < _audioSourcePool.Count; i++)
        {
            int index = (_currentPoolIndex + i) % _audioSourcePool.Count;
            if (!_audioSourcePool[index].isPlaying)
            {
                _currentPoolIndex = (index + 1) % _audioSourcePool.Count;
                return _audioSourcePool[index];
            }
        }

        // 모두 사용 중이면 라운드 로빈
        // AudioSource source = _audioSourcePool[_currentPoolIndex];
        // _currentPoolIndex = (_currentPoolIndex + 1) % _audioSourcePool.Count;
        // return source;

        return _audioSourcePool[_currentPoolIndex++];
    }
    
    void UpdateVolumeCache()
    {
        _cachedSFXVolume = masterVolume * sfxVolume;
        _cachedUIVolume = masterVolume * uiVolume;
    }

    // === 편의 함수들 ===
    
    public void PlayPerfectSound() => PlaySFX(perfectSound);
    public void PlayGreatSound() => PlaySFX(greatSound);
    public void PlayMissSound() => PlaySFX(missSound);
    
    public void PlayStealthActivate() => PlaySFX(stealthActivateSound);
    public void PlayStealthDeactivate() => PlaySFX(stealthDeactivateSound);
    public void PlayIllusionCast() => PlaySFX(illusionCastSound);
    public void PlayCharge() => PlaySFX(chargeSound);
    public void PlayAssassination() => PlaySFX(assassinationSound);
    public void PlayFlashbang() => PlaySFX(flashbangSound);
    
    public void PlayAlertIncrease() => PlaySFX(alertIncreaseSound);
    public void PlayDetectionWarning() => PlaySFX(detectionWarningSound);
    public void PlayMissionSuccess() => PlaySFX(missionSuccessSound);
    public void PlayMissionFail() => PlaySFX(missionFailSound);
    
    public void PlayGuardDeath(Vector3 position) => PlaySFXAtPosition(guardDeathSound, position);
    public void PlayGuardAlert(Vector3 position) => PlaySFXAtPosition(guardAlertSound, position);
    public void PlayGuardSearch(Vector3 position) => PlaySFXAtPosition(guardSearchSound, position);
    
    public void PlayButtonClick() => PlayUISFX(buttonClickSound);
    public void PlaySkillReady() => PlayUISFX(skillCooldownReadySound);
}