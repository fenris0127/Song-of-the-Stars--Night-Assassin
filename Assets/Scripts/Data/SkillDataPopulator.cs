using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Auto-populates skill data assets with specifications from SKILLS_DESIGN.md
    /// SKILLS_DESIGN.md 사양에 따라 스킬 데이터 자산을 자동으로 채웁니다
    ///
    /// Run from Unity Editor menu: Song of the Stars / Data / Populate All Skill Data
    /// </summary>
    public class SkillDataPopulator
    {
#if UNITY_EDITOR
        private const string SKILLS_PATH = "Assets/Data/Skills/";

        [MenuItem("Song of the Stars/Data/Populate All Skill Data", priority = 1)]
        public static void PopulateAllSkillData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Data/Skills"))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Skills");
            }

            int created = 0;
            int updated = 0;

            // Create/Update all 8 skills
            created += CreateOrUpdateSkill_CapricornTrap(out bool wasCreated1);
            if (wasCreated1) updated++; else created++;

            created += CreateOrUpdateSkill_OrionsArrow(out bool wasCreated2);
            if (wasCreated2) updated++; else created++;

            created += CreateOrUpdateSkill_Decoy(out bool wasCreated3);
            if (wasCreated3) updated++; else created++;

            created += CreateOrUpdateSkill_GeminiClone(out bool wasCreated4);
            if (wasCreated4) updated++; else created++;

            created += CreateOrUpdateSkill_ShadowBlend(out bool wasCreated5);
            if (wasCreated5) updated++; else created++;

            created += CreateOrUpdateSkill_AndromedaVeil(out bool wasCreated6);
            if (wasCreated6) updated++; else created++;

            created += CreateOrUpdateSkill_PegasusDash(out bool wasCreated7);
            if (wasCreated7) updated++; else created++;

            created += CreateOrUpdateSkill_AquariusFlow(out bool wasCreated8);
            if (wasCreated8) updated++; else created++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ Skill data population complete! {created} skills configured");
            EditorUtility.DisplayDialog("Success", $"Skill data configured!\n\nAll 8 skills populated with specs from SKILLS_DESIGN.md", "OK");
        }

        // ATTACK SKILLS

        private static int CreateOrUpdateSkill_CapricornTrap(out bool wasCreated)
        {
            string path = SKILLS_PATH + "CapricornTrap.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Capricorn Trap";
            skill.description = "Places a trap that stuns guards who step on it. Zone control and patrol blocking.";
            skill.category = SkillCategory.Attack;
            skill.constellationName = "Capricorn";

            skill.inputCount = 1;
            skill.cooldownBeats = 12;
            skill.focusCost = 20f;

            skill.effectDurationBeats = 999; // Until triggered
            skill.range = 1.5f; // Melee placement

            skill.balanceNotes = "Zone control skill. Moderate cost, good cooldown. Use to block patrol routes or trap guards in place.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        private static int CreateOrUpdateSkill_OrionsArrow(out bool wasCreated)
        {
            string path = SKILLS_PATH + "OrionsArrow.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Orion's Arrow";
            skill.description = "Fires a star-shaped projectile that eliminates a single guard at range. Requires line of sight.";
            skill.category = SkillCategory.Attack;
            skill.constellationName = "Orion";

            skill.inputCount = 2; // 1-2 rhythm pattern
            skill.cooldownBeats = 16;
            skill.focusCost = 30f;

            skill.effectDurationBeats = 1; // Instant
            skill.range = 15f;
            skill.damage = 999f; // Instant elimination

            skill.balanceNotes = "High cost prevents spam. Long cooldown = strategic use only. Line of sight adds skill. Single target only (no pierce).";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        // LURE SKILLS

        private static int CreateOrUpdateSkill_Decoy(out bool wasCreated)
        {
            string path = SKILLS_PATH + "Decoy.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Decoy";
            skill.description = "Spawns a stationary decoy that attracts guard attention. Simple distraction tool.";
            skill.category = SkillCategory.Lure;
            skill.constellationName = "Various";

            skill.inputCount = 1;
            skill.cooldownBeats = 8;
            skill.focusCost = 15f;

            skill.effectDurationBeats = 20;
            skill.range = 5f;

            skill.balanceNotes = "Cheapest lure skill. Short cooldown allows frequent use. Stationary but effective.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        private static int CreateOrUpdateSkill_GeminiClone(out bool wasCreated)
        {
            string path = SKILLS_PATH + "GeminiClone.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Gemini Clone";
            skill.description = "Creates a semi-transparent clone that mirrors your last movement pattern, drawing guard attention.";
            skill.category = SkillCategory.Lure;
            skill.constellationName = "Gemini";

            skill.inputCount = 2; // 2-2 (same key twice)
            skill.cooldownBeats = 14;
            skill.focusCost = 25f;

            skill.effectDurationBeats = 12;
            skill.range = 0f; // Spawns at player position

            skill.balanceNotes = "More expensive than Decoy (25 vs 15) but more effective - moves naturally. Guards investigate longer.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        // STEALTH SKILLS

        private static int CreateOrUpdateSkill_ShadowBlend(out bool wasCreated)
        {
            string path = SKILLS_PATH + "ShadowBlend.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Shadow Blend";
            skill.description = "Become invisible while standing completely still. Movement or actions break invisibility.";
            skill.category = SkillCategory.Stealth;
            skill.constellationName = "Cygnus";

            skill.inputCount = 2; // 3-4 pattern (but represents stealth category)
            skill.cooldownBeats = 10;
            skill.focusCost = 20f;

            skill.effectDurationBeats = 16; // Max duration
            skill.activationDelayBeats = 1; // 1 beat delay

            skill.balanceNotes = "Requires patience (standing still). Can't act while invisible. Perfect for letting patrols pass. 1 beat delay prevents panic use.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        private static int CreateOrUpdateSkill_AndromedaVeil(out bool wasCreated)
        {
            string path = SKILLS_PATH + "AndromedaVeil.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Andromeda's Veil";
            skill.description = "Complete invisibility for short duration. Can move and act freely but costs very high focus. Ultimate ability.";
            skill.category = SkillCategory.Stealth;
            skill.constellationName = "Andromeda";

            skill.inputCount = 3; // 4-4-4 (three beats, all same)
            skill.cooldownBeats = 20; // LONG cooldown
            skill.focusCost = 40f; // VERY HIGH cost

            skill.effectDurationBeats = 6; // Short duration
            skill.speedMultiplier = 1f; // Full speed allowed

            skill.balanceNotes = "ULTIMATE ABILITY. Very expensive (40 focus = 4 Perfect timings). Long cooldown prevents abuse. Short duration = must use wisely.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        // MOVEMENT SKILLS

        private static int CreateOrUpdateSkill_PegasusDash(out bool wasCreated)
        {
            string path = SKILLS_PATH + "PegasusDash.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Pegasus Dash";
            skill.description = "Instant teleport dash in facing direction. Covers 3 grid tiles instantly with brief invulnerability.";
            skill.category = SkillCategory.Movement;
            skill.constellationName = "Pegasus";

            skill.inputCount = 2; // 1-1 (quick double-tap)
            skill.cooldownBeats = 8;
            skill.focusCost = 20f;

            skill.effectDurationBeats = 1; // Instant
            skill.range = 6f; // 6 meters (3 grid tiles)

            skill.balanceNotes = "Low cooldown = frequent use. Moderate cost. Short distance (tactical, not escape). Can't phase through obstacles.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        private static int CreateOrUpdateSkill_AquariusFlow(out bool wasCreated)
        {
            string path = SKILLS_PATH + "AquariusFlow.asset";
            ConstellationSkillData skill = GetOrCreateAsset(path, out wasCreated);

            skill.skillName = "Aquarius Flow";
            skill.description = "Doubles movement speed for duration. Move like water flowing. Synergizes with stealth.";
            skill.category = SkillCategory.Movement;
            skill.constellationName = "Aquarius";

            skill.inputCount = 1; // Single beat
            skill.cooldownBeats = 12;
            skill.focusCost = 15f; // Cheapest movement skill

            skill.effectDurationBeats = 8;
            skill.speedMultiplier = 2f; // 2x speed

            skill.balanceNotes = "Low focus cost. Simple activation. Good duration. Speed boost is significant but not OP. Synergizes with stealth.";

            EditorUtility.SetDirty(skill);
            return 1;
        }

        // HELPER METHODS

        private static ConstellationSkillData GetOrCreateAsset(string path, out bool wasCreated)
        {
            ConstellationSkillData existing = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(path);

            if (existing != null)
            {
                wasCreated = false;
                return existing;
            }

            ConstellationSkillData newAsset = ScriptableObject.CreateInstance<ConstellationSkillData>();
            AssetDatabase.CreateAsset(newAsset, path);
            wasCreated = true;
            return newAsset;
        }
#endif
    }
}
