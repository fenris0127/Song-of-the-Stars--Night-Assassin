using UnityEngine;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Focus system configuration settings
    /// 집중력 시스템 설정값
    /// </summary>
    [CreateAssetMenu(fileName = "FocusConfig", menuName = "Song of the Stars/Configs/Focus Config")]
    public class FocusConfig : ScriptableObject
    {
        [Header("Focus Capacity")]
        [Tooltip("Maximum focus the player can accumulate")]
        public float maxFocus = 100f;

        [Header("Focus Gains")]
        [Tooltip("Focus gained per Perfect timing judgment")]
        public float focusPerPerfect = 10f;

        [Tooltip("Focus gained per Great timing judgment")]
        public float focusPerGreat = 5f;

        [Header("Focus Costs")]
        [Tooltip("Base focus cost for skill activation")]
        public float baseFocusCostPerSkill = 15f;

        [Tooltip("Additional focus cost per skill power level")]
        public float focusCostPerPowerLevel = 5f;

        [Header("Focus Penalties")]
        [Tooltip("Focus lost on Miss judgment")]
        public float focusDecayPerMiss = 15f;

        [Tooltip("Focus decay per second when not hitting Perfect")]
        public float focusDecayPerSecond = 2f;

        [Tooltip("Time threshold in seconds before focus starts decaying")]
        public float decayDelayThreshold = 3f;

        [Header("Skill Cooldown Bonuses")]
        [Tooltip("Cooldown multiplier for perfect combo (0.75 = 25% reduction)")]
        [Range(0.5f, 1f)]
        public float perfectComboCooldownMultiplier = 0.67f; // 33% reduction
    }
}
