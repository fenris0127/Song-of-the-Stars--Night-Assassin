using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace SongOfTheStars.Editor
{
    /// <summary>
    /// Unity Editor utilities for quick game setup
    /// Unity 에디터에서 빠른 게임 설정을 위한 유틸리티
    ///
    /// Provides menu items for common setup tasks
    /// </summary>
    public class GameSetupUtility
    {
        private const string MENU_ROOT = "Song of the Stars/";

        #region Scene Setup
        [MenuItem(MENU_ROOT + "Setup/Create Bootstrap Scene", priority = 1)]
        public static void CreateBootstrapScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Remove default camera (GameBootstrap creates its own)
            var camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                GameObject.DestroyImmediate(camera);
            }

            // Create GameBootstrap object
            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<SongOfTheStars.Core.GameBootstrap>();

            // Create SceneTransitionManager
            GameObject transitionManager = new GameObject("SceneTransitionManager");
            transitionManager.AddComponent<SongOfTheStars.Core.SceneTransitionManager>();

            // Save scene
            string scenePath = "Assets/Scenes/Bootstrap.unity";
            EnsureDirectoryExists(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"✅ Bootstrap scene created at {scenePath}");
            EditorUtility.DisplayDialog("Success", "Bootstrap scene created!\n\nConfigure GameBootstrap settings in the Inspector.", "OK");
        }

        [MenuItem(MENU_ROOT + "Setup/Create Main Menu Scene", priority = 2)]
        public static void CreateMainMenuScene()
        {
            // Create 2D scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create UI Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create simple title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasObj.transform, false);

            var text = titleObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Song of the Stars\nNight Assassin";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 48;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.sizeDelta = new Vector2(800, 200);

            // Create start button
            GameObject buttonObj = new GameObject("StartButton");
            buttonObj.transform.SetParent(canvasObj.transform, false);

            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            var buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.8f);

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
            buttonRect.sizeDelta = new Vector2(300, 80);

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);

            var buttonText = buttonTextObj.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = "Start Tutorial";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 32;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            // Save scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            EnsureDirectoryExists(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"✅ Main Menu scene created at {scenePath}");
            EditorUtility.DisplayDialog("Success", "Main Menu scene created!\n\nAdd button onClick event to load Tutorial scene.", "OK");
        }

        [MenuItem(MENU_ROOT + "Setup/Create Tutorial Scene", priority = 3)]
        public static void CreateTutorialScene()
        {
            // Create 2D scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Setup camera for 2D
            var camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                Camera cam = camera.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            }

            // Create managers container
            GameObject managers = new GameObject("--- GameManagers ---");

            // Create TutorialController
            GameObject tutorialController = new GameObject("TutorialController");
            tutorialController.AddComponent<SongOfTheStars.Tutorial.TutorialController>();

            // Create waypoint
            GameObject waypoint = new GameObject("Tutorial_Waypoint_1");
            waypoint.tag = "Tutorial_Waypoint_1";
            var waypointSprite = waypoint.AddComponent<SpriteRenderer>();
            waypointSprite.color = Color.yellow;
            waypoint.transform.position = new Vector3(3f, 0f, 0f);

            // Create dummy
            GameObject dummy = new GameObject("TutorialDummy");
            dummy.tag = "TutorialDummy";
            var dummySprite = dummy.AddComponent<SpriteRenderer>();
            dummySprite.color = Color.red;
            dummy.transform.position = new Vector3(5f, 0f, 0f);

            // Save scene
            string scenePath = "Assets/Scenes/Tutorial_Courtyard.unity";
            EnsureDirectoryExists(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"✅ Tutorial scene created at {scenePath}");
            EditorUtility.DisplayDialog("Success", "Tutorial scene created!\n\nAdd managers and player according to QUICK_START_GUIDE.md", "OK");
        }
        #endregion

        #region Folder Setup
        [MenuItem(MENU_ROOT + "Setup/Create Folder Structure", priority = 10)]
        public static void CreateFolderStructure()
        {
            string[] folders = new string[]
            {
                "Assets/Scenes",
                "Assets/Data",
                "Assets/Data/Missions",
                "Assets/Data/Skills",
                "Assets/Data/Config",
                "Assets/Prefabs",
                "Assets/Prefabs/VFX",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Skills",
                "Assets/Audio",
                "Assets/Audio/Music",
                "Assets/Audio/SFX",
                "Assets/Sprites",
                "Assets/Sprites/Player",
                "Assets/Sprites/Enemies",
                "Assets/Sprites/Environment",
                "Assets/Sprites/UI",
                "Assets/Sprites/Skills"
            };

            foreach (string folder in folders)
            {
                EnsureDirectoryExists(folder);
            }

            AssetDatabase.Refresh();

            Debug.Log("✅ Folder structure created");
            EditorUtility.DisplayDialog("Success", $"Created {folders.Length} folders for project organization.", "OK");
        }
        #endregion

        #region Tags & Layers Setup
        [MenuItem(MENU_ROOT + "Setup/Configure Tags and Layers", priority = 11)]
        public static void ConfigureTagsAndLayers()
        {
            // Add tags
            string[] requiredTags = new string[]
            {
                "Tutorial_Waypoint_1",
                "TutorialDummy",
                "Guard",
                "Player",
                "Objective",
                "ExtractionPoint"
            };

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            foreach (string tag in requiredTags)
            {
                // Check if tag already exists
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                    if (t.stringValue.Equals(tag))
                    {
                        found = true;
                        break;
                    }
                }

                // Add tag if not found
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                    newTag.stringValue = tag;
                }
            }

            tagManager.ApplyModifiedProperties();

            Debug.Log($"✅ Configured {requiredTags.Length} tags");
            EditorUtility.DisplayDialog("Success", "Tags and layers configured!", "OK");
        }
        #endregion

        #region Quick Actions
        [MenuItem(MENU_ROOT + "Quick Actions/Open Bootstrap Scene")]
        public static void OpenBootstrapScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
        }

        [MenuItem(MENU_ROOT + "Quick Actions/Open Main Menu Scene")]
        public static void OpenMainMenuScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        [MenuItem(MENU_ROOT + "Quick Actions/Open Tutorial Scene")]
        public static void OpenTutorialScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Tutorial_Courtyard.unity");
        }

        [MenuItem(MENU_ROOT + "Quick Actions/Play from Bootstrap")]
        public static void PlayFromBootstrap()
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            EditorApplication.isPlaying = true;
        }
        #endregion

        #region Build Settings
        [MenuItem(MENU_ROOT + "Setup/Add Scenes to Build Settings", priority = 20)]
        public static void AddScenesToBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Tutorial_Courtyard.unity"
            };

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            foreach (string scenePath in scenePaths)
            {
                if (File.Exists(scenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log($"✅ Added {scenes.Count} scenes to Build Settings");
            EditorUtility.DisplayDialog("Success", $"Added {scenes.Count} scenes to Build Settings in correct order.", "OK");
        }
        #endregion

        #region Helpers
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        #endregion

        #region Validation
        [MenuItem(MENU_ROOT + "Validate/Check Project Setup")]
        public static void ValidateProjectSetup()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Project Setup Validation ===\n");

            int passed = 0;
            int failed = 0;

            // Check scenes
            if (File.Exists("Assets/Scenes/Bootstrap.unity"))
            {
                report.AppendLine("✅ Bootstrap scene exists");
                passed++;
            }
            else
            {
                report.AppendLine("❌ Bootstrap scene missing");
                failed++;
            }

            if (File.Exists("Assets/Scenes/MainMenu.unity"))
            {
                report.AppendLine("✅ MainMenu scene exists");
                passed++;
            }
            else
            {
                report.AppendLine("❌ MainMenu scene missing");
                failed++;
            }

            // Check folders
            if (Directory.Exists("Assets/Data"))
            {
                report.AppendLine("✅ Data folder exists");
                passed++;
            }
            else
            {
                report.AppendLine("❌ Data folder missing");
                failed++;
            }

            // Check scripts
            string[] criticalScripts = new string[]
            {
                "Assets/Scripts/Core/Bootstrap/GameBootstrap.cs",
                "Assets/Scripts/Core/SceneManagement/SceneTransitionManager.cs",
                "Assets/Scripts/Tutorial/TutorialController.cs"
            };

            foreach (string script in criticalScripts)
            {
                if (File.Exists(script))
                {
                    report.AppendLine($"✅ {Path.GetFileName(script)} exists");
                    passed++;
                }
                else
                {
                    report.AppendLine($"❌ {Path.GetFileName(script)} missing");
                    failed++;
                }
            }

            report.AppendLine($"\nResults: {passed} passed, {failed} failed");

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Project Validation", report.ToString(), "OK");
        }
        #endregion
    }
}
