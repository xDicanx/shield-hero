using System;
using SH.Core;

namespace SH.Input
{
    // Interfaz para abstraer orígenes de input (Legacy / UI)
    public interface IPlayerInputSource
    {
        // RequestAction: solicita la decisión del actor (puede ser instantáneo o esperar interacción).
        // onDecision debe invocarse exactamente 1 vez o CancelRequest() será llamado.
        void RequestAction(IActor actor, Action<ActionData> onDecision);

        // Cancelar solicitud activa (por ejemplo, al salir de escena o cambiar modo).
        void CancelRequest();
    }
}