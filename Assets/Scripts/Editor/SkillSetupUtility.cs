using UnityEngine;
using UnityEditor;
using SongOfTheStars.Skills;

namespace SongOfTheStars.Editor
{
    /// <summary>
    /// Utility to quickly add skill components to player
    /// 플레이어에 스킬 컴포넌트를 빠르게 추가하는 유틸리티
    ///
    /// Provides menu items and shortcuts for skill setup
    /// </summary>
    public class SkillSetupUtility
    {
        private const string MENU_ROOT = "Song of the Stars/Skills/";

        #region Add All Skills
        [MenuItem(MENU_ROOT + "Add All 8 Skills to Selected", priority = 1)]
        public static void AddAllSkillsToSelected()
        {
            GameObject player = Selection.activeGameObject;

            if (player == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select the Player GameObject first.", "OK");
                return;
            }

            int added = 0;

            // Attack Skills
            if (AddSkillIfMissing<CapricornTrapSkill>(player)) added++;
            if (AddSkillIfMissing<OrionsArrowSkill>(player)) added++;

            // Lure Skills
            if (AddSkillIfMissing<DecoySkill>(player)) added++;
            if (AddSkillIfMissing<GeminiCloneSkill>(player)) added++;

            // Stealth Skills
            if (AddSkillIfMissing<ShadowBlendSkill>(player)) added++;
            if (AddSkillIfMissing<AndromedaVeilSkill>(player)) added++;

            // Movement Skills
            if (AddSkillIfMissing<PegasusDashSkill>(player)) added++;
            if (AddSkillIfMissing<AquariusFlowSkill>(player)) added++;

            EditorUtility.DisplayDialog("Success", $"Added {added} skill component(s) to {player.name}", "OK");
            EditorUtility.SetDirty(player);
        }

        [MenuItem(MENU_ROOT + "Add All 8 Skills to Selected", validate = true)]
        public static bool ValidateAddAllSkills()
        {
            return Selection.activeGameObject != null;
        }
        #endregion

        #region Add Individual Skills
        [MenuItem(MENU_ROOT + "Add Attack Skills/Capricorn Trap")]
        public static void AddCapricornTrap()
        {
            AddSkillComponentToSelected<CapricornTrapSkill>("Capricorn Trap");
        }

        [MenuItem(MENU_ROOT + "Add Attack Skills/Orion's Arrow")]
        public static void AddOrionsArrow()
        {
            AddSkillComponentToSelected<OrionsArrowSkill>("Orion's Arrow");
        }

        [MenuItem(MENU_ROOT + "Add Lure Skills/Decoy")]
        public static void AddDecoy()
        {
            AddSkillComponentToSelected<DecoySkill>("Decoy");
        }

        [MenuItem(MENU_ROOT + "Add Lure Skills/Gemini Clone")]
        public static void AddGeminiClone()
        {
            AddSkillComponentToSelected<GeminiCloneSkill>("Gemini Clone");
        }

        [MenuItem(MENU_ROOT + "Add Stealth Skills/Shadow Blend")]
        public static void AddShadowBlend()
        {
            AddSkillComponentToSelected<ShadowBlendSkill>("Shadow Blend");
        }

        [MenuItem(MENU_ROOT + "Add Stealth Skills/Andromeda's Veil")]
        public static void AddAndromedaVeil()
        {
            AddSkillComponentToSelected<AndromedaVeilSkill>("Andromeda's Veil");
        }

        [MenuItem(MENU_ROOT + "Add Movement Skills/Pegasus Dash")]
        public static void AddPegasusDash()
        {
            AddSkillComponentToSelected<PegasusDashSkill>("Pegasus Dash");
        }

        [MenuItem(MENU_ROOT + "Add Movement Skills/Aquarius Flow")]
        public static void AddAquariusFlow()
        {
            AddSkillComponentToSelected<AquariusFlowSkill>("Aquarius Flow");
        }
        #endregion

        #region Validation for Individual Skills
        [MenuItem(MENU_ROOT + "Add Attack Skills/Capricorn Trap", validate = true)]
        [MenuItem(MENU_ROOT + "Add Attack Skills/Orion's Arrow", validate = true)]
        [MenuItem(MENU_ROOT + "Add Lure Skills/Decoy", validate = true)]
        [MenuItem(MENU_ROOT + "Add Lure Skills/Gemini Clone", validate = true)]
        [MenuItem(MENU_ROOT + "Add Stealth Skills/Shadow Blend", validate = true)]
        [MenuItem(MENU_ROOT + "Add Stealth Skills/Andromeda's Veil", validate = true)]
        [MenuItem(MENU_ROOT + "Add Movement Skills/Pegasus Dash", validate = true)]
        [MenuItem(MENU_ROOT + "Add Movement Skills/Aquarius Flow", validate = true)]
        public static bool ValidateAddSkill()
        {
            return Selection.activeGameObject != null;
        }
        #endregion

