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
    
    // ✅ 볼륨 캐싱 필드    
    private float _cachedSFXVolume;
    private float _cachedUIVolume;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
        UpdateVolumeCache(); // ✅ 최초 캐시 업데이트
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject child = new GameObject($"AudioSource_{i}");
            child.transform.parent = transform;

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f; // 3D 사운드 활성화
            source.rolloffMode = AudioRolloffMode.Linear;
            source.maxDistance = 30f;
            _audioSourcePool.Add(source);
        }
    }

    // 볼륨 변경 시 캐시 업데이트
    void UpdateVolumeCache()
    {
        _cachedSFXVolume = masterVolume * sfxVolume;
        _cachedUIVolume = masterVolume * uiVolume;
    }
    
    // 개선된 볼륨 조절 API (사용 가이드 반영)
    public void SetMasterVolume(float volume) { masterVolume = volume; UpdateVolumeCache(); }
    public void SetSFXVolume(float volume) { sfxVolume = volume; UpdateVolumeCache(); }
    public void SetUIVolume(float volume) { uiVolume = volume; UpdateVolumeCache(); }

    AudioSource GetAvailableAudioSource()
    {
       for (int i = 0; i < _audioSourcePool.Count; i++)
        {
            // 현재 인덱스부터 순회하며 재생 중이 아닌 소스를 찾음
            int index = (_currentPoolIndex + i) % _audioSourcePool.Count;
            if (!_audioSourcePool[index].isPlaying)
            {
                _currentPoolIndex = (index + 1) % _audioSourcePool.Count;
                return _audioSourcePool[index];
            }
        }
        
        // 모두 사용 중이면 가장 오래된 것 (현재 인덱스)을 재사용
        AudioSource oldestSource = _audioSourcePool[_currentPoolIndex];
        _currentPoolIndex = (_currentPoolIndex + 1) % _audioSourcePool.Count;
        return oldestSource;
    }

    // 사운드 이펙트 재생 (기본)
    public void PlaySFX(AudioClip clip, bool isUI = false)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.loop = false;
        source.volume = isUI ? _cachedUIVolume : _cachedSFXVolume; // 캐싱된 볼륨 사용
        source.spatialBlend = isUI ? 0f : 1f; // UI 사운드는 2D
        source.transform.position = transform.position; // 2D 사운드는 리스너 위치에 맞춤
        source.Play();
    }
    
    // 3D 위치 기반 사운드 재생
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.loop = false;
        source.volume = _cachedSFXVolume; // 캐싱된 볼륨 사용
        source.spatialBlend = 1f;
        source.transform.position = position; // 3D 사운드는 위치에 맞춤
        source.Play();
    }
    
    // === 편의 함수들 ===
    public void PlayPerfectSound() => PlaySFX(perfectSound, true);
    public void PlayGreatSound() => PlaySFX(greatSound, true);
    public void PlayMissSound() => PlaySFX(missSound, true);
    
    public void PlayStealthActivate() => PlaySFX(stealthActivateSound);
    public void PlayStealthDeactivate() => PlaySFX(stealthDeactivateSound);
    public void PlayIllusionCast() => PlaySFX(illusionCastSound);
    public void PlayCharge() => PlaySFX(chargeSound);
    public void PlayAssassination() => PlaySFX(assassinationSound);
    public void PlayFlashbang() => PlaySFX(flashbangSound);
    
    public void PlayAlertIncrease() => PlaySFX(alertIncreaseSound, true);
    public void PlayDetectionWarning() => PlaySFX(detectionWarningSound, true);
    public void PlayMissionSuccess() => PlaySFX(missionSuccessSound, true);
    public void PlayMissionFail() => PlaySFX(missionFailSound, true);
    
    public void PlayGuardDeath(Vector3 position) => PlaySFXAtPosition(guardDeathSound, position);
    public void PlayGuardAlert(Vector3 position) => PlaySFXAtPosition(guardAlertSound, position);
    public void PlayGuardSearch(Vector3 position) => PlaySFXAtPosition(guardSearchSound, position);
}