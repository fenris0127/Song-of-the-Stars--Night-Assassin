using UnityEngine;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Rhythm system configuration settings
    /// 리듬 시스템 설정값
    /// </summary>
    [CreateAssetMenu(fileName = "RhythmConfig", menuName = "Song of the Stars/Configs/Rhythm Config")]
    public class RhythmConfig : ScriptableObject
    {
        [Header("Timing Windows (in seconds)")]
        [Tooltip("Time tolerance for Perfect judgment (±)")]
        [Range(0.02f, 0.1f)]
        public float perfectTolerance = 0.04f; // ±40ms - tightened from 50ms

        [Tooltip("Time tolerance for Great judgment (±)")]
        [Range(0.05f, 0.15f)]
        public float greatTolerance = 0.08f; // ±80ms - tightened from 100ms

        [Header("Skill Detection")]
        [Tooltip("Range for checking guards for skill effects")]
        public float skillEffectRange = 10f;

        [Tooltip("Maximum number of guards to check for effects")]
        public int maxGuardsToCheck = 20;

        [Header("Beat Synchronization")]
        [Tooltip("Default BPM if not set")]
        [Range(60f, 180f)]
        public float defaultBPM = 120f;

        [Tooltip("Default beat offset in seconds")]
        public float defaultBeatOffset = 0f;
    }
}