        #region Remove Skills
        [MenuItem(MENU_ROOT + "Remove All Skills from Selected", priority = 100)]
        public static void RemoveAllSkillsFromSelected()
        {
            GameObject player = Selection.activeGameObject;

            if (player == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select the Player GameObject first.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm", "Remove all skill components from this GameObject?", "Yes", "Cancel"))
            {
                return;
            }

            int removed = 0;

            // Remove all skill types
            removed += RemoveSkillIfPresent<CapricornTrapSkill>(player);
            removed += RemoveSkillIfPresent<OrionsArrowSkill>(player);
            removed += RemoveSkillIfPresent<DecoySkill>(player);
            removed += RemoveSkillIfPresent<GeminiCloneSkill>(player);
            removed += RemoveSkillIfPresent<ShadowBlendSkill>(player);
            removed += RemoveSkillIfPresent<AndromedaVeilSkill>(player);
            removed += RemoveSkillIfPresent<PegasusDashSkill>(player);
            removed += RemoveSkillIfPresent<AquariusFlowSkill>(player);

            EditorUtility.DisplayDialog("Success", $"Removed {removed} skill component(s) from {player.name}", "OK");
            EditorUtility.SetDirty(player);
        }

        [MenuItem(MENU_ROOT + "Remove All Skills from Selected", validate = true)]
        public static bool ValidateRemoveAllSkills()
        {
            return Selection.activeGameObject != null;
        }
        #endregion

        #region Utilities
        [MenuItem(MENU_ROOT + "Utilities/List All Skills on Selected", priority = 200)]
        public static void ListAllSkillsOnSelected()
        {
            GameObject player = Selection.activeGameObject;

            if (player == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject first.", "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine($"=== Skills on {player.name} ===\n");

            int count = 0;

            // Check each skill type
            if (player.GetComponent<CapricornTrapSkill>() != null)
            {
                report.AppendLine("✓ Capricorn Trap (Attack)");
                count++;
            }

            if (player.GetComponent<OrionsArrowSkill>() != null)
            {
                report.AppendLine("✓ Orion's Arrow (Attack)");
                count++;
            }

            if (player.GetComponent<DecoySkill>() != null)
            {
                report.AppendLine("✓ Decoy (Lure)");
                count++;
            }

            if (player.GetComponent<GeminiCloneSkill>() != null)
            {
                report.AppendLine("✓ Gemini Clone (Lure)");
                count++;
            }

            if (player.GetComponent<ShadowBlendSkill>() != null)
            {
                report.AppendLine("✓ Shadow Blend (Stealth)");
                count++;
            }

            if (player.GetComponent<AndromedaVeilSkill>() != null)
            {
                report.AppendLine("✓ Andromeda's Veil (Stealth)");
                count++;
            }

            if (player.GetComponent<PegasusDashSkill>() != null)
            {
                report.AppendLine("✓ Pegasus Dash (Movement)");
                count++;
            }

            if (player.GetComponent<AquariusFlowSkill>() != null)
            {
                report.AppendLine("✓ Aquarius Flow (Movement)");
                count++;
            }

            report.AppendLine($"\nTotal: {count}/8 skills");

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Skill List", report.ToString(), "OK");
        }

        [MenuItem(MENU_ROOT + "Utilities/List All Skills on Selected", validate = true)]
        public static bool ValidateListAllSkills()
        {
            return Selection.activeGameObject != null;
        }
        #endregion

        #region Helper Methods
        private static void AddSkillComponentToSelected<T>(string skillName) where T : MonoBehaviour
        {
            GameObject player = Selection.activeGameObject;

            if (player == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select the Player GameObject first.", "OK");
                return;
            }

            if (AddSkillIfMissing<T>(player))
            {
                EditorUtility.DisplayDialog("Success", $"Added {skillName} component to {player.name}", "OK");
                EditorUtility.SetDirty(player);
            }
            else
            {
                EditorUtility.DisplayDialog("Already Exists", $"{skillName} component already exists on {player.name}", "OK");
            }
        }

        private static bool AddSkillIfMissing<T>(GameObject obj) where T : MonoBehaviour
        {
            if (obj.GetComponent<T>() == null)
            {
                obj.AddComponent<T>();
                return true;
            }
            return false;
        }

        private static int RemoveSkillIfPresent<T>(GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
                return 1;
            }
            return 0;
        }
        #endregion

        #region Create Skill Data Assets
        [MenuItem(MENU_ROOT + "Create ScriptableObject Data/Create All 8 Skill Data Assets", priority = 300)]
        public static void CreateAllSkillDataAssets()
        {
            string basePath = "Assets/Data/Skills/";

            if (!AssetDatabase.IsValidFolder(basePath.TrimEnd('/')))
            {
                EditorUtility.DisplayDialog("Error", $"Folder '{basePath}' does not exist. Create it first using Setup > Create Folder Structure.", "OK");
                return;
            }

            string[] skillNames = new string[]
            {
                "CapricornTrap",
                "OrionsArrow",
                "Decoy",
                "GeminiClone",
                "ShadowBlend",
                "AndromedaVeil",
                "PegasusDash",
                "AquariusFlow"
            };

            int created = 0;

            foreach (string skillName in skillNames)
            {
                string assetPath = basePath + skillName + ".asset";

                if (!AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(assetPath))
                {
                    ConstellationSkillData asset = ScriptableObject.CreateInstance<ConstellationSkillData>();
                    asset.skillName = AddSpacesToCamelCase(skillName);
                    AssetDatabase.CreateAsset(asset, assetPath);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Created {created} skill data asset(s) in {basePath}", "OK");
        }

        private static string AddSpacesToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && !char.IsUpper(text[i - 1]))
                {
                    result.Append(' ');
                }
                result.Append(text[i]);
            }

            return result.ToString();
        }
        #endregion
    }
}
