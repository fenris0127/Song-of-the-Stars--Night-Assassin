using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SongOfTheStars.Missions;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Auto-populates mission data assets with specifications from MISSION_DESIGNS.md
    /// MISSION_DESIGNS.md 사양에 따라 미션 데이터 자산을 자동으로 채웁니다
    ///
    /// Run from Unity Editor menu: Song of the Stars / Data / Populate All Mission Data
    /// </summary>
    public class MissionDataPopulator
    {
#if UNITY_EDITOR
        private const string MISSIONS_PATH = "Assets/Data/Missions/";
        private const string SKILLS_PATH = "Assets/Data/Skills/";

        [MenuItem("Song of the Stars/Data/Populate All Mission Data", priority = 2)]
        public static void PopulateAllMissionData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Data/Missions"))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Missions");
            }

            int created = 0;

            created += CreateOrUpdateMission_Tutorial(out bool _);
            created += CreateOrUpdateMission_SilentApproach(out bool _);
            created += CreateOrUpdateMission_NightMarket(out bool _);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ Mission data population complete! {created} missions configured");
            EditorUtility.DisplayDialog("Success", $"Mission data configured!\n\n3 missions populated from MISSION_DESIGNS.md", "OK");
        }

        // MISSION 0: TUTORIAL

        private static int CreateOrUpdateMission_Tutorial(out bool wasCreated)
        {
            string path = MISSIONS_PATH + "Tutorial_FirstSteps.asset";
            MissionData mission = GetOrCreateMissionAsset(path, out wasCreated);

            // Mission Info
            mission.missionID = "tutorial_first_steps";
            mission.missionName = "First Steps";
            mission.briefing = "Welcome, Night Assassin. This training courtyard will teach you the fundamentals of rhythm-based stealth. Master the beat, and the shadows will become your ally.";
            mission.difficulty = DifficultyLevel.Tutorial;
            mission.sceneName = "Tutorial_Courtyard";
            mission.musicBPM = 100f;
            mission.timeLimit = 0f; // No limit
            mission.maxAlertLevel = 5;

            // Primary Objectives
            mission.primaryObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Move to the marked location using arrow keys",
                    type = ObjectiveType.Reach,
                    targetTag = "Tutorial_Waypoint_1",
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Hit 5 Perfect rhythm inputs",
                    type = ObjectiveType.Investigate, // Custom type, tracked by tutorial
                    targetCount = 5,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Activate Capricorn Trap (Press 1 on beat)",
                    type = ObjectiveType.Investigate, // Custom
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Eliminate the practice dummy",
                    type = ObjectiveType.Eliminate,
                    targetTag = "TutorialDummy",
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Optional Objectives
            mission.optionalObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Complete tutorial without missing a beat",
                    type = ObjectiveType.Stealth,
                    maxDetections = 0,
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Skill Loadout
            ConstellationSkillData capricorn = LoadSkillAsset("CapricornTrap");
            if (capricorn != null)
            {
                mission.availableSkills = new List<ConstellationSkillData> { capricorn };
                mission.defaultLoadout = new List<ConstellationSkillData> { capricorn };
            }

            // Unlocked Skills
            ConstellationSkillData decoy = LoadSkillAsset("Decoy");
            if (decoy != null)
            {
                mission.unlockedSkills = new List<ConstellationSkillData> { decoy };
            }

            // Scripted Events
            mission.scriptedEvents = new List<ScriptedEventDefinition>
            {
                new ScriptedEventDefinition
                {
                    eventID = "welcome",
                    trigger = EventTrigger.OnMissionStart,
                    eventType = EventType.ShowDialog,
                    dialogText = "Feel the rhythm of the stars. Each beat is an opportunity."
                },
                new ScriptedEventDefinition
                {
                    eventID = "first_perfect",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 1,
                    eventType = EventType.ShowDialog,
                    dialogText = "Excellent! Perfect timing grants Focus energy."
                },
                new ScriptedEventDefinition
                {
                    eventID = "skill_unlocked",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 2,
                    eventType = EventType.ShowDialog,
                    dialogText = "Skills are powerful but require Focus. Use them wisely."
                }
            };

            // Rewards
            mission.experienceReward = 50;
            mission.bonusExperiencePerOptional = 25;

            EditorUtility.SetDirty(mission);
            return 1;
        }

        // MISSION 1: SILENT APPROACH

        private static int CreateOrUpdateMission_SilentApproach(out bool wasCreated)
        {
            string path = MISSIONS_PATH + "Mission_01_SilentApproach.asset";
            MissionData mission = GetOrCreateMissionAsset(path, out wasCreated);

            // Mission Info
            mission.missionID = "mission_01_silent_approach";
            mission.missionName = "Silent Approach";
            mission.briefing = "Your first real contract. Lord Malvern's estate guards a valuable artifact in the east courtyard. Eliminate the two patrol captains and reach the artifact vault. The city watch is on high alert - stay in the shadows.";
            mission.difficulty = DifficultyLevel.Easy;
            mission.sceneName = "Mission_01_Courtyard";
            mission.musicBPM = 110f;
            mission.timeLimit = 0f; // No hard limit (180s for S-rank)
            mission.maxAlertLevel = 10;

            // Primary Objectives
            mission.primaryObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Eliminate the patrol captain in the west wing",
                    type = ObjectiveType.Eliminate,
                    targetTag = "Captain_Thorne",
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Eliminate the patrol captain in the east wing",
                    type = ObjectiveType.Eliminate,
                    targetTag = "Captain_Vale",
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Reach the artifact vault",
                    type = ObjectiveType.Reach,
                    targetTag = "Vault_Entrance",
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Optional Objectives
            mission.optionalObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Complete mission without being detected",
                    type = ObjectiveType.Stealth,
                    maxDetections = 0,
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Complete in under 120 seconds",
                    type = ObjectiveType.Survive,
                    duration = 120f,
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Maintain 100% Perfect timing (no Great or Miss)",
                    type = ObjectiveType.Investigate,
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Skill Loadout
            ConstellationSkillData capricorn = LoadSkillAsset("CapricornTrap");
            ConstellationSkillData decoy = LoadSkillAsset("Decoy");
            ConstellationSkillData orion = LoadSkillAsset("OrionsArrow");

            List<ConstellationSkillData> available = new List<ConstellationSkillData>();
            if (capricorn != null) available.Add(capricorn);
            if (decoy != null) available.Add(decoy);
            if (orion != null) available.Add(orion);

            mission.availableSkills = available;

            List<ConstellationSkillData> loadout = new List<ConstellationSkillData>();
            if (capricorn != null) loadout.Add(capricorn);
            if (decoy != null) loadout.Add(decoy);

            mission.defaultLoadout = loadout;

            // Unlocked Skills
            ConstellationSkillData gemini = LoadSkillAsset("GeminiClone");
            ConstellationSkillData shadowBlend = LoadSkillAsset("ShadowBlend");

            List<ConstellationSkillData> unlocked = new List<ConstellationSkillData>();
            if (gemini != null) unlocked.Add(gemini);
            if (shadowBlend != null) unlocked.Add(shadowBlend);

            mission.unlockedSkills = unlocked;

            // Scripted Events
            mission.scriptedEvents = new List<ScriptedEventDefinition>
            {
                new ScriptedEventDefinition
                {
                    eventID = "mission_brief",
                    trigger = EventTrigger.OnMissionStart,
                    eventType = EventType.ShowDialog,
                    dialogText = "Two targets, one vault. Remember your training."
                },
                new ScriptedEventDefinition
                {
                    eventID = "first_captain_down",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 0,
                    eventType = EventType.ShowDialog,
                    dialogText = "One down. Stay focused on the rhythm."
                },
                new ScriptedEventDefinition
                {
                    eventID = "reinforcements",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 0,
                    eventType = EventType.SpawnGuards,
                    spawnCount = 2,
                    spawnPointTag = "ReinforcementSpawn"
                },
                new ScriptedEventDefinition
                {
                    eventID = "alert_warning",
                    trigger = EventTrigger.OnAlertLevel,
                    triggerDelay = 5f, // Alert level 5
                    eventType = EventType.ShowDialog,
                    dialogText = "Guards are getting suspicious. Lower your profile!"
                },
                new ScriptedEventDefinition
                {
                    eventID = "vault_reached",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 2,
                    eventType = EventType.CameraFocus,
                    dialogText = "Mission complete. Extracting..."
                }
            };

            // Rewards
            mission.experienceReward = 100;
            mission.bonusExperiencePerOptional = 50;

            EditorUtility.SetDirty(mission);
            return 1;
        }

        // MISSION 2: NIGHT MARKET

        private static int CreateOrUpdateMission_NightMarket(out bool wasCreated)
        {
            string path = MISSIONS_PATH + "Mission_02_NightMarket.asset";
            MissionData mission = GetOrCreateMissionAsset(path, out wasCreated);

            // Mission Info
            mission.missionID = "mission_02_night_market";
            mission.missionName = "Night Market";
            mission.briefing = "The night market is alive with activity - perfect cover, or perfect trap. Your target is the merchant prince Hassan, who trades in stolen constellation artifacts. He's surrounded by mercenary guards. Eliminate Hassan and recover the 3 stolen star maps before the market closes at dawn.";
            mission.difficulty = DifficultyLevel.Normal;
            mission.sceneName = "Mission_02_Market";
            mission.musicBPM = 120f;
            mission.timeLimit = 300f; // 5 minutes
            mission.maxAlertLevel = 10;

            // Primary Objectives
            mission.primaryObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Eliminate the merchant prince Hassan",
                    type = ObjectiveType.Eliminate,
                    targetTag = "Target_Hassan",
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Recover the 3 stolen star maps",
                    type = ObjectiveType.Collect,
                    itemID = "StarMap",
                    targetCount = 3,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Reach the extraction point",
                    type = ObjectiveType.Reach,
                    targetTag = "ExtractionPoint",
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Optional Objectives
            mission.optionalObjectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Complete without being spotted (Alert Level 0)",
                    type = ObjectiveType.Stealth,
                    maxDetections = 0,
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Eliminate all 4 mercenary lieutenants",
                    type = ObjectiveType.Eliminate,
                    targetTag = "Mercenary_Lieutenant",
                    targetCount = 4,
                    isSecret = true // Revealed after first lieutenant killed
                },
                new ObjectiveDefinition
                {
                    description = "Complete in under 180 seconds",
                    type = ObjectiveType.Survive,
                    duration = 180f,
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Skill Loadout
            ConstellationSkillData capricorn = LoadSkillAsset("CapricornTrap");
            ConstellationSkillData orion = LoadSkillAsset("OrionsArrow");
            ConstellationSkillData decoy = LoadSkillAsset("Decoy");
            ConstellationSkillData gemini = LoadSkillAsset("GeminiClone");
            ConstellationSkillData shadowBlend = LoadSkillAsset("ShadowBlend");
            ConstellationSkillData pegasus = LoadSkillAsset("PegasusDash");

            List<ConstellationSkillData> available = new List<ConstellationSkillData>();
            if (capricorn != null) available.Add(capricorn);
            if (orion != null) available.Add(orion);
            if (decoy != null) available.Add(decoy);
            if (gemini != null) available.Add(gemini);
            if (shadowBlend != null) available.Add(shadowBlend);
            if (pegasus != null) available.Add(pegasus);

            mission.availableSkills = available;

            List<ConstellationSkillData> loadout = new List<ConstellationSkillData>();
            if (orion != null) loadout.Add(orion);
            if (gemini != null) loadout.Add(gemini);
            if (shadowBlend != null) loadout.Add(shadowBlend);
            if (pegasus != null) loadout.Add(pegasus);

            mission.defaultLoadout = loadout;

            // Unlocked Skills
            ConstellationSkillData andromeda = LoadSkillAsset("AndromedaVeil");
            ConstellationSkillData aquarius = LoadSkillAsset("AquariusFlow");

            List<ConstellationSkillData> unlocked = new List<ConstellationSkillData>();
            if (andromeda != null) unlocked.Add(andromeda);
            if (aquarius != null) unlocked.Add(aquarius);

            mission.unlockedSkills = unlocked;

            // Scripted Events
            mission.scriptedEvents = new List<ScriptedEventDefinition>
            {
                new ScriptedEventDefinition
                {
                    eventID = "market_opening",
                    trigger = EventTrigger.OnMissionStart,
                    eventType = EventType.ShowDialog,
                    dialogText = "The market is crowded. Use the chaos to your advantage."
                },
                new ScriptedEventDefinition
                {
                    eventID = "hassan_alerted",
                    trigger = EventTrigger.OnAlertLevel,
                    triggerDelay = 3f, // Alert level 3
                    eventType = EventType.ShowDialog,
                    dialogText = "Hassan is fleeing! Don't let him escape!"
                },
                new ScriptedEventDefinition
                {
                    eventID = "hassan_guards_spawn",
                    trigger = EventTrigger.OnAlertLevel,
                    triggerDelay = 3f,
                    eventType = EventType.SpawnGuards,
                    spawnCount = 4,
                    spawnPointTag = "Hassan_Guards"
                },
                new ScriptedEventDefinition
                {
                    eventID = "first_map",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 1, // First map collected (progress: 1/3)
                    eventType = EventType.ShowDialog,
                    dialogText = "Star map secured. Two more to find."
                },
                new ScriptedEventDefinition
                {
                    eventID = "hassan_eliminated",
                    trigger = EventTrigger.OnObjectiveComplete,
                    triggerObjectiveIndex = 0,
                    eventType = EventType.ShowDialog,
                    dialogText = "Target eliminated. Recover the maps and extract."
                },
                new ScriptedEventDefinition
                {
                    eventID = "time_warning",
                    trigger = EventTrigger.OnTimer,
                    triggerDelay = 240f, // 4 minutes (60s remaining)
                    eventType = EventType.ShowDialog,
                    dialogText = "60 seconds until market closes!"
                }
            };

            // Rewards
            mission.experienceReward = 200;
            mission.bonusExperiencePerOptional = 75;

            EditorUtility.SetDirty(mission);
            return 1;
        }

        // HELPER METHODS

        private static MissionData GetOrCreateMissionAsset(string path, out bool wasCreated)
        {
            MissionData existing = AssetDatabase.LoadAssetAtPath<MissionData>(path);

            if (existing != null)
            {
                wasCreated = false;
                return existing;
            }

            MissionData newAsset = ScriptableObject.CreateInstance<MissionData>();
            AssetDatabase.CreateAsset(newAsset, path);
            wasCreated = true;
            return newAsset;
        }

        private static ConstellationSkillData LoadSkillAsset(string skillName)
        {
            string path = SKILLS_PATH + skillName + ".asset";
            return AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(path);
        }
#endif
    }
}
