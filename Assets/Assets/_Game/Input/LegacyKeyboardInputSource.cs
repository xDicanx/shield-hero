using System;
using UnityEngine;
using SH.Core;

namespace SH.Input
{
    // Adaptador que replica el sistema actual de teclas (A/D/S/W -> acciones).
    // Basta para testing y para mantener compatibilidad.
    [AddComponentMenu("Input/LegacyKeyboardInputSource")]
    public class LegacyKeyboardInputSource : MonoBehaviour, IPlayerInputSource
    {
        private Action<ActionData> pendingCallback;
        private IActor pendingActor;

        public void RequestAction(IActor actor, Action<ActionData> onDecision)
        {
            pendingActor = actor;
            pendingCallback = onDecision;
        }

        public void CancelRequest()
        {
            pendingCallback = null;
            pendingActor = null;
        }

        private void Update()
        {
            if (pendingCallback == null) return;

            // Mapeo simple — ajustar según ActionType real.
            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                InvokeDecision(ActionType.Attack);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                InvokeDecision(ActionType.Defend);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                InvokeDecision(ActionType.ShieldSkill); // ejemplo
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                InvokeDecision(ActionType.Wait); // ejemplo
            }
        }

        private void InvokeDecision(ActionType type)
        {
            var action = new ActionData(type, pendingActor, null);
            var cb = pendingCallback;
            CancelRequest();
            cb?.Invoke(action);
        }
    }
}
