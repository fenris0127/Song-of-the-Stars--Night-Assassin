using UnityEngine;
using UnityEditor;
using System.IO;

namespace SongOfTheStars.Editor
{
    /// <summary>
    /// One-click content generation for initial project setup
    /// 초기 프로젝트 설정을 위한 원클릭 콘텐츠 생성
    ///
    /// Creates all skills, missions, and placeholder assets
    /// </summary>
    public class ContentInitializer : EditorWindow
    {
        private bool _skillsGenerated = false;
        private bool _missionsGenerated = false;
        private bool _iconsGenerated = false;
        private bool _audioGenerated = false;

        private static readonly string SKILLS_PATH = "Assets/Data/Skills/";
        private static readonly string MISSIONS_PATH = "Assets/Data/Missions/";
        private static readonly string ICONS_PATH = "Assets/Art/Icons/Skills/";
        private static readonly string AUDIO_PATH = "Assets/Audio/";

        [MenuItem("Song of the Stars/Content/Initialize All Content", priority = 0)]
        public static void ShowWindow()
        {
            ContentInitializer window = GetWindow<ContentInitializer>("Content Initializer");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Content Initialization", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This will generate all initial game content:\n\n" +
                "• 8 Constellation Skills\n" +
                "• 3 Missions (Tutorial + 2 missions)\n" +
                "• Placeholder icon textures\n" +
                "• Audio file structure\n\n" +
                "Total time: ~30 seconds",
                MessageType.Info
            );

            GUILayout.Space(20);

            // Status indicators
            GUILayout.BeginVertical("box");
            GUILayout.Label("Generation Status:", EditorStyles.boldLabel);

            DrawStatusLine("Skills (8 assets)", _skillsGenerated);
            DrawStatusLine("Missions (3 assets)", _missionsGenerated);
            DrawStatusLine("Placeholder Icons", _iconsGenerated);
            DrawStatusLine("Audio Structure", _audioGenerated);

            GUILayout.EndVertical();

            GUILayout.Space(20);

            // Generate buttons
            GUI.enabled = !_skillsGenerated;
            if (GUILayout.Button("1. Generate Skills", GUILayout.Height(40)))
            {
                GenerateSkills();
            }
            GUI.enabled = true;

            GUI.enabled = !_missionsGenerated;
            if (GUILayout.Button("2. Generate Missions", GUILayout.Height(40)))
            {
                GenerateMissions();
            }
            GUI.enabled = true;

            GUI.enabled = !_iconsGenerated;
            if (GUILayout.Button("3. Generate Placeholder Icons", GUILayout.Height(40)))
            {
                GeneratePlaceholderIcons();
            }
            GUI.enabled = true;

            GUI.enabled = !_audioGenerated;
            if (GUILayout.Button("4. Setup Audio Structure", GUILayout.Height(40)))
            {
                SetupAudioStructure();
            }
            GUI.enabled = true;

            GUILayout.Space(20);

            // All at once button
            bool allGenerated = _skillsGenerated && _missionsGenerated && _iconsGenerated && _audioGenerated;
            GUI.enabled = !allGenerated;

            if (GUILayout.Button("🚀 GENERATE ALL CONTENT", GUILayout.Height(60)))
            {
                GenerateAllContent();
            }
            GUI.enabled = true;

            if (allGenerated)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("✅ All content generated successfully!", MessageType.Info);

