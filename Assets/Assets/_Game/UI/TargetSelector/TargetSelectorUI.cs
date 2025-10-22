using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SH.Core;

namespace SH.UI
{
    [AddComponentMenu("UI/TargetSelectorUI")]
    public class TargetSelectorUI : MonoBehaviour
    {
        public TargetSelectorConfig config;
        public RectTransform canvasRect;
        public RectTransform markerPrefab;

        public enum HideMode { SetActive, CanvasGroup }

        [Header("Visibilidad")]
        [SerializeField] HideMode hideMode = HideMode.CanvasGroup; // por defecto CanvasGroup (recomendado)
        [SerializeField] bool startHidden = true;                  // arranca oculto pero activo con CanvasGroup

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;

        [Header("Estado (solo lectura)")]
        [SerializeField] bool isOpen;
        [SerializeField] int selectedIndex;

        public event Action<IActor> OnTargetChosen;
        public event Action OnClosed;

        readonly List<IActor> actors = new List<IActor>();
        readonly List<RectTransform> markers = new List<RectTransform>();
        Camera cam;
        CanvasGroup cg; // usado en CanvasGroup mode

        void Awake()
        {
            if (!canvasRect)
            {
                var canvas = GetComponentInParent<Canvas>();
                canvasRect = canvas ? canvas.GetComponent<RectTransform>() : null;
            }
            cam = Camera.main;

            if (hideMode == HideMode.CanvasGroup)
            {
                cg = GetComponent<CanvasGroup>();
                if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
                if (startHidden) ApplyVisible(false);
                else ApplyVisible(true);
            }
            else
            {
                // En SetActive, respetamos el estado inicial del GO
                // (no lo auto-desactivamos para no cortar Update/input).
            }
        }

        // No forzamos Close() en OnDisable para evitar interacciones inesperadas con SetActive externos.
        void OnDisable() { /* no-op */ }

        public void Open(IEnumerable<IActor> candidates)
        {
            if (hideMode == HideMode.SetActive)
                gameObject.SetActive(true);
            else
                ApplyVisible(true);

            // Reset y construir
            actors.Clear();
            ClearMarkers();

            if (candidates != null) actors.AddRange(candidates);

            BuildMarkers();
            isOpen = markers.Count > 0;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, markers.Count - 1));
            RefreshVisuals();

            if (debugLogs) Debug.Log($"[TargetSelector] Open → {markers.Count} candidatos (idx={selectedIndex})");
        }

        public void Close()
        {
            isOpen = false;
            ClearMarkers();
            actors.Clear();

            if (hideMode == HideMode.SetActive)
                gameObject.SetActive(false);
            else
                ApplyVisible(false);

            OnClosed?.Invoke();
        }

        void ApplyVisible(bool visible)
        {
            if (hideMode == HideMode.CanvasGroup)
            {
                if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
                cg.alpha = visible ? 1f : 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
        }

        public void SetFilter(TargetFilter filter) { if (config) config.filter = filter; }

        void Update()
        {
            if (!isOpen || markers.Count == 0) return;

            UpdateMarkersPositions();

            bool left  = UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(config?.leftKey  ?? KeyCode.None)  || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow);
            bool right = UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(config?.rightKey ?? KeyCode.None)  || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow);
            bool up    = UnityEngine.Input.GetKeyDown(KeyCode.W) || UnityEngine.Input.GetKeyDown(config?.upKey    ?? KeyCode.None)  || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
            bool down  = UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(config?.downKey  ?? KeyCode.None)  || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);

            if (left || up) Move(-1);
            else if (right || down) Move(+1);

            bool confirm = UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(config?.confirmKey ?? KeyCode.None) || UnityEngine.Input.GetKeyDown(KeyCode.Space);
            if (confirm) ConfirmCurrent();

            bool cancel = UnityEngine.Input.GetKeyDown(config?.cancelKey ?? KeyCode.None) || UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            if (cancel) Close();
        }

        void Move(int delta)
        {
            if (markers.Count == 0) return;
            int next = selectedIndex + delta;

            if (config != null && config.wrapNavigation)
                next = (next + markers.Count) % markers.Count;
            else
                next = Mathf.Clamp(next, 0, markers.Count - 1);

            if (next != selectedIndex)
            {
                selectedIndex = next;
                RefreshVisuals();
                if (debugLogs) Debug.Log($"[TargetSelector] Move {delta:+#;-#} → {selectedIndex}");
            }
        }

        void ConfirmCurrent()
        {
            if (selectedIndex < 0 || selectedIndex >= actors.Count) return;
            var chosen = actors[selectedIndex];
            if (debugLogs) Debug.Log($"[TargetSelector] Confirm → {chosen?.Name}");
            OnTargetChosen?.Invoke(chosen);
            Close();
        }

        void BuildMarkers()
        {
            if (!markerPrefab || !canvasRect) return;
            foreach (var a in actors)
            {
                if (a == null || a.Transform == null) continue;
                var m = Instantiate(markerPrefab, canvasRect);
                m.name = $"TargetMarker_{a.Name}";
                markers.Add(m);
            }
            UpdateMarkersPositions();
        }

        void ClearMarkers()
        {
            for (int i = markers.Count - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (markers[i]) DestroyImmediate(markers[i].gameObject);
#else
                if (markers[i]) Destroy(markers[i].gameObject);
#endif
            }
            markers.Clear();
        }

        void UpdateMarkersPositions()
        {
            if (!canvasRect) return;
            for (int i = 0; i < markers.Count; i++)
            {
                var a = actors[i];
                var m = markers[i];
                if (a == null || a.Transform == null || m == null) continue;

                var world = a.Transform.position + Vector3.up * (config ? config.worldOffsetY : 1.5f);
                var screen = cam ? cam.WorldToScreenPoint(world) : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var local);
                m.anchoredPosition = local;
            }
        }

        void RefreshVisuals()
        {
            for (int i = 0; i < markers.Count; i++)
            {
                var m = markers[i];
                if (!m) continue;

                bool sel = (i == selectedIndex);

                float s = sel ? (config ? config.selectedScale : 1.2f) : 1f;
                m.localScale = new Vector3(s, s, 1f);

                var img = m.GetComponent<Image>();
                if (img)
                {
                    var normal = config ? config.normalColor : new Color(1, 1, 1, 0.85f);
                    var selected = config ? config.selectedColor : new Color(1f, 0.9f, 0.4f, 1f);
                    img.color = sel ? selected : normal;
                }
            }
        }
    }
}