using System;
using UnityEngine;
using SH.Core;

namespace SH.Core
{
    /// <summary>
    /// Ejecuta el turno de un actor y delega input si aplica.
    /// Clase no-MonoBehaviour, usada por TurnLoop.
    /// </summary>
    public class TurnRunner
    {
        /// <summary>
        /// Ejecuta el turno de un actor. Usa callback para resolución.
        /// </summary>
        public void RunActorTurn(IActor actor, Action<ActionData> onActionReady, Action<Action<(bool attack, bool defend, bool shield)>> inputRequest)
        {
            // Si el actor es jugador, delega input; si no, turno automático.
            actor.TakeTurn(onActionReady);
        }

        /// <summary>
        /// Manejo de input global del jugador. Extraído de Update().
        /// </summary>
        public void HandlePlayerInput(ref Action<(bool attack, bool defend, bool shield)> onPlayerKeys)
        {
            if (onPlayerKeys != null)
            {
                bool atk = UnityEngine.Input.GetKeyDown(KeyCode.A);
                bool def = UnityEngine.Input.GetKeyDown(KeyCode.D);
                bool sh = UnityEngine.Input.GetKeyDown(KeyCode.S);
                bool any = atk || def || sh || UnityEngine.Input.GetKeyDown(KeyCode.W);

                if (any)
                {
                    var cb = onPlayerKeys;
                    onPlayerKeys = null;
                    cb((atk, def, sh));
                }
            }
        }
    }
}