                if (GUILayout.Button("Reset Status (for re-generation)"))
                {
                    ResetStatus();
                }
            }
        }

        private void DrawStatusLine(string label, bool isComplete)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(isComplete ? "✅" : "⬜", GUILayout.Width(30));
            GUILayout.Label(label);
            GUILayout.EndHorizontal();
        }

        private void GenerateAllContent()
        {
            if (EditorUtility.DisplayDialog(
                "Generate All Content",
                "This will generate all skills, missions, icons, and audio structure.\n\n" +
                "This may take 30-60 seconds. Continue?",
                "Generate",
                "Cancel"))
            {
                EditorUtility.DisplayProgressBar("Generating Content", "Starting...", 0f);

                try
                {
                    GenerateSkills();
                    EditorUtility.DisplayProgressBar("Generating Content", "Skills complete...", 0.25f);

                    GenerateMissions();
                    EditorUtility.DisplayProgressBar("Generating Content", "Missions complete...", 0.5f);

                    GeneratePlaceholderIcons();
                    EditorUtility.DisplayProgressBar("Generating Content", "Icons complete...", 0.75f);

                    SetupAudioStructure();
                    EditorUtility.DisplayProgressBar("Generating Content", "Audio structure complete...", 1f);

                    EditorUtility.ClearProgressBar();

                    EditorUtility.DisplayDialog(
                        "Success!",
                        "All content generated successfully!\n\n" +
                        "• 8 Skills created\n" +
                        "• 3 Missions created\n" +
                        "• Placeholder icons created\n" +
                        "• Audio folders ready\n\n" +
                        "Check the Console for details.",
                        "OK"
                    );
                }
                catch (System.Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Error", $"Content generation failed:\n\n{e.Message}", "OK");
                    Debug.LogError($"Content generation error: {e}");
                }
            }
        }

        private void GenerateSkills()
        {
            Debug.Log("🎯 Generating Skills...");

            // Ensure directory exists
            if (!Directory.Exists(SKILLS_PATH))
            {
                Directory.CreateDirectory(SKILLS_PATH);
            }

            // Call the SkillDataPopulator
            int created = Data.SkillDataPopulator.PopulateAllSkillData();

            _skillsGenerated = true;
            Debug.Log($"✅ {created} skills generated!");

            AssetDatabase.Refresh();
        }

        private void GenerateMissions()
        {
            Debug.Log("🗺️ Generating Missions...");

            // Ensure directory exists
            if (!Directory.Exists(MISSIONS_PATH))
            {
                Directory.CreateDirectory(MISSIONS_PATH);
            }

            // Call the MissionDataPopulator
            int created = Data.MissionDataPopulator.PopulateAllMissionData();

            _missionsGenerated = true;
            Debug.Log($"✅ {created} missions generated!");

            AssetDatabase.Refresh();
        }

        private void GeneratePlaceholderIcons()
        {
            Debug.Log("🎨 Generating Placeholder Icons...");

            // Ensure directory exists
            if (!Directory.Exists(ICONS_PATH))
            {
                Directory.CreateDirectory(ICONS_PATH);
            }

            // Create simple colored textures as placeholders
            string[] skillNames = new string[]
            {
                "CapricornTrap",
                "OrionsArrow",
                "LeoDecoy",
                "GeminiClone",
                "ShadowBlend",
                "AndromedaVeil",
                "PegasusDash",
                "AquariusFlow"
            };

            Color[] skillColors = new Color[]
            {
                new Color(0.8f, 0.6f, 0.2f), // Gold - Trap
                new Color(0.2f, 0.6f, 1.0f), // Blue - Arrow
                new Color(1.0f, 0.7f, 0.2f), // Orange - Decoy
                new Color(0.7f, 0.4f, 0.9f), // Purple - Clone
                new Color(0.2f, 0.2f, 0.3f), // Dark - Blend
                new Color(0.4f, 0.8f, 1.0f), // Cyan - Veil
                new Color(1.0f, 0.3f, 0.3f), // Red - Dash
                new Color(0.3f, 0.8f, 0.8f)  // Teal - Flow
            };

            int iconSize = 128;

            for (int i = 0; i < skillNames.Length; i++)
            {
                Texture2D icon = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
                Color color = skillColors[i];

                // Create gradient circle
                for (int y = 0; y < iconSize; y++)
                {
                    for (int x = 0; x < iconSize; x++)
                    {
                        float dx = x - iconSize / 2f;
                        float dy = y - iconSize / 2f;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float normalizedDistance = distance / (iconSize / 2f);

                        if (normalizedDistance <= 1f)
                        {
                            float gradient = 1f - normalizedDistance;
                            Color pixelColor = color * gradient;
                            pixelColor.a = gradient;
                            icon.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            icon.SetPixel(x, y, Color.clear);
                        }
                    }
                }

                icon.Apply();

                // Save as PNG
                byte[] pngData = icon.EncodeToPNG();
                string path = ICONS_PATH + skillNames[i] + "_Icon.png";
                File.WriteAllBytes(path, pngData);

                Debug.Log($"  Created icon: {skillNames[i]}_Icon.png");
            }

            _iconsGenerated = true;
            Debug.Log($"✅ {skillNames.Length} placeholder icons generated!");

            AssetDatabase.Refresh();

            // Set import settings for the icons
            foreach (string skillName in skillNames)
            {
                string path = ICONS_PATH + skillName + "_Icon.png";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 256;
                    importer.SaveAndReimport();
                }
            }
        }

        private void SetupAudioStructure()
        {
            Debug.Log("🎵 Setting up Audio Structure...");

            // Create audio folder structure
            string[] audioFolders = new string[]
            {
                "Assets/Audio/Music",
                "Assets/Audio/SFX/Skills",
                "Assets/Audio/SFX/Combat",
                "Assets/Audio/SFX/UI",
                "Assets/Audio/SFX/Ambient",
                "Assets/Audio/SFX/Footsteps",
                "Assets/Audio/SFX/Environmental"
            };

            foreach (string folder in audioFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Debug.Log($"  Created folder: {folder}");
                }
            }

            // Create README files in each folder
            CreateAudioReadme("Assets/Audio/Music",
                "Music Tracks\n\n" +
                "Place mission music tracks here.\n" +
                "Format: .mp3, .ogg, or .wav\n" +
                "Recommended: 120-140 BPM for rhythm gameplay\n\n" +
                "Required tracks:\n" +
                "- Tutorial_Music.ogg (100 BPM)\n" +
                "- Mission01_Music.ogg (120 BPM)\n" +
                "- Mission02_Music.ogg (130 BPM)\n" +
                "- MainMenu_Music.ogg\n");

            CreateAudioReadme("Assets/Audio/SFX/Skills",
                "Skill Sound Effects\n\n" +
                "Place skill activation sounds here.\n\n" +
                "Required sounds (one per skill):\n" +
                "- CapricornTrap_Activate.ogg\n" +
                "- OrionsArrow_Activate.ogg\n" +
                "- LeoDecoy_Activate.ogg\n" +
                "- GeminiClone_Activate.ogg\n" +
                "- ShadowBlend_Activate.ogg\n" +
                "- AndromedaVeil_Activate.ogg\n" +
                "- PegasusDash_Activate.ogg\n" +
                "- AquariusFlow_Activate.ogg\n");

            CreateAudioReadme("Assets/Audio/SFX/Combat",
                "Combat Sound Effects\n\n" +
                "Required sounds:\n" +
                "- Assassination_Success.ogg\n" +
                "- Detection_Alert.ogg\n" +
                "- Footstep_01.ogg ... Footstep_04.ogg\n");

            CreateAudioReadme("Assets/Audio/SFX/UI",
                "UI Sound Effects\n\n" +
                "Required sounds:\n" +
                "- Button_Click.ogg\n" +
                "- Button_Hover.ogg\n" +
                "- Achievement_Unlock.ogg\n" +
                "- Mission_Complete.ogg\n" +
                "- Mission_Failed.ogg\n");

            _audioGenerated = true;
            Debug.Log($"✅ Audio structure created!");

            AssetDatabase.Refresh();
        }

        private void CreateAudioReadme(string folder, string content)
        {
            string readmePath = folder + "/README.txt";
            File.WriteAllText(readmePath, content);
        }

        private void ResetStatus()
        {
            _skillsGenerated = false;
            _missionsGenerated = false;
            _iconsGenerated = false;
            _audioGenerated = false;

            Debug.Log("Status reset. You can now re-generate content if needed.");
        }

        [MenuItem("Song of the Stars/Content/1. Generate Skills Only", priority = 1)]
        public static void GenerateSkillsMenuItem()
        {
            if (!Directory.Exists(SKILLS_PATH))
            {
                Directory.CreateDirectory(SKILLS_PATH);
            }

            int created = Data.SkillDataPopulator.PopulateAllSkillData();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Skills Generated",
                $"Successfully created {created} skill assets!\n\n" +
                "Check Assets/Data/Skills/",
                "OK"
            );
        }

        [MenuItem("Song of the Stars/Content/2. Generate Missions Only", priority = 2)]
        public static void GenerateMissionsMenuItem()
        {
            if (!Directory.Exists(MISSIONS_PATH))
            {
                Directory.CreateDirectory(MISSIONS_PATH);
            }

            int created = Data.MissionDataPopulator.PopulateAllMissionData();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Missions Generated",
                $"Successfully created {created} mission assets!\n\n" +
                "Check Assets/Data/Missions/",
                "OK"
            );
        }
    }
}
