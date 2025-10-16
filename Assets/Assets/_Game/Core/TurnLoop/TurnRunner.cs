using UnityEngine;
using SH.Core;

/// <summary>
/// Ejecuta el turno de un actor y delega input si aplica.
/// </summary>
namespace SH.Core
{
    public class TurnRunner
    {
        /// <summary>
        /// Ejecuta el turno de un actor. Usa callback para resolución.
        /// </summary>
        public void RunActorTurn(IActor actor, System.Action<ActionData> onActionReady, System.Action<System.Action<(bool, bool, bool)>> inputRequest)
        {
            // Si el actor es jugador, delega input; si no, turno automático.
            actor.TakeTurn(onActionReady);
        }

        /// <summary>
        /// Manejo de input global del jugador. Extraído de Update().
        /// </summary>
        public void HandlePlayerInput(ref System.Action<(bool attack, bool defend, bool shield)> onPlayerKeys)
        {
            if (onPlayerKeys != null)
            {
                bool atk = Input.GetKeyDown(KeyCode.A);
                bool def = Input.GetKeyDown(KeyCode.D);
                bool sh = Input.GetKeyDown(KeyCode.S);
                bool any = atk || def || sh || Input.GetKeyDown(KeyCode.W);

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