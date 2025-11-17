using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SongOfTheStars.Tools
{
    /// <summary>
    /// Analyzes game balance and provides recommendations
    /// 게임 밸런스를 분석하고 권장 사항 제공
    ///
    /// Analyzes: Focus economy, skill usage, difficulty curves, player performance
    /// </summary>
    public class BalanceAnalyzer : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("▶ Analysis Settings")]
        public bool autoAnalyze = false;
        public int minSampleSize = 10;

        [Header("▶ Thresholds")]
        [Range(0f, 1f)]
        public float skillUsageWarningThreshold = 0.1f; // Warn if skill used <10%
        [Range(0f, 1f)]
        public float difficultySpikeTolerance = 0.3f; // Warn if difficulty spikes >30%

        private BalanceReport _lastReport;

        [ContextMenu("Analyze Game Balance")]
        public void AnalyzeBalance()
        {
            Debug.Log("🔍 Analyzing game balance...");

            _lastReport = new BalanceReport();

            AnalyzeSkillUsage();
            AnalyzeFocusEconomy();
            AnalyzeDifficultyCurve();
            AnalyzePlayerPerformance();
            GenerateRecommendations();

            DisplayReport();
        }

        #region Skill Analysis

        private void AnalyzeSkillUsage()
        {
            // Load all skills
            string[] skillGuids = AssetDatabase.FindAssets("t:ConstellationSkillData");
            List<ConstellationSkillData> skills = new List<ConstellationSkillData>();

            foreach (string guid in skillGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ConstellationSkillData skill = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(path);
                if (skill != null) skills.Add(skill);
            }

            _lastReport.totalSkills = skills.Count;

            // Analyze cost/cooldown ratios
            foreach (ConstellationSkillData skill in skills)
            {
                float efficiency = skill.focusCost / Mathf.Max(1f, skill.cooldownBeats);

                SkillBalanceData data = new SkillBalanceData
                {
                    skillName = skill.skillName,
                    category = skill.category.ToString(),
                    focusCost = skill.focusCost,
                    cooldown = skill.cooldownBeats,
                    efficiency = efficiency,
                    usageRate = 0f // Would need real gameplay data
                };

                _lastReport.skillBalances.Add(data);

                // Check for outliers
                if (efficiency > 3f)
                {
                    _lastReport.warnings.Add($"⚠️ {skill.skillName} may be too expensive (cost/cooldown: {efficiency:F2})");
                }
                else if (efficiency < 1f)
                {
                    _lastReport.warnings.Add($"⚠️ {skill.skillName} may be too cheap (cost/cooldown: {efficiency:F2})");
                }
            }

            // Check category balance
            var categoryGroups = skills.GroupBy(s => s.category);
            foreach (var group in categoryGroups)
            {
                float avgCost = group.Average(s => s.focusCost);
                float avgCooldown = group.Average(s => s.cooldownBeats);

                _lastReport.categoryBalances.Add(new CategoryBalanceData
                {
                    category = group.Key.ToString(),
                    skillCount = group.Count(),
                    averageFocusCost = avgCost,
                    averageCooldown = avgCooldown
                });
            }
        }

        #endregion

        #region Focus Economy

        private void AnalyzeFocusEconomy()
        {
            // Simulate focus generation/spending
            float maxFocus = 100f;
            float perfectFocusGain = 10f;
            float beatInterval = 0.5f; // Example: 120 BPM

            // Calculate how many Perfect inputs needed for each skill
            string[] skillGuids = AssetDatabase.FindAssets("t:ConstellationSkillData");

            List<float> inputsRequired = new List<float>();

            foreach (string guid in skillGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ConstellationSkillData skill = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(path);

                if (skill != null)
                {
                    float required = skill.focusCost / perfectFocusGain;
                    inputsRequired.Add(required);

                    if (required > 5f)
                    {
                        _lastReport.warnings.Add($"⚠️ {skill.skillName} requires {required:F1} Perfect inputs (may feel too grindy)");
                    }
                }
            }

            if (inputsRequired.Count > 0)
            {
                _lastReport.averageInputsForSkill = inputsRequired.Average();
                _lastReport.maxInputsForSkill = inputsRequired.Max();
            }

            // Check if players can maintain focus spending
            if (_lastReport.averageInputsForSkill > 3f)
            {
                _lastReport.warnings.Add($"⚠️ Focus economy may be too tight (avg {_lastReport.averageInputsForSkill:F1} inputs needed)");
            }
        }

        #endregion

        #region Difficulty Curve

        private void AnalyzeDifficultyCurve()
        {
            // Load all missions
            string[] missionGuids = AssetDatabase.FindAssets("t:MissionData");
            List<SongOfTheStars.Missions.MissionData> missions = new List<SongOfTheStars.Missions.MissionData>();

            foreach (string guid in missionGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mission = AssetDatabase.LoadAssetAtPath<SongOfTheStars.Missions.MissionData>(path);
                if (mission != null) missions.Add(mission);
            }

            if (missions.Count < 2)
            {
                _lastReport.warnings.Add("⚠️ Not enough missions to analyze difficulty curve");
                return;
            }

            // Sort by difficulty
            missions = missions.OrderBy(m => m.difficulty).ToList();

            // Analyze difficulty progression
            for (int i = 0; i < missions.Count - 1; i++)
            {
                var current = missions[i];
                var next = missions[i + 1];

                float currentDifficulty = EstimateDifficulty(current);
                float nextDifficulty = EstimateDifficulty(next);

                float increase = (nextDifficulty - currentDifficulty) / currentDifficulty;

                if (increase > difficultySpikeTolerance)
                {
                    _lastReport.warnings.Add($"⚠️ Large difficulty spike between '{current.missionName}' and '{next.missionName}' (+{increase * 100:F0}%)");
                }
                else if (increase < 0f)
                {
                    _lastReport.warnings.Add($"⚠️ Difficulty decrease between missions ('{current.missionName}' → '{next.missionName}')");
                }
            }
        }

        private float EstimateDifficulty(SongOfTheStars.Missions.MissionData mission)
        {
            float score = 0f;

            // Objective count
            score += mission.primaryObjectives.Count * 10f;
            score += mission.optionalObjectives.Count * 5f;

            // Time pressure
            if (mission.timeLimit > 0f)
            {
                score += 20f;
            }

            // Alert level
            score += (10 - mission.maxAlertLevel) * 5f;

            // Difficulty multiplier
            score *= mission.difficulty switch
            {
                SongOfTheStars.Missions.DifficultyLevel.Tutorial => 0.5f,
                SongOfTheStars.Missions.DifficultyLevel.Easy => 1f,
                SongOfTheStars.Missions.DifficultyLevel.Normal => 1.5f,
                SongOfTheStars.Missions.DifficultyLevel.Hard => 2.5f,
                _ => 1f
            };

            return score;
        }

        #endregion

        #region Player Performance

        private void AnalyzePlayerPerformance()
        {
            // This would analyze actual gameplay data
            // For now, provide general metrics

            _lastReport.recommendations.Add("📊 Collect gameplay telemetry to analyze:");
            _lastReport.recommendations.Add("  • Average mission completion times");
            _lastReport.recommendations.Add("  • Success rates per mission");
            _lastReport.recommendations.Add("  • Skill usage frequency");
            _lastReport.recommendations.Add("  • Common failure points");
        }

        #endregion

        #region Recommendations

        private void GenerateRecommendations()
        {
            // Skill balance recommendations
            if (_lastReport.skillBalances.Count > 0)
            {
                var mostExpensive = _lastReport.skillBalances.OrderByDescending(s => s.efficiency).First();
                var cheapest = _lastReport.skillBalances.OrderBy(s => s.efficiency).First();

                float efficiencyGap = mostExpensive.efficiency / cheapest.efficiency;

                if (efficiencyGap > 3f)
                {
                    _lastReport.recommendations.Add($"💡 Consider rebalancing: {mostExpensive.skillName} is {efficiencyGap:F1}x more expensive than {cheapest.skillName}");
                }
            }

            // Category balance recommendations
            var categoryVariance = _lastReport.categoryBalances
                .Select(c => c.averageFocusCost)
                .Variance();

            if (categoryVariance > 50f)
            {
                _lastReport.recommendations.Add($"💡 Category costs vary significantly (variance: {categoryVariance:F1})");
            }

            // Focus economy recommendations
            if (_lastReport.averageInputsForSkill > 4f)
            {
                _lastReport.recommendations.Add($"💡 Reduce average skill costs OR increase Perfect focus gain");
            }

            // General recommendations
            if (_lastReport.totalSkills < 8)
            {
                _lastReport.recommendations.Add($"💡 Consider adding more skills (currently: {_lastReport.totalSkills})");
            }
        }

        #endregion

        #region Display

        private void DisplayReport()
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("╔══════════════════════════════════════╗");
            report.AppendLine("║     BALANCE ANALYSIS REPORT          ║");
            report.AppendLine("╚══════════════════════════════════════╝");
            report.AppendLine();

            // Summary
            report.AppendLine("📊 SUMMARY:");
            report.AppendLine($"  Total Skills: {_lastReport.totalSkills}");
            report.AppendLine($"  Avg Inputs for Skill: {_lastReport.averageInputsForSkill:F1}");
            report.AppendLine($"  Max Inputs for Skill: {_lastReport.maxInputsForSkill:F1}");
            report.AppendLine();

            // Category Balance
            report.AppendLine("📂 CATEGORY BALANCE:");
            foreach (var cat in _lastReport.categoryBalances)
            {
                report.AppendLine($"  {cat.category}:");
                report.AppendLine($"    Skills: {cat.skillCount}");
                report.AppendLine($"    Avg Cost: {cat.averageFocusCost:F1}");
                report.AppendLine($"    Avg Cooldown: {cat.averageCooldown:F1}");
            }
            report.AppendLine();

            // Skill Details
            report.AppendLine("🎯 SKILL EFFICIENCY (Cost/Cooldown):");
            foreach (var skill in _lastReport.skillBalances.OrderByDescending(s => s.efficiency))
            {
                report.AppendLine($"  {skill.skillName} [{skill.category}]: {skill.efficiency:F2} ({skill.focusCost}F / {skill.cooldown}CD)");
            }
            report.AppendLine();

            // Warnings
            if (_lastReport.warnings.Count > 0)
            {
                report.AppendLine("⚠️  WARNINGS:");
                foreach (string warning in _lastReport.warnings)
                {
                    report.AppendLine($"  {warning}");
                }
                report.AppendLine();
            }

            // Recommendations
            if (_lastReport.recommendations.Count > 0)
            {
                report.AppendLine("💡 RECOMMENDATIONS:");
                foreach (string rec in _lastReport.recommendations)
                {
                    report.AppendLine($"  {rec}");
                }
                report.AppendLine();
            }

            report.AppendLine("═══════════════════════════════════════");

            Debug.Log(report.ToString());

            // Show dialog
            EditorUtility.DisplayDialog(
                "Balance Analysis Complete",
                $"Analysis complete!\n\n" +
                $"Analyzed: {_lastReport.totalSkills} skills\n" +
                $"Warnings: {_lastReport.warnings.Count}\n" +
                $"Recommendations: {_lastReport.recommendations.Count}\n\n" +
                $"See Console for full report.",
                "OK"
            );
        }

        #endregion
#endif
    }

    #region Data Structures

    [System.Serializable]
    public class BalanceReport
    {
        public int totalSkills;
        public float averageInputsForSkill;
        public float maxInputsForSkill;

        public List<SkillBalanceData> skillBalances = new List<SkillBalanceData>();
        public List<CategoryBalanceData> categoryBalances = new List<CategoryBalanceData>();

        public List<string> warnings = new List<string>();
        public List<string> recommendations = new List<string>();
    }

    [System.Serializable]
    public class SkillBalanceData
    {
        public string skillName;
        public string category;
        public float focusCost;
        public int cooldown;
        public float efficiency; // cost/cooldown ratio
        public float usageRate; // % of players using this skill
    }

    [System.Serializable]
    public class CategoryBalanceData
    {
        public string category;
        public int skillCount;
        public float averageFocusCost;
        public float averageCooldown;
    }

    #endregion

    #region Extensions

    public static class BalanceExtensions
    {
        public static float Variance(this IEnumerable<float> values)
        {
            if (!values.Any()) return 0f;

            float average = values.Average();
            return values.Average(v => Mathf.Pow(v - average, 2));
        }
    }

    #endregion
}
