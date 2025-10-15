using UnityEngine;
using SH.Core;

namespace SH.Core
{
    public static class ActionResolver
    {
        public static void Resolve(ActionData action)
        {
            switch (action.Type)
            {
                case ActionType.Attack:
                    if (action.Target != null && action.Target.IsAlive)
                    {
                        Debug.Log(action.ToString());
                        action.Target.ReceiveDamage(action.Amount);
                    }
                    break;

                case ActionType.Defend:
                    action.Attacker?.ReceiveDefend();
                    Debug.Log($"{action.Attacker?.Name} DEFEND");
                    break;

                case ActionType.Wait:
                    Debug.Log($"{action.Attacker?.Name} WAIT");
                    break;
            }
        }
    }
}
