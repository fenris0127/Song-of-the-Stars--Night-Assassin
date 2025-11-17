using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace SongOfTheStars.UI
{
    /// <summary>
    /// Mission objectives display overlay
    /// 미션 목표 표시 오버레이
    ///
    /// Features:
    /// - Primary and optional objectives
    /// - Progress tracking
    /// - Completion checkmarks
    /// - Minimizable to corner
    /// </summary>
    public class MissionObjectivesUI : MonoBehaviour
    {
        #region Inspector References
        [Header("▶ UI Elements")]
        [Tooltip("Main panel containing all objectives")]
        public RectTransform objectivesPanel;

        [Tooltip("Container for objective list items")]
        public RectTransform objectivesContainer;

        [Tooltip("Mission title text")]
        public TextMeshProUGUI missionTitleText;

        [Tooltip("Toggle button to minimize/maximize panel")]
        public Button toggleButton;

        [Tooltip("Icon for toggle button (changes based on state)")]
        public Image toggleButtonIcon;

        [Header("▶ Prefabs")]
        [Tooltip("Prefab for objective list item")]
        public GameObject objectiveItemPrefab;

        [Header("▶ Visual Settings")]
        [Tooltip("Color for completed objectives")]
        public Color completedColor = new Color(0.5f, 1f, 0.5f, 1f); // Light green

        [Tooltip("Color for in-progress objectives")]
        public Color inProgressColor = Color.white;

        [Tooltip("Color for failed objectives")]
        public Color failedColor = new Color(1f, 0.5f, 0.5f, 1f); // Light red

        [Tooltip("Color for optional objectives")]
        public Color optionalColor = new Color(1f, 1f, 0.7f, 1f); // Light yellow

        [Header("▶ Minimization Settings")]
        [Tooltip("Minimized panel size")]
        public Vector2 minimizedSize = new Vector2(50f, 50f);

        [Tooltip("Expanded panel size")]
        public Vector2 expandedSize = new Vector2(400f, 300f);

        [Tooltip("Animation duration for minimize/expand")]
        [Range(0.1f, 1f)]
        public float animationDuration = 0.3f;

        [Header("▶ Icons")]
        public Sprite checkmarkIcon;
        public Sprite crossIcon;
        public Sprite inProgressIcon;
        public Sprite expandIcon;
        public Sprite collapseIcon;
        #endregion

        #region Private State
        private bool _isMinimized = false;
        private List<ObjectiveItem> _activeObjectives = new List<ObjectiveItem>();
        private MissionManager _missionManager;

        private class ObjectiveItem
        {
            public GameObject gameObject;
            public TextMeshProUGUI textComponent;
            public Image iconComponent;
            public Image backgroundComponent;
            public ObjectiveData data;
        }

        public class ObjectiveData
        {
            public string description;
            public bool isOptional;
            public bool isCompleted;
            public bool isFailed;
            public int currentProgress;
            public int targetProgress;
        }
        #endregion

        #region Initialization
        void Start()
        {
            _missionManager = GameServices.MissionManager;

            if (objectivesPanel == null)
            {
                Debug.LogError("MissionObjectivesUI: Objectives panel not assigned!");
                enabled = false;
                return;
            }

            if (objectiveItemPrefab == null)
            {
                Debug.LogError("MissionObjectivesUI: Objective item prefab not assigned!");
                enabled = false;
                return;
            }

            SetupToggleButton();
            InitializeObjectives();
        }

        private void SetupToggleButton()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleMinimize);
                UpdateToggleButtonIcon();
            }
        }

        private void InitializeObjectives()
        {
            // Clear any existing objectives
            ClearAllObjectives();

            // Set mission title
            if (missionTitleText != null && _missionManager != null)
            {
                missionTitleText.text = GetMissionTitle();
            }
        }
        #endregion

        #region Objective Management
        /// <summary>
        /// Adds a new objective to the display
        /// 디스플레이에 새로운 목표 추가
        /// </summary>
        public void AddObjective(ObjectiveData objective)
        {
            if (objective == null || objectiveItemPrefab == null || objectivesContainer == null)
                return;

            // Create objective item
            GameObject itemObj = Instantiate(objectiveItemPrefab, objectivesContainer);
            ObjectiveItem item = new ObjectiveItem
            {
                gameObject = itemObj,
                data = objective
            };

            // Get components
            item.textComponent = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            item.iconComponent = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            item.backgroundComponent = itemObj.GetComponent<Image>();

            // Setup visual
            UpdateObjectiveItem(item);

            // Add to list
            _activeObjectives.Add(item);
        }

        /// <summary>
        /// Updates an objective's status
        /// 목표의 상태 업데이트
        /// </summary>
        public void UpdateObjective(int index, ObjectiveData updatedData)
        {
            if (index < 0 || index >= _activeObjectives.Count) return;

            ObjectiveItem item = _activeObjectives[index];
            item.data = updatedData;
            UpdateObjectiveItem(item);
        }

        /// <summary>
        /// Marks an objective as completed
        /// 목표를 완료로 표시
        /// </summary>
        public void CompleteObjective(int index)
        {
            if (index < 0 || index >= _activeObjectives.Count) return;

            ObjectiveItem item = _activeObjectives[index];
            item.data.isCompleted = true;
            UpdateObjectiveItem(item);
        }

        /// <summary>
        /// Marks an objective as failed
        /// 목표를 실패로 표시
        /// </summary>
        public void FailObjective(int index)
        {
            if (index < 0 || index >= _activeObjectives.Count) return;

            ObjectiveItem item = _activeObjectives[index];
            item.data.isFailed = true;
            UpdateObjectiveItem(item);
        }

        /// <summary>
        /// Updates the visual representation of an objective item
        /// </summary>
        private void UpdateObjectiveItem(ObjectiveItem item)
        {
            if (item == null || item.data == null) return;

            // Update text
            if (item.textComponent != null)
            {
                string text = item.data.description;

                // Add progress if tracking
                if (item.data.targetProgress > 1)
                {
                    text += $" ({item.data.currentProgress}/{item.data.targetProgress})";
                }

                // Add optional tag
                if (item.data.isOptional)
                {
                    text = "[Optional] " + text;
                }

                item.textComponent.text = text;

                // Set color based on state
                if (item.data.isCompleted)
                {
                    item.textComponent.color = completedColor;
                }
                else if (item.data.isFailed)
                {
                    item.textComponent.color = failedColor;
                }
                else if (item.data.isOptional)
                {
                    item.textComponent.color = optionalColor;
                }
                else
                {
                    item.textComponent.color = inProgressColor;
                }
            }

            // Update icon
            if (item.iconComponent != null)
            {
                if (item.data.isCompleted && checkmarkIcon != null)
                {
                    item.iconComponent.sprite = checkmarkIcon;
                    item.iconComponent.color = completedColor;
                }
                else if (item.data.isFailed && crossIcon != null)
                {
                    item.iconComponent.sprite = crossIcon;
                    item.iconComponent.color = failedColor;
                }
                else if (inProgressIcon != null)
                {
                    item.iconComponent.sprite = inProgressIcon;
                    item.iconComponent.color = inProgressColor;
                }
            }
        }

        /// <summary>
        /// Removes all objectives
        /// </summary>
        public void ClearAllObjectives()
        {
            foreach (var item in _activeObjectives)
            {
                if (item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _activeObjectives.Clear();
        }
        #endregion

        #region Minimize/Maximize
        /// <summary>
        /// Toggles between minimized and expanded state
        /// 최소화 및 확장 상태 전환
        /// </summary>
        public void ToggleMinimize()
        {
            _isMinimized = !_isMinimized;
            StartCoroutine(AnimatePanelSize());
            UpdateToggleButtonIcon();
        }

        private System.Collections.IEnumerator AnimatePanelSize()
        {
            if (objectivesPanel == null) yield break;

            Vector2 startSize = objectivesPanel.sizeDelta;
            Vector2 targetSize = _isMinimized ? minimizedSize : expandedSize;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                // Ease out curve
                t = 1f - Mathf.Pow(1f - t, 3f);

                objectivesPanel.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
                yield return null;
            }

            objectivesPanel.sizeDelta = targetSize;

            // Hide/show objectives container when minimized
            if (objectivesContainer != null)
            {
                objectivesContainer.gameObject.SetActive(!_isMinimized);
            }

            if (missionTitleText != null)
            {
                missionTitleText.gameObject.SetActive(!_isMinimized);
            }
        }

        private void UpdateToggleButtonIcon()
        {
            if (toggleButtonIcon == null) return;

            if (_isMinimized && expandIcon != null)
            {
                toggleButtonIcon.sprite = expandIcon;
            }
            else if (!_isMinimized && collapseIcon != null)
            {
                toggleButtonIcon.sprite = collapseIcon;
            }
        }
        #endregion

        #region Helper Methods
        private string GetMissionTitle()
        {
            // TODO: Get from MissionManager when implemented
            return "Mission Objectives";
        }
        #endregion

        #region Public API
        /// <summary>
        /// Example: Add a primary objective
        /// </summary>
        public void AddPrimaryObjective(string description, int targetProgress = 1)
        {
            AddObjective(new ObjectiveData
            {
                description = description,
                isOptional = false,
                isCompleted = false,
                isFailed = false,
                currentProgress = 0,
                targetProgress = targetProgress
            });
        }

        /// <summary>
        /// Example: Add an optional objective
        /// </summary>
        public void AddOptionalObjective(string description, int targetProgress = 1)
        {
            AddObjective(new ObjectiveData
            {
                description = description,
                isOptional = true,
                isCompleted = false,
                isFailed = false,
                currentProgress = 0,
                targetProgress = targetProgress
            });
        }

        /// <summary>
        /// Updates progress for a specific objective
        /// </summary>
        public void UpdateObjectiveProgress(int index, int currentProgress)
        {
            if (index < 0 || index >= _activeObjectives.Count) return;

            ObjectiveItem item = _activeObjectives[index];
            item.data.currentProgress = currentProgress;

            // Auto-complete if progress reached target
            if (currentProgress >= item.data.targetProgress)
            {
                item.data.isCompleted = true;
            }

            UpdateObjectiveItem(item);
        }

        /// <summary>
        /// Gets the number of completed objectives
        /// </summary>
        public int GetCompletedCount()
        {
            int count = 0;
            foreach (var item in _activeObjectives)
            {
                if (item.data.isCompleted)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Gets the total number of objectives
        /// </summary>
        public int GetTotalCount()
        {
            return _activeObjectives.Count;
        }
        #endregion

        #region Debug
        #if UNITY_EDITOR
        [ContextMenu("Test: Add Primary Objective")]
        private void TestAddPrimaryObjective()
        {
            if (Application.isPlaying)
            {
                AddPrimaryObjective("Eliminate the target");
            }
        }

        [ContextMenu("Test: Add Optional Objective")]
        private void TestAddOptionalObjective()
        {
            if (Application.isPlaying)
            {
                AddOptionalObjective("Complete without detection", 1);
            }
        }

        [ContextMenu("Test: Complete First Objective")]
        private void TestCompleteObjective()
        {
            if (Application.isPlaying && _activeObjectives.Count > 0)
            {
                CompleteObjective(0);
            }
        }

        [ContextMenu("Test: Toggle Minimize")]
        private void TestToggleMinimize()
        {
            if (Application.isPlaying)
            {
                ToggleMinimize();
            }
        }
        #endif
        #endregion

        void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(ToggleMinimize);
            }
        }
    }
}
