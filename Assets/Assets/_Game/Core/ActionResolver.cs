using UnityEngine;

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

        // Ataque básico (con parry)
        private static void ResolveAttack(ActionData action)
        {
            if (action.Target != null && action.Target.IsAlive)
            {
                bool parry = false;
                var targetMB = (action.Target as UnityEngine.MonoBehaviour);
                if (targetMB && action.Target.Team == Team.Player)
                    parry = ParryWindow.IsActiveNow();

                int dmg = action.Amount;
                if (parry)
                {
                    dmg = 0;
                    Debug.Log($"[PARRY] {action.Target.Name} parrea el golpe (+1 carga).");
                }

                Debug.Log(action.ToString());
                var steps = ActionTimelineBuilder.BuildAttackSteps(action, dmg, parry, targetMB);
                ActionStepSequencer.PlayNow(steps);
            }
        }

        // Defensa básica
        private static void ResolveDefend(ActionData action)
        {
            var steps = ActionTimelineBuilder.BuildDefendSteps(action);
            ActionStepSequencer.PlayNow(steps);
            Debug.Log($"{action.Attacker?.Name} DEFEND");
        }

        // Espera
        private static void ResolveWait(ActionData action)
        {
            Debug.Log($"{action.Attacker?.Name} WAIT");
        }

        // Golpe de escudo (usa timeline)
        private static void ResolveShieldSkill(ActionData action)
        {
            if (action.Target != null && action.Target.IsAlive)
            {
                Debug.Log($"[SHIELD] {action.Attacker?.Name} golpea con escudo por {action.Amount}");
                var steps = ActionTimelineBuilder.BuildShieldSkillSteps(action);
                ActionStepSequencer.PlayNow(steps);
            }
        }
    }
}