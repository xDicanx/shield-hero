using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SH.Core;

namespace SH.UI
{
    [AddComponentMenu("UI/CommandMenuUI")]
    public class CommandMenuUI : MonoBehaviour
    {
        [Header("Config y Prefabs")]
        public CommandMenuConfig config;
        public RectTransform contentRoot;   // contenedor (Horizontal/Vertical Layout Group)
        public Button buttonPrefab;         // botón con Image (icon) + TMP_Text (label)

        [Header("Opciones")]
        public bool autoBuildOnEnable = true;
        public bool openOnEnable = true;

        [Header("Visibilidad")]
        [SerializeField] bool manageRootVisibility = true; // si true, Open/Close activan/desactivan este GO

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;

        public event Action<ActionType> OnCommandSelected;

        readonly List<Button> buttons = new List<Button>();
        int selectedIndex = 0;
        bool isOpen = false;

        void OnEnable()
        {
            if (autoBuildOnEnable) Rebuild();
            if (openOnEnable) Open();
        }

        void OnDisable()
        {
            // No destruyas botones aquí si esperas reusar entre activaciones
            isOpen = false;
        }

        public void Rebuild()
        {
            ClearChildren(contentRoot);
            buttons.Clear();
            if (!config || !buttonPrefab || !contentRoot) return;

            foreach (var e in config.entries)
            {
                if (!e.enabled) continue;
                var btn = Instantiate(buttonPrefab, contentRoot);
                btn.name = $"Cmd_{e.label}";
                SetupButtonVisuals(btn, e);
                var action = e.action;
                btn.onClick.AddListener(() => Select(action));
                buttons.Add(btn);
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, buttons.Count - 1));
            FocusSelected();
        }

        public void Open()
        {
            isOpen = true;
            if (manageRootVisibility && !gameObject.activeSelf)
                gameObject.SetActive(true);

            FocusSelected();
        }

        public void Close()
        {
            isOpen = false;
            if (manageRootVisibility && gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        void Update()
        {
            if (!isOpen || buttons.Count == 0) return;

            // Navegación básica teclado (izq/der o arriba/abajo)
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                Move(-1);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                Move(+1);

            // Confirmación
            if (UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(KeyCode.Space))
                buttons[selectedIndex].onClick.Invoke();

            // Hotkeys configuradas
            int idx = 0;
            foreach (var e in config.entries)
            {
                if (!e.enabled) continue;
                if (e.hotkey != KeyCode.None && UnityEngine.Input.GetKeyDown(e.hotkey))
                {
                    selectedIndex = idx;
                    FocusSelected();
                    buttons[selectedIndex].onClick.Invoke();
                    break;
                }
                idx++;
            }
        }

        void Move(int delta)
        {
            if (buttons.Count == 0) return;
            selectedIndex = (selectedIndex + delta + buttons.Count) % buttons.Count;
            FocusSelected();
        }

        void FocusSelected()
        {
            if (buttons.Count == 0) return;
            var go = buttons[selectedIndex].gameObject;
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(go);
        }

        void Select(ActionType action)
        {
            if (debugLogs) Debug.Log($"[CommandMenuUI] Selected: {action}");
            OnCommandSelected?.Invoke(action);
        }

        void SetupButtonVisuals(Button btn, CommandMenuConfig.Entry entry)
        {
            // Label: intenta TMP_Text, luego Text legacy
            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp) tmp.text = entry.label;
            var legacyText = btn.GetComponentInChildren<Text>();
            if (legacyText) legacyText.text = entry.label;

            // Icon: si el prefab tiene Image adicional (no el targetGraphic del Button)
            var images = btn.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == btn.targetGraphic) continue;
                if (entry.icon) { img.sprite = entry.icon; img.enabled = true; }
            }
        }

        void ClearChildren(Transform root)
        {
            if (!root) return;
            for (int i = root.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
                DestroyImmediate(root.GetChild(i).gameObject);
#else
                Destroy(root.GetChild(i).gameObject);
#endif
        }
    }
}