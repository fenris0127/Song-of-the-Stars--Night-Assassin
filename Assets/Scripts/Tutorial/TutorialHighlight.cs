using UnityEngine;
using System.Collections;

namespace SongOfTheStars.Tutorial
{
    /// <summary>
    /// Visual highlight effect for tutorial target objects
    /// 튜토리얼 대상 오브젝트에 대한 시각적 강조 효과
    ///
    /// Adds pulsing glow and arrow indicator to targets
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TutorialHighlight : MonoBehaviour
    {
        [Header("▶ Highlight Settings")]
        public Color highlightColor = Color.yellow;
        public float pulseSpeed = 2f;
        public float minAlpha = 0.3f;
        public float maxAlpha = 0.8f;

        [Header("▶ Arrow Indicator")]
        public bool showArrow = true;
        public GameObject arrowPrefab;
        public float arrowDistance = 2f;
        public Vector2 arrowOffset = new Vector2(0f, 1.5f);

        [Header("▶ Outline")]
        public bool useOutline = true;
        public float outlineWidth = 0.1f;
        public Color outlineColor = Color.white;

        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private bool _isHighlighting = false;
        private GameObject _arrowInstance;
        private Material _highlightMaterial;
        private Material _originalMaterial;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _originalMaterial = _spriteRenderer.material;
        }

        public void StartHighlight()
        {
            if (_isHighlighting) return;

            _isHighlighting = true;

            // Create outline effect
            if (useOutline)
            {
                CreateOutline();
            }

            // Start pulsing
            StartCoroutine(PulseRoutine());

            // Show arrow
            if (showArrow)
            {
                CreateArrow();
            }

            Debug.Log($"Highlighting tutorial target: {gameObject.name}");
        }

        public void StopHighlight()
        {
            if (!_isHighlighting) return;

            _isHighlighting = false;

            StopAllCoroutines();

            // Restore original color
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
                _spriteRenderer.material = _originalMaterial;
            }

            // Remove arrow
            if (_arrowInstance != null)
            {
                Destroy(_arrowInstance);
            }

            Debug.Log($"Stopped highlighting: {gameObject.name}");
        }

        private IEnumerator PulseRoutine()
        {
            while (_isHighlighting)
            {
                // Pulse alpha
                float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

                Color pulseColor = new Color(
                    highlightColor.r,
                    highlightColor.g,
                    highlightColor.b,
                    alpha
                );

                // Blend with original color
                _spriteRenderer.color = Color.Lerp(_originalColor, pulseColor, 0.5f);

                yield return null;
            }
        }

        private void CreateOutline()
        {
            // Simple outline using sprite duplication
            // For better results, use a shader with outline support
            GameObject outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one * (1f + outlineWidth);

            SpriteRenderer outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = _spriteRenderer.sprite;
            outlineRenderer.color = outlineColor;
            outlineRenderer.sortingLayerName = _spriteRenderer.sortingLayerName;
            outlineRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
        }

        private void CreateArrow()
        {
            // Create arrow indicator above target
            if (arrowPrefab != null)
            {
                _arrowInstance = Instantiate(arrowPrefab);
            }
            else
            {
                // Create simple arrow from scratch
                _arrowInstance = CreateDefaultArrow();
            }

            _arrowInstance.transform.SetParent(transform);
            _arrowInstance.transform.localPosition = new Vector3(arrowOffset.x, arrowOffset.y, 0f);

            // Start bobbing animation
            StartCoroutine(BobArrow());
        }

        private GameObject CreateDefaultArrow()
        {
            GameObject arrow = new GameObject("TutorialArrow");

            // Create sprite renderer
            SpriteRenderer renderer = arrow.AddComponent<SpriteRenderer>();
            renderer.color = highlightColor;
            renderer.sortingLayerName = "UI"; // Render on top
            renderer.sortingOrder = 100;

            // Create simple triangle sprite
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            // Draw triangle pointing down
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    // Triangle shape
                    bool isTriangle = (x >= 16 - y / 2 && x <= 16 + y / 2 && y < 24);
                    pixels[y * 32 + x] = isTriangle ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0f)
            );

            renderer.sprite = sprite;

            return arrow;
        }

        private IEnumerator BobArrow()
        {
            if (_arrowInstance == null) yield break;

            Vector3 startPos = _arrowInstance.transform.localPosition;
            float bobAmount = 0.3f;

            while (_isHighlighting && _arrowInstance != null)
            {
                float offset = Mathf.Sin(Time.time * 3f) * bobAmount;
                _arrowInstance.transform.localPosition = startPos + new Vector3(0f, offset, 0f);
                yield return null;
            }
        }

        void OnDestroy()
        {
            StopHighlight();
        }

        void OnDisable()
        {
            StopHighlight();
        }
    }
}
