using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

namespace SongOfTheStars.Core
{
    /// <summary>
    /// Manages scene transitions with fade effects
    /// 페이드 효과와 함께 씬 전환 관리
    ///
    /// Singleton that persists between scenes
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        #region Singleton
        public static SceneTransitionManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupCanvas();
        }
        #endregion

        [Header("▶ Transition Settings")]
        [Tooltip("Duration of fade in/out in seconds")]
        [Range(0.1f, 2f)]
        public float fadeDuration = 0.5f;

        [Tooltip("Color of fade overlay")]
        public Color fadeColor = Color.black;

        [Header("▶ Loading Screen")]
        [Tooltip("Show loading screen during transition?")]
        public bool showLoadingScreen = true;

        [Tooltip("Minimum time to show loading screen")]
        public float minimumLoadingTime = 1f;

        #region Private State
        private Canvas _canvas;
        private Image _fadeImage;
        private GameObject _loadingPanel;
        private Text _loadingText;
        private bool _isTransitioning = false;
        #endregion

        #region Setup
        private void SetupCanvas()
        {
            // Create canvas for fade overlay
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1000; // Always on top

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create fade image
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(_canvas.transform, false);

            _fadeImage = fadeObj.AddComponent<Image>();
            _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            // Stretch to fill screen
            RectTransform rect = _fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // Create loading panel (optional)
            if (showLoadingScreen)
            {
                CreateLoadingPanel();
            }

            // Start with fade cleared
            _fadeImage.gameObject.SetActive(false);
        }

        private void CreateLoadingPanel()
        {
            // Create loading panel
            _loadingPanel = new GameObject("LoadingPanel");
            _loadingPanel.transform.SetParent(_canvas.transform, false);

            Image panelImage = _loadingPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);

            RectTransform panelRect = _loadingPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Create loading text
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(_loadingPanel.transform, false);

            _loadingText = textObj.AddComponent<Text>();
            _loadingText.text = "Loading...";
            _loadingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _loadingText.fontSize = 36;
            _loadingText.alignment = TextAnchor.MiddleCenter;
            _loadingText.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            _loadingPanel.SetActive(false);
        }
        #endregion

        #region Scene Loading

        /// <summary>
        /// Loads a scene with fade transition
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Scene transition already in progress!");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Loads a mission scene with proper setup
        /// </summary>
        public void LoadMission(string missionSceneName, SongOfTheStars.Missions.MissionData missionData)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Scene transition already in progress!");
                return;
            }

            StartCoroutine(LoadMissionCoroutine(missionSceneName, missionData));
        }

        /// <summary>
        /// Returns to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            LoadScene("MainMenu");
        }

        /// <summary>
        /// Reloads current scene
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        #endregion

        #region Coroutines

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _isTransitioning = true;

            // Fade out
            yield return StartCoroutine(FadeOut());

            // Show loading screen
            if (showLoadingScreen && _loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }

            float startTime = Time.time;

            // Load scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait for loading to complete
            while (!asyncLoad.isDone)
            {
                // Update loading progress (0 to 0.9 is loading, 0.9 is ready to activate)
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (_loadingText != null)
                {
                    _loadingText.text = $"Loading... {progress * 100f:F0}%";
                }

                // When loading is done (0.9), activate the scene
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time (for visual consistency)
                    float elapsed = Time.time - startTime;
                    if (elapsed < minimumLoadingTime)
                    {
                        yield return new WaitForSeconds(minimumLoadingTime - elapsed);
                    }

                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading screen
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }

            // Fade in
            yield return StartCoroutine(FadeIn());

            _isTransitioning = false;

            Debug.Log($"Scene loaded: {sceneName}");
        }

        private IEnumerator LoadMissionCoroutine(string missionSceneName, SongOfTheStars.Missions.MissionData missionData)
        {
            _isTransitioning = true;

            // Update loading text for mission
            if (_loadingText != null)
            {
                _loadingText.text = $"Loading: {missionData.missionName}";
            }

            // Fade out
            yield return StartCoroutine(FadeOut());

            // Show loading screen
            if (showLoadingScreen && _loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }

            float startTime = Time.time;

            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(missionSceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (_loadingText != null)
                {
                    _loadingText.text = $"Loading: {missionData.missionName}\n{progress * 100f:F0}%";
                }

                if (asyncLoad.progress >= 0.9f)
                {
                    float elapsed = Time.time - startTime;
                    if (elapsed < minimumLoadingTime)
                    {
                        yield return new WaitForSeconds(minimumLoadingTime - elapsed);
                    }

                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Scene loaded - initialize mission
            // EnhancedMissionManager should be in the scene and will auto-start
            // We just need to verify it's set up correctly
            yield return new WaitForSeconds(0.1f); // Wait for scene initialization

            var missionManager = FindObjectOfType<EnhancedMissionManager>();
            if (missionManager != null && missionManager.currentMission == null)
            {
                // Set mission data if not already set
                missionManager.currentMission = missionData;
            }

            // Hide loading screen
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }

            // Fade in
            yield return StartCoroutine(FadeIn());

            _isTransitioning = false;

            Debug.Log($"Mission loaded: {missionData.missionName}");
        }

        #endregion

        #region Fade Effects

        /// <summary>
        /// Fades screen to black
        /// </summary>
        public IEnumerator FadeOut()
        {
            _fadeImage.gameObject.SetActive(true);

            float elapsed = 0f;
            Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                _fadeImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            _fadeImage.color = endColor;
        }

        /// <summary>
        /// Fades screen from black to clear
        /// </summary>
        public IEnumerator FadeIn()
        {
            float elapsed = 0f;
            Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                _fadeImage.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            _fadeImage.color = endColor;
            _fadeImage.gameObject.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Is a transition currently in progress?
        /// </summary>
        public bool IsTransitioning()
        {
            return _isTransitioning;
        }

        /// <summary>
        /// Manually trigger fade out (without scene load)
        /// </summary>
        public void ManualFadeOut()
        {
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Manually trigger fade in (without scene load)
        /// </summary>
        public void ManualFadeIn()
        {
            StartCoroutine(FadeIn());
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test Fade Out")]
        private void TestFadeOut()
        {
            if (Application.isPlaying)
            {
                ManualFadeOut();
            }
        }

        [ContextMenu("Test Fade In")]
        private void TestFadeIn()
        {
            if (Application.isPlaying)
            {
                ManualFadeIn();
            }
        }

        [ContextMenu("Test Load Scene (MainMenu)")]
        private void TestLoadMainMenu()
        {
            if (Application.isPlaying)
            {
                LoadScene("MainMenu");
            }
        }
#endif

        #endregion
    }
}
