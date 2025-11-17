using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SongOfTheStars.Procedural
{
    /// <summary>
    /// Applies random challenge modifiers to increase difficulty and replayability
    /// 난이도와 재플레이성을 높이기 위한 랜덤 챌린지 모디파이어 적용
    ///
    /// Examples: Limited skills, reduced focus, tighter timing, etc.
    /// </summary>
    public class RandomChallengeModifier : MonoBehaviour
    {
        [Header("▶ Challenge Settings")]
        [Range(0, 5)]
        public int challengeCount = 2;

        [Tooltip("Higher = more difficult challenges")]
        [Range(1f, 3f)]
        public float difficultyMultiplier = 1f;

        public bool allowStackingChallenges = true;

        [Header("▶ Active Challenges")]
        [SerializeField]
        private List<Challenge> _activeChallenges = new List<Challenge>();

        [Header("▶ Reward Multiplier")]
        [SerializeField]
        private float _experienceMultiplier = 1f;

        [SerializeField]
        private float _scoreMultiplier = 1f;

        private static readonly ChallengeDefinition[] ALL_CHALLENGES = new ChallengeDefinition[]
        {
            // Skill Restrictions
            new ChallengeDefinition
            {
                id = "no_attack_skills",
                name = "Pacifist",
                description = "Attack skills disabled",
                category = ChallengeCategory.SkillRestriction,
                difficulty = 2,
                rewardMultiplier = 1.5f,
                applyAction = (modifier) => modifier.DisableSkillCategory(SkillCategory.Attack)
            },
            new ChallengeDefinition
            {
                id = "no_stealth_skills",
                name = "Exposed",
                description = "Stealth skills disabled",
                category = ChallengeCategory.SkillRestriction,
                difficulty = 3,
                rewardMultiplier = 1.8f,
                applyAction = (modifier) => modifier.DisableSkillCategory(SkillCategory.Stealth)
            },
            new ChallengeDefinition
            {
                id = "one_skill_only",
                name = "Minimalist",
                description = "Can only use 1 skill (random)",
                category = ChallengeCategory.SkillRestriction,
                difficulty = 4,
                rewardMultiplier = 2.0f,
                applyAction = (modifier) => modifier.LimitToOneSkill()
            },

            // Focus Penalties
            new ChallengeDefinition
            {
                id = "half_focus",
                name = "Exhausted",
                description = "Max focus reduced by 50%",
                category = ChallengeCategory.FocusPenalty,
                difficulty = 2,
                rewardMultiplier = 1.4f,
                applyAction = (modifier) => modifier.ModifyMaxFocus(0.5f)
            },
            new ChallengeDefinition
            {
                id = "no_focus_regen",
                name = "Drained",
                description = "Focus does not regenerate from Perfect inputs",
                category = ChallengeCategory.FocusPenalty,
                difficulty = 3,
                rewardMultiplier = 1.7f,
                applyAction = (modifier) => modifier.DisableFocusRegeneration()
            },
            new ChallengeDefinition
            {
                id = "double_focus_cost",
                name = "Inefficient",
                description = "All skill costs doubled",
                category = ChallengeCategory.FocusPenalty,
                difficulty = 3,
                rewardMultiplier = 1.8f,
                applyAction = (modifier) => modifier.ModifyFocusCosts(2.0f)
            },

            // Timing Challenges
            new ChallengeDefinition
            {
                id = "tight_timing",
                name = "Precision Master",
                description = "Timing windows reduced by 50%",
                category = ChallengeCategory.TimingChallenge,
                difficulty = 3,
                rewardMultiplier = 1.6f,
                applyAction = (modifier) => modifier.ModifyTimingWindows(0.5f)
            },
            new ChallengeDefinition
            {
                id = "perfect_only",
                name = "Perfectionist",
                description = "Only Perfect inputs count (no Great)",
                category = ChallengeCategory.TimingChallenge,
                difficulty = 4,
                rewardMultiplier = 2.2f,
                applyAction = (modifier) => modifier.RequirePerfectInputsOnly()
            },
            new ChallengeDefinition
            {
                id = "fast_bpm",
                name = "Accelerated",
                description = "Music BPM increased by 25%",
                category = ChallengeCategory.TimingChallenge,
                difficulty = 2,
                rewardMultiplier = 1.5f,
                applyAction = (modifier) => modifier.ModifyBPM(1.25f)
            },

            // Movement Restrictions
            new ChallengeDefinition
            {
                id = "slow_movement",
                name = "Injured",
                description = "Movement speed reduced by 30%",
                category = ChallengeCategory.MovementRestriction,
                difficulty = 2,
                rewardMultiplier = 1.3f,
                applyAction = (modifier) => modifier.ModifyMovementSpeed(0.7f)
            },
            new ChallengeDefinition
            {
                id = "no_free_movement",
                name = "Beat Locked",
                description = "Can only move on beat (no free movement)",
                category = ChallengeCategory.MovementRestriction,
                difficulty = 3,
                rewardMultiplier = 1.7f,
                applyAction = (modifier) => modifier.DisableFreeMovement()
            },

            // Detection Penalties
            new ChallengeDefinition
            {
                id = "enhanced_guards",
                name = "Alert Guards",
                description = "Guard vision range +50%",
                category = ChallengeCategory.DetectionPenalty,
                difficulty = 2,
                rewardMultiplier = 1.4f,
                applyAction = (modifier) => modifier.ModifyGuardVision(1.5f)
            },
            new ChallengeDefinition
            {
                id = "instant_alert",
                name = "High Security",
                description = "Being spotted raises alert to max immediately",
                category = ChallengeCategory.DetectionPenalty,
                difficulty = 4,
                rewardMultiplier = 2.0f,
                applyAction = (modifier) => modifier.EnableInstantAlert()
            },

            // Cooldown Penalties
            new ChallengeDefinition
            {
                id = "long_cooldowns",
                name = "Rusty",
                description = "All skill cooldowns +50%",
                category = ChallengeCategory.CooldownPenalty,
                difficulty = 2,
                rewardMultiplier = 1.4f,
                applyAction = (modifier) => modifier.ModifySkillCooldowns(1.5f)
            },

            // Extreme Challenges
            new ChallengeDefinition
            {
                id = "no_skills",
                name = "Pure Stealth",
                description = "All skills disabled",
                category = ChallengeCategory.Extreme,
                difficulty = 5,
                rewardMultiplier = 3.0f,
                applyAction = (modifier) => modifier.DisableAllSkills()
            },
            new ChallengeDefinition
            {
                id = "one_life",
                name = "Iron Mode",
                description = "Mission fails on first detection",
                category = ChallengeCategory.Extreme,
                difficulty = 5,
                rewardMultiplier = 2.5f,
                applyAction = (modifier) => modifier.EnableOneLifeMode()
            }
        };

        [ContextMenu("Apply Random Challenges")]
        public void ApplyRandomChallenges()
        {
            ClearChallenges();

            List<ChallengeDefinition> availableChallenges = new List<ChallengeDefinition>(ALL_CHALLENGES);

            // Filter by difficulty
            availableChallenges = availableChallenges
                .Where(c => c.difficulty <= difficultyMultiplier * 3f)
                .ToList();

            // Randomly select challenges
            int actualChallengeCount = Mathf.Min(challengeCount, availableChallenges.Count);

            for (int i = 0; i < actualChallengeCount; i++)
            {
                if (availableChallenges.Count == 0) break;

                ChallengeDefinition selectedDef = availableChallenges[Random.Range(0, availableChallenges.Count)];

                // Check category conflicts if not allowing stacking
                if (!allowStackingChallenges)
                {
                    bool hasConflict = _activeChallenges.Any(c => c.definition.category == selectedDef.category);
                    if (hasConflict)
                    {
                        availableChallenges.Remove(selectedDef);
                        i--; // Retry
                        continue;
                    }
                }

                // Apply challenge
                Challenge challenge = new Challenge { definition = selectedDef };
                _activeChallenges.Add(challenge);

                selectedDef.applyAction?.Invoke(this);

                availableChallenges.Remove(selectedDef);

                Debug.Log($"Applied challenge: {selectedDef.name} (+{selectedDef.rewardMultiplier:F1}x rewards)");
            }

            // Calculate total reward multiplier
            CalculateRewardMultipliers();

            Debug.Log($"✅ {_activeChallenges.Count} challenges active. " +
                      $"XP: {_experienceMultiplier:F1}x, Score: {_scoreMultiplier:F1}x");
        }

        [ContextMenu("Clear All Challenges")]
        public void ClearChallenges()
        {
            _activeChallenges.Clear();
            _experienceMultiplier = 1f;
            _scoreMultiplier = 1f;

            // Reset any applied modifiers
            // (In real implementation, you'd revert actual game state changes)
        }

        private void CalculateRewardMultipliers()
        {
            _experienceMultiplier = 1f;
            _scoreMultiplier = 1f;

            foreach (Challenge challenge in _activeChallenges)
            {
                _experienceMultiplier += (challenge.definition.rewardMultiplier - 1f);
                _scoreMultiplier += (challenge.definition.rewardMultiplier - 1f);
            }
        }

        // Challenge Application Methods

        private void DisableSkillCategory(SkillCategory category)
        {
            // Implementation: Disable all skills of this category
            Debug.Log($"[Modifier] Disabled skill category: {category}");
        }

        private void LimitToOneSkill()
        {
            Debug.Log($"[Modifier] Limited to one random skill");
        }

        private void ModifyMaxFocus(float multiplier)
        {
            Debug.Log($"[Modifier] Max focus × {multiplier}");
        }

        private void DisableFocusRegeneration()
        {
            Debug.Log($"[Modifier] Focus regeneration disabled");
        }

        private void ModifyFocusCosts(float multiplier)
        {
            Debug.Log($"[Modifier] Skill costs × {multiplier}");
        }

        private void ModifyTimingWindows(float multiplier)
        {
            Debug.Log($"[Modifier] Timing windows × {multiplier}");
        }

        private void RequirePerfectInputsOnly()
        {
            Debug.Log($"[Modifier] Only Perfect inputs accepted");
        }

        private void ModifyBPM(float multiplier)
        {
            Debug.Log($"[Modifier] BPM × {multiplier}");
        }

        private void ModifyMovementSpeed(float multiplier)
        {
            Debug.Log($"[Modifier] Movement speed × {multiplier}");
        }

        private void DisableFreeMovement()
        {
            Debug.Log($"[Modifier] Free movement disabled");
        }

        private void ModifyGuardVision(float multiplier)
        {
            Debug.Log($"[Modifier] Guard vision × {multiplier}");
        }

        private void EnableInstantAlert()
        {
            Debug.Log($"[Modifier] Instant max alert on detection");
        }

        private void ModifySkillCooldowns(float multiplier)
        {
            Debug.Log($"[Modifier] Skill cooldowns × {multiplier}");
        }

        private void DisableAllSkills()
        {
            Debug.Log($"[Modifier] All skills disabled");
        }

        private void EnableOneLifeMode()
        {
            Debug.Log($"[Modifier] One life mode enabled");
        }

        // Public API

        public List<Challenge> GetActiveChallenges()
        {
            return new List<Challenge>(_activeChallenges);
        }

        public float GetExperienceMultiplier() => _experienceMultiplier;
        public float GetScoreMultiplier() => _scoreMultiplier;

        public string GetChallengesSummary()
        {
            if (_activeChallenges.Count == 0)
                return "No active challenges";

            string summary = $"Active Challenges ({_activeChallenges.Count}):\n";
            foreach (Challenge challenge in _activeChallenges)
            {
                summary += $"• {challenge.definition.name}: {challenge.definition.description}\n";
            }
            summary += $"\nRewards: XP ×{_experienceMultiplier:F1}, Score ×{_scoreMultiplier:F1}";
            return summary;
        }
    }

    [System.Serializable]
    public class Challenge
    {
        public ChallengeDefinition definition;
        public bool isActive = true;
    }

    public class ChallengeDefinition
    {
        public string id;
        public string name;
        public string description;
        public ChallengeCategory category;
        public int difficulty; // 1-5
        public float rewardMultiplier; // 1.0 - 3.0
        public System.Action<RandomChallengeModifier> applyAction;
    }

    public enum ChallengeCategory
    {
        SkillRestriction,
        FocusPenalty,
        TimingChallenge,
        MovementRestriction,
        DetectionPenalty,
        CooldownPenalty,
        Extreme
    }
}
