using UnityEngine;
using UnityEditor;
using SongOfTheStars.Missions;

namespace SongOfTheStars.Editor
{
    /// <summary>
    /// Custom inspector for MissionData ScriptableObject
    /// MissionData ScriptableObject용 커스텀 인스펙터
    ///
    /// Provides enhanced UI for mission configuration
    /// </summary>
    [CustomEditor(typeof(MissionData))]
    public class MissionDataEditor : UnityEditor.Editor
    {
        private SerializedProperty missionID;
        private SerializedProperty missionName;
        private SerializedProperty difficulty;
        private SerializedProperty musicBPM;
        private SerializedProperty timeLimit;
        private SerializedProperty maxAlertLevel;
        private SerializedProperty briefingText;

        private SerializedProperty primaryObjectives;
        private SerializedProperty optionalObjectives;
        private SerializedProperty scriptedEvents;
        private SerializedProperty skillLoadout;

        private SerializedProperty experienceReward;
        private SerializedProperty bonusExperiencePerOptional;
        private SerializedProperty unlockedSkills;

        private bool showMissionInfo = true;
        private bool showObjectives = true;
        private bool showEvents = true;
        private bool showRewards = true;

        void OnEnable()
        {
            // Cache serialized properties
            missionID = serializedObject.FindProperty("missionID");
            missionName = serializedObject.FindProperty("missionName");
            difficulty = serializedObject.FindProperty("difficulty");
            musicBPM = serializedObject.FindProperty("musicBPM");
            timeLimit = serializedObject.FindProperty("timeLimit");
            maxAlertLevel = serializedObject.FindProperty("maxAlertLevel");
            briefingText = serializedObject.FindProperty("briefingText");

            primaryObjectives = serializedObject.FindProperty("primaryObjectives");
            optionalObjectives = serializedObject.FindProperty("optionalObjectives");
            scriptedEvents = serializedObject.FindProperty("scriptedEvents");
            skillLoadout = serializedObject.FindProperty("skillLoadout");

            experienceReward = serializedObject.FindProperty("experienceReward");
            bonusExperiencePerOptional = serializedObject.FindProperty("bonusExperiencePerOptional");
            unlockedSkills = serializedObject.FindProperty("unlockedSkills");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 16;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Mission Configuration", headerStyle);
            EditorGUILayout.Space();

            // Mission Info Section
            showMissionInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showMissionInfo, "Mission Info");
            if (showMissionInfo)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(missionID);
                EditorGUILayout.PropertyField(missionName);
                EditorGUILayout.PropertyField(difficulty);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Audio & Timing", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(musicBPM);
                EditorGUILayout.PropertyField(timeLimit, new GUIContent("Time Limit (seconds, 0 = none)"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Stealth Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(maxAlertLevel);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Briefing", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(briefingText, GUIContent.none, GUILayout.Height(60));

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Objectives Section
            showObjectives = EditorGUILayout.BeginFoldoutHeaderGroup(showObjectives, "Objectives");
            if (showObjectives)
            {
                EditorGUI.indentLevel++;

                // Primary Objectives
                EditorGUILayout.LabelField("Primary Objectives", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Primary objectives must be completed to finish the mission.", MessageType.Info);
                EditorGUILayout.PropertyField(primaryObjectives, true);

                EditorGUILayout.Space(10);

                // Optional Objectives
                EditorGUILayout.LabelField("Optional Objectives", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Optional objectives provide bonus rewards and replay value.", MessageType.Info);
                EditorGUILayout.PropertyField(optionalObjectives, true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Events Section
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Scripted Events & Loadout");
            if (showEvents)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Scripted Events", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Events trigger during mission based on conditions.", MessageType.Info);
                EditorGUILayout.PropertyField(scriptedEvents, true);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Skill Loadout", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Assign skills to slots 0-3 for this mission.", MessageType.Info);
                EditorGUILayout.PropertyField(skillLoadout, true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Rewards Section
            showRewards = EditorGUILayout.BeginFoldoutHeaderGroup(showRewards, "Rewards");
            if (showRewards)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Experience", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(experienceReward);
                EditorGUILayout.PropertyField(bonusExperiencePerOptional);

                // Calculate total possible XP
                int totalXP = experienceReward.intValue;
                if (optionalObjectives.arraySize > 0)
                {
                    totalXP += optionalObjectives.arraySize * bonusExperiencePerOptional.intValue;
                }
                EditorGUILayout.HelpBox($"Total possible XP: {totalXP} ({experienceReward.intValue} base + {optionalObjectives.arraySize} × {bonusExperiencePerOptional.intValue} optional)", MessageType.None);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Unlocked Skills", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(unlockedSkills, true);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // Quick Stats Summary
            if (GUILayout.Button("Show Mission Summary", GUILayout.Height(30)))
            {
                ShowMissionSummary();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowMissionSummary()
        {
            MissionData mission = (MissionData)target;

            string summary = $"=== {mission.missionName} ===\n\n";
            summary += $"ID: {mission.missionID}\n";
            summary += $"Difficulty: {mission.difficulty}\n";
            summary += $"BPM: {mission.musicBPM}\n";
            summary += $"Time Limit: {(mission.timeLimit > 0 ? mission.timeLimit + "s" : "None")}\n";
            summary += $"Max Alert: {mission.maxAlertLevel}\n\n";

            summary += $"Primary Objectives: {mission.primaryObjectives.Count}\n";
            summary += $"Optional Objectives: {mission.optionalObjectives.Count}\n";
            summary += $"Scripted Events: {mission.scriptedEvents.Count}\n\n";

            int totalXP = mission.experienceReward;
            if (mission.optionalObjectives.Count > 0)
            {
                totalXP += mission.optionalObjectives.Count * mission.bonusExperiencePerOptional;
            }
            summary += $"Base XP: {mission.experienceReward}\n";
            summary += $"Max XP: {totalXP}\n";
            summary += $"Unlocked Skills: {mission.unlockedSkills.Count}\n\n";

            summary += "Briefing:\n";
            summary += mission.briefingText;

            Debug.Log(summary);
            EditorUtility.DisplayDialog("Mission Summary", summary, "OK");
        }
    }
}
