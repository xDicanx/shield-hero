using System.Collections.Generic;
using SH.Core;

/// <summary>
/// Encapsula el chequeo de condiciones de victoria o derrota.
/// </summary>
namespace SH.Core
{
    public enum VictoryState { None, PlayersWin, EnemiesWin }

    public class TurnVictoryChecker
    {
        /// <summary>
        /// Determina el estado de victoria o derrota basado en los actores vivos.
        /// </summary>
        public VictoryState CheckVictoryOrDefeat(List<IActor> order)
        {
            bool playersAlive = false, enemiesAlive = false;
            foreach (var a in order)
            {
                if (a.Team == Team.Player && a.IsAlive) playersAlive = true;
                if (a.Team == Team.Enemy && a.IsAlive) enemiesAlive = true;
            }
            if (!playersAlive) return VictoryState.EnemiesWin;
            if (!enemiesAlive) return VictoryState.PlayersWin;
            return VictoryState.None;
        }
    }
}