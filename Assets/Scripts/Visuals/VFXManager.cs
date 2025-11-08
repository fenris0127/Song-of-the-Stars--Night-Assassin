using UnityEngine;

public class VFXManager : MonoBehaviour
{
    // private RhythmSyncManager _rhythmManager;
    private RhythmSyncManager RhythmManager => GameServices.RhythmManager;


    // void Start()
    // {
    //     _rhythmManager = FindObjectOfType<RhythmSyncManager>();
    // }

    public void PlayVFXAt(GameObject vfxPrefab, Vector3 position, float customDuration = 0f)
    {
        if (vfxPrefab == null) return;

        GameObject vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);
        ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            float duration = customDuration > 0 ? customDuration : ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(vfxInstance, duration);
        }
        else
        {
            Destroy(vfxInstance, 3f); 
        }
    }
    
    public void BurstVFXOnBeat(GameObject burstPrefab, Vector3 position)
    {
        if (burstPrefab == null || RhythmManager == null) return;
        PlayVFXAt(burstPrefab, position, RhythmManager.beatInterval * 2f);
    }
}