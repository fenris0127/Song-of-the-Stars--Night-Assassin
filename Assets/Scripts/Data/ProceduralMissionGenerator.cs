using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SongOfTheStars.Missions;

namespace SongOfTheStars.Data
{
    /// <summary>
    /// Procedurally generates random missions with balanced objectives and events
    /// 균형잡힌 목표와 이벤트로 랜덤 미션을 프로시저럴하게 생성
    ///
    /// Menu: Song of the Stars / Data / Generate Random Mission
    /// </summary>
    public class ProceduralMissionGenerator
    {
#if UNITY_EDITOR
        private const string MISSIONS_PATH = "Assets/Data/Missions/";
        private const string SKILLS_PATH = "Assets/Data/Skills/";

        // Mission name pools
        private static readonly string[] MISSION_PREFIXES = new string[]
        {
            "Silent", "Shadow", "Phantom", "Night", "Twilight", "Midnight", "Dark",
            "Stellar", "Lunar", "Cosmic", "Nebula", "Astral", "Celestial"
        };

        private static readonly string[] MISSION_SUFFIXES = new string[]
        {
            "Approach", "Infiltration", "Operation", "Strike", "Heist", "Extraction",
            "Assignment", "Contract", "Maneuver", "Gambit", "Convergence", "Eclipse"
        };

        private static readonly string[] TARGET_NAMES = new string[]
        {
            "Commander Vex", "Captain Thorne", "General Kane", "Baron Crowley",
            "Duchess Nyx", "Count Draven", "Lady Morgana", "Lord Malvern",
            "Hassan the Merchant", "Viktor the Collector", "Aria the Spy",
            "Soren the Enforcer", "Cassia the Oracle", "Darius the Warden"
        };

        private static readonly string[] LOCATION_NAMES = new string[]
        {
            "Courtyard", "Market", "Cathedral", "Palace", "Fortress", "Manor",
            "Plaza", "District", "Quarter", "Estate", "Sanctum", "Spire"
        };

        private static readonly string[] ITEM_NAMES = new string[]
        {
            "Star Map", "Constellation Relic", "Celestial Artifact", "Ancient Tome",
            "Royal Seal", "Encrypted Documents", "Sacred Crystal", "Astrolabe"
        };

        [MenuItem("Song of the Stars/Data/Generate Random Mission", priority = 10)]
        public static void GenerateRandomMission()
        {
            GenerateRandomMissionWithSeed(Random.Range(0, 999999));
        }

        [MenuItem("Song of the Stars/Data/Generate Random Mission (With Seed)", priority = 11)]
        public static void GenerateRandomMissionWithSeedPrompt()
        {
            string seedInput = EditorUtility.DisplayDialogComplex(
                "Random Mission Generator",
                "Generate mission with specific seed?\n(Same seed = same mission)",
                "Random Seed",
                "Cancel",
                "Custom Seed"
            ) switch
            {
                0 => Random.Range(0, 999999).ToString(),
                2 => ShowSeedInputDialog(),
                _ => null
            };

            if (seedInput != null && int.TryParse(seedInput, out int seed))
            {
                GenerateRandomMissionWithSeed(seed);
            }
        }

        private static string ShowSeedInputDialog()
        {
            // Simple prompt (Unity doesn't have built-in text input dialog)
            return EditorUtility.DisplayDialogComplex(
                "Custom Seed",
                "Enter seed in Console and run again, or use Random",
                "Use Random",
                "Cancel",
                "OK"
            ) == 0 ? Random.Range(0, 999999).ToString() : null;
        }

