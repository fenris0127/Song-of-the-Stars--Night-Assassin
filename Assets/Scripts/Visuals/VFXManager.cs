using UnityEngine;
using System.Collections;

/// <summary>
/// Manages visual effects with object pooling support
/// 오브젝트 풀링을 지원하는 시각 효과 관리자
/// </summary>
public class VFXManager : MonoBehaviour
{
    private const float DEFAULT_VFX_DURATION = 3f;

    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;

    /// <summary>
    /// Plays VFX at position using object pooling when available
    /// 오브젝트 풀링을 사용하여 VFX를 재생합니다
    /// </summary>
    /// <param name="vfxPrefab">VFX prefab to instantiate</param>
    /// <param name="position">World position to spawn at</param>
    /// <param name="customDuration">Custom duration (0 = auto-calculate from particle system)</param>
    public void PlayVFXAt(GameObject vfxPrefab, Vector3 position, float customDuration = 0f)
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("VFXManager: Attempted to play null VFX prefab");
            return;
        }

        GameObject vfxInstance = null;

        // Try to use object pool if available
        if (ObjectPoolManager.Instance != null)
        {
            vfxInstance = ObjectPoolManager.Instance.GetPooledObject(vfxPrefab);
            if (vfxInstance != null)
            {
                vfxInstance.transform.position = position;
                vfxInstance.SetActive(true);
            }
        }

        // Fallback to instantiation if pooling unavailable
        if (vfxInstance == null)
        {
            vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);
        }

        // Calculate duration and return to pool
        float duration = CalculateVFXDuration(vfxInstance, customDuration);
        StartCoroutine(ReturnVFXToPool(vfxInstance, duration));
    }

    /// <summary>
    /// Plays VFX synchronized to beat duration
    /// 비트 길이에 맞춰 VFX를 재생합니다
    /// </summary>
    public void BurstVFXOnBeat(GameObject burstPrefab, Vector3 position)
    {
        if (burstPrefab == null)
        {
            Debug.LogWarning("VFXManager: Attempted to play null burst VFX");
            return;
        }

        if (RhythmManager == null)
        {
            Debug.LogWarning("VFXManager: RhythmManager not found, using default duration");
            PlayVFXAt(burstPrefab, position);
            return;
        }

        PlayVFXAt(burstPrefab, position, RhythmManager.beatInterval * 2f);
    }

    /// <summary>
    /// Calculates VFX duration from ParticleSystem or uses custom value
    /// </summary>
    private float CalculateVFXDuration(GameObject vfxInstance, float customDuration)
    {
        if (customDuration > 0f)
            return customDuration;

        ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            return main.duration + main.startLifetime.constantMax;
        }

        return DEFAULT_VFX_DURATION;
    }

    /// <summary>
    /// Returns VFX to pool after duration, or destroys if pooling unavailable
    /// </summary>
    private IEnumerator ReturnVFXToPool(GameObject vfxInstance, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (vfxInstance == null) yield break;

        // Return to pool if available
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(vfxInstance);
        }
        else
        {
            Destroy(vfxInstance);
        }
    }
}