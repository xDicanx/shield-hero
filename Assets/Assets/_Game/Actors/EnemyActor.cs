using UnityEngine;
using System.Linq;
using SH.Core;

namespace SH.Actors
{
    public class EnemyActor : ActorBase
    {
        [Tooltip("Referencia al TurnLoop en escena")]
        public TurnLoop loop;

        void OnValidate()
        {
            if (!loop) loop = FindObjectOfType<TurnLoop>();
        }

        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // Elegir objetivo jugador vivo (simple: primero que encontremos)
            var target = loop.AliveActors().FirstOrDefault(a => a.Team == Team.Player);
            if (target == null)
            {
                onActionReady(new ActionData(ActionType.Wait, this, null));
                return;
            }

            // 15% chance de defender, sino atacar
            if (Random.value < 0.15f)
                onActionReady(new ActionData(ActionType.Defend, this, null));
            else
                onActionReady(new ActionData(ActionType.Attack, this, target, Attack));
        }
    }
}
