using UnityEngine;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Difficulty-specific game settings
    /// 난이도별 게임 설정
    ///
    /// Balanced values based on playtesting recommendations:
    /// - Easy: Forgiving timing, slow guards, high player mobility
    /// - Normal: Balanced gameplay for most players
    /// - Hard: Tight timing, aggressive guards, resource scarcity
    /// - Expert: Unforgiving, requires mastery of all mechanics
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultySettings", menuName = "Song of the Stars/Configs/Difficulty Settings")]
    public class DifficultySettings : ScriptableObject
    {
        [Header("Rhythm Judgment")]
        [Tooltip("Timing tolerance for beat judgments (seconds)")]
        [Range(0.05f, 0.2f)]
        public float beatTolerance = 0.1f;

        [Tooltip("Timing tolerance for Perfect judgments (seconds)")]
        [Range(0.03f, 0.12f)]
        public float perfectTolerance = 0.05f;

        [Header("Guard AI Settings")]
        [Tooltip("How far guards can see")]
        [Range(5f, 15f)]
        public float guardViewDistance = 10f;

        [Tooltip("Guard field of view angle")]
        [Range(60f, 140f)]
        public float guardViewAngle = 100f;

        [Tooltip("Guard movement speed")]
        [Range(4f, 10f)]
        public float guardMoveSpeed = 6f;

        [Tooltip("Beats between guard patrol movements")]
        [Range(2, 8)]
        public int guardPatrolInterval = 4;

        [Tooltip("Time to full detection when in sight")]
        [Range(0.5f, 4f)]
        public float detectionTime = 2f;

        [Header("Player Settings")]
        [Tooltip("Player movement animation speed")]
        [Range(4f, 12f)]
        public float playerMoveSpeed = 8f;

        [Tooltip("Movement speed multiplier when in stealth")]
        [Range(0.3f, 1f)]
        public float stealthSpeedMultiplier = 0.5f;

        [Tooltip("Maximum focus capacity")]
        [Range(50f, 150f)]
        public float maxFocus = 100f;

        [Tooltip("Focus gained per Perfect timing")]
        [Range(5f, 15f)]
        public float focusPerPerfect = 10f;

        [Header("Skill System")]
        [Tooltip("Cooldown multiplier for all skills (lower = faster cooldowns)")]
        [Range(0.5f, 2f)]
        public float cooldownMultiplier = 1f;

        [Header("Alert System")]
        [Tooltip("Maximum alert level before mission failure")]
        [Range(3, 15)]
        public int maxAlertLevel = 10; // Increased from 5

        [Tooltip("Alert increase when fully detected")]
        [Range(1, 5)]
        public int alertIncreaseOnDetection = 2;

        [Tooltip("Alert increase when spotted (not fully detected)")]
        [Range(1, 3)]
        public int alertIncreaseOnSpotted = 1;

        [Tooltip("Time in seconds before alert decays by 1")]
        [Range(0f, 120f)]
        public float alertDecayInterval = 45f; // Decay 1 alert every 45 seconds

        [Header("Focus Decay")]
        [Tooltip("Enable time-based focus decay")]
        public bool enableFocusDecay = false;

        [Tooltip("Focus decay per second when not hitting Perfect")]
        [Range(0f, 10f)]
        public float focusDecayPerSecond = 2f;

        [Tooltip("Delay before focus starts decaying (seconds)")]
        [Range(1f, 10f)]
        public float focusDecayDelay = 3f;
    }
}
