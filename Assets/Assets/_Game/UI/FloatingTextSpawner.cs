using UnityEngine;
using UnityEngine.UI;

namespace SH.UI
{
    [DisallowMultipleComponent]
    public class FloatingTextSpawner : MonoBehaviour
    {
        public static FloatingTextSpawner Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private FloatingText floatingTextPrefab;

        [Header("Defaults")]
        [SerializeField] private Color defaultColor = new Color(1f, 0.95f, 0.2f);
        [SerializeField, Min(0.1f)] private float defaultDuration = 0.75f;

        private Camera cachedCam;
        private RectTransform canvasRect;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureCanvas();
            cachedCam = Camera.main;
        }
        void Start()
        {
            // Asegura que el singleton tiene las referencias correctas
            if (FloatingTextSpawner.Instance != this)
            {
                FloatingTextSpawner.Instance.Configure(targetCanvas, floatingTextPrefab);
            }
        }

        private void EnsureCanvas()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
                if (targetCanvas == null)
                {
                    var go = new GameObject("FloatingTextCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    var canvas = go.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    var scaler = go.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);

                    targetCanvas = canvas;
                }
            }
            canvasRect = targetCanvas.transform as RectTransform;
        }

        public void Configure(Canvas canvas, FloatingText prefab)
        {
            targetCanvas = canvas;
            floatingTextPrefab = prefab;
            canvasRect = targetCanvas.transform as RectTransform;
        }

        public static FloatingText Show(Vector3 worldPos, string text)
        {
            EnsureInstance();
            return Instance.InternalShow(worldPos, text, Instance.defaultColor, Instance.defaultDuration);
        }

        public static FloatingText Show(Vector3 worldPos, string text, Color color, float duration = -1f)
        {
            EnsureInstance();
            float dur = duration > 0f ? duration : Instance.defaultDuration;
            return Instance.InternalShow(worldPos, text, color, dur);
        }

        private static void EnsureInstance()
        {
            if (Instance == null)
            {
                var go = new GameObject("~FloatingTextSpawner");
                Instance = go.AddComponent<FloatingTextSpawner>();
            }
        }

        private FloatingText InternalShow(Vector3 worldPos, string text, Color color, float duration)
        {
            Debug.Log($"[FTSpawner] InternalShow called. prefab={(floatingTextPrefab ? floatingTextPrefab.name : "NULL")} canvas={(targetCanvas ? targetCanvas.name : "NULL")}");
            if (floatingTextPrefab == null)
            {
                Debug.LogError("FloatingTextSpawner: Missing FloatingText prefab reference.");
                return null;
            }

            if (cachedCam == null) cachedCam = Camera.main;

            Vector2 screenPoint = cachedCam != null
                ? (Vector2)cachedCam.WorldToScreenPoint(worldPos)
                : (Vector2)worldPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cachedCam,
                out var localPoint
            );

            var instance = Instantiate(floatingTextPrefab, targetCanvas.transform);
            var rt = instance.transform as RectTransform;
            rt.anchoredPosition = localPoint;

            instance.Play(text, color, duration);
            return instance;
        }
    }
}