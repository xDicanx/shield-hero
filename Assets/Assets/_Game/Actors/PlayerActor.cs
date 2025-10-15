
using UnityEngine;
using System.Linq;
using SH.Core;

namespace SH.Actors
{
    public class PlayerActor : ActorBase
    {
        [Tooltip("Referencia al TurnLoop en escena")]
        public TurnLoop loop;

        void OnValidate()
        {
            if (!loop) loop = FindObjectOfType<TurnLoop>();
        }

        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // GUI mínima por consola:
            Debug.Log($"-- TURNO de {Name} -- (A=Attack  D=Defend  W=Wait)");

            // Esperar input en Update-like: usamos un helper coroutine en TurnLoop
            loop.WaitForPlayerInput(keys =>
            {
                if (keys.attack)
                {
                    var target = loop.AliveActors()
                                     .FirstOrDefault(a => a.Team == Team.Enemy);
                    if (target == null)
                    {
                        onActionReady(new ActionData(ActionType.Wait, this, null));
                        return;
                    }
                    onActionReady(new ActionData(ActionType.Attack, this, target, Attack));
                }
                else if (keys.defend)
                {
                    onActionReady(new ActionData(ActionType.Defend, this, null));
                }
                else
                {
                    onActionReady(new ActionData(ActionType.Wait, this, null));
                }
            });
        }
    }
}