        public static void GenerateRandomMissionWithSeed(int seed)
        {
            Random.InitState(seed);

            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Missions"))
                AssetDatabase.CreateFolder("Assets/Data", "Missions");

            // Generate mission data
            DifficultyLevel difficulty = ChooseRandomDifficulty();
            string missionName = GenerateRandomMissionName();
            string missionID = "mission_random_" + seed;

            // Create asset
            string path = MISSIONS_PATH + $"Random_{seed}.asset";
            MissionData mission = ScriptableObject.CreateInstance<MissionData>();
            AssetDatabase.CreateAsset(mission, path);

            // Populate mission data
            PopulateRandomMissionData(mission, seed, missionID, missionName, difficulty);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created asset
            Selection.activeObject = mission;
            EditorGUIUtility.PingObject(mission);

            Debug.Log($"✅ Random mission generated!\nSeed: {seed}\nName: {missionName}\nDifficulty: {difficulty}");
            EditorUtility.DisplayDialog(
                "Random Mission Generated!",
                $"Mission: {missionName}\nDifficulty: {difficulty}\nSeed: {seed}\n\nAsset created at:\n{path}",
                "OK"
            );
        }

        private static void PopulateRandomMissionData(
            MissionData mission,
            int seed,
            string missionID,
            string missionName,
            DifficultyLevel difficulty)
        {
            // Basic Info
            mission.missionID = missionID;
            mission.missionName = missionName;
            mission.difficulty = difficulty;
            mission.sceneName = $"Mission_Random_{seed}";

            // Music BPM based on difficulty
            mission.musicBPM = difficulty switch
            {
                DifficultyLevel.Tutorial => 100f,
                DifficultyLevel.Easy => Random.Range(100f, 115f),
                DifficultyLevel.Normal => Random.Range(110f, 125f),
                DifficultyLevel.Hard => Random.Range(120f, 140f),
                _ => 120f
            };

            // Time limit and alert
            mission.timeLimit = GenerateTimeLimit(difficulty);
            mission.maxAlertLevel = difficulty switch
            {
                DifficultyLevel.Tutorial => 5,
                DifficultyLevel.Easy => 10,
                DifficultyLevel.Normal => 8,
                DifficultyLevel.Hard => 6,
                _ => 10
            };

            // Briefing
            mission.briefing = GenerateRandomBriefing(missionName, difficulty);

            // Objectives
            mission.primaryObjectives = GeneratePrimaryObjectives(difficulty);
            mission.optionalObjectives = GenerateOptionalObjectives(difficulty);

            // Skill loadout
            GenerateSkillLoadout(mission, difficulty);

            // Scripted events
            mission.scriptedEvents = GenerateScriptedEvents(mission.primaryObjectives.Count, difficulty);

            // Rewards
            mission.experienceReward = difficulty switch
            {
                DifficultyLevel.Tutorial => 50,
                DifficultyLevel.Easy => Random.Range(80, 120),
                DifficultyLevel.Normal => Random.Range(150, 250),
                DifficultyLevel.Hard => Random.Range(300, 500),
                _ => 100
            };

            mission.bonusExperiencePerOptional = difficulty switch
            {
                DifficultyLevel.Tutorial => 25,
                DifficultyLevel.Easy => Random.Range(30, 60),
                DifficultyLevel.Normal => Random.Range(60, 100),
                DifficultyLevel.Hard => Random.Range(100, 150),
                _ => 50
            };

            // Unlocked skills (random 1-2 skills)
            mission.unlockedSkills = GenerateRandomUnlockedSkills(difficulty);

            EditorUtility.SetDirty(mission);
        }

        // GENERATION METHODS

        private static DifficultyLevel ChooseRandomDifficulty()
        {
            float rand = Random.value;
            if (rand < 0.4f) return DifficultyLevel.Easy;
            if (rand < 0.8f) return DifficultyLevel.Normal;
            return DifficultyLevel.Hard;
        }

        private static string GenerateRandomMissionName()
        {
            string prefix = MISSION_PREFIXES[Random.Range(0, MISSION_PREFIXES.Length)];
            string suffix = MISSION_SUFFIXES[Random.Range(0, MISSION_SUFFIXES.Length)];
            return $"{prefix} {suffix}";
        }

