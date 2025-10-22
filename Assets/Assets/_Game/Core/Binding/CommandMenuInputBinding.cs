using UnityEngine;
using SH.Core;
using SH.UI;
using SH.Input;

namespace SH.Core.Bindings
{
    // Conecta CommandMenuUI → CommandMenuInputSource.
    // Además cierra el menú al confirmar para no solaparlo con el TargetSelector.
    [AddComponentMenu("Bindings/CommandMenu Input Binding")]
    public class CommandMenuInputBinding : MonoBehaviour
    {
        [Header("References")]
        public CommandMenuUI menu;
        public CommandMenuInputSource uiSource;

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;

        void Awake()
        {
            if (!menu)     menu     = FindObjectOfType<CommandMenuUI>(includeInactive: true);
            if (!uiSource) uiSource = FindObjectOfType<CommandMenuInputSource>(includeInactive: true);

            if (debugLogs)
                Debug.Log($"[Binding] Awake → menu={(menu?menu.name:"null")}, uiSource={(uiSource?uiSource.name:"null")}");
        }

        void OnEnable()
        {
            if (menu) menu.OnCommandSelected += HandleSelected;
        }

        void OnDisable()
        {
            if (menu) menu.OnCommandSelected -= HandleSelected;
        }

        void HandleSelected(ActionType type)
        {
            if (debugLogs) Debug.Log($"[Binding] OnCommandSelected → {type}");
            // Cerrar menú inmediatamente para no solapar con el TargetSelector.
            if (menu) menu.Close();

            if (uiSource) uiSource.Confirm(type);
        }

        [ContextMenu("Auto-Wire References")]
        void AutoWire()
        {
            Awake();
            if (debugLogs) Debug.Log("[Binding] Auto-Wire executed.");
        }
    }
}