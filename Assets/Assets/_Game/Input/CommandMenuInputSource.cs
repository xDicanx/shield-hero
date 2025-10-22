using System;
using UnityEngine;
using SH.Core;

namespace SH.Input
{
    // Fuente UI básica: guarda el actor y el callback cuando TurnLoop pide acción.
    // El binding del menú (CommandMenuInputBinding) llama Confirm(ActionType) al seleccionar.
    [AddComponentMenu("Input/CommandMenuInputSource")]
    public class CommandMenuInputSource : MonoBehaviour, IPlayerInputSource
    {
        private Action<ActionData> pendingCallback;
        private IActor pendingActor;

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;

        public void RequestAction(IActor actor, Action<ActionData> onDecision)
        {
            pendingActor = actor;
            pendingCallback = onDecision;
            if (debugLogs) Debug.Log($"[UIInputSource] RequestAction for {actor?.Name ?? "(null)"}");
        }

        public void CancelRequest()
        {
            if (debugLogs) Debug.Log("[UIInputSource] CancelRequest");
            pendingCallback = null;
            pendingActor = null;
        }

        // NUEVO: llamado por CommandMenuInputBinding cuando el jugador elige en el menú.
        public void Confirm(ActionType type)
        {
            if (debugLogs) Debug.Log($"[UIInputSource] Confirm {type}");
            if (pendingCallback == null || pendingActor == null)
            {
                if (debugLogs) Debug.LogWarning("[UIInputSource] Confirm() sin pendingCallback o pendingActor (ignorado).");
                return;
            }

            // En esta fase (MenuOnly) no hay target; el MenuThenTargetInputSource agrega el objetivo después.
            var data = new ActionData(type, pendingActor, null, 0);
            SubmitSelectedAction(data);
        }

        // También útil para tests o si en el futuro el menú construye ActionData completo.
        public void SubmitSelectedAction(ActionData selected)
        {
            if (debugLogs) Debug.Log($"[UIInputSource] SubmitSelectedAction {selected.Type}");
            var cb = pendingCallback;
            CancelRequest(); // limpia estado
            cb?.Invoke(selected);
        }
    }
}