        private static string GenerateRandomBriefing(string missionName, DifficultyLevel difficulty)
        {
            string target = TARGET_NAMES[Random.Range(0, TARGET_NAMES.Length)];
            string location = LOCATION_NAMES[Random.Range(0, LOCATION_NAMES.Length)];

            string[] briefingTemplates = new string[]
            {
                $"Intel suggests {target} is operating out of the {location}. Eliminate the target and secure any intelligence. The stars are aligned for this operation.",
                $"Your target is {target}, a key figure in the constellation artifact trade. Infiltrate the {location} and complete your objectives before dawn.",
                $"The {location} holds secrets that cannot fall into enemy hands. {target} must be dealt with. Move swiftly, stay silent.",
                $"A critical mission awaits. {target} has been tracked to the {location}. Complete your objectives and extract before the city watch arrives."
            };

            string briefing = briefingTemplates[Random.Range(0, briefingTemplates.Length)];

            if (difficulty == DifficultyLevel.Hard)
            {
                briefing += " This is a high-risk operation. Expect heavy resistance.";
            }

            return briefing;
        }

        private static float GenerateTimeLimit(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Tutorial => 0f, // No limit
                DifficultyLevel.Easy => Random.value < 0.5f ? 0f : Random.Range(240f, 360f),
                DifficultyLevel.Normal => Random.value < 0.3f ? 0f : Random.Range(180f, 300f),
                DifficultyLevel.Hard => Random.Range(120f, 240f), // Always time limit
                _ => 0f
            };
        }

        private static List<ObjectiveDefinition> GeneratePrimaryObjectives(DifficultyLevel difficulty)
        {
            List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();

            int objectiveCount = difficulty switch
            {
                DifficultyLevel.Tutorial => 3,
                DifficultyLevel.Easy => Random.Range(2, 4),
                DifficultyLevel.Normal => Random.Range(3, 5),
                DifficultyLevel.Hard => Random.Range(4, 6),
                _ => 3
            };

            // Always include at least one elimination objective
            string targetName = TARGET_NAMES[Random.Range(0, TARGET_NAMES.Length)];
            objectives.Add(new ObjectiveDefinition
            {
                description = $"Eliminate {targetName}",
                type = ObjectiveType.Eliminate,
                targetTag = "Target_Primary",
                targetCount = 1,
                isSecret = false
            });

            // Randomly add other objective types
            List<ObjectiveType> availableTypes = new List<ObjectiveType>
            {
                ObjectiveType.Eliminate,
                ObjectiveType.Collect,
                ObjectiveType.Reach,
                ObjectiveType.Sabotage,
                ObjectiveType.Investigate
            };

            for (int i = 1; i < objectiveCount; i++)
            {
                ObjectiveType type = availableTypes[Random.Range(0, availableTypes.Count)];

                objectives.Add(type switch
                {
                    ObjectiveType.Eliminate => new ObjectiveDefinition
                    {
                        description = $"Eliminate {Random.Range(2, 5)} guards",
                        type = ObjectiveType.Eliminate,
                        targetTag = "Guard",
                        targetCount = Random.Range(2, 5),
                        isSecret = false
                    },
                    ObjectiveType.Collect => new ObjectiveDefinition
                    {
                        description = $"Recover {Random.Range(2, 4)} {ITEM_NAMES[Random.Range(0, ITEM_NAMES.Length)]}s",
                        type = ObjectiveType.Collect,
                        itemID = "CollectibleItem",
                        targetCount = Random.Range(2, 4),
                        isSecret = false
                    },
                    ObjectiveType.Reach => new ObjectiveDefinition
                    {
                        description = "Reach the extraction point",
                        type = ObjectiveType.Reach,
                        targetTag = "ExtractionPoint",
                        targetCount = 1,
                        isSecret = false
                    },
                    ObjectiveType.Sabotage => new ObjectiveDefinition
                    {
                        description = $"Destroy {Random.Range(2, 4)} alarm systems",
                        type = ObjectiveType.Sabotage,
                        targetTag = "AlarmSystem",
                        targetCount = Random.Range(2, 4),
                        isSecret = false
                    },
                    ObjectiveType.Investigate => new ObjectiveDefinition
                    {
                        description = $"Investigate {Random.Range(2, 4)} suspicious locations",
                        type = ObjectiveType.Investigate,
                        targetTag = "InvestigationPoint",
                        targetCount = Random.Range(2, 4),
                        isSecret = false
                    },
                    _ => new ObjectiveDefinition
                    {
                        description = "Complete the mission",
                        type = ObjectiveType.Reach,
                        targetTag = "ExtractionPoint",
                        targetCount = 1,
                        isSecret = false
                    }
                });
            }

            return objectives;
        }

