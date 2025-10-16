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
                    ResolveAttack(action);
                    break;
                case ActionType.Defend:
                    ResolveDefend(action);
                    break;
                case ActionType.Wait:
                    ResolveWait(action);
                    break;
                case ActionType.ShieldSkill:
                    ResolveShieldSkill(action);
                    break;
            }
        }

        // Encapsula la lógica de ataque (incluye parry y log)
        private static void ResolveAttack(ActionData action)
        {
            if (action.Target != null && action.Target.IsAlive)
            {
                // ¿Parry activo justo ahora?
                bool parry = false;
                var targetMB = (action.Target as UnityEngine.MonoBehaviour);
                if (targetMB && action.Target.Team == Team.Player)
                {
                    parry = SH.Core.ParryWindow.IsActiveNow();
                }

                int dmg = action.Amount;

                if (parry)
                {
                    // Parry exitoso: reducimos (o anulamos) y añadimos 1 carga.
                    dmg = 0;
                    var charge = targetMB.GetComponent<SH.Core.ShieldChargeSystem>();
                    if (charge) charge.AddCharge(1);
                    UnityEngine.Debug.Log($"[PARRY] {action.Target.Name} parrea el golpe (+1 carga).");
                }

                UnityEngine.Debug.Log(action.ToString());
                action.Target.ReceiveDamage(dmg);
            }
        }

        // Lógica de defensa
        private static void ResolveDefend(ActionData action)
        {
            action.Attacker?.ReceiveDefend();
            Debug.Log($"{action.Attacker?.Name} DEFEND");
        }

        // Lógica de espera
        private static void ResolveWait(ActionData action)
        {
            Debug.Log($"{action.Attacker?.Name} WAIT");
        }

        // Lógica de golpe de escudo
        private static void ResolveShieldSkill(ActionData action)
        {
            if (action.Target != null && action.Target.IsAlive)
            {
                UnityEngine.Debug.Log($"[SHIELD] {action.Attacker?.Name} golpea con escudo por {action.Amount}");
                action.Target.ReceiveDamage(action.Amount);
            }
        }
    }
}