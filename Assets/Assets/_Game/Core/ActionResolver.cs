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
                            // Ajuste de balance: daño reducido a 0 y +1 carga.
                            dmg = 0;
                            var charge = targetMB.GetComponent<SH.Core.ShieldChargeSystem>();
                            if (charge) charge.AddCharge(1);
                            UnityEngine.Debug.Log($"[PARRY] {action.Target.Name} parrea el golpe (+1 carga).");
                        }

                        UnityEngine.Debug.Log(action.ToString());
                        action.Target.ReceiveDamage(dmg);
                    }
                    break;
                case ActionType.Defend:
                    action.Attacker?.ReceiveDefend();
                    Debug.Log($"{action.Attacker?.Name} DEFEND");
                    break;

                case ActionType.Wait:
                    Debug.Log($"{action.Attacker?.Name} WAIT");
                    break;
                case ActionType.ShieldSkill:
                if (action.Target != null && action.Target.IsAlive)
                {
                    // Golpe de escudo: daño directo (ya viene con el bonus aplicado).
                    UnityEngine.Debug.Log($"[SHIELD] {action.Attacker?.Name} golpea con escudo por {action.Amount}");
                    action.Target.ReceiveDamage(action.Amount);
                }
                break;
            }
        }
    }
}