        private static List<ObjectiveDefinition> GenerateOptionalObjectives(DifficultyLevel difficulty)
        {
            List<ObjectiveDefinition> optionals = new List<ObjectiveDefinition>();

            int optionalCount = difficulty switch
            {
                DifficultyLevel.Tutorial => 1,
                DifficultyLevel.Easy => Random.Range(1, 3),
                DifficultyLevel.Normal => Random.Range(2, 4),
                DifficultyLevel.Hard => Random.Range(3, 5),
                _ => 2
            };

            // Common optional objectives
            List<ObjectiveDefinition> pool = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition
                {
                    description = "Complete without being detected",
                    type = ObjectiveType.Stealth,
                    maxDetections = 0,
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = $"Complete in under {Random.Range(90, 180)} seconds",
                    type = ObjectiveType.Survive,
                    duration = Random.Range(90f, 180f),
                    targetCount = 1,
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Eliminate all lieutenants",
                    type = ObjectiveType.Eliminate,
                    targetTag = "Lieutenant",
                    targetCount = Random.Range(2, 5),
                    isSecret = true
                },
                new ObjectiveDefinition
                {
                    description = "Collect all bonus items",
                    type = ObjectiveType.Collect,
                    itemID = "BonusItem",
                    targetCount = Random.Range(3, 6),
                    isSecret = false
                },
                new ObjectiveDefinition
                {
                    description = "Maintain 100% Perfect timing",
                    type = ObjectiveType.Investigate,
                    targetCount = 1,
                    isSecret = false
                }
            };

            // Randomly select from pool
            for (int i = 0; i < optionalCount && pool.Count > 0; i++)
            {
                int index = Random.Range(0, pool.Count);
                optionals.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return optionals;
        }

        private static void GenerateSkillLoadout(MissionData mission, DifficultyLevel difficulty)
        {
            // Load all available skills
            List<ConstellationSkillData> allSkills = new List<ConstellationSkillData>();
            string[] skillNames = new string[]
            {
                "CapricornTrap", "OrionsArrow", "Decoy", "GeminiClone",
                "ShadowBlend", "AndromedaVeil", "PegasusDash", "AquariusFlow"
            };

            foreach (string skillName in skillNames)
            {
                ConstellationSkillData skill = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(
                    SKILLS_PATH + skillName + ".asset"
                );
                if (skill != null) allSkills.Add(skill);
            }

            if (allSkills.Count == 0)
            {
                Debug.LogWarning("No skills found! Run 'Populate All Skill Data' first.");
                return;
            }

            // Determine how many skills to make available
            int availableCount = difficulty switch
            {
                DifficultyLevel.Tutorial => Random.Range(1, 3),
                DifficultyLevel.Easy => Random.Range(3, 5),
                DifficultyLevel.Normal => Random.Range(4, 7),
                DifficultyLevel.Hard => allSkills.Count, // All skills
                _ => 4
            };

            // Randomly select available skills
            List<ConstellationSkillData> shuffled = allSkills.OrderBy(x => Random.value).ToList();
            mission.availableSkills = shuffled.Take(availableCount).ToList();

            // Default loadout (max 4)
            int loadoutSize = Mathf.Min(4, difficulty switch
            {
                DifficultyLevel.Tutorial => 1,
                DifficultyLevel.Easy => Random.Range(2, 3),
                DifficultyLevel.Normal => Random.Range(3, 5),
                DifficultyLevel.Hard => 4,
                _ => 3
            });

            mission.defaultLoadout = mission.availableSkills.Take(loadoutSize).ToList();
        }

        private static List<ScriptedEventDefinition> GenerateScriptedEvents(int objectiveCount, DifficultyLevel difficulty)
        {
            List<ScriptedEventDefinition> events = new List<ScriptedEventDefinition>();

            // Always add mission start event
            events.Add(new ScriptedEventDefinition
            {
                eventID = "mission_start",
                trigger = EventTrigger.OnMissionStart,
                eventType = EventType.ShowDialog,
                dialogText = "Mission started. Stay focused on the rhythm."
            });

            // Add random events based on difficulty
            int eventCount = difficulty switch
            {
                DifficultyLevel.Tutorial => 2,
                DifficultyLevel.Easy => Random.Range(2, 4),
                DifficultyLevel.Normal => Random.Range(3, 6),
                DifficultyLevel.Hard => Random.Range(5, 8),
                _ => 3
            };

            for (int i = 0; i < eventCount; i++)
            {
                EventTrigger trigger = (EventTrigger)Random.Range(0, 5);
                EventType eventType = (EventType)Random.Range(0, 6);

                ScriptedEventDefinition evt = new ScriptedEventDefinition
                {
                    eventID = $"event_{i + 1}",
                    trigger = trigger,
                    eventType = eventType
                };

                // Configure based on trigger type
                switch (trigger)
                {
                    case EventTrigger.OnObjectiveComplete:
                        evt.triggerObjectiveIndex = Random.Range(0, objectiveCount);
                        break;
                    case EventTrigger.OnTimer:
                        evt.triggerDelay = Random.Range(30f, 180f);
                        break;
                    case EventTrigger.OnAlertLevel:
                        evt.triggerDelay = Random.Range(3f, 7f); // Alert level
                        break;
                }

                // Configure based on event type
                switch (eventType)
                {
                    case EventType.ShowDialog:
                        string[] dialogOptions = new string[]
                        {
                            "Stay alert. Guards are nearby.",
                            "Objective updated. Check your mission log.",
                            "Time is running out!",
                            "Well done. Keep moving.",
                            "You've been spotted! Evade quickly!"
                        };
                        evt.dialogText = dialogOptions[Random.Range(0, dialogOptions.Length)];
                        break;
                    case EventType.SpawnGuards:
                        evt.spawnCount = Random.Range(2, 6);
                        evt.spawnPointTag = "GuardSpawn";
                        break;
                }

                events.Add(evt);
            }

            return events;
        }

        private static List<ConstellationSkillData> GenerateRandomUnlockedSkills(DifficultyLevel difficulty)
        {
            List<ConstellationSkillData> unlocked = new List<ConstellationSkillData>();

            int unlockCount = difficulty switch
            {
                DifficultyLevel.Tutorial => 1,
                DifficultyLevel.Easy => Random.Range(1, 2),
                DifficultyLevel.Normal => Random.Range(1, 3),
                DifficultyLevel.Hard => Random.Range(2, 4),
                _ => 1
            };

            string[] skillNames = new string[]
            {
                "CapricornTrap", "OrionsArrow", "Decoy", "GeminiClone",
                "ShadowBlend", "AndromedaVeil", "PegasusDash", "AquariusFlow"
            };

            List<string> shuffled = skillNames.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < unlockCount && i < shuffled.Count; i++)
            {
                ConstellationSkillData skill = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(
                    SKILLS_PATH + shuffled[i] + ".asset"
                );
                if (skill != null) unlocked.Add(skill);
            }

            return unlocked;
        }
#endif
    }
}
